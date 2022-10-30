using Magpie.Engine;
using Magpie.Graphics;
using Magpie.Graphics.UI;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Magpie.Engine.Controls;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Magpie {
    public static class EngineState {
        public static volatile bool running = true;

        public static GBuffer buffer;
        public static Camera camera;

        public static XYPair resolution => gvars.get_xypair("resolution");
        public static float display_refresh_rate = 60;

        public static List<DisplayInfo.display_mode> display_modes;
        static int current_display_mode_index = -1;
        //public static DisplayInfo.display_mode current_display_mode => display_modes[current_display_mode_index];
        public static Vector2 aspect_ratios => new Vector2((float)resolution.X / (float)resolution.Y, (float)resolution.Y / (float)resolution.X);

        public static GameWindow window;
        public static GraphicsDeviceManager graphics;
        public static GraphicsDevice graphics_device;
        public static GameTime gametime;
        public static Game game;
        public static SpriteBatch spritebatch;
        public static Viewport viewport => graphics_device.Viewport;

        public static bool ui_layer_clicked;
        public static bool is_active;
        public static bool was_active;

        public static ControlBinds player_binds_one;
        public static ControlBinds player_binds_two;
        public static ControlBinds player_binds_three;
        public static ControlBinds player_binds_four;

        public static UIWindowManager window_manager;

        public static World world;

        public static void initialize(XYPair game_resolution, GameWindow game_window,
            GraphicsDevice gd, GraphicsDeviceManager gdm, Game game) {

            window = game_window;
            EngineState.game = game;
            graphics = gdm;
            graphics_device = gd;
            spritebatch = new SpriteBatch(graphics_device);

            Draw2D.init();

            //set up default settings and configure some gvars

            //on first run we need the primary screen to be the main display, so set display to get_primary_screen()
            gvars.add_gvar("display", gvar_data_type.INT, DisplayInfo.get_primary_screen(), true);

            //get screen info
            var scrn = Screen.AllScreens[gvars.get_int("display")];
            var screen_bounds = scrn.Bounds.Size.ToXYPair();
            var screen_pos = scrn.Bounds.Location.ToXYPair();
            
            DisplayInfo.get_display_modes((uint)gvars.get_int("display"), out display_modes, out _, out _);

            //find the highest refresh rate for the current game resolution
            var mode = DisplayInfo.find_display_mode_highest_hz_at_res(screen_bounds.X, screen_bounds.Y, display_modes, out current_display_mode_index);

            //if one isn't found, use the highest refresh rate supported by the highest resolution
            //(reasoning behind this being that the highest resolution is likely to not have the highest refresh rates, and its refresh rate should be valid for all other resolutions (we hope (though this only matters for fullscreen)))
            if (current_display_mode_index == -1) {
                display_refresh_rate = DisplayInfo.highest_hz_supported_by_highest_res(display_modes, out current_display_mode_index);
            }

            gvars.add_gvar("resolution", gvar_data_type.XYPAIR, screen_bounds,      true);
            gvars.add_gvar("super_resolution_scale", gvar_data_type.FLOAT, 1f,      true);
            gvars.add_gvar("borderless", gvar_data_type.BOOL, true,                 true);
            gvars.add_gvar("fullscreen", gvar_data_type.BOOL, false,                true);
            gvars.add_gvar("vsync", gvar_data_type.BOOL, true,                      true);
            gvars.add_gvar("frame_limit", gvar_data_type.FLOAT, mode.refresh_rate,  true);
            
            gvars.add_gvar("window_position", gvar_data_type.XYPAIR, game_window.Position.ToXYPair(), false);
            gvars.add_gvar("window_size", gvar_data_type.XYPAIR, game_window.ClientBounds.Size.ToXYPair(), false);

            gvars.add_gvar("light_enabled", gvar_data_type.BOOL, true,              false);
            gvars.add_gvar("light_follow", gvar_data_type.BOOL, true,               false);
            gvars.add_gvar("light_shadows", gvar_data_type.BOOL, true,              false);
            gvars.add_gvar("light_cookie", gvar_data_type.STRING, "radial_glow",    false);
            gvars.add_gvar("light_resolution", gvar_data_type.INT, 2048,            false);
            gvars.add_gvar("light_far", gvar_data_type.FLOAT, 30f,                  false);
            gvars.add_gvar("light_near", gvar_data_type.FLOAT, 1f,                  false);
            gvars.add_gvar("light_C", gvar_data_type.FLOAT, 5f,                     false);
            gvars.add_gvar("light_bias", gvar_data_type.FLOAT, 0.0008f,             false);


            //after we set up, read gvar file, then write it out again so that if there aren't any contents, there is now a file and it will have the defaults from above written to it
            gvars.read_gvars_from_disk();
            gvars.write_gvars_to_disk();

            game.MaxElapsedTime = TimeSpan.FromMilliseconds(1000f);

            //set up resolution/screen mode/etc
            gdm.PreferredBackBufferWidth = resolution.X;
            gdm.PreferredBackBufferHeight = resolution.Y;

            game_window.IsBorderless = gvars.get_bool("borderless");

            gdm.HardwareModeSwitch = true;
            gdm.IsFullScreen = gvars.get_bool("fullscreen");
            was_fullscreen = gdm.IsFullScreen;

            gdm.SynchronizeWithVerticalRetrace = gvars.get_bool("vsync");

            gdm.ApplyChanges();

            //set up window gvars and recenter window
            scrn = Screen.AllScreens[gvars.get_int("display")];
            screen_bounds = scrn.Bounds.Size.ToXYPair();
            screen_pos = scrn.Bounds.Location.ToXYPair();

            window.Position = ((screen_pos + (screen_bounds / 2)) - (resolution / 2)).ToPoint();

            gvars.set("window_position", game_window.Position.ToXYPair());
            gvars.set("window_size", game_window.ClientBounds.Size.ToXYPair());

            //more graphics setup
            buffer = new GBuffer();
            buffer.CreateInPlace(graphics_device, resolution.X, resolution.Y, gvars.get_float("super_resolution_scale"));

            Scene.configure_renderer();

            Draw3D.init();

            window_manager = new UIWindowManager();

            //add actions to all the gvars that need them, this comes late so that they don't get triggered during setup
            gvars.add_change_action("resolution", apply_resolution);
            gvars.add_change_action("super_resolution_scale", apply_internal_scale);
            gvars.add_change_action("fullscreen", fullscreen);
            gvars.add_change_action("vsync", vsync);
            gvars.add_change_action("display", change_display);
            gvars.add_change_action("borderless", () => { window.IsBorderless = gvars.get_bool("borderless"); });

            //add controls
            StaticControlBinds.add_bindings(
                (bind_type.digital, controller_type.keyboard, Keys.F2, "switch_buffer"),
                (bind_type.digital, controller_type.keyboard, Keys.F5, "screenshot")


                );
        }
        static bool was_fullscreen = false;
        static void fullscreen() {
            if (gvars.get_bool("fullscreen") == was_fullscreen) return;

            var scrn = Screen.AllScreens[gvars.get_int("display")];
            var screen_bounds = scrn.Bounds.Size.ToXYPair();
            var screen_pos = scrn.Bounds.Location.ToXYPair();

            window.Position = ((screen_pos + (screen_bounds / 2)) - (resolution / 2)).ToPoint();

            graphics.HardwareModeSwitch = true;
            graphics.IsFullScreen = gvars.get_bool("fullscreen");
            graphics.ApplyChanges();

            scrn = Screen.AllScreens[gvars.get_int("display")];
            screen_bounds = scrn.Bounds.Size.ToXYPair();
            screen_pos = scrn.Bounds.Location.ToXYPair();

            window.Position = ((screen_pos + (screen_bounds / 2)) - (resolution / 2)).ToPoint();
            
            was_fullscreen = graphics.IsFullScreen;
        }

        static void change_display() {
            var scrn = Screen.AllScreens[gvars.get_int("display")];
            var screen_bounds = scrn.Bounds.Size.ToXYPair();
            var screen_pos = scrn.Bounds.Location.ToXYPair();

            window.Position = ((screen_pos + (screen_bounds / 2)) - (resolution / 2)).ToPoint();

            DisplayInfo.get_display_modes((uint)gvars.get_int("display"), out display_modes, out _, out _);

            var mode = DisplayInfo.find_display_mode_highest_hz_at_res(screen_bounds.X, screen_bounds.Y, display_modes, out current_display_mode_index);
            if (current_display_mode_index == -1) {
                display_refresh_rate = DisplayInfo.highest_hz_supported_by_highest_res(display_modes, out current_display_mode_index);
            }
        }
        static void vsync() {
            graphics.SynchronizeWithVerticalRetrace = gvars.get_bool("vsync");
            graphics.ApplyChanges();
        }

        static void apply_resolution() {
            graphics.PreferredBackBufferWidth = resolution.X;
            graphics.PreferredBackBufferHeight = resolution.Y;

            buffer.change_resolution(graphics_device, resolution.X, resolution.Y);

            graphics.ApplyChanges();

            var scrn = Screen.AllScreens[gvars.get_int("display")];
            var screen_bounds = scrn.Bounds.Size.ToXYPair();
            var screen_pos = scrn.Bounds.Location.ToXYPair();

            window.Position = ((screen_pos + (screen_bounds / 2)) - (resolution / 2)).ToPoint();

            gvars.set("window_position", window.Position.ToXYPair());
            gvars.set("window_size", window.ClientBounds.Size.ToXYPair());
        }

        public static void change_resolution(int X, int Y) {

            graphics.PreferredBackBufferWidth = X;
            graphics.PreferredBackBufferHeight = Y;
            
            graphics.ApplyChanges();

            gvars.set("resolution", new XYPair(X, Y));
            buffer.change_resolution(graphics_device, X, Y);

            var scrn = Screen.AllScreens[gvars.get_int("display")];
            var screen_bounds = scrn.Bounds.Size.ToXYPair();
            var screen_pos = scrn.Bounds.Location.ToXYPair();

            window.Position = ((screen_pos + (screen_bounds / 2)) - (resolution / 2)).ToPoint();

            gvars.set("window_position", window.Position.ToXYPair());
            gvars.set("window_size", window.ClientBounds.Size.ToXYPair());
        }

        public static void apply_internal_scale() {
            buffer.change_resolution_super(graphics_device, resolution.X, resolution.Y, gvars.get_float("super_resolution_scale"));
        }

        public static void Update(GameTime gt, Game game) {
            if (running == false) {
                game.Exit();
            }

            was_active = is_active;
            gametime = gt;
            
            Clock.update(gametime, game);

            Log.update();

            Controls.update(EngineState.window, EngineState.game.IsActive, EngineState.resolution);

            StaticControlBinds.update();

            window_manager.update();

            EngineState.camera.update();
            EngineState.camera.update_projection(EngineState.resolution);

            //if (player_binds_one != null && player_binds_one.player_index != PlayerIndex.One) {player_binds_one.change_player_index(PlayerIndex.One);}
            //if (player_binds_two != null && player_binds_two.player_index != PlayerIndex.Two) {player_binds_two.change_player_index(PlayerIndex.Two);}
            //if (player_binds_three != null && player_binds_three.player_index != PlayerIndex.Three) {player_binds_three.change_player_index(PlayerIndex.Three);}
            //if (player_binds_four != null && player_binds_four.player_index != PlayerIndex.Four) {player_binds_four.change_player_index(PlayerIndex.Four);}

            is_active = game.IsActive;

            gvars.set("window_position", window.Position.ToXYPair());
            gvars.set("window_size", window.ClientBounds.Size.ToXYPair());

            Scene.sun_moon.update();
        }

        public static void draw2d() {
            EngineState.window_manager.draw();
        }
    }
}
