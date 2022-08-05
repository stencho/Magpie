using Magpie;
using Magpie.Engine;
using Magpie.Graphics;
using Magpie.Graphics.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using static Magpie.Engine.Controls;
using static Magpie.Engine.DigitalControlBindings;
namespace SDFTool2D {
    public class SDFEditor : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        ImageViewer image_view;
        ImageViewer sdf_view;

        Effect sdf_effect;

        public SDFEditor()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1600;
            graphics.PreferredBackBufferHeight = 900;
            graphics.ApplyChanges();

            this.IsMouseVisible = true;
            this.graphics.SynchronizeWithVerticalRetrace = false;
            this.IsFixedTimeStep = false;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += Window_ClientSizeChanged;
        }
               
        private void Window_ClientSizeChanged(object sender, System.EventArgs e) {
            Console.WriteLine(Window.ClientBounds.Size.ToVector2().simple_vector2_x_string_no_dec());

            graphics.PreferredBackBufferWidth = Window.ClientBounds.Size.X;
            graphics.PreferredBackBufferHeight = Window.ClientBounds.Size.Y;
            graphics.ApplyChanges();

            Console.WriteLine(graphics.PreferredBackBufferWidth);
            Console.WriteLine(graphics.PreferredBackBufferHeight);
            //image_view.change_size()
        }

        protected override void Initialize() { 
            base.Initialize();
            Console.WriteLine(Window.ClientBounds.Size.ToVector2().simple_vector2_x_string_no_dec());
            EngineState.initialize(new XYPair(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Window, GraphicsDevice, graphics, this);
                        
            image_view = new ImageViewer(0,50, EngineState.resolution.X / 2, EngineState.resolution.Y-50);

            sdf_view = new ImageViewer(EngineState.resolution.X /2, 50, EngineState.resolution.X / 2, EngineState.resolution.Y-50);
            sdf_view.shader = "sdf_pixel";

            add_bind(new KeyBind(Keys.LeftShift, "shift"));
            add_bind(new KeyBind(Keys.LeftControl, "ctrl"));
            add_bind(new KeyBind(Keys.LeftAlt, "alt"));

            add_bind(new KeyBind(Keys.F5, "screenshot"));
            add_bind(new KeyBind(Keys.OemTilde, "tilde"));

            add_bind(new KeyBind(Keys.Home, "zero"));
            add_bind(new KeyBind(Keys.End, "one"));            

            add_bind(new KeyBind(Keys.Left, "left"));
            add_bind(new KeyBind(Keys.Right, "right"));
            add_bind(new KeyBind(Keys.Up, "up"));
            add_bind(new KeyBind(Keys.Down, "down"));

            add_bind(new KeyBind(Keys.A, "wasd_left"));
            add_bind(new KeyBind(Keys.D, "wasd_right"));
            add_bind(new KeyBind(Keys.W, "wasd_forward"));
            add_bind(new KeyBind(Keys.S, "wasd_backward"));

            add_bind(new KeyBind(Keys.Q, "activate_left"));
            add_bind(new KeyBind(Keys.E, "activate_right"));

            add_bind(new MouseButtonBind(MouseButtons.Left, "ui_select"));
            add_bind(new MouseButtonBind(MouseButtons.Right, "click_right"));
            add_bind(new MouseButtonBind(MouseButtons.Middle, "click_middle"));
            add_bind(new MouseButtonBind(MouseButtons.ScrollUp, "scroll_up"));
            add_bind(new MouseButtonBind(MouseButtons.ScrollDown, "scroll_down"));
            
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            ContentHandler.LoadContent(Content, GraphicsDevice);
            ContentHandler.LoadAllResources();

            sdf_effect = ContentHandler.resources["sdf_pixel"].value_fx;

        }

        protected override void UnloadContent() {
            ContentHandler.UnloadAll();
        }

        protected override void Update(GameTime gameTime)
        {
            EngineState.Update(gameTime, this);
            
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            image_view.update();
            sdf_view.update();

            base.Update(gameTime);
            Clock.update_fps();
        }

        protected override void Draw(GameTime gameTime) {
            //draw SDF map preview on left preview pane if one is loaded

            //ContentHandler.resources["sdf_pixel"].value_fx.Parameters["tint"].SetValue(Color.Black.ToVector3());
            ContentHandler.resources["sdf_pixel"].value_fx.Parameters["alpha_scissor"].SetValue(0.5f);
            ContentHandler.resources["sdf_pixel"].value_fx.Parameters["SDFTEX"].SetValue(ContentHandler.resources["zerocool_sharper"].value_tx);

            image_view.draw();
            sdf_view.draw();

            //Draw both preview panes to the screen
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Transparent);
            

            EngineState.spritebatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap);

            image_view.draw_output_to_screen();
            sdf_view.draw_output_to_screen();

            //Draw2D.image(image_view.image_viewed, XYPair.Zero, new XYPair(EngineState.resolution.X, EngineState.resolution.Y), Color.White);

            Draw2D.image(sdf_view.image_viewed, sdf_view.viewport_position, sdf_view.viewport_size, Color.White);

            EngineState.spritebatch.End();


            base.Draw(gameTime);
        }
    }
}
