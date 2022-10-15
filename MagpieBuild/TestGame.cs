using Magpie;
using Magpie.Engine;
using Magpie.Engine.Brushes;
using Magpie.Engine.Stages;
using Magpie.Graphics;
using MagpieTestbed.TestActors;
using MagpieTestbed.TestObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

using static Magpie.Engine.Controls;
using static Magpie.Engine.StaticControlBinds;
using Magpie.Engine.Collision;
using Magpie.Engine.Collision.Support3D;
using Magpie.Graphics.Lights;
using Magpie.Engine.Physics;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using static Magpie.Engine.ControlBinds;
using Magpie.Graphics.UI;
using Magpie.Graphics.Particles;
using static Magpie.Graphics.Particles.PointCloud;

namespace MagpieBuild
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class BuildGame : Game
    {
        GraphicsDeviceManager graphics;
        World world;
        
        SDFSprite2D crosshair_sdf;

        SDFSprite2D test_sdf;
        SDFSprite2D test_sdf2;

        Color crosshair_color = Color.White;

        GJK.gjk_result[] results;
        PointCloud pctest;
        Particle2D parttest;
        //Sphere test_a = new Sphere();
        //sphere_data test_b = new sphere_data();

        //Point3D test_a = new Point3D();
        //Sphere test_a = new Sphere();
        //Tetrahedron test_a = new Tetrahedron();
        //Sphere test_b = new Sphere();
        //Tetrahedron test_b = new Tetrahedron();



        public BuildGame() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1600;
            graphics.PreferredBackBufferHeight = 900;
            
            this.IsMouseVisible = true;
            this.graphics.SynchronizeWithVerticalRetrace = false;
            this.Disposed += BuildGame_Disposed;
            this.graphics.PreferHalfPixelOffset = false;
            IsFixedTimeStep = true;
            
        }

        private void BuildGame_Disposed(object sender, EventArgs e) {
            World.running = false;
        }

        protected override void Initialize() {
            base.Initialize();

            EngineState.initialize(new XYPair(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Window, GraphicsDevice, graphics, this);


            world = new World();
            EngineState.world = world;

            world.current_map.player_actor = new FreeCamActor();
            EngineState.camera = ((FreeCamActor)world.current_map.player_actor).cam;


            add_bindings(            
                (bind_type.digital, controller_type.keyboard, Keys.Up, "t_forward"),
                (bind_type.digital, controller_type.keyboard, Keys.Left, "t_left"),
                (bind_type.digital, controller_type.keyboard, Keys.Right, "t_right"),
                (bind_type.digital, controller_type.keyboard, Keys.Down, "t_backward"),

                (bind_type.digital, controller_type.keyboard, Keys.PageUp, "t_up"),
                (bind_type.digital, controller_type.keyboard, Keys.PageDown, "t_down"),

                (bind_type.digital, controller_type.keyboard, Keys.Space, "up"),
                (bind_type.digital, controller_type.keyboard, Keys.C, "down"),

                (bind_type.digital, controller_type.keyboard, Keys.T, "test"),
                (bind_type.digital, controller_type.keyboard, Keys.Y, "test_sweep"),
                (bind_type.digital, controller_type.keyboard, Keys.F2, "switch_buffer"),
                (bind_type.digital, controller_type.keyboard, Keys.P, "poopy_butt"),

                (bind_type.digital, controller_type.keyboard, Keys.F5, "screenshot"),

                (bind_type.digital, controller_type.keyboard, Keys.LeftShift, "shift"),

                (bind_type.digital, controller_type.mouse, MouseButtons.Left, "ui_select"), 
                (bind_type.digital, controller_type.mouse, MouseButtons.Right, "click_right"),
                (bind_type.digital, controller_type.mouse, MouseButtons.Middle, "click_middle"),
                (bind_type.digital, controller_type.mouse, MouseButtons.ScrollUp, "scroll_up"),
                (bind_type.digital, controller_type.mouse, MouseButtons.ScrollDown, "scroll_down")
                );

            add_bindings((bind_type.digital, controller_type.keyboard, Keys.T, new string[] { "fart", "cum", "shit" } ));

            force_enable("screenshot");
            /*
            add_bind(new KeyBind(Keys.Up, "t_forward"));
            add_bind(new KeyBind(Keys.Left, "t_left"));
            add_bind(new KeyBind(Keys.Right, "t_right"));
            add_bind(new KeyBind(Keys.Down, "t_backward"));

            add_bind(new KeyBind(Keys.PageUp, "t_up"));
            add_bind(new KeyBind(Keys.PageDown, "t_down"));

            add_bind(new KeyBind(Keys.Space, "up"));
            add_bind(new KeyBind(Keys.C, "down"));

            add_bind(new KeyBind(Keys.T, "test"));
            add_bind(new KeyBind(Keys.Y, "test_sweep"));
            add_bind(new KeyBind(Keys.F2, "switch_buffer"));
            add_bind(new KeyBind(Keys.F3, "test", "switch_buffer"));
            add_bind(new KeyBind(Keys.P, "poopy_butt"));

            add_bind(new KeyBind(Keys.F5, "screenshot"));

            add_bind(new KeyBind(Keys.LeftShift, "shift"));

            //add_bind(new KeyBind(Keys.LeftAlt, "ui_alt"));

            add_bind(new MouseButtonBind(MouseButtons.Left, "ui_select"));
            add_bind(new MouseButtonBind(MouseButtons.Right, "click_right"));
            add_bind(new MouseButtonBind(MouseButtons.Middle, "click_middle"));
            add_bind(new MouseButtonBind(MouseButtons.ScrollUp, "scroll_up"));
            add_bind(new MouseButtonBind(MouseButtons.ScrollDown, "scroll_down"));
            */
            

            /*world.current_map.add_object("test_sphere2", new TestSphere());
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

            for (int i = 0; i < 50; i++) {
                world.current_map.add_object("test_sphere" + i, new TestSphere());
                world.current_map.objects["test_sphere" + i].position = (Vector3.Forward * (RNG.rng_float * 30)) + (Vector3.Right * (RNG.rng_float_neg_one_to_one* 10)) + (Vector3.Up * (RNG.rng_float * 20));
            }

            /*
            world.current_map.add_object("test_cube", new TestSphere());
            world.current_map.objects["test_cube"].model = "cube";
            world.current_map.objects["test_cube"].position = Vector3.Up * 60;
            world.current_map.objects["test_cube"].scale = Vector3.One + (Vector3.UnitX * 25f) + (Vector3.UnitY * 10f);

            world.current_map.add_object("test_cube1", new TestSphere());
            world.current_map.objects["test_cube1"].model = "cube";
            world.current_map.objects["test_cube1"].position = Vector3.Up * 60  + (Vector3.Backward * 50);
            //world.current_map.objects["test_cube1"].orientation = Matrix.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(180f));
            world.current_map.objects["test_cube1"].scale = Vector3.One + (Vector3.UnitX * 25f) + (Vector3.UnitY * 10f);
            
            world.current_map.add_object("test_cube2", new TestSphere());
            world.current_map.objects["test_cube2"].model = "cube";
            world.current_map.objects["test_cube2"].position = Vector3.Up * 60 + (Vector3.Backward * 25f) +  (Vector3.Right * 25f);
           // world.current_map.objects["test_cube2"].orientation = Matrix.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(90f));
            world.current_map.objects["test_cube2"].scale = Vector3.One + (Vector3.UnitZ * 25f) + (Vector3.UnitY * 10f);

            world.current_map.add_object("test_cube3", new TestSphere());
            world.current_map.objects["test_cube3"].model = "cube";
            world.current_map.objects["test_cube3"].position = Vector3.Up * 60 + (Vector3.Backward * 25f) + (Vector3.Left * 25f);
           // world.current_map.objects["test_cube3"].orientation = Matrix.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(90f));
            world.current_map.objects["test_cube3"].scale = Vector3.One + (Vector3.UnitZ * 25f) + (Vector3.UnitY * 10f);

            world.current_map.add_object("test_cube4", new TestSphere());
            world.current_map.objects["test_cube4"].model = "cube";
            world.current_map.objects["test_cube4"].position = Vector3.Up * 50 + (Vector3.Backward * 25f);
            world.current_map.objects["test_cube4"].scale = Vector3.One + (Vector3.UnitX * 25f) + (Vector3.UnitZ * 25f);

            
            world.current_map.add_object("butt_a", new TestSphere());
            world.current_map.objects["butt_a"].model = "smoothsphere";
            world.current_map.objects["butt_a"].position = Vector3.Up * 90 + (Vector3.Backward * 25f) + (Vector3.Left * 3f);
            world.current_map.objects["butt_a"].scale = Vector3.One * 8f - (Vector3.UnitX * 0.5f) - (Vector3.UnitZ * 1.5f);
            world.current_map.objects["butt_a"].orientation = Matrix.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(90f));

            world.current_map.add_object("butt_b", new TestSphere());
            world.current_map.objects["butt_b"].model = "smoothsphere";
            world.current_map.objects["butt_b"].position = Vector3.Up * 90 + (Vector3.Backward * 25f) + (Vector3.Right * 3f);
            world.current_map.objects["butt_b"].scale = Vector3.One * 8f - (Vector3.UnitX * 0.5f) - (Vector3.UnitZ * 1.5f);
            world.current_map.objects["butt_b"].orientation = Matrix.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(90f));
            */

            world.current_map.add_object("desk", new TestSphere());
            world.current_map.objects["desk"].model = "desk";
            world.current_map.objects["desk"].position = Vector3.Zero;
            world.current_map.objects["desk"].scale = Vector3.One * 10;
            world.current_map.objects["desk"].orientation = Matrix.Identity;


            //world.current_map.add_brush("test_floor", new FloorPlane());
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

            world.current_map.add_actor("test_actor", new MoveTestActor());


            /*
            test_sdf = new SDFSprite2D(Vector2.One * 330, Vector2.One * 350);
            test_sdf.inside_color = Color.HotPink;
            test_sdf.resource_name = "sdf_quiet";

            test_sdf2 = new SDFSprite2D((Vector2.One * 330f) + (Vector2.UnitX * 300f), Vector2.One * 450 * (Vector2.One - (Vector2.UnitX * 0.5f)));
            test_sdf2.inside_color = Color.Orange;
            //test_sdf2.alpha_scissor = 0.01f;
            test_sdf2.resource_name = "sdf_quiet_2";
            */

            crosshair_sdf = new SDFSprite2D(
                (EngineState.resolution.ToVector2() * 0.5f), 
                new Vector2(16, 16), 0.75f);

            parttest = new Particle2D("trump_tex");
            pctest = new PointCloud(test_trums, test_b);

            //test_window = new Magpie.Graphics.UI.UIWindow(new XYPair(100,100), new XYPair(500,250));

            //EngineState.window_manager.add_window(test_window);
            //test_window2 = new Magpie.Graphics.UI.UIWindow(new XYPair(150, 150), new XYPair(100, 100));

            //EngineState.window_manager.add_window(test_window2);

            Clock.frame_probe.set("overhead");
            Clock.frame_probe.false_set("overhead");

            results = new GJK.gjk_result[world.current_map.objects.Count];
        }

        void test_b(point_in_cloud pic, PointCloud p) {
            pic.point -= Vector3.UnitY * 4 * Clock.frame_time_delta;
            
            Raycasting.raycast_result res;

            pic.lerp = true;
            pic.lerp_speed = 4f * Clock.frame_time_delta;

            if (Clock.frame_count % 30 == 0) {
                pic.lerp_to = (RNG.rng_v3_near_v3(pic.point, 8f));
            }
            (int,int) qi;
            (int,int) si;
            if (world.test_hf.raycast(pic.point_previous, pic.point, out qi, out si, out res)) {

                //pic.point = pic.point_previous + (Vector3.Normalize(pic.point - pic.point_previous) * res.distance);
                pic.point = pic.point_previous;
                pic.alive = false;
            } else {

                pic.point_previous = pic.point;
            }

        }

        int test_trums = 20;

        protected override void LoadContent()
        {
            ContentHandler.LoadContent(Content, GraphicsDevice);
            ContentHandler.LoadAllResources();

        }

        protected override void UnloadContent() {
            ContentHandler.UnloadAll();
        }

        protected override void Update(GameTime gameTime) {
            Clock.frame_probe.false_set("overhead");
            Clock.frame_probe.start_of_frame();
            Clock.frame_probe.false_set("overhead");

            Clock.frame_probe.set("frame_start");

            base.Update(gameTime);

            EngineState.Update(gameTime, this);

            world.Update();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) {
                World.running = false;
                Exit();
            }

            /*
            if (world.current_map.objects.Count != results.Length) {
                results = new GJK.gjk_result[world.current_map.objects.Count];
            }


            //test collision detection between above test actor and all the objects in the scene

            Clock.frame_probe.set("300 GJK tests");
            int i = 0;
            foreach (GameObject go in world.current_map.objects.Values) {
                //if (go.name.StartsWith("test_sphere")) {
                    results[i] = GJK.gjk_intersects(world.current_map.actors["test_actor"].collision, go.collision,
                        world.current_map.actors["test_actor"].world, go.world);
                    i++;
                //}
            }

            //int i = 0;
            foreach (GameObject o in world.current_map.objects.Values) {
                //if (i % 3 == 0)
                    //world.current_map.objects["test_sphere" + i].orientation *= Matrix.CreateFromAxisAngle(Vector3.Up, ((float)i / 3f / 5f) * Clock.frame_time_delta);

                results[i] = GJK.gjk_intersects(world.current_map.actors["test_actor"].collision, o.collision,
                world.current_map.actors["test_actor"].world,
                o.world);
                i++;
            }

            if (bind_pressed("scroll_up") && bind_released("shift")) {
                if (test_sdf.alpha_scissor < 1f)
                    test_sdf.alpha_scissor += 0.02f * (wheel_delta / 120f);
                if (test_sdf.alpha_scissor > 1f)
                    test_sdf.alpha_scissor = 1f;
            }
            if (bind_pressed("scroll_down") && bind_released("shift")) {
                if (test_sdf.alpha_scissor > 0f)
                    test_sdf.alpha_scissor -= 0.02f * (wheel_delta / -120f);
                if (test_sdf.alpha_scissor <= 0f)
                    test_sdf.alpha_scissor = 0f;
            }

            if (bind_pressed("scroll_up") && bind_pressed("shift")) {
                test_sdf.enable_outline = true;

                if (test_sdf.outline_width < 0.5f)
                    test_sdf.outline_width += 0.01f;
                if (test_sdf.outline_width > 0.5f)
                    test_sdf.outline_width = 0.5f;
            }
            if (bind_pressed("scroll_down") && bind_pressed("shift")) {
                if (test_sdf.outline_width > -0.5f)
                    test_sdf.outline_width -= 0.01f;
                if (test_sdf.outline_width <= -0.5f) {
                    test_sdf.outline_width = -0.5f;
                    test_sdf.enable_outline = false;
                }
            }


            if (bind_just_pressed("click_middle")) {
                test_sdf.invert_map = !test_sdf.invert_map;
            }
            */



            if (just_pressed("screenshot")) Scene.screenshot();
            bool held_test = true;

            if (tapped("test") && !held("test")) {
                Scene.sun_moon.time_stopped = !Scene.sun_moon.time_stopped;
            }
            
            if (pressed("test")) {

                /*
                world.current_map.objects.Clear();

                for (int o = 0; o < 150; o++) {
                    world.current_map.add_object("test_sphere" + o, new TestSphere());
                    world.current_map.objects["test_sphere" + o].position = (Vector3.Forward * (RNG.rng_float * 30)) + (Vector3.Right * (RNG.rng_float_neg_one_to_one * 10)) + (Vector3.Up * (RNG.rng_float * 20));
                }*/

                if (pressed("scroll_up")) {
                    Scene.sun_moon.set_time_of_day(Scene.sun_moon.current_day_value + ((Controls.wheel_delta / 240.0) * 0.05));
                }
                if (pressed("scroll_down")) {
                    Scene.sun_moon.set_time_of_day(Scene.sun_moon.current_day_value + ((Controls.wheel_delta / -240.0) * -0.05));
                }
                
            }
            pctest.update();

        }

        private string print_ts(TimeSpan ts) {
            string s = string.Format("{0:F0}m{1:F0}s", ts.TotalMinutes, ts.Seconds);
            return s;
        }

        frame_snapshot snap = new frame_snapshot();



        public void renderextra() {
            ((SegmentedTerrain)world.test_hf).debug_draw();

            snap.snap("tump");
            parttest.instance_onto_point_cloud(pctest);
            snap.snap("another tum");

            pctest.draw_debug();
            snap.snap("another tump");
        }
        bool draw_debug_info = false;
        protected override void Draw(GameTime gameTime) {
            Scene.render_after_world = renderextra;
            Clock.frame_probe.set("draw_world");
            GraphicsDevice.SetRenderTargets(EngineState.buffer.buffer_targets);

            world.Draw(GraphicsDevice, EngineState.camera);

            // world.current_map.actors["test_actor"].debug_draw();

            foreach (GJK.gjk_result res in results) {
                Draw3D.line(res.closest_point_A, res.closest_point_B, Color.MonoGameOrange);
            }

            


            EngineState.window_manager.render_window_internals();
            Clock.frame_probe.set("draw_2D");
            GraphicsDevice.SetRenderTarget(EngineState.buffer.rt_2D);
            GraphicsDevice.Clear(Color.Transparent);

            if (draw_debug_info) {
                EngineState.spritebatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap);


                Draw2D.graph_line(50, EngineState.resolution.Y - 80, 200, 50,
                    "FPS", 60, true, true, true, Color.HotPink,
                    (Clock.FPS_immediate_buffer, string.Format("render fps [{0} ticks]", Clock.FPS_immediate_buffer.Length), Color.Red),
                    (World.last_fps, string.Format("world fps [{0} ticks]", World.last_fps.Length), Color.LimeGreen)
                    );

                double fps = 1000f * (1 / World.last_ticks[World.last_ticks.Length - 1]);

                Draw2D.graph_line(350, EngineState.resolution.Y - 80, 200, 50,
                    "deltas", 20, true, true, true, Color.HotPink,
                    (Clock.delta_buffer, "clock delta ms", Color.Red),
                    (World.last_ticks, "world update thread", Color.LimeGreen)
                    );


                Draw2D.text_shadow("pf",
                    $"ext {Clock.frame_rate.ToString()}/{Clock.frame_limit}FPS\n" +
                    $"int {Clock.internal_frame_rate_immediate.ToString()}/{Clock.internal_frame_limit}FPS\n" +
                    "\n" +
                    string.Format("[ delta s  [int {0:F3}] [ext {1:F3}] ]\n", Clock.internal_frame_time_delta, Clock.frame_time_delta) +
                    string.Format("[ delta ms [int {0:F3}] [ext {1:F3}] ]\n", Clock.internal_frame_time_delta_ms, Clock.frame_time_delta_ms) + "\n" +

                    "[ camera position " + EngineState.camera.position.simple_vector3_string_brackets() + " ]\n" +
                    "[ actor position " + world.player_actor.position.simple_vector3_string_brackets() + " ]\n\n" +
                    "[ buffer [" + (((int)Scene.buffer == -1) ? "combined" : ((Scene.buffers)Scene.buffer).ToString()) + "] ]\n\n" +
                    "mouse over UI: " + EngineState.window_manager.mouse_over_UI() + "\n\n" +
                    "## gvars ##\n" +
                    gvars.list_all() + "\n\n" +
                    EngineState.window_manager.list_windows()


                    , Vector2.One * 2 + (Vector2.UnitY * 20), Color.White);




                Clock.frame_probe.set("draw_bind_states");
                StaticControlBinds.draw_state(200, 650, 100, 15, 15);
                ((FreeCamActor)world.player_actor).binds.draw_state(6, 650, 100, 15, 15, "player");


                EngineState.spritebatch.End();
            }
            EngineState.draw2d();

            //test_sdf.draw();
            //test_sdf2.draw();

            GraphicsDevice.SetRenderTarget(EngineState.buffer.rt_2D);

            if (mouse_lock) {
                crosshair_sdf.position = EngineState.resolution * 0.5f;
            } else {
                crosshair_sdf.position = Controls.mouse_position.ToVector2();
            }
            crosshair_sdf.draw();

            Scene.draw_texture_to_screen(world.test_light.depth_map, Vector2.One * 20, Vector2.One * 300);
            //Scene.draw_texture_to_screen(ContentHandler.resources["radial_glow"].value_tx, Vector2.One * 200, Vector2.One * 200);
            //Draw2D.SDFCircle(Vector2.One * 300, 200f, Color.White);
            GraphicsDevice.SetRenderTarget(null);

            Scene.compose();
            //base.Draw(gameTime);
            Clock.update_fps();

            Clock.frame_probe.end_of_frame();
            Clock.frame_probe.set("overhead");

            Clock.frame_probe.draw(EngineState.resolution.X - 450, 60, 300, out _, out th);
            int t = th;

            lock (World.internal_frame_probe)
                World.internal_frame_probe.draw(EngineState.resolution.X - 402, th + 70, 300, out _, out t);

            lock (Controls.control_poll_probe)
                Controls.control_poll_probe.draw(EngineState.resolution.X - 360, th + t + 80, 300, out _, out _);

            snap.draw(test_trums.ToString(), EngineState.resolution.X - 360, 10, 300);


            //lock (trum_sphere_probe)
                //trum_sphere_probe.draw(EngineState.resolution.X / 2, EngineState.resolution.Y / 2, 200, out _, out _);
        }
        int th = 0;
    }
}
