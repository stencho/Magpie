using Magpie.Engine;
using Magpie.Engine.Brushes;
using Magpie.Engine.Physics;
using Magpie.Engine.Stages;
using Magpie.Graphics;
using Magpie.Graphics.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Magpie { 
    public class World {
        public volatile Map current_map;
        public Actor player_actor => current_map.player_actor;

        //public SpotLight test_light;
        //public SpotLight test_light2;

        public volatile SegmentedTerrain test_hf;


        public World() {
            load_map();

            test_hf = new SegmentedTerrain(Vector3.Zero, 1000, 5);

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

            current_map.add_brush(test_hf);

            //current_map.lights.Add(test_light);

            
            //Scene.parent_world = this;
            //lights.Add(test_light);
        }
        //float3 Depth = tex2D(DEPTH, UV).rgb;

        public void load_map() {
            current_map = new Map();
        }

        public void load_map(string name) {

        }

        public void LoadContent() {
        }

        
        public (float, Brush) highest_floor(Vector3 pos) {
            float highest = float.MinValue;
            float c = 0f;
            Brush f = null;

            foreach (Brush floor in current_map.brushes) {
                if (!floor.within_vertical_bounds(pos)) continue;

                c = floor.get_footing_height(pos);

                if (c > highest) {
                    highest = c;
                    f = floor;
                }
            }

            if (f != null)
                return (highest, f);
            else
                return (float.MinValue, null);
        }
        

        public (float, Brush) highest_floor_below(Vector3 pos) {
            float highest = float.MinValue;
            float c = 0f;
            Brush f = null;

            foreach (Brush floor in current_map.brushes) {
                if (!floor.within_vertical_bounds(pos)) continue;

                c = floor.get_footing_height(pos);

                if (c > highest && c <= pos.Y) {
                    highest = c;
                    f = floor;
                }
            }

            if (f != null)
                return (highest, f);
            else
                return (float.MinValue, null);
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

        private void do_world_update() {            
            while (EngineState.running) {
                internal_frame_probe.start_of_frame();

                Controls.mouse_delta_int = Controls.md_int;
                Controls.md_int = Vector2.Zero;

                update_frame_rate_avg = 0;
                lock(last_ticks) {
                    for (int i = 0; i < last_ticks.Length - 1; i++) {
                        last_ticks[i] = last_ticks[i + 1];
                        update_frame_rate_avg += last_ticks[i];
                    }
                }

                internal_frame_probe.set("lights");

                lock (current_map.brushes) {
                    internal_frame_probe.set("brushes");
                    int brushes_updated = 0;

                    foreach (Brush brush in current_map.brushes) {
                        if (brushes_updated >= current_map.brush_count) break;
                        if (brush == null) continue;

                        lock (brush)
                            brush.Update();
                        brushes_updated++;
                    }

                }
                lock (current_map.objects) {
                    internal_frame_probe.set("objects");
                    int objects_updated = 0;

                    foreach (GameObject go in current_map.objects) {
                        if (objects_updated >= current_map.object_count) break;
                        if (go == null) continue;
                            //if (  go.dead) {
                            // dead_objects.Add(go.name);                    
                            //continue;
                            //}
                        if (!go.dead) {
                            lock (go)
                                go.Update();
                            
                        }

                        objects_updated++;
                    }

                }

                    // lock (dead_objects) {
                    //    for (int i = 0; i < dead_objects.Count; i++) {
                    //        current_map.objects.Remove(dead_objects[i]);
                    //     }
                    // }

                lock (current_map.actors) {
                    internal_frame_probe.set("actors");
                    int actors_updated = 0;

                    foreach (Actor actor in current_map.actors) {
                        if (actors_updated >= current_map.actor_count) break;
                        if (actor == null) continue;

                        lock (actor)
                            actor.Update();
                        actors_updated++;
                    }


                }
                lock (current_map.player_actor) {
                    internal_frame_probe.set("player_actor");
                    if (current_map.player_actor != null)
                        lock (current_map.player_actor)
                            current_map.player_actor.Update();
                }

                internal_frame_probe.set("physics");

                PhysicsSolver.do_movement(current_map);
                PhysicsSolver.do_base_physics_and_ground_interaction(current_map);
                PhysicsSolver.finalize_collisions(current_map);

                dead_objects.Clear();

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
                 
                Clock.internal_frame_time_delta = (float)(Clock.internal_frame_time_delta_ms / 1000.0);
                Clock.internal_frame_time_delta_ms = (float)(DateTime.Now - dt).TotalMilliseconds;
                world_running_slow = Clock.frame_time_delta_ms > Clock.internal_frame_time_delta_ms;
               
                dt = DateTime.Now; 

                internal_frame_probe.end_of_frame(Clock.internal_frame_limit_ms);
            }
        }

        Matrix l_current = Matrix.Identity;
        Vector3 p_current = Vector3.Zero;

        public void Update() {
            if (physics_movement_thread == null) {
                physics_movement_thread = new Thread(do_world_update);
                
                physics_movement_thread.Start();
            }

            Clock.frame_probe.set("update");

            //NEED TO MAKE A SYSTEM LIKE THIS FOR ALL LIGHTS
            //INSTEAD OF A LIGHT LIST IN CURRENT MAP, OBJECTS HAVE INDIVIDUAL LISTS OF A
            //SIMPLE STRUCT CONTAINING THINGS LIKE CURRENT POSITION AND COLOR AND SUCH
            //THEN THE NEW UPGRADED RENDERER WILL USE THIS INFO TO RENDER THEM ALL
            //INSTEAD OF KEEPING INFO ON HOW TO RENDER IN THE INDIVIDUAL LIGHT CLASSES

            /*
            current_map.lights[current_map.lights.Count - 1].position
                = EngineState.camera.position;
            ((SpotLight)current_map.lights[current_map.lights.Count - 1]).orientation
                = EngineState.camera.orientation;
            */

            //BUILD LIST OF VISIBLE OBJECTS HERE THAT SEEMS TO NOT BE A HUGE ISSUE WITH THE GC


            /*
            PhysicsSolver.do_movement(current_map);

            PhysicsSolver.do_base_physics_and_ground_interaction(current_map);

            PhysicsSolver.finalize_collisions(current_map);
            */


            //test_light.update();

        }



        int frame_count = 0;
        SceneObject[] current_scene;
        public bool use_new_renderer = true;

        //EngineState.world.use_new_renderer=true;

        public void Draw(GraphicsDevice gd, Camera camera) {

            player_actor.after_movement_update();

            Renderer.render(current_map, EngineState.camera, EngineState.buffer);
            


            //Scene.draw_world_immediate(this);

            frame_count++;
        }


    }
}
