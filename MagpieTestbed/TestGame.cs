using Magpie;
using Magpie.Engine;
using Magpie.Engine.Floors;
using Magpie.Engine.Stages;
using Magpie.Graphics;
using MagpieTestbed.TestActors;
using MagpieTestbed.TestObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

using static Magpie.Engine.Controls;
using static Magpie.Engine.DigitalControlBindings;

namespace MagpieTestbed
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class TestGame : Game
    {
        GraphicsDeviceManager graphics;
        World world = new World();
        
        public TestGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1600;
            graphics.PreferredBackBufferHeight = 900;

            this.IsMouseVisible = true;
            this.graphics.SynchronizeWithVerticalRetrace = false;
            this.IsFixedTimeStep = false;
        }

        protected override void Initialize()
        {
            base.Initialize();

            EngineState.initialize(new XYPair(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Window, GraphicsDevice, graphics, this);

            add_bind(new KeyBind(Keys.W, "forward"));
            add_bind(new KeyBind(Keys.A, "left"));
            add_bind(new KeyBind(Keys.D, "right"));
            add_bind(new KeyBind(Keys.S, "backward"));
            add_bind(new KeyBind(Keys.Space, "up"));
            add_bind(new KeyBind(Keys.C, "down"));
            add_bind(new MouseButtonBind(MouseButtons.Right, "click_right"));

            world.current_map.add_object("test_sphere", new TestSphere());
            world.current_map.add_floor("test_floor", new FloorPlane());

            world.current_map.player_actor = new FreeCamActor();
            
            EngineState.camera = ((FreeCamActor)world.current_map.player_actor).cam;
        }

        protected override void LoadContent()
        {
            ContentHandler.LoadContent(Content, GraphicsDevice);
            ContentHandler.LoadAll();
        }

        protected override void UnloadContent() {
            ContentHandler.UnloadAll();
        }

        protected override void Update(GameTime gameTime)
        {
            EngineState.Update(gameTime, this);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            world.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.SetRenderTarget(null);
            Renderer.clear_all_and_draw_skybox(EngineState.camera, EngineState.buffer);

            GraphicsDevice.SetRenderTargets(EngineState.buffer.buffer_targets);
            
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            Vector3 highest_point = new Vector3(world.player_actor.position.X, world.highest_floor(world.player_actor.position.XZ()).Item1, world.player_actor.position.Z);

            world.Draw(GraphicsDevice, EngineState.camera);

            Draw3D.xyz_cross(GraphicsDevice, highest_point, 1f, Color.Red, EngineState.camera.view, EngineState.camera.projection);

            GraphicsDevice.SetRenderTarget(EngineState.buffer.rt_2D);
            GraphicsDevice.Clear(Color.Transparent);

            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;


            Draw2D.sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            Draw2D.text_shadow("pf",
                Clock.frame_rate_immediate.ToString() + " FPS\n" +

                "Position " + world.player_actor.position.simple_vector3_string_brackets() + "\n" +

                "Height below [" + highest_point.Y.ToString() + "]\n"
                    
                , Vector2.One * 2, Color.White);

            Draw2D.sb.End();
            
            GraphicsDevice.SetRenderTarget(null);
            Renderer.compose(EngineState.buffer);
            //base.Draw(gameTime);
        }
    }
}
