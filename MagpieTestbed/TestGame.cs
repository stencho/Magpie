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

using Magpie.Engine.Collision.Support3D;

using static Magpie.Engine.Controls;
using static Magpie.Engine.DigitalControlBindings;
using static Magpie.Engine.Collision.GJK3D;
using Magpie.Engine.Collision;

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

            add_bind(new KeyBind(Keys.PageUp, "t_up"));
            add_bind(new KeyBind(Keys.PageDown, "t_down"));

            add_bind(new KeyBind(Keys.Space, "up"));
            add_bind(new KeyBind(Keys.C, "down"));

            add_bind(new KeyBind(Keys.T, "test"));

            add_bind(new MouseButtonBind(MouseButtons.Right, "click_right"));

            //world.current_map.add_object("test_sphere", new TestSphere());
            //world.current_map.add_floor("test_floor", new FloorPlane());
            //world.current_map.add_floor("test_floor2", new FloorPlane());
            //world.current_map.add_actor("test_actor", new MoveTestActor());

            //((FloorPlane)world.current_map.floors["test_floor2"]).size = new Vector2(50, 20);
            //world.current_map.floors["test_floor2"].position = new Vector3(0,4f,0);
            //world.current_map.floors["test_floor2"].orientation = 
               // Matrix.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(36f)) * Matrix.CreateFromAxisAngle(Vector3.Right, MathHelper.ToRadians(26f));
            

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

        gjk_result res;

        protected override void Update(GameTime gameTime)
        {
            EngineState.Update(gameTime, this);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            world.Update();

            res = GJK3D.intersects(test_a, test_b);

            Vector3 mv = Vector3.Zero;

            if (bind_pressed("t_forward")) {
                mv += Vector3.Forward;
            }
            if (bind_pressed("t_backward")) {
                mv += Vector3.Backward;
            }
            if (bind_pressed("t_left")) {
                mv += Vector3.Left;
            }
            if (bind_pressed("t_right")) {
                mv += Vector3.Right;
            }
            if (bind_pressed("t_up")) {
                mv += Vector3.Up;
            }
            if (bind_pressed("t_down")) {
                mv += Vector3.Down;
            }


            if (mv != Vector3.Zero)
                mv = (Vector3.Normalize(mv) * 4f * Clock.frame_time_delta);

            //test_b.position += mv;

            test_b.A += mv;
            test_b.B += mv;
            test_b.C += mv;
            test_b.D += mv;


            //if (bind_pressed("test"))
            //world.current_map.floors["test_floor2"].orientation *= Matrix.CreateFromAxisAngle(Vector3.Right, MathHelper.ToRadians(6F * Clock.frame_time_delta));

            base.Update(gameTime);
        }

        Vector2 fake_origin = new Vector2(EngineState.resolution.X - 200, 200);
        Vector3 fake_origin_3d = Vector3.Zero;

        //Point3D test_a = new Point3D();
        Sphere test_a = new Sphere();
        //Tetrahedron test_a = new Tetrahedron();
        //Sphere test_b = new Sphere();
        Tetrahedron test_b = new Tetrahedron();

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

            test_a.draw();
            test_b.draw();

            Draw3D.xyz_cross(GraphicsDevice, fake_origin_3d, 1f, Color.MonoGameOrange, EngineState.camera.view, EngineState.camera.projection);


            for (int i = 0; i < res.num_points; i++) {
                Draw3D.xyz_cross(GraphicsDevice, fake_origin_3d + res.simplex[i], 1f, Color.Purple, EngineState.camera.view, EngineState.camera.projection);
                for (int l = 0; l < res.num_points-1; l++) {
                    if (l != i)
                        Draw3D.line(GraphicsDevice, fake_origin_3d + res.simplex[i], fake_origin_3d + res.simplex[l], Color.Pink, EngineState.camera.view, EngineState.camera.projection);
                }
            }

            Vector3 abc = (res.A + res.B + res.C) / 3f;
            Vector3 bdc = (res.B + res.C + res.D) / 3f;
            Vector3 acd = (res.A + res.C + res.D) / 3f;
            Vector3 adb = (res.A + res.D + res.B) / 3f;

            Draw3D.xyz_cross(GraphicsDevice, res.closest_point_A, 1f, Color.Pink, EngineState.camera.view, EngineState.camera.projection);
            Draw3D.xyz_cross(GraphicsDevice, res.closest_point_B, 1f, Color.HotPink, EngineState.camera.view, EngineState.camera.projection);
            Draw3D.xyz_cross(GraphicsDevice, res.closest_simplex_point_to_origin, 1f, Color.LightBlue, EngineState.camera.view, EngineState.camera.projection);

            Draw3D.line(GraphicsDevice, fake_origin_3d + abc, fake_origin_3d + abc + GJK3D.ABC, Color.Red, EngineState.camera.view, EngineState.camera.projection);
            Draw3D.line(GraphicsDevice, fake_origin_3d + acd, fake_origin_3d + acd + GJK3D.ACD, Color.Green, EngineState.camera.view, EngineState.camera.projection);
            Draw3D.line(GraphicsDevice, fake_origin_3d + bdc, fake_origin_3d + bdc + GJK3D.BDC, Color.Blue, EngineState.camera.view, EngineState.camera.projection);
            Draw3D.line(GraphicsDevice, fake_origin_3d + adb, fake_origin_3d + adb + GJK3D.ADB, Color.Orange, EngineState.camera.view, EngineState.camera.projection);

            Draw3D.line(GraphicsDevice, fake_origin_3d, fake_origin_3d - res.A * 0.3f, Color.Red, EngineState.camera.view, EngineState.camera.projection);

            GraphicsDevice.SetRenderTarget(EngineState.buffer.rt_2D);
            GraphicsDevice.Clear(Color.Transparent);

            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            string s = "";
            for (int i = 0; i < res.num_points; i++) {
                
                switch (i) {
                    case 0: s = "A"; break;
                    case 1: s = "B"; break;
                    case 2: s = "C"; break;
                    case 3: s = "D"; break;
                }

                Draw3D.text_3D(GraphicsDevice, Draw2D.sb, 
                    s,
                    "pf", fake_origin_3d + res.simplex[i], null, 1f, EngineState.camera.view, EngineState.camera.projection, Color.Red);
            }

            Draw2D.sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            Draw2D.cross(fake_origin, 5, 5, Color.Purple);


            Draw2D.text_shadow("pf",
                Clock.frame_rate_immediate.ToString() + " FPS\n" +

                "Position " + world.player_actor.position.simple_vector3_string_brackets() + "\n" +
                highest.simple_vector3_string_brackets() +"\n"+
                res.distance + "\n" + res.penetration + "\n HIT: " + res.hit + "\n" + res.closest_simplex_point_to_origin.simple_vector3_string_brackets() + "\n" + 
                res.iterations + " iterations"
                
                , Vector2.One * 2, Color.White);
            
            Draw2D.sb.End();
            
            GraphicsDevice.SetRenderTarget(null);
            Renderer.compose(EngineState.buffer);
            //base.Draw(gameTime);
        }
    }
}
