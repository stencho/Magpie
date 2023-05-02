using Magpie.Engine;
using Magpie.Engine.Brushes;
using Magpie.Engine.Collision;
using Magpie.Engine.Collision.Solver;
using Magpie.Engine.Collision.Support3D;
using Magpie.Engine.Stages;
using Magpie.Engine.WorldElements;
using Magpie.Graphics;
using Magpie.Graphics.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Magpie { 
    public class World {
        public volatile Map current_map;

        //public SpotLight test_light;
        //public SpotLight test_light2;


        public World() {
            load_map();

            //test_hf = new SegmentedTerrain(Vector3.Zero, 1000, 5);

            //test_light = new SpotLight();
            //test_light2 = new SpotLight();
            //test_light2.position += Vector3.Left * 14f;
            //test_light2.light_color = Color.LightPink;
            
            /*
            for (int i = 0; i < 20; i++) {
                 current_map.lights.Add(new PointLight(
                     (Vector3.UnitX * (10f * RNG.rng_float_neg_one_to_one)) + (Vector3.UnitY * (20f * RNG.rng_float)) + (Vector3.Forward * (30f * RNG.rng_float)), 
                     5 + (RNG.rng_float * 10f), 
                     RNG.random_opaque_color()
                     ));
            }
            */

           // current_map.add_brush(test_hf);

            //current_map.lights.Add(test_light);

            
            //Scene.parent_world = this;
            //lights.Add(test_light);
        }

        public void load_map() {
            current_map = new Map();
        }

        public void load_map(string name) {

        }

        public void LoadContent() {
        }

        
        static List<string> dead_objects = new List<string>();

        public static Thread physics_movement_thread;


        public static double world_update_thread_timer = 0;

        public static bool running_slow => world_running_slow;
        static volatile bool world_running_slow = false;

        public static bool match_external_fps_below_limit = false;

        public static double last_tick_timer_val = 0;

        public static double[] last_ticks = new double[200];
        public static bool update_timer_tick = false;
        public static double update_frame_rate_avg = 0;

        public static double[] last_fps = new double[300];

        public static DateTime dt = DateTime.Now;
        public static TimeSpan ts;

        public static volatile frame_probe internal_frame_probe = new frame_probe();

        public static CollisionSolver solver = new CollisionSolver();

        private void do_world_update() {            
            while (EngineState.running) {
                internal_frame_probe.start_of_frame();

                Controls.pull_accumulated_md_internal();

                update_frame_rate_avg = 0;
                lock(last_ticks) {
                    for (int i = 0; i < last_ticks.Length - 1; i++) {
                        last_ticks[i] = last_ticks[i + 1];
                        update_frame_rate_avg += last_ticks[i];
                    }
                }
                


                internal_frame_probe.set("update");
                lock (current_map) {
                    foreach (var obj in current_map.game_objects.Keys) {
                        current_map.game_objects[obj].pre_update();
                    }
                        //while (EngineState.drawing) {}
                    foreach (var obj in current_map.game_objects.Keys) {
                            current_map.game_objects[obj].update();

                    }


                    current_map.do_spawn_queue();

                        //solver.solve();

                    //current_map.test_solve();

                    
                    foreach (int oi in current_map.game_objects.Keys) {

                        current_map.game_objects[oi].pre_update();
                        current_map.game_objects[oi].update();

                        if (current_map.game_objects[oi].wants_movement != Vector3.Zero) {

                            current_map.game_objects[oi].previous_position = current_map.game_objects[oi].position;
                            current_map.game_objects[oi].position += current_map.game_objects[oi].wants_movement;
                            current_map.game_objects[oi].wants_movement = Vector3.Zero;

                            current_map.game_objects[oi].resting = false;
                        } else {
                            current_map.game_objects[oi].resting = true;
                        }

                        current_map.game_objects[oi].post_solve();
                    }

                    current_map.octree.update_leaves_within_radius(current_map.update_range, EngineState.camera.position);


                    //current_map.octree.update_tree_structure();
                }



                lock (last_fps) {
                    for (int i = 0; i < last_fps.Length - 1; i++) {
                        last_fps[i] = last_fps[i + 1];
                    }
                    last_fps[last_fps.Length - 1] = 1000.0 / last_ticks[last_ticks.Length - 1];
                }

                internal_frame_probe.set("sleep");

                while (EngineState.running) {
                    ts = (DateTime.Now - dt);

                    if (ts.TotalMilliseconds >= (match_external_fps_below_limit ? (world_running_slow ? Clock.d_frame_time_delta_ms : Clock.internal_frame_limit_ms) : Clock.internal_frame_limit_ms)) {
                        ts = TimeSpan.Zero;
                        break;
                    }
                }
                


                lock (last_ticks) {
                    last_tick_timer_val = (DateTime.Now - dt).TotalMilliseconds;
                    last_ticks[last_ticks.Length - 1] = last_tick_timer_val;

                    update_frame_rate_avg += last_ticks[last_ticks.Length - 1];
                    update_frame_rate_avg /= last_ticks.Length;
                }

                //Clock.internal_frame_time_delta_ms = (float)(DateTime.Now - dt).TotalMilliseconds;
                //Clock.internal_frame_time_delta = (float)(Clock.internal_frame_time_delta_ms / 1000.0);
                world_running_slow = Clock.frame_time_delta_ms > Clock.internal_frame_time_delta_ms;
               
                dt = DateTime.Now; 

                internal_frame_probe.end_of_frame(Clock.internal_frame_limit_ms);

            }
        }





        Matrix l_current = Matrix.Identity;
        Vector3 p_current = Vector3.Zero;



        public void Update() {
            Clock.frame_probe.set("update");
            if (physics_movement_thread == null) {
                physics_movement_thread = new Thread(do_world_update);
                
                physics_movement_thread.Start();
            }
        }



        int frame_count = 0;
        SceneObject[] current_scene;
        public bool use_new_renderer = true;

        //EngineState.world.use_new_renderer=true;

        public void Draw(GraphicsDevice gd, Camera camera) {

            int u = 0;

            EngineState.camera.update();
            EngineState.camera.update_projection(EngineState.resolution);

            lock (EngineState.camera) {
                Renderer.render(current_map, EngineState.camera, EngineState.buffer);
            }


            //Scene.draw_world_immediate(this);

            frame_count++;
        }


    }
}
