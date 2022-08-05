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
using System.Threading.Tasks;

namespace Magpie { 
    public class World {
        public Map current_map;
        public Actor player_actor => current_map.player_actor;

        public SpotLight test_light;
        public SpotLight test_light2;

        public SegmentedHeightfield test_hf;

        public World() {
            load_map();

            test_hf = new SegmentedHeightfield(Vector3.Zero, XYPair.One * 128, 4);

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
            //lights.Add(test_light2);
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

        

        public void Update() {
            test_light.position = EngineState.camera.position + (EngineState.camera.orientation.Right * 0.6f) + (EngineState.camera.orientation.Down * 0.2f);
            test_light.orientation = EngineState.camera.orientation * Matrix.CreateFromAxisAngle(EngineState.camera.orientation.Up, MathHelper.ToRadians(5f));

            foreach (DynamicLight light in current_map.lights) {
                light.update();
            }

            foreach (GameObject go in current_map.objects.Values) {
                go.Update();
            }

            foreach (Brush brush in current_map.brushes.Values) {
                brush.Update();
            }

            foreach (Actor actor in current_map.actors.Values) {
                actor.Update();
            }

            current_map.player_actor.Update();

            PhysicsSolver.do_movement(current_map);

            PhysicsSolver.do_base_physics_and_ground_interaction(current_map);

            PhysicsSolver.finalize_collisions(current_map);


            Scene.sun_moon.update();
            //test_light.update();
            
        }

        SceneObject[] current_scene;
        public void Draw(GraphicsDevice gd, Camera camera) {
             current_scene = Scene.create_scene_from_lists(current_map.brushes, current_map.objects, current_map.actors, current_map.lights, EngineState.camera.frustum);

            


            //test_light.view = Matrix.CreateLookAt(test_light.position, test_light.position + (camera.orientation.Forward * camera.far_clip), Vector3.Up);
            
            Scene.build_lighting(current_map.lights, current_scene);

            Scene.clear_all_and_draw_skybox(EngineState.camera, EngineState.buffer);

            Scene.draw(current_scene);

            Scene.draw_lighting(current_map.lights);
        }


    }
}
