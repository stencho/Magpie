using Magpie.Engine;
using Magpie.Graphics;
using Magpie.Graphics.UI;
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
        public static UILayer ui;
        public static SpriteBatch spritebatch;

        public static bool ui_layer_clicked;
        public static bool is_active;
        public static bool was_active;

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

            Scene.configure_renderer();
            ui = new UILayer(EngineState.resolution);

            Draw3D.init(graphics_device);
        }

        public static void Update(GameTime gt, Game game) {
            was_active = is_active;

            gametime = gt;

            Clock.update(gametime, game);
            Clock.update_fps();

            Controls.update(window,game.IsActive, resolution);

            ui.update(out ui_layer_clicked);
            ui.hit_scan(mouse_position.X, mouse_position.Y);

            is_active = game.IsActive;
            
        }
    }
}
