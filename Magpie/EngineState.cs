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
using System.Text;
using System.Threading.Tasks;

using static Magpie.Engine.Controls;

namespace Magpie {
    public static class EngineState {
        public static GBuffer buffer;
        public static Camera camera;
        
        //rename resolution to internal_resolution
        //split resolution changes into external and internal
        //make output_resolution's gvar's change_action point at the external one
        //make internal_resolution's gvar's change action point at the internal

        //should just work? I think everything accounts for this basically
        //well maybe not EVERYTHING

        public static XYPair resolution => gvars.get_xypair("internal_resolution");
        public static XYPair output_resolution => gvars.get_xypair("output_resolution");
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

            Draw2D.init();

            gvars.add_gvar("internal_resolution", gvar_data_type.XYPAIR, game_resolution);
            gvars.add_change_action("internal_resolution", apply_resolution);

            gvars.add_gvar("test_gvar", gvar_data_type.FLOAT, 5f);
            
            window = game_window;
            //resolution = game_resolution;
            EngineState.game = game;
            graphics = gdm;
            graphics_device = gd;
            spritebatch = new SpriteBatch(graphics_device);
            buffer = new GBuffer();
            buffer.CreateInPlace(graphics_device, resolution.X, resolution.Y);

            try {
                Scene.configure_renderer();
            } catch (Exception ex) { return; }

            Draw3D.init();

            window_manager = new UIWindowManager();
            //gvars.add_gvar("")
        }

        static void apply_resolution() {
            graphics.PreferredBackBufferWidth = resolution.X;
            graphics.PreferredBackBufferHeight = resolution.Y;

            buffer.change_resolution(graphics_device, resolution.X, resolution.Y);

            graphics.ApplyChanges();
        }

        public static void change_resolution(int X, int Y) {

            graphics.PreferredBackBufferWidth = X;
            graphics.PreferredBackBufferHeight = Y;
            
            graphics.ApplyChanges();

            //resolution = new XYPair(X, Y);
            gvars.set("internal_resolution", new XYPair(X, Y));
            buffer.change_resolution(graphics_device, X, Y);            
        }

        public static void Update(GameTime gt, Game game) {
            was_active = is_active;
            gametime = gt;
            
            Clock.update(gametime, game);

            Log.update();

            Controls.update(window, game.IsActive, resolution);

            StaticControlBinds.update();

            window_manager.update();

            //if (player_binds_one != null && player_binds_one.player_index != PlayerIndex.One) {player_binds_one.change_player_index(PlayerIndex.One);}
            //if (player_binds_two != null && player_binds_two.player_index != PlayerIndex.Two) {player_binds_two.change_player_index(PlayerIndex.Two);}
            //if (player_binds_three != null && player_binds_three.player_index != PlayerIndex.Three) {player_binds_three.change_player_index(PlayerIndex.Three);}
            //if (player_binds_four != null && player_binds_four.player_index != PlayerIndex.Four) {player_binds_four.change_player_index(PlayerIndex.Four);}

            is_active = game.IsActive;
            
        }

        public static void draw2d() {
            EngineState.window_manager.draw();
        }
    }
}
