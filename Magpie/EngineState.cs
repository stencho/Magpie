using Magpie.Engine;
using Magpie.Engine.Stages;
using Magpie.Graphics;
using Magpie.Graphics.UI;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        public static volatile bool drawing = true;
        public static volatile bool started = false;

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

        public static int player_id_from_index(PlayerIndex index) {
            switch (index) {
                case PlayerIndex.One:
                    return player_id;                    
                case PlayerIndex.Two:
                    return player_id_two;
                case PlayerIndex.Three:
                    return player_id_three;
                case PlayerIndex.Four:
                    return player_id_four;
            }

            return player_id;
        }

        public static int player_id;
        public static int player_id_two;
        public static int player_id_three;
        public static int player_id_four;

        public static ControlBinds player_binds;
        public static ControlBinds player_binds_two;
        public static ControlBinds player_binds_three;
        public static ControlBinds player_binds_four;

        public static UIWindowManager window_manager;
        public static UIWindow octree_info_window;

        public static World world;

        public static StringBuilder startup_log;

        public static void debug_info_window_draw() {
        }
        public static void debug_info_window_draw_internal() {
            world.current_map.octree.draw_info_2D();

        }

        public static void initialize(XYPair game_resolution, GameWindow game_window,
            GraphicsDevice gd, GraphicsDeviceManager gdm, Game game) {

            startup_log = new StringBuilder();
            startup_log.AppendLine("starting Magpie");

#if DEBUG
           try {
#endif
                window = game_window;
                EngineState.game = game;
                graphics = gdm;
                graphics_device = gd;
                spritebatch = new SpriteBatch(graphics_device);

                startup_log.Append("initializing 2D... ");
                Draw2D.init();

                //set up default settings and configure some gvars
                startup_log.AppendLine("finding primary display and resolution");

                //on first run we need the primary screen to be the main display, so set display to get_primary_screen()
                gvars.add_gvar("display", gvar_data_type.INT, DisplayInfo.get_primary_screen(), true);

                //get screen info
                var scrn = Screen.AllScreens[gvars.get_int("display")];
                var screen_bounds = scrn.Bounds.Size.ToXYPair();
                var screen_pos = scrn.Bounds.Location.ToXYPair();

                startup_log.AppendLine($"monitor {gvars.get_int("display").ToString()} is primary: res [{screen_bounds.ToXString()}] pos [{screen_pos.ToXString()}]");

                startup_log.Append($"configuring gvar defaults... ");

                gvars.add_gvar("resolution", gvar_data_type.XYPAIR, screen_bounds, true);
                gvars.add_gvar("super_resolution_scale", gvar_data_type.FLOAT, 1f, true);
                gvars.add_gvar("borderless", gvar_data_type.BOOL, true, true);
                gvars.add_gvar("fullscreen", gvar_data_type.BOOL, false, true);
                gvars.add_gvar("vsync", gvar_data_type.BOOL, true, true);
                gvars.add_gvar("frame_limit", gvar_data_type.FLOAT, 60.0f, true);
                gvars.add_gvar("light_spot_resolution", gvar_data_type.INT, 1024, true);

                gvars.add_gvar("window_position", gvar_data_type.XYPAIR, game_window.Position.ToXYPair(), false);
                gvars.add_gvar("window_size", gvar_data_type.XYPAIR, game_window.ClientBounds.Size.ToXYPair(), false);

                gvars.add_gvar("draw_probes", gvar_data_type.BOOL, false, true);

                startup_log.AppendLine($"done");


                //after we set up, read gvar file, then write it out again so that if there aren't any contents, there is now a file and it will have the defaults from above written to it
                startup_log.Append("reading gvars from disk... ");
                bool gvars_file_found = gvars.read_gvars_from_disk();
                if (gvars_file_found) {
                    startup_log.AppendLine("loaded from file");
                } else {
                    startup_log.AppendLine("gvars file not found or had errors");
                }

                startup_log.Append("writing gvars back to disk... ");
                gvars.write_gvars_to_disk();
                startup_log.AppendLine("done");

                startup_log.AppendLine("configuring graphics");

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

                startup_log.AppendLine("moving window to correct display");

                var d = gvars.get_int("display");
                if (d > Screen.AllScreens.Length - 1 || d < 0) {
                    d = DisplayInfo.get_primary_screen();
                    gvars.set("display", d);
                }

                scrn = Screen.AllScreens[d];
                screen_bounds = scrn.Bounds.Size.ToXYPair();
                screen_pos = scrn.Bounds.Location.ToXYPair();

                window.Position = ((screen_pos + (screen_bounds / 2)) - (resolution / 2)).ToPoint();

                gvars.set("window_position", game_window.Position.ToXYPair());
                gvars.set("window_size", game_window.ClientBounds.Size.ToXYPair());

                startup_log.AppendLine("setting up the graphics buffer");

                //more graphics setup
                buffer = new GBuffer();
                buffer.CreateInPlace(graphics_device, resolution.X, resolution.Y, gvars.get_float("super_resolution_scale"));

                startup_log.AppendLine("configuring renderer");
                Scene.configure_renderer();

                startup_log.AppendLine("initializing 3D");
                Draw3D.init();

                startup_log.AppendLine("initializing window manager");
                window_manager = new UIWindowManager();

                //OCTREE DEBUG INFO
                octree_info_window = new UIWindow(XYPair.One * 400, XYPair.One * 260);
                octree_info_window.draw_action = debug_info_window_draw;
                octree_info_window.internal_draw_action = debug_info_window_draw_internal;
                octree_info_window.change_text("world octree info");

                octree_info_window.add_subform(new UIButton(3, 70, 200, 30, "go to selected octree node"));

                ((UIButton)octree_info_window.subforms[0]).set_action(goto_node_clicked);
                
                window_manager.add_window(octree_info_window);

                startup_log.AppendLine("setting up gvar actions");
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
                    (bind_type.digital, controller_type.keyboard, Keys.F5, "screenshot"));

                File.WriteAllText("log_startup", startup_log.ToString());
#if DEBUG
            } catch (Exception ex){

                startup_log.AppendLine("");
                startup_log.AppendLine(ex.ToString());

                File.WriteAllText("log_startup", startup_log.ToString());

                throw new Exception("Startup failed, see log_startup for information");
            }
