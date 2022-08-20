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
using static Magpie.Engine.DigitalControlBindings;
using Magpie.Engine.Collision;
using Magpie.Engine.Collision.Support3D;
using Magpie.Graphics.UI;
using Magpie.Graphics.Lights;
using Magpie.Engine.Physics;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace MagpieTestbed
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class TestGame : Game
    {
        GraphicsDeviceManager graphics;
        World world;
        
        SDFSprite2D crosshair_sdf;

        SDFSprite2D test_sdf;
        SDFSprite2D test_sdf2;

        Color crosshair_color = Color.White;

        GJK.gjk_result[] results;

        List<string> poos = new List<string>();
        
        private void add_poo() {
            int r = RNG.rng_int();

            while (poos.Contains(r.ToString())) {
                r = RNG.rng_int();
            }

            world.current_map.add_object(r.ToString(), new TestPoo());

            world.current_map.objects[r.ToString()].position = Vector3.Up * 85 + (Vector3.Backward * 25f);

            world.current_map.objects[r.ToString()].inertia_dir = Vector3.Down * 10 + (RNG.rng_v3_neg_one_to_one * 5f);
            world.current_map.objects[r.ToString()].inertia_dir = Vector3.Normalize(world.current_map.objects[r.ToString()].inertia_dir);
            world.current_map.objects[r.ToString()].velocity = 60f + (RNG.rng_float * 50f);           

        }

        //Sphere test_a = new Sphere();
        //sphere_data test_b = new sphere_data();

        //Point3D test_a = new Point3D();
        //Sphere test_a = new Sphere();
        //Tetrahedron test_a = new Tetrahedron();
        //Sphere test_b = new Sphere();
        //Tetrahedron test_b = new Tetrahedron();



        public TestGame() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1600;
            graphics.PreferredBackBufferHeight = 900;

            this.IsMouseVisible = true;
            this.graphics.SynchronizeWithVerticalRetrace = false;
            this.IsFixedTimeStep = false;
        }


        protected override void Initialize() {
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

            for (int i = 0; i < 150; i++) {
                //world.current_map.add_object("test_sphere" + i, new TestSphere());
                //world.current_map.objects["test_sphere" + i].position = (Vector3.Forward * (RNG.rng_float * 30)) + (Vector3.Right * (RNG.rng_float_neg_one_to_one* 10)) + (Vector3.Up * (RNG.rng_float * 20));
            }


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

            world.current_map.player_actor = new FreeCamActor();
            
            EngineState.camera = ((FreeCamActor)world.current_map.player_actor).cam;

            EngineState.ui.add_form("top_panel", new UIPanel(XYPair.One * -3, new XYPair(EngineState.resolution.X + 5, 16)));

            EngineState.ui.add_form("test_form", new UIButton(new XYPair(EngineState.resolution.X - 17, 0), new XYPair(17, 18), "close_button", "X", false));

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

        }

        SoundEffect fart;

        protected override void LoadContent()
        {
            ContentHandler.LoadContent(Content, GraphicsDevice);
            ContentHandler.LoadAllResources();

            fart = Content.Load<SoundEffect>("snd/hchv");

        }

        protected override void UnloadContent() {
            ContentHandler.UnloadAll();
        }

        bool do_poo = false;
        float poo_timer = 0;
        double poo_timer_number_2 = 0;
        

        protected override void Update(GameTime gameTime)
        {
            EngineState.Update(gameTime, this);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape) || ((UIButton)EngineState.ui.forms["test_form"]).clicking)
                Exit();


            world.Update();

            //test collision detection between above test actor and all the objects in the scene
            results = new GJK.gjk_result[5];

            int i = 0;
            foreach (GameObject go in world.current_map.objects.Values) {
                if (go.name.StartsWith("test_cube")) {
                    results[i] = GJK.gjk_intersects(world.current_map.actors["test_actor"].collision, go.collision,
                        world.current_map.actors["test_actor"].world, go.world);
                    i++;
                }
            }
            /*
            int i = 0;
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



            //start poo
            if (bind_just_pressed("poopy_butt") && !do_poo) {
                fart.Play();
                do_poo = true;
            }

            //pooing   ||  vv pause near the start for silent part of the sound file || vv stop caring about this at end 
            if (do_poo && (poo_timer_number_2 < 3200 || poo_timer_number_2 > 4800) && (poo_timer_number_2 < 16000)) {
                while ((poo_timer > 50 && poo_timer_number_2 < 11000) || (poo_timer > 250)) {
                    poo_timer -= poo_timer_number_2 < 11000 ? 50 : 125;

                    int poo_count = RNG.rng_int(1, 4);

                    while (poo_count > 0) {
                        add_poo();
                        poo_count--;
                    }
                }
            }

            //do final spurt and then reset
            if (do_poo && poo_timer_number_2 > 16350) {
                poo_timer = 375;
                do_poo = false;

                while (poo_timer > 250) {
                    poo_timer -= 125;

                    int poo_count = RNG.rng_int(1, 4);

                    while (poo_count > 0) {
                        add_poo();
                        poo_count--;
                    }
                }
            }

            //advance poo timers
            if (do_poo) {
                poo_timer += Clock.frame_time_delta_ms;
                poo_timer_number_2 += Clock.frame_time_delta_ms;
            }



            if (bind_just_pressed("screenshot")) Scene.screenshot_at_end_of_frame();
            bool held_test = true;

            if (bind_pressed("test")) {
                if (bind_pressed("scroll_up") || bind_pressed("scroll_down")) {
                    DigitalControlBindings.set_bind_special("test", special_action_status.held);
                    held_test = false;
                }
            }

            if (bind_just_released("test")) {
                if (!bind_held("test")) {

                    Scene.sun_moon.time_stopped = !Scene.sun_moon.time_stopped;
                }
            }
            if (bind_held("test")) {

                /*
                world.current_map.objects.Clear();

                for (int o = 0; o < 150; o++) {
                    world.current_map.add_object("test_sphere" + o, new TestSphere());
                    world.current_map.objects["test_sphere" + o].position = (Vector3.Forward * (RNG.rng_float * 30)) + (Vector3.Right * (RNG.rng_float_neg_one_to_one * 10)) + (Vector3.Up * (RNG.rng_float * 20));
                }*/

                if (!held_test) {
                    if (bind_pressed("scroll_up")) {
                        Scene.sun_moon.set_time_of_day(Scene.sun_moon.current_day_value + ((Controls.wheel_delta / 240.0) * 0.05));
                    }
                    if (bind_pressed("scroll_down")) {
                        Scene.sun_moon.set_time_of_day(Scene.sun_moon.current_day_value + ((Controls.wheel_delta / -240.0) * -0.05));
                    }
                } 
            }


            base.Update(gameTime);
        }

        private string print_ts(TimeSpan ts) {
            string s = string.Format("{0:F0}m{1:F0}s", ts.TotalMinutes, ts.Seconds);
            return s;
        }

        protected override void Draw(GameTime gameTime)
        {            
            GraphicsDevice.SetRenderTargets(EngineState.buffer.buffer_targets);



            world.Draw(GraphicsDevice, EngineState.camera);


            //GraphicsDevice.SetRenderTargets(EngineState.buffer.buffer_targets);
            GraphicsDevice.SetRenderTarget(EngineState.buffer.rt_2D);
            GraphicsDevice.Clear(Color.Transparent);

            
            foreach (Brush brush in world.current_map.brushes.Values) {
                //brush.debug_draw();
            }
            foreach (GameObject go in world.current_map.objects.Values) {
                //go.debug_draw();
                //go.collision.draw();
            }
            foreach (Actor actor in world.current_map.actors.Values) {
                //actor.debug_draw();

            }
            
            foreach (GJK.gjk_result result in results) {
                //Draw3D.xyz_cross(result.closest_point_A, 1f, result.hit ? Color.LightGreen : Color.Red);
                //Draw3D.xyz_cross(result.closest_point_B, 1f, result.hit ? Color.LightGreen : Color.Red);

                //Draw3D.line(result.closest_point_A, result.closest_point_B, result.hit ? Color.LightGreen: Color.Red);
            }

            //foreach (DynamicLight l in world.lights) {
            //if (l.type == LightType.POINT)
            //Draw3D.xyz_cross(GraphicsDevice, l.position, 0.1f, l.light_color, EngineState.camera.view, EngineState.camera.projection);
            //}

            //Draw3D.xyz_cross(GraphicsDevice, world.test_light.position, 1f, Color.Red, EngineState.camera.view, EngineState.camera.projection);
            ///Draw3D.line(GraphicsDevice, world.test_light.position, world.test_light.position + (world.test_light.orientation.Forward * world.test_light.far_clip), Color.HotPink, EngineState.camera.view, EngineState.camera.projection);
            //Draw3D.line(GraphicsDevice, world.test_light.position, world.test_light.position + (Vector3.Transform(Vector3.Normalize(world.test_light.orientation.Forward + world.test_light.orientation.Down), ((SpotLight)world.test_light).actual_scale)), Color.Orange, EngineState.camera.view, EngineState.camera.projection);

            
            foreach(Intersection i in PhysicsSolver.intersections) {
                //Draw3D.xyz_cross(i.gjkr.closest_point_A, 1f, Color.Blue);
                //Draw3D.xyz_cross(i.gjkr.closest_point_B, 1f, Color.ForestGreen);
            }
           

            EngineState.spritebatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap);

            Draw2D.text_shadow("pf",
                Clock.frame_rate.ToString() + " FPS [" + Clock.frame_rate_immediate + " average/" + Clock.FPS_buffer_length + " frames] " + Clock.frame_time_delta_ms + "ms\n" +

                "Position " + world.player_actor.position.simple_vector3_string_brackets() + "\n" + (((int)Scene.buffer == -1) ? "combined" : ((Scene.buffers)Scene.buffer).ToString()) + "\n" +
                PhysicsSolver.list_intersections() + Scene.terrain_segments_rendered + "\n" +
                "test_hf cursor/crosshair\n hit: " + world.test_hf.cursor_hit_result.hit + "\n seg: " +  world.test_hf.cursor_segment_index.Item1 + "," +  world.test_hf.cursor_segment_index.Item2 +
                "\n quad: " + world.test_hf.cursor_quad_index.Item1 + "," + world.test_hf.cursor_quad_index.Item2 + " (global: " + ((world.test_hf.cursor_segment_index.Item1 * world.test_hf.segment_size.X) + world.test_hf.cursor_quad_index.Item1) + "," + ((world.test_hf.cursor_segment_index.Item2 * world.test_hf.segment_size.Y) + world.test_hf.cursor_quad_index.Item2) + ")" +
                "\n point: " + world.test_hf.cursor_hit_result.point.simple_vector3_string()


                , Vector2.One * 2 + (Vector2.UnitY * 20), Color.White);


            //Draw2D.line(Vector2.One * 2 + (Vector2.UnitX * 330) + (Vector2.UnitY * 42f) + Vector2.One, Vector2.One * 2 + (Vector2.UnitX * 330) + (Vector2.UnitY * 42f) + (Vector2.UnitX * 200f) + Vector2.One, 1, Color.Black);
            //Draw2D.line(Vector2.One * 2 + (Vector2.UnitX * 330) + (Vector2.UnitY * 42f), Vector2.One * 2 + (Vector2.UnitX * 330) + (Vector2.UnitY * 42f) + (Vector2.UnitX * 200f), 1, Color.Red);

            //Draw2D.image(Scene.sun_moon.lerps.debug_band, XYPair.One * 2 + (XYPair.UnitX * 300) + (XYPair.UnitY * 69f), XYPair.One + (XYPair.UnitY * 30) + (XYPair.UnitX * 256), Color.White);

            //Draw2D.image(ContentHandler.resources["OnePXWhite"].value_tx, XYPair.One * 2 + (XYPair.UnitX * 300) + (XYPair.UnitY * 69f) - (XYPair.UnitX * 35), XYPair.One * 30, Scene.sun_moon.current_color);

           /* Draw2D.line(
                (XYPair.UnitX * ((float)Scene.sun_moon.current_day_value * 256)) + XYPair.One * 2 + (XYPair.UnitX * 300) + (XYPair.UnitY * 69f),
                (XYPair.UnitX * ((float)Scene.sun_moon.current_day_value * 256)) + XYPair.One * 2 + (XYPair.UnitX * 300) + (XYPair.UnitY * 70f) + (XYPair.UnitY * 30), 
                2f, Color.Red);*/

            /*
            Draw2D.text_shadow("pf",
string.Format(@"
{0:F0}ms/{1}ms ({2:F3}%)
{3:F0}/{4:F0} {5} 
1 day = {7}/{6:F2}x speed multiplier ({8})

",
Scene.sun_moon.current_time_ms, Scene.sun_moon.entire_day_cycle_length_ms, Scene.sun_moon.current_day_value * 100f,
Scene.sun_moon.current_time_ms / 1000f, Scene.sun_moon.entire_day_cycle_length_ms / 1000f, "Time is " + (Scene.sun_moon.time_stopped ? "stopped" : "ticking"),
Scene.sun_moon.time_multiplier, print_ts(Scene.sun_moon.cycle_ts), print_ts(Scene.sun_moon.cycle_ts_scaled)




)           , Vector2.One * 2 + (Vector2.UnitX * 300), Color.White);

    */
            Draw2D.text_shadow("pf", list_active_binds_w_status(), (Vector2.One * 2) + (Vector2.UnitY * 200));

            EngineState.ui.draw();

            //Draw2D.image(ContentHandler.resources["radial_glow"].value_tx, XYPair.One * 50, XYPair.One * 200, Color.White);
            //Draw2D.image(ContentHandler.resources["circle"].value_tx, XYPair.One * 150, XYPair.One * 200, Color.White);


            EngineState.spritebatch.End();
            
            crosshair_sdf.draw();
            //test_sdf.draw();
            //test_sdf2.draw();

            GraphicsDevice.SetRenderTarget(null);


            Scene.compose();
            //base.Draw(gameTime);
        }
    }
}
