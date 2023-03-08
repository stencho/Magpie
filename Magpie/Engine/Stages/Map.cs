using Magpie.Engine.Brushes;
using Magpie.Engine.Collision;
using Magpie.Engine.WorldElements;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Magpie.Engine.Stages {
    public class Map {
        public float update_range = 1500f;
        
        public DynamicOctree octree = new DynamicOctree();

        public struct resource_info {
            public string name;
            public List<int> objects_using;
        }
        public List<resource_info> required_resources = new List<resource_info>();

        public void load_required_resources() {

        }

        public volatile Dictionary<int, object_info> game_objects = new Dictionary<int, object_info>();

        public int add_object(object_info object_info) {
            var id = make_id();
            lock (this) {
                game_objects.Add(id, object_info);
                game_objects[id].id = id;
                octree.add(id);
            }
            return id;
        }

        public void remove_object(int id) {
            game_objects.Remove(id);
        } 


        public int make_id() {
            int id = RNG.rng_int();
            while (game_objects.ContainsKey(id)) {id = RNG.rng_int();}
            return id;
        }

    }
}
