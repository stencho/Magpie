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
        public Dictionary<string, Brush> brushes = new Dictionary<string, Brush>();
        public Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();
        public Dictionary<string, Actor> actors = new Dictionary<string, Actor>();

        public List<DynamicLight> lights = new List<DynamicLight>();

        public Actor player_actor;

        public void add_brush(string name, Brush floor) {
            brushes.Add(name, floor);
        }

        public void add_object(string name, GameObject gameobject) {
            objects.Add(name, gameobject);
            objects[name].name = name;
            objects[name].parent_map = this;
        }

        public void add_actor(string name, Actor actor) {
            actors.Add(name, actor);
        }        

        public void load_required_resources() {

        }
    }
}
