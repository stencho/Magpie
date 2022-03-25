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

        public Spotlight test_light;

        public World() {
            load_map();
            test_light = new Spotlight();
        }

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
        }

        public void Draw(GraphicsDevice gd, Camera camera) {
            foreach (Floor floor in current_map.floors.Values) {
                floor.Draw();
            }
            foreach (GameObject go in current_map.objects.Values) {
                go.Draw();
            }
            foreach (Actor actor in current_map.actors.Values) {
                actor.Draw();
            }
        }

        public void build_lighting() {
            EngineState.graphics_device.SetRenderTarget(test_light.depth_map);
            test_light.position = EngineState.camera.position + Vector3.Up * 10f;
            test_light.orientation = EngineState.camera.orientation * Matrix.CreateFromAxisAngle(EngineState.camera.orientation.Left, MathHelper.ToRadians(30f));

            test_light.view = Matrix.CreateLookAt(test_light.position, test_light.position + test_light.orientation.Forward, Vector3.Up);

            EngineState.graphics_device.Clear(Color.White);

            foreach (Floor floor in current_map.floors.Values) {
                floor.draw_depth(test_light);
            }

            foreach (GameObject go in current_map.objects.Values) {
                go.draw_depth(test_light);
            }

        }

    }
}
