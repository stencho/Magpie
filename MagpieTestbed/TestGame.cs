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
using Magpie.Engine.Collision;
using Magpie.Engine.Collision.Support3D;
using Magpie.Graphics.UI;
using Magpie.Graphics.Lights;

namespace MagpieTestbed
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class TestGame : Game
    {
        GraphicsDeviceManager graphics;
        World world;
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

            world = new World();

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
            add_bind(new KeyBind(Keys.F2, "switch_buffer"));
            add_bind(new KeyBind(Keys.F3, "bias_minus"));
            add_bind(new KeyBind(Keys.F4, "bias_plus"));
            add_bind(new KeyBind(Keys.F5, "screenshot"));
            //add_bind(new KeyBind(Keys.LeftAlt, "ui_alt"));

            add_bind(new MouseButtonBind(MouseButtons.Left, "ui_select"));
            add_bind(new MouseButtonBind(MouseButtons.Right, "click_right"));
            /*
            world.current_map.add_object("test_sphere", new TestSphere());
            world.current_map.add_object("test_sphere2", new TestSphere());
            world.current_map.add_object("test_sphere3", new TestSphere());
            world.current_map.add_object("test_sphere4", new TestSphere());
            world.current_map.add_object("test_sphere5", new TestSphere());
            world.current_map.add_object("test_sphere6", new TestSphere());

            


            world.current_map.objects["test_sphere2"].position += Vector3.Forward * 5f;
            world.current_map.objects["test_sphere3"].position += Vector3.Forward * 5f + Vector3.Right * 2f;
            world.current_map.objects["test_sphere4"].position += Vector3.Forward * 5f + Vector3.Left * 2f;
            world.current_map.objects["test_sphere5"].position += Vector3.Forward * 8f + Vector3.Up * 2f;
            world.current_map.objects["test_sphere6"].position += Vector3.Forward * 12f + Vector3.Up * 2f;


            world.current_map.objects["test_sphere6"].model = "bigcube";
            */

            for (int i = 0; i < 100; i++) {
                world.current_map.add_object("test_sphere" + i, new TestSphere());
                world.current_map.objects["test_sphere" + i].position = (Vector3.Forward * (RNG.rng_float * 30)) + (Vector3.Right * (RNG.rng_float_neg_one_to_one* 10)) + (Vector3.Up * (RNG.rng_float * 20));
            }

            world.current_map.add_floor("test_floor", new FloorPlane());
            //world.current_map.floors["test_floor"].position = Vector3.Forward * 10f + Vector3.Up * 5f;
            //world.current_map.add_floor("test_floor2", new FloorPlane());
            //world.current_map.add_actor("test_actor", new MoveTestActor());

            //((FloorPlane)world.current_map.floors["test_floor2"]).size = new Vector2(50, 20);
            //world.current_map.floors["test_floor2"].position = new Vector3(0,4f,0);
            //world.current_map.floors["test_floor2"].orientation = 
            // Matrix.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(36f)) * Matrix.CreateFromAxisAngle(Vector3.Right, MathHelper.ToRadians(26f));

            //((Quad)test_b).B += Vector3.Forward * 40f;
            //((Quad)test_b).C += Vector3.Forward * 40f;

            //((Quad)test_b).position += Vector3.Forward * 40f;

            world.current_map.player_actor = new FreeCamActor();
            
            EngineState.camera = ((FreeCamActor)world.current_map.player_actor).cam;

            EngineState.ui.add_form("top_panel", new UIPanel(XYPair.One * -3, new XYPair(EngineState.resolution.X + 5, 16)));

            EngineState.ui.add_form("test_form", new UIButton(new XYPair(EngineState.resolution.X - 17, 0), new XYPair(17, 18), "close_button", "X", false));

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

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape) || ((UIButton)EngineState.ui.forms["test_form"]).clicking)
                Exit();


            world.Update();

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

            test_a.position += mv;

            //if (bind_pressed("test"))
            //world.current_map.floors["test_floor2"].orientation *= Matrix.CreateFromAxisAngle(Vector3.Right, MathHelper.ToRadians(6F * Clock.frame_time_delta));


            results = new GJK.gjk_result[world.current_map.objects.Count];
            int i = 0;
            foreach (GameObject o in world.current_map.objects.Values) {
                if (i % 3 == 0)
                    world.current_map.objects["test_sphere" + i].orientation *= Matrix.CreateFromAxisAngle(Vector3.Up, ((float)i / 3f / 5f) * Clock.frame_time_delta);

                results[i] = GJK.gjk_intersects(test_a, o.collision,
                test_a.orientation * Matrix.CreateTranslation(test_a.position),
                o.world);
                i++;
            }

            if (bind_just_pressed("bias_minus")) Scene.LIGHT_BIAS -= 0.00001f;
            if (bind_just_pressed("bias_plus")) Scene.LIGHT_BIAS += 0.00001f;

            if (bind_just_pressed("screenshot")) Renderer.screenshot_at_end_of_frame();



            base.Update(gameTime);
        }
        GJK.gjk_result[] results;
        Vector2 fake_origin = new Vector2(EngineState.resolution.X - 200, 200);
        Vector3 fake_origin_3d = Vector3.Zero;

        //Sphere test_a = new Sphere();
        Capsule test_a = new Capsule(1.85f, 1f);
        //sphere_data test_b = new sphere_data();
        
        //Point3D test_a = new Point3D();
        //Sphere test_a = new Sphere();
        //Tetrahedron test_a = new Tetrahedron();
        //Sphere test_b = new Sphere();
        //Tetrahedron test_b = new Tetrahedron();

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.SetRenderTarget(null);

            GraphicsDevice.SetRenderTargets(EngineState.buffer.buffer_targets);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            world.Draw(GraphicsDevice, EngineState.camera);

            test_a.draw();


            //Draw3D.xyz_cross(GraphicsDevice, world.test_light.position, .1f, Color.Pink, EngineState.camera.view, EngineState.camera.projection);
            Draw3D.line(GraphicsDevice, world.test_light.position, (Vector3.Up * 2f + Vector3.Forward * 8f) - world.test_light.position, Color.HotPink, EngineState.camera.view, EngineState.camera.projection);

            foreach (GJK.gjk_result result in results) {
                Draw3D.xyz_cross(GraphicsDevice, result.closest_point_A, 1f, result.hit ? Color.LightGreen : Color.Red, EngineState.camera.view, EngineState.camera.projection);
                Draw3D.xyz_cross(GraphicsDevice, result.closest_point_B, 1f, result.hit ? Color.LightGreen : Color.Red, EngineState.camera.view, EngineState.camera.projection);

                //Draw3D.line(GraphicsDevice, result.closest_point_A, result.closest_point_B, result.hit ? Color.LightGreen: Color.Red, EngineState.camera.view, EngineState.camera.projection);
            }           


            GraphicsDevice.SetRenderTarget(EngineState.buffer.rt_2D);
            GraphicsDevice.Clear(Color.Transparent);

            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            
            Draw2D.sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            Draw2D.cross(fake_origin, 5, 5, Color.Purple);


            Draw2D.text_shadow("pf",
                Clock.frame_rate_immediate.ToString() + " FPS\n" +

                "Position " + world.player_actor.position.simple_vector3_string_brackets() + "\n" + (((int)Renderer.buffer == -1) ? "combined" : ((Renderer.buffers)Renderer.buffer).ToString()) +"\n" +
                Scene.LIGHT_BIAS.ToString()
                
                , Vector2.One * 2 + (Vector2.UnitY * 20), Color.White);

            EngineState.ui.draw();
            
            Draw2D.sb.End();


            GraphicsDevice.SetRenderTarget(null);
            Renderer.compose(EngineState.buffer);
            //base.Draw(gameTime);
        }
    }
}
