using Magpie.Engine;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Magpie.Engine.Controls;
using static Magpie.Engine.DigitalControlBindings;

namespace Magpie {
    public static class EngineState {
        public static GBuffer buffer;
        public static Camera camera;
        public static XYPair resolution;
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

        public static InstancedBinds player_binds_one;
        public static InstancedBinds player_binds_two;
        public static InstancedBinds player_binds_three;
        public static InstancedBinds player_binds_four;

        public static void initialize(XYPair game_resolution, GameWindow game_window, 
            GraphicsDevice gd, GraphicsDeviceManager gdm, Game game) {

            window = game_window;
            resolution = game_resolution;
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
        }

        public static void change_resolution(int X, int Y) {

            graphics.PreferredBackBufferWidth = X;
            graphics.PreferredBackBufferHeight = Y;
            
            graphics.ApplyChanges();

            resolution = new XYPair(X, Y);
            buffer.change_resolution(graphics_device, X, Y);            
        }

        public static void Update(GameTime gt, Game game) {
            was_active = is_active;

            gametime = gt;

            Clock.update(gametime, game);

            if (player_binds_one != null && player_binds_one.player_index != PlayerIndex.One) {player_binds_one.change_player_index(PlayerIndex.One);}
            if (player_binds_two != null && player_binds_two.player_index != PlayerIndex.Two) {player_binds_two.change_player_index(PlayerIndex.Two);}
            if (player_binds_three != null && player_binds_three.player_index != PlayerIndex.Three) {player_binds_three.change_player_index(PlayerIndex.Three);}
            if (player_binds_four != null && player_binds_four.player_index != PlayerIndex.Four) {player_binds_four.change_player_index(PlayerIndex.Four);}

            is_active = game.IsActive;
            
        }
    }
}
