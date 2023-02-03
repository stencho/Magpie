using Magpie.Engine.Brushes;
using Magpie.Engine.WorldElements;
using Magpie.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine.Stages {
    [Serializable]
    public class Map {

        public const int max_objects = 3000;
        public const int max_actors = 100;

        public int object_count = 0;
        public int actor_count = 0;
        
        public volatile GameObject[] objects = new GameObject[max_objects];
        public volatile Actor[] actors = new Actor[max_actors];

        public volatile Dictionary<int, object_info> game_objects = new Dictionary<int, object_info>();
        public int make_id() {
            int id = RNG.rng_int();
            while (game_objects.ContainsKey(id)) {id = RNG.rng_int();}
            return id;
        }

        //public volatile List<DynamicLight> lights = new List<DynamicLight>();

        public volatile Actor player_actor;


        public int add_object(string name, GameObject gameobject) {
            for (int i = 0; i < max_objects; i++) {
                if (objects[i] == null) {
                    objects[i] = gameobject;
                    objects[i].parent_map = this;
                    objects[i].name = name;
                    object_count++;
                    return i;
                }
            }
            return -1;
        }

        public int add_actor(Actor actor) {
            for (int i = 0; i < max_actors; i++) {
                if (actors[i] == null) {
                    actors[i] = actor;
                    actor_count++;
                    return i;
                }
            }
            return -1;
        }    
        
        /*
        public void remove_brush(int index) {
            if (brushes[index] != null) {
                brushes[index] = null;
                brush_count--;
            }
        }
        */

        public void remove_object(int index) {
            if (objects[index] != null) {
                objects[index] = null;
                object_count--;
            }
        }
        public void remove_actor(int index) {
            if (actors[index] != null) {
                actors[index] = null;
                actor_count--;
            }
        }

        public void load_required_resources() {

        }
    }
}
