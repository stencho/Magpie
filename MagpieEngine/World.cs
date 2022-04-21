using Magpie.Engine;
using Magpie.Engine.Floors;
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
        public List<DynamicLight> lights = new List<DynamicLight>();

        public World() {
            load_map();
            test_light = new SpotLight();
            test_light2 = new SpotLight();
            test_light2.position += Vector3.Left * 14f;
            test_light2.light_color = Color.LightPink;
            
            for (int i = 0; i < 20; i++) {
                lights.Add(new PointLight(
                    (Vector3.UnitX * (10f * RNG.rng_float_neg_one_to_one)) + (Vector3.UnitY * (20f * RNG.rng_float)) + (Vector3.Forward * (30f * RNG.rng_float)), 
                    5 + (RNG.rng_float * 10f), 
                    RNG.random_opaque_color()
                    ));
            }
            
            lights.Add(test_light);
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

        
        public (float, Floor) highest_floor(Vector3 pos) {
            float highest = float.MinValue;
            float c = 0f;
            Floor f = null;

            foreach (Floor floor in current_map.floors.Values) {
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
        

        public (float, Floor) highest_floor_below(Vector3 pos) {
            float highest = float.MinValue;
            float c = 0f;
            Floor f = null;

            foreach (Floor floor in current_map.floors.Values) {
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
            test_light.position = EngineState.camera.position + (EngineState.camera.orientation.Right * 0.6f) + (EngineState.camera.orientation.Down * 0.5f);
            test_light.orientation = EngineState.camera.orientation * Matrix.CreateFromAxisAngle(EngineState.camera.orientation.Up, MathHelper.ToRadians(5f));

            foreach (DynamicLight light in lights) {
                light.update();
            }

            foreach (GameObject go in current_map.objects.Values) {
                go.Update();
            }

            foreach (Floor floor in current_map.floors.Values) {
                floor.Update();
            }

            foreach (Actor actor in current_map.actors.Values) {
                actor.Update();

                if (actor.wants_movement != Vector3.Zero) {
                    actor.position += actor.wants_movement;

                    actor.wants_movement = Vector3.Zero;
                }
            }

            current_map.player_actor.Update();

            if (player_actor.wants_movement != Vector3.Zero) {
                player_actor.position += player_actor.wants_movement;

                player_actor.wants_movement = Vector3.Zero;
            }
            //test_light.update();
            
        }

        SceneObject[] current_scene;
        public void Draw(GraphicsDevice gd, Camera camera) {
             current_scene = Scene.create_scene_from_lists(current_map.floors, current_map.objects, current_map.actors, lights, EngineState.camera.frustum);

            /*
            foreach (Floor floor in current_map.floors.Values) {
                floor.Draw();
            }
            foreach (GameObject go in current_map.objects.Values) {
                go.Draw();
            }
            foreach (Actor actor in current_map.actors.Values) {
                actor.Draw();
            }
            */


            //test_light.view = Matrix.CreateLookAt(test_light.position, test_light.position + (camera.orientation.Forward * camera.far_clip), Vector3.Up);

            Scene.build_lighting(lights, current_scene);

            Scene.clear_all_and_draw_skybox(EngineState.camera, EngineState.buffer);

            Scene.draw(current_scene);
            Scene.draw_lighting(lights);
        }


    }
}
