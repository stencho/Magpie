using Magpie;
using Magpie.Engine;
using Magpie.Engine.Brushes;
using Magpie.Engine.Stages;
using Magpie.Graphics;
using MagpieTestbed.TestActors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

using static Magpie.Engine.Controls;
using static Magpie.Engine.StaticControlBinds;
using Magpie.Engine.Collision;
using Magpie.Engine.Collision.Support3D;
using Magpie.Graphics.Lights;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using static Magpie.Engine.ControlBinds;
using Magpie.Graphics.UI;
using Magpie.Graphics.Particles;
using static Magpie.Graphics.Particles.PointCloud;
using Microsoft.VisualBasic.ApplicationServices;
using Magpie.Engine.WorldElements;
using System.Transactions;
using static Magpie.GJK;
using System.Drawing.Drawing2D;
using Matrix = Microsoft.Xna.Framework.Matrix;
using static Magpie.Engine.Collision.MixedCollision;
using MagpieBuild.TestActors;

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

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            
            this.IsMouseVisible = true;
            this.graphics.SynchronizeWithVerticalRetrace = false;
            this.Disposed += BuildGame_Disposed;
            this.graphics.HardwareModeSwitch = false;
            this.graphics.IsFullScreen = false;
            this.graphics.PreferHalfPixelOffset = false;
            IsFixedTimeStep = true;
            
        }

        private void BuildGame_Disposed(object sender, EventArgs e) {
            EngineState.running = false;

            gvars.write_gvars_to_disk();
        }
        int mid;
        protected override void Initialize() {
            base.Initialize();

            EngineState.initialize(new XYPair(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Window, GraphicsDevice, graphics, this);

            while (!EngineState.started) {  }

            world = new World();
            EngineState.world = world;

            while (EngineState.world == null) { }

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
                (bind_type.digital, controller_type.keyboard, Keys.Q, "t_L"),
                (bind_type.digital, controller_type.keyboard, Keys.E, "t_R"),
                (bind_type.digital, controller_type.keyboard, Keys.R, "t_S"),


                (bind_type.digital, controller_type.keyboard, Keys.LeftShift, "shift"),

                (bind_type.digital, controller_type.mouse, MouseButtons.Left, "ui_select"), 
                (bind_type.digital, controller_type.mouse, MouseButtons.Right, "click_right"),
                (bind_type.digital, controller_type.mouse, MouseButtons.Middle, "click_middle"),
                (bind_type.digital, controller_type.mouse, MouseButtons.ScrollUp, "scroll_up"),
                (bind_type.digital, controller_type.mouse, MouseButtons.ScrollDown, "scroll_down")
                );

            add_bindings((bind_type.digital, controller_type.keyboard, Keys.T, new string[] { "fart", "cum", "shit" } ));

            force_enable("screenshot");

            for (int i = 0; i < 150; i++) {
                var id = EngineState.world.current_map.make_id();
                //world.current_map.game_objects.Add(id,
                  //  new object_info(
                    //    (Vector3.Forward * (RNG.rng_float * 30)) + (Vector3.Right * (RNG.rng_float_neg_one_to_one * 10)) + (Vector3.Up * (RNG.rng_float * 20)), 
                      //  new render_info_model("sphere", "trumpmap")));
                

                //var ind = world.current_map.add_object("test_sphere" + i, new TestSphere());
                //world.current_map.objects[ind].position = (Vector3.Forward * (RNG.rng_float * 30)) + (Vector3.Right * (RNG.rng_float_neg_one_to_one* 10)) + (Vector3.Up * (RNG.rng_float * 20));
                //world.current_map.objects[ind].textures = new string[1] { "trumpmap" };




            }

            var cubeid = world.current_map.add_object(new object_info(Vector3.Left * 6, new collision_info(new Cube(0.5f))));
            var sphereid = world.current_map.add_object(new object_info(Vector3.Right * 3, new collision_info(new Sphere(1f))));
            var triid = world.current_map.add_object(new object_info(Vector3.Right * 6, new collision_info(new Triangle(Vector3.Right + Vector3.Forward, Vector3.Up, Vector3.Right + Vector3.Down))));

            var capsuleid = world.current_map.add_object(new object_info(Vector3.Left * 3, new collision_info(new Capsule(1.85f, 0.8f))));

            var rcubeid = world.current_map.add_object(new object_info(Vector3.Right * 8, new collision_info(new Cube(0.5f))));
            world.current_map.game_objects[rcubeid].orientation = Matrix.CreateFromAxisAngle(Vector3.Up, 16f);

            var moveid = world.current_map.add_object(new gjkTestActor(Vector3.Left * 5f, new collision_info(new Sphere(1f))));

            world.current_map.game_objects[rcubeid].orientation = Matrix.CreateFromAxisAngle(Vector3.Up, 16f);
            //new collision_info(new Triangle(Vector3.Right + Vector3.Forward, Vector3.Up, Vector3.Left + Vector3.Down))));

            ((gjkTestActor)world.current_map.game_objects[moveid]).gjk_targets.Add(cubeid, new collision_result());
            ((gjkTestActor)world.current_map.game_objects[moveid]).gjk_targets.Add(sphereid, new collision_result());
            ((gjkTestActor)world.current_map.game_objects[moveid]).gjk_targets.Add(triid, new collision_result());
            ((gjkTestActor)world.current_map.game_objects[moveid]).gjk_targets.Add(rcubeid, new collision_result());

            mid = world.current_map.add_object(new object_info(
                Vector3.Up * 5f,
                new render_info_model("desk")));

           // world.current_map.game_objects[mid].orientation *= Matrix.CreateFromAxisAngle(Vector3.Up, 12f);
            world.current_map.game_objects[mid].scale *= 16f;
            world.current_map.game_objects[mid].lights =
                new light[1] {                    
                    new light {
                        type = LightType.SPOT,
                        color = RNG.random_opaque_color(),
                        spot_info = new spot_info() {
                            position = world.current_map.game_objects[mid].position,
                            orientation = Matrix.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(RNG.rng_float_neg_one_to_one * 180f))
                        }
                    }
                };



            //world.current_map.add_actor(new MoveTestActor());


            crosshair_sdf = new SDFSprite2D(
                (EngineState.resolution.ToVector2() * 0.5f), 
                new Vector2(16, 16), 0.75f);


            Clock.frame_probe.set("overhead");
            Clock.frame_probe.false_set("overhead");

        }


        protected override void LoadContent()
        {
            ContentHandler.LoadContent(Content, GraphicsDevice);
            ContentHandler.LoadAllResources();

        }

        protected override void UnloadContent() {
            ContentHandler.UnloadAll();
        }
        GJK3DParallel gjkp = new GJK3DParallel();
        protected override void Update(GameTime gameTime) {
            while (!EngineState.started) { }

            Clock.frame_probe.false_set("overhead");
            Clock.frame_probe.start_of_frame();
            Clock.frame_probe.false_set("overhead");

            Clock.frame_probe.set("frame_start");

            base.Update(gameTime);

            EngineState.Update(gameTime, this);

            world.Update();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) {
                EngineState.running = false;
                Exit();
            }
            
           // results = new GJK.gjk_result[world.current_map.game_objects.Count];

            //test collision detection between above test actor and all the objects in the scene
            int i=0;
            foreach (object_info go in world.current_map.game_objects.Values) {
                if (go == null) continue;
                //if (go.name.StartsWith("test_sphere")) {
                    //results[i] = gjkp.gjk_intersects(world.current_map.actors[0].collision, go.collision.move_shape, world.current_map.actors[0].world, go.collision.world);
                i++;
                //}
            }

            world.current_map.game_objects[mid].lights[0].position = 
                world.player_actor.position 
                + (EngineState.camera.orientation.Right * 0.5f) 
                + (EngineState.camera.orientation.Down * 0.4f) 
                + (EngineState.camera.orientation.Forward * 0.6f);
            world.current_map.game_objects[mid].lights[0].spot_info.orientation =
                EngineState.camera.orientation * Matrix.CreateFromAxisAngle(EngineState.camera.orientation.Up, MathHelper.ToRadians(5f));
            /*

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


            //pctest.update();

        }

        private string print_ts(TimeSpan ts) {
            string s = string.Format("{0:F0}m{1:F0}s", ts.TotalMinutes, ts.Seconds);
            return s;
        }

        frame_snapshot snap = new frame_snapshot();



        public void renderextra() {
            //((SegmentedTerrain)world.test_hf).debug_draw();

            snap.snap("tump");
            //parttest.instance_onto_point_cloud(pctest);
            snap.snap("another tum");

            //pctest.draw_debug();
            snap.snap("another tump");

            //Draw3D.xyz_cross(world.current_map.lights[world.current_map.lights.Count - 1].position, 0.3f, Color.HotPink);
        }

        bool draw_debug_info = true;



        protected override void Draw(GameTime gameTime) {






            //Scene.render_after_world = renderextra;
            
            
            
            
            
            
            
            Clock.frame_probe.set("draw_world");
            GraphicsDevice.SetRenderTargets(EngineState.buffer.buffer_targets);

            world.Draw(GraphicsDevice, EngineState.camera);

            //world.current_map.actors[0].debug_draw();
                        
            Clock.frame_probe.set("GJK tests/drawing");

            while (true) { 
                if (!world.current_map.game_objects[mid].collision.doing_collisions) {
                    lock (world.current_map.game_objects[mid].collision.gjk_results) {
                        foreach (gjk_result r in world.current_map.game_objects[mid].collision.gjk_results) {
                            //var r = mc.gjk(world.current_map.actors[0].collision, world.current_map.actors[0].world, world.current_map.game_objects[mid].render[0].world);
                            Draw3D.line(r.closest_point_A, r.closest_point_B, Color.MonoGameOrange);
                            Draw3D.text_3D(
                                EngineState.spritebatch,
                                (r.distance).ToString(),
                                "pf", r.closest_point_B,
                                Vector3.Normalize(EngineState.camera.position - r.closest_point_B), 1f,
                                Color.Black);

                            // Draw3D.xyz_cross(r.closest_point_A + (r.AB * (fr > r.distance ? r.distance : fr)), 0.1f, Color.MonoGameOrange);
                        }

                       // ((Capsule)world.current_map.actors[0].collision).draw(world.current_map.actors[0].world * Matrix.Invert(world.current_map.game_objects[mid].collision.world));

                       // Draw3D.xyz_cross((world.current_map.actors[0].world * Matrix.Invert(world.current_map.game_objects[mid].collision.world)).Translation, 1f / 16f, Color.Red);
                    }
                    break;
                }
            }
            
            
            /*
            foreach (GJK.gjk_result res in results) {
                Draw3D.line(res.closest_point_A, res.closest_point_B, Draw2D.ColorInterpolate(Color.Green, Color.MonoGameOrange, 
                    MathHelper.Clamp(Vector3.Distance(res.closest_point_A, res.closest_point_B) / 50f, 0f,1f)));

                var p =  res.closest_point_A + ((res.closest_point_B - res.closest_point_A) / 2f);
                Draw3D.text_3D(EngineState.spritebatch,
                    $"d:{Vector3.Distance(res.closest_point_A, res.closest_point_B)}\nh:{res.hit}", 
                    "pf", p, Vector3.Normalize(EngineState.camera.position - p), 1f, 
                    res.hit ? Color.Green : Color.MonoGameOrange);

                //res.shape_B.draw();
            }*/
            


            EngineState.window_manager.render_window_internals();
            Clock.frame_probe.set("draw_2D");
            GraphicsDevice.SetRenderTarget(EngineState.buffer.rt_2D);
            GraphicsDevice.Clear(Color.Transparent);

            if (draw_debug_info) {
                EngineState.spritebatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap);
                double fps = 1000f * (1 / World.last_ticks[World.last_ticks.Length - 1]);
                /*

                Draw2D.graph_line(50, EngineState.resolution.Y - 80, 200, 50,
                    "FPS", 60, true, true, true, Color.HotPink,
                    (Clock.FPS_immediate_buffer, string.Format("render fps [{0} ticks]", Clock.FPS_immediate_buffer.Length), Color.Red),
                    (World.last_fps, string.Format("world fps [{0} ticks]", World.last_fps.Length), Color.LimeGreen)
                    );


                Draw2D.graph_line(350, EngineState.resolution.Y - 80, 200, 50,
                    "deltas", 20, true, true, true, Color.HotPink,
                    (Clock.delta_buffer, "clock delta ms", Color.Red),
                    (World.last_ticks, "world update thread", Color.LimeGreen)
                    );
                */

                Draw2D.text_shadow("pf",
                    $"render {Clock.frame_rate.ToString()}/{Clock.frame_limit} FPS {(gvars.get_bool("vsync") ? "[vsync]" : "")}\n" +
                    $"update {Clock.internal_frame_rate_immediate.ToString()}/{Clock.internal_frame_limit} FPS\n" +
                    "\n" +
                    string.Format("[ delta s  [int {0:F3}] [ext {1:F3}] ]\n", Clock.internal_frame_time_delta, Clock.frame_time_delta) +
                    string.Format("[ delta ms [int {0:F3}] [ext {1:F3}] ]\n", Clock.internal_frame_time_delta_ms, Clock.frame_time_delta_ms) + "\n" +

                    "[ camera position " + EngineState.camera.position.simple_vector3_string_brackets() + " ]\n" +
                    "[ actor position " + world.player_actor.position.simple_vector3_string_brackets() + " ]\n\n" +
                    "[ buffer [" + (((int)Scene.buffer == -1) ? "combined" : ((Scene.buffers)Scene.buffer).ToString()) + "] ]\n\n" +
                    "mouse over UI: " + EngineState.window_manager.mouse_over_UI() + "\n\n" +
                    "## gvars ##\n" +
                    gvars.list_all() + "\n\n" +
                    EngineState.window_manager.list_windows() + "\n\n"
                    



                    , Vector2.One * 2 + (Vector2.UnitY * 20), Color.White);



                /*
                Clock.frame_probe.set("draw_bind_states");
                StaticControlBinds.draw_state(200, EngineState.resolution.Y - 250, 100, 15, 15);
                ((FreeCamActor)world.player_actor).binds.draw_state(6, EngineState.resolution.Y - 250, 100, 15, 15, "player");
                */

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

            //Scene.draw_texture_to_screen(world.test_light.depth_map, Vector2.One * 20, Vector2.One * 300);
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
            
            //snap.draw(test_trums.ToString(), EngineState.resolution.X - 360, 10, 300);


            //lock (trum_sphere_probe)
                //trum_sphere_probe.draw(EngineState.resolution.X / 2, EngineState.resolution.Y / 2, 200, out _, out _);
        }
        int th = 0;
    }
}
