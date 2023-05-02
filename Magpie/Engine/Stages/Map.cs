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
using static Magpie.Engine.Collision.Collision2D;

namespace Magpie.Engine.Stages {
    public class Map {
        public float update_range = 200f;

        public Octree octree = new Octree(Vector3.Zero, ((Vector3.Right + Vector3.Forward) * 1000) + (Vector3.Up * 750),6);

        public SegmentedTerrain terrain;

        public struct resource_info {
            public string name;
            public List<int> objects_using;
        }
        public List<resource_info> required_resources = new List<resource_info>();

        public void load_required_resources() {

        }

        public volatile Dictionary<int, object_info> game_objects = new Dictionary<int, object_info>();

        Queue<(int id, object_info info)> spawn_queue = new Queue<(int id, object_info info)>();

        public void do_spawn_queue() {
            lock (game_objects) {
                while (spawn_queue.Count > 0) {
                    var q = spawn_queue.Dequeue();
                    if (q.id == 0) return;

                    lock(octree)
                        add_object(q.id, q.info);
                }
            }
        }

        public int spawn_object(object_info object_info) {
            var id = make_id();

            spawn_queue.Enqueue((id,object_info));

            return id;
        }
        void add_object(int id, object_info object_info) {
            lock (this) {
                game_objects.Add(id, object_info);
                game_objects[id].id = id;

                //var nids = octree.add(id, object_info.bounding_box());

                //game_objects[id].octree_base_nodes.AddRange(nids);
            }
        }
        public int add_object(object_info object_info) {
            var id = make_id();
            lock (this) {
                game_objects.Add(id, object_info);
                game_objects[id].id = id;

                //var nids = octree.add(id, object_info.bounding_box());

                //game_objects[id].octree_base_nodes.AddRange(nids);
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

        struct frame_collision {

            collision_result[] results;
        }


    }
}
