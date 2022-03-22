using Magpie.Engine;
using Magpie.Engine.Floors;
using Magpie.Engine.Stages;
using Magpie.Graphics;
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
        public World() {
            load_map();
        }

        public void load_map() {
            current_map = new Map();
        }

        public void load_map(string name) {

        }

        public void LoadContent() {

        }

        public (float, Floor) highest_floor(Vector2 xz) {
            float highest = float.MinValue;
            float c = 0f;
            Floor f = null;

            foreach (Floor floor in current_map.floors.Values) {
                if (!floor.within_vertical_bounds(xz)) continue;

                c = floor.get_footing_height(xz.X, xz.Y);

                if (c > highest) {
                    highest = c;
                    f = floor;
                }
            }

            if (f != null)
                return (highest, f);
            else
                return (0, null);
        }

        public (float, Floor) highest_floor_below(Vector3 pos) {
            float highest = float.MinValue;
            float c = 0f;
            Floor f = null;

            foreach (Floor floor in current_map.floors.Values) {
                if (!floor.within_vertical_bounds(pos.XZ())) continue;

                c = floor.get_footing_height(pos.X, pos.Y);

                if (c > highest && c < pos.Y) {
                    highest = c;
                    f = floor;
                }
            }

            if (f != null)
                return (highest, f);
            else
                return (0, null);
        }

        public void Update() {
            foreach (Actor actor in current_map.actors.Values) {
                actor.Update();
            }

            current_map.player_actor.Update();
        }

        public void Draw(GraphicsDevice gd, Camera camera) {
            foreach (Floor floor in current_map.floors.Values) {
                floor.Draw();
            }
            foreach (GameObject go in current_map.objects.Values) {
                go.Draw(gd, camera);
            }
            foreach (Actor actor in current_map.actors.Values) {
                actor.Draw();
            }
        }

    }
}
