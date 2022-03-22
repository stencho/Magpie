using Magpie.Engine.Floors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine.Stages {
    [Serializable]
    public class Map {
        public Dictionary<string, Floor> floors = new Dictionary<string, Floor>();
        public Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();
        public Dictionary<string, Actor> actors = new Dictionary<string, Actor>();

        public Actor player_actor;

        public void add_floor(string name, Floor floor) {
            floors.Add(name, floor);
        }

        public void add_object(string name, GameObject gameobject) {
            objects.Add(name, gameobject);
        }

        public void add_actor(string name, Actor actor) {
            actors.Add(name, actor);
        }        

        public void load_required_resources() {

        }
    }
}
