using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MG2DSDFTool
{

    public class SDFTool_game : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Viewport viewport;

        sdf_lib.sdf_circle circ;
        sdf_lib.sdf_rect rect;

        static Engine engine;

        public SDFTool_game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        public void change_resolution(int x, int y) {
            graphics.PreferredBackBufferWidth = 1600;
            graphics.PreferredBackBufferHeight = 900;
            graphics.ApplyChanges();

            engine.change_resolution(x, y);
        }

        protected override void Initialize() {
            base.Initialize();

            graphics.PreferredBackBufferWidth = 1600;
            graphics.PreferredBackBufferHeight = 900;
            graphics.ApplyChanges();
            
            engine = new Engine(
                new XYPair(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), 
                Content, GraphicsDevice, graphics, spriteBatch, Window);

            IsMouseVisible = true;
        }

        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);


            sdf_lib.load_content(Content);
            /*
            circ = new sdf_lib.sdf_circle(Content, Vector2.Zero, Vector2.One * 300);
            circ.inner_texture = Texture2D.FromFile(GraphicsDevice, "J:\\nrol_39.png");
            circ.border_texture = Texture2D.FromFile(GraphicsDevice, "J:\\r0341187.jpg");
            circ.outer_texture = Texture2D.FromFile(GraphicsDevice, "J:\\download.jpg");

            rect = new sdf_lib.sdf_rect(Content, Vector2.UnitX * 190 + (Vector2.UnitY * 50), Vector2.One *200 + (Vector2.UnitY * 500));
            rect.inner_texture = Texture2D.FromFile(GraphicsDevice, "J:\\Deanshitting.png");
            rect.border_texture = Texture2D.FromFile(GraphicsDevice, "J:\\download.jpg");
            */
            
            //viewport = new Viewport()
        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            
            base.Update(gameTime);
            engine.update(gameTime, IsActive);
            //rect.update();
        }

        protected override void Draw(GameTime gameTime) {

            engine.draw();


            //don't need to test rectangle, literally just don't use an effect
            //rect.draw(spriteBatch);
            
            //circle test
            //circ.draw(spriteBatch);


            //base.Draw(gameTime);
        }
    }
}