#endif

            started = true;
        }

        private static void goto_node_clicked() {
            world.current_map.game_objects[player_id].position =
                world.current_map.octree.get_node(world.current_map.octree.walk_test_node).center;
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

            DisplayInfo.get_display_modes(gvars.get_int("display"), out display_modes, out _, out _);

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

            buffer.change_resolution(graphics_device, resolution.X, resolution.Y, gvars.get_float("super_resolution_scale"));

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
            buffer.change_resolution(graphics_device, resolution.X, resolution.Y, gvars.get_float("super_resolution_scale"));
        }
        public static bool updating_controls = false;
        public static void Update(GameTime gt, Game game) {
            if (running == false) {
                game.Exit();
            }

            was_active = is_active;
            gametime = gt;
            
            Clock.update(gametime, game);

            Log.update();

            updating_controls = true;

            //MOVE THIS TO A SEPARATE THREAD PROBABLY
            Controls.spawn_thread_if_null();
            Controls.update(EngineState.window, EngineState.game.IsActive, EngineState.resolution);
            StaticControlBinds.update();
            updating_controls = false;

            if (StaticControlBinds.just_released("switch_buffer")) {
                if (Scene.buffer < Scene.buffer_count) Scene.buffer++; 
                else Scene.buffer = -1;
            }

            if (StaticControlBinds.just_pressed("screenshot")) Scene.screenshot();

            window_manager.update();
                                    
            if (player_binds != null && player_binds.player_index != PlayerIndex.One) {player_binds.change_player_index(PlayerIndex.One);}
            if (player_binds_two != null && player_binds_two.player_index != PlayerIndex.Two) {player_binds_two.change_player_index(PlayerIndex.Two);}
            if (player_binds_three != null && player_binds_three.player_index != PlayerIndex.Three) {player_binds_three.change_player_index(PlayerIndex.Three);}
            if (player_binds_four != null && player_binds_four.player_index != PlayerIndex.Four) {player_binds_four.change_player_index(PlayerIndex.Four);}

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
