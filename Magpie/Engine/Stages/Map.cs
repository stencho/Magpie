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

                var nids = octree.add(id, object_info.bounding_box());

                game_objects[id].octree_base_nodes.AddRange(nids);
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

        public void do_broad_phase() {

            foreach (var obj in game_objects.Keys) {
                
                    game_objects[obj].pre_update();
                    game_objects[obj].update();
                
            }
            foreach (var obj in game_objects.Keys) {
                if (game_objects[obj].collision != null && game_objects[obj].collision.dynamic) {

                    var bb = game_objects[obj].bounding_box();
                    var octree_hits = octree.objects_in_intersecting_nodes(bb);

                    Vector3 shortest_sweep = game_objects[obj].wants_movement;
                    collision_result shortest_hit = new collision_result();
                    int shortest_id = -1;

                    foreach (var hit in octree_hits) {
                        if (game_objects[hit].collision == null || hit == obj) continue;

                        var shortest_sweep_current = game_objects[obj].wants_movement;

                        if (bb.Intersects(game_objects[hit].bounding_box())) {

                            Shape3D shape_a = game_objects[obj].collision.movebox;
                            Shape3D shape_b = game_objects[hit].collision.movebox;

                            if (shape_b == null) continue;
                            var result = GJK.swept_gjk_intersects_with_halving(
                                shape_a, shape_b,
                                game_objects[obj].world, game_objects[hit].world,
                                game_objects[obj].wants_movement, game_objects[hit].wants_movement);
                                //Vector3.Zero, Vector3.Zero);

                            if (result.intersects) {
                                var p = result.penetration * Vector3.Normalize(result.penetration_normal);
                                var dist = result.end_simplex.sweep_A.Length();


                                if (dist < shortest_sweep.Length()) {
                                    shortest_sweep = Vector3.Normalize(game_objects[obj].wants_movement) * dist;
                                    game_objects[obj].wants_movement = shortest_sweep;
                                    shortest_hit = result;
                                    shortest_id = obj;
                                }
                                if (!p.contains_nan() && p != Vector3.Zero) {
                                    //game_objects[obj].wants_movement += p;

                                }
                            }
                        }
                    }

                    if (shortest_id >= 0) {
                        var p = shortest_hit.penetration * Vector3.Normalize(shortest_hit.penetration_normal);
                        if (!p.contains_nan() && p != Vector3.Zero) {
                            //wants_movement -= gjk_targets[shortest_id].end_simplex.sweep_A;
                            //wants_movement += p ;
                            game_objects[obj].wants_movement = shortest_sweep + p;
                        }
                    }

                }


                if (game_objects[obj].wants_movement != Vector3.Zero) {
                    game_objects[obj].position += game_objects[obj].wants_movement;
                    game_objects[obj].wants_movement = Vector3.Zero;
                    game_objects[obj].resting = false;
                }
            }
            /*
            foreach (var obj in game_objects.Keys) {
                if (game_objects[obj].collision != null && game_objects[obj].collision.dynamic) {

                    var bb = game_objects[obj].bounding_box();
                    var octree_hits = octree.objects_in_intersecting_nodes(bb);

                    Vector3 shortest_sweep = game_objects[obj].wants_movement;
                    int shortest_id = -1;

                    foreach (var hit in octree_hits) {
                        if (game_objects[hit].collision == null) continue;

                        if (bb.Intersects(game_objects[hit].bounding_box())) {
                            Shape3D shape_a = game_objects[obj].collision.movebox;
                            Shape3D shape_b = game_objects[hit].collision.movebox;

                            if (shape_b == null) continue;
                            var result = GJK.swept_gjk_intersects_with_halving(
                                shape_a, shape_b,
                                game_objects[obj].world, game_objects[hit].world,
                                shortest_sweep, game_objects[hit].wants_movement);

                            if (result.intersects) {
                                var p = result.penetration * Vector3.Normalize(result.penetration_normal);
                                var dist = result.end_simplex.sweep_A.Length();


                                if (dist < shortest_sweep.Length()) {
                                    shortest_sweep = Vector3.Normalize(game_objects[obj].wants_movement) * dist;
                                    shortest_id = hit;
                                }

                                game_objects[obj].wants_movement += p;
                            }

                        } else {
                            octree_hits.Remove(hit);
                        }
                    }
                }
            }
            */
            foreach (var obj in game_objects.Keys) {

                game_objects[obj].post_solve();
            }
        }

    }
}
