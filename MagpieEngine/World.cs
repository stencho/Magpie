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

namespace Magpie { 
    public class World {
        public Map current_map;
        public Actor player_actor => current_map.player_actor;

        public SpotLight test_light;
        public SpotLight test_light2;

        public SegmentedTerrain test_hf;

        public bool running = true;

        public World() {
            load_map();

            test_hf = new SegmentedTerrain(Vector3.Zero, 200, 5);

            test_light = new SpotLight();
            test_light2 = new SpotLight();
            test_light2.position += Vector3.Left * 14f;
            test_light2.light_color = Color.LightPink;
            
            for (int i = 0; i < 20; i++) {
                 current_map.lights.Add(new PointLight(
                     (Vector3.UnitX * (10f * RNG.rng_float_neg_one_to_one)) + (Vector3.UnitY * (20f * RNG.rng_float)) + (Vector3.Forward * (30f * RNG.rng_float)), 
                     5 + (RNG.rng_float * 10f), 
                     RNG.random_opaque_color()
                     ));
            }
            current_map.brushes.Add("test_heightfield", test_hf);

            current_map.lights.Add(test_light);

            physics_movement_thread = new Thread(do_world_update);
            physics_movement_thread.Start();
            
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

            foreach (Brush floor in current_map.brushes.Values) {
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

            foreach (Brush floor in current_map.brushes.Values) {
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


        List<string> dead_objects = new List<string>();

        private Thread physics_movement_thread;

        private double world_update_thread_frame_rate = 60;
        private double world_update_thread_frame_time_ms => 1000f / world_update_thread_frame_rate;

        public double world_update_thread_timer = 0;
        public bool world_update_thread_timer_tick = false;
        public bool world_running_slow = false;

        public double last_tick_timer_val = 0;

        public double[] last_ticks = new double[60*5];
        public bool update_timer_tick = false;
        public double update_frame_rate_avg = 0;

        private void do_world_update() {
            DateTime dt = DateTime.Now;
            double loop_time = 0;

            while (running) {
                while ((DateTime.Now - dt).TotalMilliseconds <= (world_running_slow ? Clock.frame_time_delta_ms : world_update_thread_frame_time_ms) - (loop_time * .75)) {
                    world_running_slow = Clock.frame_time_delta_ms > world_update_thread_frame_time_ms;
                }

                update_frame_rate_avg = 0;
                for (int i = 0; i < last_ticks.Length - 1; i++) {
                    last_ticks[i] = last_ticks[i + 1];
                    update_frame_rate_avg += last_ticks[i];
                } 

                PhysicsSolver.do_movement(current_map);

                PhysicsSolver.do_base_physics_and_ground_interaction(current_map);

                PhysicsSolver.finalize_collisions(current_map);

                last_tick_timer_val = (DateTime.Now - dt).TotalMilliseconds;
                last_ticks[last_ticks.Length - 1] = last_tick_timer_val;

                update_frame_rate_avg += last_ticks[last_ticks.Length - 1];
                update_frame_rate_avg /= last_ticks.Length;

                loop_time = (DateTime.Now - dt).TotalMilliseconds - world_update_thread_frame_time_ms;
                dt = DateTime.Now;
            }
            /*
            while (running) {

                if (force) {
                    world_update_thread_timer = world_running_slow ? Clock.frame_time_delta_ms : world_update_thread_frame_time_ms;
                } else {
                    world_update_thread_timer += Clock.frame_time_delta_ms;                    
                }

                if (world_update_thread_timer >= (
                    world_running_slow ? Clock.frame_time_delta_ms : world_update_thread_frame_time_ms
                    )) {

                    last_tick_timer_val = world_update_thread_timer;

                    for (int i = 0; i < last_ticks.Length - 1; i++) {
                        last_ticks[i] = last_ticks[i + 1];
                    } last_ticks[last_ticks.Length - 1] = last_tick_timer_val;

                    world_update_thread_timer = 0;
                    update_timer_tick = true;


                    PhysicsSolver.do_movement(current_map);

                    PhysicsSolver.do_base_physics_and_ground_interaction(current_map);

                    PhysicsSolver.finalize_collisions(current_map);

                } else {
                    update_timer_tick = false;
                }

                if (world_update_thread_timer < (world_running_slow ? Clock.frame_time_delta_ms : world_update_thread_frame_time_ms)) {
                    Thread.Sleep(TimeSpan.FromMilliseconds(world_update_thread_frame_time_ms));
                    force = true;
                }
            }
            */

        }

        public void Update() {

            if (running == false) {
                EngineState.game.Exit();
            }

            current_map.lights[current_map.lights.Count-1].position = EngineState.camera.position + (EngineState.camera.orientation.Right * 0.6f) + (EngineState.camera.orientation.Down * 0.2f);
            ((SpotLight)current_map.lights[current_map.lights.Count-1]).orientation = EngineState.camera.orientation * Matrix.CreateFromAxisAngle(EngineState.camera.orientation.Up, MathHelper.ToRadians(5f));

            foreach (DynamicLight light in current_map.lights) {
                light.update();
            }


            foreach (Brush brush in current_map.brushes.Values) {
                brush.Update();
            }

            foreach (GameObject go in current_map.objects.Values) {
                if (go.dead) {
                   // dead_objects.Add(go.name);                    
                    continue;
                }

                go.Update();                
            }

            for (int i = 0; i < dead_objects.Count; i++) {
                current_map.objects.Remove(dead_objects[i]);
            }

            foreach (Actor actor in current_map.actors.Values) {
                actor.Update();
            }

            current_map.player_actor.Update();

            //BUILD LIST OF VISIBLE OBJECTS HERE THAT SEEMS TO NOT BE A HUGE ISSUE WITH THE GC


            /*
            PhysicsSolver.do_movement(current_map);

            PhysicsSolver.do_base_physics_and_ground_interaction(current_map);

            PhysicsSolver.finalize_collisions(current_map);
            */


            Scene.sun_moon.update();
            //test_light.update();

            dead_objects.Clear();
        }


        int frame_count = 0;
        SceneObject[] current_scene;
        public void Draw(GraphicsDevice gd, Camera camera) {


            //test_light.view = Matrix.CreateLookAt(test_light.position, test_light.position + (camera.orientation.Forward * camera.far_clip), Vector3.Up);

           // current_scene = Scene.create_scene_from_lists(current_map.brushes, current_map.objects, current_map.actors, current_map.lights, EngineState.camera.frustum);
           // Scene.build_lighting(current_map.lights, current_scene);
           // Scene.clear_all_and_draw_skybox(EngineState.camera, EngineState.buffer);
           // Scene.draw(current_scene);
           // EngineState.graphics_device.BlendState = BlendState.Opaque;
           // foreach (Brush brush in current_map.brushes.Values) {
           //     brush.debug_draw();
           // }
           // EngineState.graphics_device.BlendState = BlendState.AlphaBlend;
           // Scene.draw_lighting(current_map.lights);


            Scene.draw_world_immediate(this);

            frame_count++;
        }


    }
}
