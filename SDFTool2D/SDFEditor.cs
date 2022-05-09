using Magpie;
using Magpie.Engine;
using Magpie.Graphics;
using Magpie.Graphics.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static Magpie.Engine.Controls;
using static Magpie.Engine.DigitalControlBindings;

namespace SDFTool2D {
    public class SDFEditor : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        RenderTarget2D buffer;
        //RenderTarget2D preview_left, preview_right;

        ImageViewer preview_left, preview_right;

        Effect sdf_effect;
        SDFSprite2D sdf_sprite;

        internal float _vsp = 0.2f;
        float vertical_split_position {
            get {
                return _vsp;
            }
            set {
                preview_left.change_size(graphics.PreferredBackBufferWidth * value, graphics.PreferredBackBufferHeight);
                preview_right.change_size(graphics.PreferredBackBufferWidth * (1-value), graphics.PreferredBackBufferHeight);
                _vsp = value;
            }
        }

        public SDFEditor()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1600;
            graphics.PreferredBackBufferHeight = 800;
            graphics.ApplyChanges();

            this.IsMouseVisible = true;
            this.graphics.SynchronizeWithVerticalRetrace = false;
            this.IsFixedTimeStep = false;
        }

        protected override void Initialize() { 
            base.Initialize();
            EngineState.initialize(new XYPair(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Window, GraphicsDevice, graphics, this);

            buffer = new RenderTarget2D(GraphicsDevice, EngineState.resolution.X, EngineState.resolution.Y);

            //preview_left = new RenderTarget2D(GraphicsDevice, EngineState.resolution.X / 2, EngineState.resolution.Y);
            //preview_right = new RenderTarget2D(GraphicsDevice, EngineState.resolution.X / 2, EngineState.resolution.Y);

            add_bind(new KeyBind(Keys.LeftShift, "shift"));
            add_bind(new KeyBind(Keys.LeftControl, "ctrl"));
            add_bind(new KeyBind(Keys.LeftAlt, "alt"));

            add_bind(new KeyBind(Keys.F5, "screenshot"));
            add_bind(new KeyBind(Keys.OemTilde, "tilde"));
            add_bind(new KeyBind(Keys.D1, "one"));

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

            if (preview_left == null || preview_right == null) {
                preview_left = new ImageViewer(0, 0, EngineState.resolution.X * vertical_split_position, EngineState.resolution.Y);
                preview_right = new ImageViewer(EngineState.resolution.X * vertical_split_position, 0, EngineState.resolution.X * (1-vertical_split_position), EngineState.resolution.Y);
                preview_right.active = false;
            } else {
                preview_left.update();
                preview_right.update();
            }

            if (bind_just_pressed("activate_left")) {
                preview_left.active = true;
                preview_right.active = false;
            }
            if (bind_just_pressed("activate_right")) {
                preview_left.active = false;
                preview_right.active = true;
            }

            base.Update(gameTime);
            Clock.update_fps();
        }

        protected override void Draw(GameTime gameTime) {
            //draw SDF map preview on left preview pane if one is loaded
            GraphicsDevice.SetRenderTarget(preview_left.image_viewed);
            EngineState.spritebatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null);

            EngineState.spritebatch.End();
            GraphicsDevice.Clear(Color.HotPink);


            //draw SDF using shader onto the right preview pane
            GraphicsDevice.SetRenderTarget(preview_right.image_viewed);
            EngineState.spritebatch.Begin(SpriteSortMode.Immediate, null, null, null, null, sdf_effect);

            EngineState.spritebatch.End();
            GraphicsDevice.Clear(Color.PowderBlue);

            //Draw both preview panes to the screen
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Transparent);

            preview_left.draw();
            preview_right.draw();
            

            EngineState.spritebatch.Begin(SpriteSortMode.Immediate);
            Draw2D.image(preview_left.image_viewed, XYPair.Zero, new XYPair(EngineState.resolution.X * vertical_split_position, EngineState.resolution.Y), Color.White);
            Draw2D.image(preview_right.image_viewed, new XYPair(EngineState.resolution.X * vertical_split_position, 0), new XYPair(EngineState.resolution.X * (1f-vertical_split_position), EngineState.resolution.Y), Color.White);


            EngineState.spritebatch.End();
            

            base.Draw(gameTime);
        }
    }
}
