using Magpie.Engine.Brushes;
using Magpie.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine.Stages {
    [Serializable]
    public class Map {
        public const int max_brushes = 400;
        public const int max_objects = 200;
        public const int max_actors = 50;

        public volatile Brush[] brushes = new Brush[max_brushes];
        public volatile GameObject[] objects = new GameObject[max_objects];
        public volatile Actor[] actors = new Actor[max_actors];

        //public volatile List<DynamicLight> lights = new List<DynamicLight>();

        public volatile Actor player_actor;

        public int add_brush(Brush floor) {
            for (int i = 0; i < max_brushes; i++) {
                if (brushes[i] == null) {                    
                    brushes[i] = floor;
                    return i;
                }
            }
            return -1;
        }

        public int add_object(string name, GameObject gameobject) {
            for (int i = 0; i < max_objects; i++) {
                if (objects[i] == null) {
                    objects[i] = gameobject;
                    objects[i].parent_map = this;
                    objects[i].name = name;
                    return i;
                }
            }
            return -1;
        }

        public int add_actor(Actor actor) {
            for (int i = 0; i < max_actors; i++) {
                if (actors[i] == null) {
                    actors[i] = actor;
                    return i;
                }
            }
            return -1;
        }    
        
        public void remove_brush(int index) {
            brushes[index] = null;
        }
        public void remove_object(int index) {
            objects[index] = null;
        }
        public void remove_actor(int index) {
            actors[index] = null;
        }

        public void load_required_resources() {

        }
    }
}
