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

        public const int max_actors = 100;

        public int actor_count = 0;
        
        public volatile Actor[] actors = new Actor[max_actors];

        public volatile Dictionary<int, object_info> game_objects = new Dictionary<int, object_info>();
        public int make_id() {
            int id = RNG.rng_int();
            while (game_objects.ContainsKey(id)) {id = RNG.rng_int();}
            return id;
        }

        public volatile Actor player_actor;

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
