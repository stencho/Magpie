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

            add_bind(new KeyBind(Keys.Up, "t_forward"));
            add_bind(new KeyBind(Keys.Left, "t_left"));
            add_bind(new KeyBind(Keys.Right, "t_right"));
            add_bind(new KeyBind(Keys.Down, "t_backward"));

            add_bind(new KeyBind(Keys.Space, "up"));
            add_bind(new KeyBind(Keys.C, "down"));

            add_bind(new KeyBind(Keys.T, "test"));

            add_bind(new MouseButtonBind(MouseButtons.Right, "click_right"));

            world.current_map.add_object("test_sphere", new TestSphere());
            world.current_map.add_floor("test_floor", new FloorPlane());
            world.current_map.add_floor("test_floor2", new FloorPlane());
            world.current_map.add_actor("test_actor", new MoveTestActor());

            ((FloorPlane)world.current_map.floors["test_floor2"]).size = new Vector2(50, 20);
            world.current_map.floors["test_floor2"].position = new Vector3(0,4f,0);
            world.current_map.floors["test_floor2"].orientation = 
                Matrix.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(36f)) * Matrix.CreateFromAxisAngle(Vector3.Right, MathHelper.ToRadians(26f));
            

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

            if (bind_pressed("test"))
                world.current_map.floors["test_floor2"].orientation *= Matrix.CreateFromAxisAngle(Vector3.Right, MathHelper.ToRadians(6F * Clock.frame_time_delta));

            base.Update(gameTime);
        }

        Vector2 fake_origin = new Vector2(EngineState.resolution.X - 200, 200);

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.SetRenderTarget(null);
            Renderer.clear_all_and_draw_skybox(EngineState.camera, EngineState.buffer);

            GraphicsDevice.SetRenderTargets(EngineState.buffer.buffer_targets);
            
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            world.Draw(GraphicsDevice, EngineState.camera);


            Vector3 highest = new Vector3(world.player_actor.position.X, world.highest_floor_below(world.player_actor.position).Item1, world.player_actor.position.Z);
            fake_origin = new Vector2(EngineState.resolution.X - 200, 200);


            Draw3D.xyz_cross(GraphicsDevice, highest, 1f, Color.Red, EngineState.camera.view, EngineState.camera.projection);
            Draw3D.xyz_cross(GraphicsDevice, ((FloorPlane)world.current_map.floors["test_floor"]).testpos, 1f, Color.ForestGreen, EngineState.camera.view, EngineState.camera.projection);

            GraphicsDevice.SetRenderTarget(EngineState.buffer.rt_2D);
            GraphicsDevice.Clear(Color.Transparent);

            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;


            Draw2D.sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            Draw2D.cross(fake_origin, 5, 5, Color.Purple);


            Draw2D.text_shadow("pf",
                Clock.frame_rate_immediate.ToString() + " FPS\n" +

                "Position " + world.player_actor.position.simple_vector3_string_brackets() + "\n" +
                highest.simple_vector3_string_brackets()
                
                , Vector2.One * 2, Color.White);
            
            Draw2D.sb.End();
            
            GraphicsDevice.SetRenderTarget(null);
            Renderer.compose(EngineState.buffer);
            //base.Draw(gameTime);
        }
    }
}
