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
using System.Drawing.Drawing2D;
using Matrix = Microsoft.Xna.Framework.Matrix;
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
            IsFixedTimeStep = false;
            
        }

        private void BuildGame_Disposed(object sender, EventArgs e) {
            EngineState.running = false;

            gvars.write_gvars_to_disk();
        }


        int move_id = 0;
        int cube_id = 0;


        protected override void Initialize() {
            base.Initialize();

            EngineState.initialize(new XYPair(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Window, GraphicsDevice, graphics, this);

            while (!EngineState.started) {  }

            world = new World();
            EngineState.world = world;

            while (EngineState.world == null) { }

            EngineState.player_id = world.current_map.add_object(new freecam_objinfo(Vector3.Backward * 15f + (Vector3.Up * 2f)));

            
            EngineState.camera = ((freecam_objinfo)world.current_map.game_objects[EngineState.player_id]).cam;


            add_bindings(
                (bind_type.digital, controller_type.keyboard, Keys.W, "forward"),
                (bind_type.digital, controller_type.keyboard, Keys.A, "left"),
                (bind_type.digital, controller_type.keyboard, Keys.D, "right"),
                (bind_type.digital, controller_type.keyboard, Keys.S, "backward"),

                (bind_type.digital, controller_type.keyboard, Keys.Up, "t_forward"),
                (bind_type.digital, controller_type.keyboard, Keys.Left, "t_left"),
                (bind_type.digital, controller_type.keyboard, Keys.Right, "t_right"),
                (bind_type.digital, controller_type.keyboard, Keys.Down, "t_backward"),

                (bind_type.digital, controller_type.keyboard, Keys.PageUp, "t_up"),
                (bind_type.digital, controller_type.keyboard, Keys.PageDown, "t_down"),

                (bind_type.digital, controller_type.keyboard, Keys.Home, "t_upper"),
                (bind_type.digital, controller_type.keyboard, Keys.End, "t_downer"),

                (bind_type.digital, controller_type.keyboard, Keys.Space, "up"),
                (bind_type.digital, controller_type.keyboard, Keys.C, "down"),

                (bind_type.digital, controller_type.keyboard, Keys.T, "test"),
                (bind_type.digital, controller_type.keyboard, Keys.Y, "test_sweep"),
                (bind_type.digital, controller_type.keyboard, Keys.Q, "t_L"),
                (bind_type.digital, controller_type.keyboard, Keys.E, "t_R"),
                (bind_type.digital, controller_type.keyboard, Keys.R, "t_S"),


                (bind_type.digital, controller_type.keyboard, Keys.LeftShift, "shift"),
                (bind_type.digital, controller_type.keyboard, Keys.LeftControl, "ctrl"),

                (bind_type.digital, controller_type.mouse, MouseButtons.Left, "click"), 
                (bind_type.digital, controller_type.mouse, MouseButtons.Right, "click_right"),
                (bind_type.digital, controller_type.mouse, MouseButtons.Middle, "click_middle"),
                (bind_type.digital, controller_type.mouse, MouseButtons.ScrollUp, "scroll_up"),
                (bind_type.digital, controller_type.mouse, MouseButtons.ScrollDown, "scroll_down"),

                (bind_type.digital, controller_type.keyboard, Keys.F, "t_supp" ),
                (bind_type.digital, controller_type.keyboard, Keys.D1, "speenL"),
                (bind_type.digital, controller_type.keyboard, Keys.D3, "speenR" ),
                (bind_type.digital, controller_type.keyboard, Keys.R,  "t_S"),
                (bind_type.digital, controller_type.keyboard, Keys.Q, "t_L" ),
                (bind_type.digital, controller_type.keyboard, Keys.E,"t_R" ),

                (bind_type.digital, controller_type.keyboard, Keys.OemTilde, "toggle_console" )
                );

            add_bindings((bind_type.digital, controller_type.keyboard, Keys.T, new string[] { "fart", "cum", "shit" } ));

            force_enable("toggle_console");
            force_enable("screenshot");

            
            cube_id = world.current_map.add_object(new object_info(Vector3.Left * 6 + (Vector3.Up * 3f), new render_info_model("cube", "zerocool_sharper"), new collision_info(new Cube(1f))));

            
            var weirdone  = world.current_map.add_object(new object_info(Vector3.Left * 3, new collision_info(new Polyhedron(
                RNG.rng_v3_neg_one_to_one,
                RNG.rng_v3_neg_one_to_one,
                RNG.rng_v3_neg_one_to_one,
                RNG.rng_v3_neg_one_to_one,
                RNG.rng_v3_neg_one_to_one,
                RNG.rng_v3_neg_one_to_one,
                RNG.rng_v3_neg_one_to_one,
                RNG.rng_v3_neg_one_to_one,
                RNG.rng_v3_neg_one_to_one,
                RNG.rng_v3_neg_one_to_one,
                RNG.rng_v3_neg_one_to_one,
                RNG.rng_v3_neg_one_to_one,
                RNG.rng_v3_neg_one_to_one,
                RNG.rng_v3_neg_one_to_one))));
            
            /*
            Vector3.Up,
            Vector3.Forward,
            Vector3.Right,
            Vector3.Backward,
            Vector3.Left,
            Vector3.Down))));
            */
            /*
            for (int i = 0; i < 250; i++) {
                world.current_map.add_object(
                    new object_info(RNG.rng_v3_neg_one_to_one * 500, new render_info_model("sphere", "trumpmap"),
                    new collision_info(new Sphere(1f)))
                    );
            }
            */
            
            var sphereid = world.current_map.add_object(new object_info(Vector3.Left * 1, new render_info_model("sphere", "trumpmap"), new collision_info(new Sphere(1f))));

            var triid = world.current_map.add_object(new object_info(Vector3.Right * 16, new collision_info(
                new ProjectedTriangle(Vector3.Right + Vector3.Forward, Vector3.Up, Vector3.Right + Vector3.Down))));
           
            //var quad = world.current_map.add_object(new object_info(Vector3.Down * 2, new collision_info(new Quad(250f))));

            var q1 = world.current_map.add_object(new object_info(Vector3.Up + (Vector3.Right * (60 - 12f)), new collision_info(new Quad(25f))));
            world.current_map.game_objects[q1].orientation = Matrix.CreateFromAxisAngle(Vector3.Forward, 16f);
            world.current_map.game_objects[q1].post_solve();

            var q2 = world.current_map.add_object(new object_info(Vector3.Up + (Vector3.Right * (60 + 12f)), new collision_info(new Quad(25f))));
            world.current_map.game_objects[q2].orientation = Matrix.CreateFromAxisAngle(Vector3.Backward, 16f);
            world.current_map.game_objects[q2].post_solve();
             
            var q3 = world.current_map.add_object(new object_info(Vector3.Down * 2 + (Vector3.Right * 0)+ (Vector3.Forward * 0), new collision_info(new Quad(25f))));
            world.current_map.game_objects[q3].orientation = Matrix.CreateFromAxisAngle(Vector3.Right, 16f);
            world.current_map.game_objects[q3].post_solve();

            
            var bigcube = world.current_map.add_object(new object_info(Vector3.Forward * 65, new render_info_model("cube", "zerocool_sharper"), new collision_info(new Cube(1f))));
            world.current_map.game_objects[bigcube].scale = Vector3.One * 5f;
            world.current_map.game_objects[bigcube].post_solve();

            var capsuleid = world.current_map.add_object(new object_info(Vector3.Right * 3, new collision_info(new Capsule(1.85f, 0.8f))));

            var rcubeid = world.current_map.add_object(new object_info(Vector3.Right * 8, new render_info_model("cube", "zerocool_sharper"), new collision_info(new Cube(1f))));
            world.current_map.game_objects[rcubeid].orientation = Matrix.CreateFromAxisAngle(Vector3.Up, 16f);
            world.current_map.game_objects[rcubeid].post_solve();
            
            /*
            move_id = world.current_map.add_object(new gjkTestActor((Vector3.UnitX + Vector3.UnitZ) * 500, 
                new render_info_model("skull", "OnePXWhite"), 
                new collision_info(
                new Cube(1f)
                //new Sphere(1f)
                //new Capsule(1.85f,1f)
                /*new Polyhedron(
                RNG.rng_v3_neg_one_to_one,
                RNG.rng_v3_neg_one_to_one,
                RNG.rng_v3_neg_one_to_one,
                RNG.rng_v3_neg_one_to_one,
                RNG.rng_v3_neg_one_to_one,
                RNG.rng_v3_neg_one_to_one,
                RNG.rng_v3_neg_one_to_one,
                RNG.rng_v3_neg_one_to_one,
                RNG.rng_v3_neg_one_to_one,
                RNG.rng_v3_neg_one_to_one,
                RNG.rng_v3_neg_one_to_one,
                RNG.rng_v3_neg_one_to_one,
                RNG.rng_v3_neg_one_to_one,
                RNG.rng_v3_neg_one_to_one)*/
                //)));

            //world.current_map.game_objects[move_id].scale = Vector3.One * 800;
            //world.current_map.game_objects[move_id].orientation = Matrix.CreateFromAxisAngle(Vector3.Up, -60);
            
            //new collision_info(new Triangle(Vector3.Right + Vector3.Forward, Vector3.Up, Vector3.Left + Vector3.Down))));

            /*((gjkTestActor)world.current_map.game_objects[move_id]).gjk_targets.Add(quad, new collision_result());
            ((gjkTestActor)world.current_map.game_objects[move_id]).gjk_targets.Add(cube_id, new collision_result());
            ((gjkTestActor)world.current_map.game_objects[move_id]).gjk_targets.Add(sphereid, new collision_result());
            ((gjkTestActor)world.current_map.game_objects[move_id]).gjk_targets.Add(triid, new collision_result());
            ((gjkTestActor)world.current_map.game_objects[move_id]).gjk_targets.Add(rcubeid, new collision_result());
            ((gjkTestActor)world.current_map.game_objects[move_id]).gjk_targets.Add(weirdone, new collision_result());
            ((gjkTestActor)world.current_map.game_objects[move_id]).gjk_targets.Add(capsuleid, new collision_result());
            */


            //world.current_map.add_actor(new MoveTestActor());


            crosshair_sdf = new SDFSprite2D(
                (EngineState.resolution.ToVector2() * 0.5f), 
                new Vector2(16, 16), 0.75f);


            Clock.frame_probe.set("overhead");
            Clock.frame_probe.false_set("overhead");
            init = true;
        }
        bool init = false;

        protected override void LoadContent()
        {
            ContentHandler.LoadContent(Content, GraphicsDevice);
            ContentHandler.LoadAllResources();

        }

        protected override void UnloadContent() {
            ContentHandler.UnloadAll();
        }

        protected override void Update(GameTime gameTime) {
            while (!EngineState.started) { }

            Clock.frame_probe.false_set("overhead");
            Clock.frame_probe.start_of_frame();
            Clock.frame_probe.false_set("overhead");

            Clock.frame_probe.set("frame_start");

            base.Update(gameTime);

            EngineState.Update(gameTime, this);

            world.Update();

            if (Controls.is_pressed(Keys.Escape)) {
                EngineState.running = false;
                Exit();
            }
            /*
            if (tapped("test") && !held("test")) {
                Scene.sun_moon.time_stopped = !Scene.sun_moon.time_stopped;
            }
            */

            if (tapped("test")) {
                world.current_map.add_object(
                    new object_info(world.current_map.game_objects[EngineState.player_id].position + (RNG.rng_v3_neg_one_to_one * 25), new render_info_model("sphere", "trumpmap"),
                    new collision_info(new Sphere(1f)))
                    );
            }

            if (pressed("test")) {
                if (pressed("scroll_up")) {
                    Scene.sun_moon.set_time_of_day(Scene.sun_moon.current_day_value + ((Controls.wheel_delta / 240.0) * 0.05));
                }
                if (pressed("scroll_down")) {
                    Scene.sun_moon.set_time_of_day(Scene.sun_moon.current_day_value + ((Controls.wheel_delta / -240.0) * -0.05));
                }
                
            }

        }

        private string print_ts(TimeSpan ts) {
            string s = string.Format("{0:F0}m{1:F0}s", ts.TotalMinutes, ts.Seconds);
            return s;
        }

        frame_snapshot snap = new frame_snapshot();



        bool draw_debug_info = true;



        protected override void Draw(GameTime gameTime) {
            while ((!init || World.solver.solving)) { if (!EngineState.running) return; }
            EngineState.drawing = true; 
            Clock.frame_probe.set("draw_world");
            GraphicsDevice.SetRenderTargets(EngineState.buffer.buffer_targets);

            world.Draw(GraphicsDevice, EngineState.camera);
            world.current_map.octree.draw_nodes();


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
                        "[ buffer [" + (((int)Scene.buffer == -1) ? "combined" : ((Scene.buffers)Scene.buffer).ToString()) + "] ]\n\n" +
                        "mouse over UI: " + EngineState.window_manager.mouse_over_UI() + "\n\n" +
                        "## gvars ##\n" +
                        gvars.list_all() + "\n\n" +
                        EngineState.window_manager.list_windows() + "\n\n" +
                        $"{EngineState.world.current_map.game_objects[EngineState.player_id].binds.info()}\n"




                        , Vector2.One * 2 + (Vector2.UnitY * 20), Color.White);
                
                EngineState.spritebatch.End();
            }
            EngineState.draw2d();

            GraphicsDevice.SetRenderTarget(EngineState.buffer.rt_2D);

            if (enable_mouse_lock) {
                crosshair_sdf.position = EngineState.resolution * 0.5f;
            } else {
                crosshair_sdf.position = Controls.mouse_position.ToVector2();
            }
            crosshair_sdf.draw();

            GraphicsDevice.SetRenderTarget(null);

            Scene.compose();

            Clock.update_fps();

            Clock.frame_probe.end_of_frame();
            Clock.frame_probe.set("overhead");
            if (gvars.get_bool("draw_probes")) {
                Clock.frame_probe.draw(EngineState.resolution.X - 450, 60, 300, out _, out th);
                int t = th;

                lock (World.internal_frame_probe)
                    World.internal_frame_probe.draw(EngineState.resolution.X - 402, th + 70, 300, out _, out t);

                lock (Controls.control_poll_probe)
                    Controls.control_poll_probe.draw(EngineState.resolution.X - 360, th + t + 80, 300, out _, out _);
            }
            EngineState.drawing = false;
        }
        int th = 0; 
    }
}
