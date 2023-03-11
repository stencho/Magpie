using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Magpie.Engine.Collision.Octrees;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Magpie.Engine.Collision {


    public class DynamicOctree {
        public float base_node_size = 500f;
        public Dictionary<long, DynamicNode> base_nodes = new Dictionary<long, DynamicNode>();
        public int object_total = 0;
        public void draw() {
            foreach (long node_id in base_nodes.Keys) {
                StringBuilder sb = new StringBuilder();
                foreach(var a in base_nodes[node_id].values.Values) {
                    if (EngineState.world.current_map.game_objects[a.id].collision != null)
                    sb.Append($"{a.id.ToString()} {EngineState.world.current_map.game_objects[a.id].collision.movebox.shape.ToString()}\n");
                }
                Draw3D.text_3D(EngineState.spritebatch, 
                    $"ID {node_id.ToString()}\n{base_nodes[node_id].values.Count.ToString()} objects\n{sb.ToString()}" , 
                    "pf", base_nodes[node_id].bounds.Min + ((base_nodes[node_id].bounds.Max - base_nodes[node_id].bounds.Min) / 2), -EngineState.camera.direction, 15f, Color.Black);

                base_nodes[node_id].draw();
                Draw3D.cube(base_nodes[node_id].bounds, Color.Red);
            }
        }

        public void update_tree_structure() {
            //take into account whether objects are dynamic/at rest or not
            //static and resting objects never need to be moved



        }

        //static objects maybe should be added by bounds and be allowed to be in multiple nodes?
        //maybe everything should be added by bounds after all?
        //it's slower but it makes things easier in some ways

        public long[] add(int object_id, BoundingBox bb) {
            var min = bb.Min;
            var max = bb.Max;

            var min_node = base_node_pos_from_pos(min);
            var max_node = base_node_pos_from_pos(max);

            List<long> node_ids = new List<long>();

            if (min_node == max_node) {

                var nid = create_node_id(min_node.X, min_node.Y, min_node.Z);
                node_ids.Add(nid);

                if (!base_nodes.Keys.Contains(nid)) {
                    create_base_node(min_node);
                    base_nodes[nid].add_value(object_id, bb);
                } else {
                    base_nodes[nid].add_value(object_id, bb);
                }

                return node_ids.ToArray();
            } else {

                (long X, long Y, long Z) node_coverage = (max_node.X - min_node.X, max_node.Y - min_node.Y, max_node.Z - min_node.Z);

                for (int z = min_node.Z; z <= min_node.Z + node_coverage.Z; z++) {
                    for (int y = min_node.Y; y <= min_node.Y + node_coverage.Y; y++) {
                        for (int x = min_node.X; x <= min_node.X + node_coverage.X; x++) {
                            (long X, long Y, long Z) node = (x, y, z);
                            //Vector3 node_min = new Vector3(node.X, node.Y, node.Z) * base_node_size;
                            //Vector3 node_max = new Vector3(node.X+1, node.Y+1, node.Z+1) * base_node_size;

                            var nid = create_node_id(node.X, node.Y, node.Z);
                            node_ids.Add(nid);

                            if (!base_nodes.Keys.Contains(nid)) {
                                create_base_node(node);
                                base_nodes[nid].add_value(object_id, bb);
                            } else {
                                base_nodes[nid].add_value(object_id, bb);
                            }

                        }
                    }
                }


            }




            return node_ids.ToArray();
        }
        
        public long add(int id) {
            var id_pos = EngineState.world.current_map.game_objects[id].position;
            var base_node_pos = base_node_pos_from_pos(id_pos);
            var nid = create_node_id(base_node_pos.X, base_node_pos.Y, base_node_pos.Z);

            if (!base_nodes.Keys.Contains(nid)) {
                create_base_node(base_node_pos);
            }
            
            base_nodes[nid].add_value(id);
            return nid;
        }

        public void remove(int id) {
            var node_id = node_id_from_object_id(id);
            base_nodes[node_id].remove_value(id);            
        }
        
        
        public HashSet<int> objects_in_intersecting_nodes(BoundingBox bounds) {
            HashSet<int> objects = new HashSet<int>();
            foreach(DynamicNode n in base_nodes.Values) {
                if (n.bounds.Intersects(bounds)) {
                    if (n.subdivided)
                        add_lowest_objects(ref objects, bounds, n.nodes);
                    else {
                        foreach (octree_value v in n.values.Values) {
                            objects.Add(v.id);
                        }
                    }

                }
            }
            return objects;
        }

        void add_lowest_objects(ref HashSet<int> values, BoundingBox bb, DynamicNode[,,] nodes) {
            foreach (DynamicNode n in nodes) {
                if (n.bounds.Intersects(bb)) {
                    if (n.subdivided) {
                        add_lowest_objects(ref values, bb, n.nodes);
                    } else {
                        foreach (octree_value v in n.values.Values) {
                            values.Add(v.id);
                        }
                    }
                }
            }
        }

        BoundingBox node_pos_to_bb((long X, long Y, long Z) node) {
            Vector3 min = new Vector3(node.X * base_node_size, node.Y * base_node_size, node.Z * base_node_size);
            Vector3 max = new Vector3((node.X+1) * base_node_size, (node.Y+1) * base_node_size, (node.Z+1) * base_node_size);
            return new BoundingBox(min, max);
        }

        (int X, int Y, int Z) base_node_pos_from_id(int id) { var pos = EngineState.world.current_map.game_objects[id].position; return base_node_pos_from_pos(pos); }

        (int X, int Y, int Z) base_node_pos_from_pos(Vector3 pos) {
            int x = pos.X >= 0 ? (int)(pos.X / base_node_size) : (int)(pos.X / base_node_size)-1;
            int y = pos.Y >= 0 ? (int)(pos.Y / base_node_size) : (int)(pos.Y / base_node_size)-1;
            int z = pos.Z >= 0 ? (int)(pos.Z / base_node_size) : (int)(pos.Z / base_node_size)-1;

            return (x,y,z); 
        }

        long node_id_from_object_id(int id) {
            var bnp = base_node_pos_from_id(id);
            return create_node_id(bnp.X, bnp.Y, bnp.Z);
        }

        long create_node_id(long X, long Y, long Z) {
            return X << 40 | (Y & 0xFFFFL) << 24 | (Z & 0xFFFFFFL);
        }
        (int X, int Y, int Z) node_pos_from_node_id(long id) {
            return (
                (int)(id >> 40),
                (int)((id >> 8) >> 16),
                (int)(id << 8) >> 8);
        }

        long create_base_node((long X, long Y, long Z) pos) {
            return create_base_node(pos.X, pos.Y, pos.Z);
        }
        long create_base_node(long X, long Y, long Z) {
            var min = Vector3.Zero;
            var max = Vector3.Zero;

            min = new Vector3((X) * base_node_size, (Y) * base_node_size, (Z) * base_node_size);
            max = new Vector3((X+1) * base_node_size, (Y+1) * base_node_size, (Z+1) * base_node_size);

            var node_id = create_node_id(X,Y,Z);
            base_nodes.Add(node_id, new DynamicNode(null, min, max));
            return node_id;
        }

    }

    public struct octree_value {
        public int id, next_subdiv_x, next_subdiv_y, next_subdiv_z;
    }

    public class DynamicNode {
        DynamicNode parent;

        public volatile DynamicNode[,,] nodes;

        public bool subdivided = false;
        public int x, y, z;
        //public uint node_id = 0;

        public volatile Dictionary<int, octree_value> values = new Dictionary<int, octree_value>();
        public int count => values.Count;

        public BoundingBox bounds;

        public Color color;

        public DynamicNode(DynamicNode parent, Vector3 min, Vector3 max) {
            this.parent = parent;

            bounds = new BoundingBox(min, max);
        }


        public void add_value(int object_id) {
            var pos = EngineState.world.current_map.game_objects[object_id].position;
            if (subdivided) {
                for (byte z = 0; z < 2; z++) {
                    for (byte y = 0; y < 2; y++) {
                        for (byte x = 0; x < 2; x++) {
                            if (nodes[x,y,z].bounds.Contains(pos) != ContainmentType.Disjoint) {
                                values.Add(object_id, new octree_value() { id = object_id, next_subdiv_x = x, next_subdiv_y = y, next_subdiv_z = z });
                                nodes[x,y,z].add_value(object_id);
                                goto hit;
                            }
                        }
                    }
                }

                hit:
                bool hit = true;
            } else {
                values.Add(object_id, new octree_value() { id = object_id });
            }
        }

        public void add_value(int object_id, BoundingBox bb) {

            if (subdivided) {
                for (byte z = 0; z < 2; z++) {
                    for (byte y = 0; y < 2; y++) {
                        for (byte x = 0; x < 2; x++) {
                            if (nodes[x, y, z].bounds.Contains(bb) != ContainmentType.Disjoint) {
                                values.Add(object_id, new octree_value() { id = object_id, next_subdiv_x = x, next_subdiv_y = y, next_subdiv_z = z });
                                nodes[x, y, z].add_value(object_id, bb);
                            }
                        }
                    }
                }
            } else {
                values.Add(object_id, new octree_value() { id = object_id });
            }
        }

        public void remove_value(int object_id) {
            if (subdivided) {


                (int X, int Y, int Z) next_subdiv = (values[object_id].next_subdiv_x, values[object_id].next_subdiv_y, values[object_id].next_subdiv_z);
                values.Remove(object_id);
                nodes[next_subdiv.X, next_subdiv.Y, next_subdiv.Z].remove_value(object_id);


            } else {
                values.Remove(object_id);
            }
        }

        public void draw() {
            if (!subdivided) return;
            foreach(DynamicNode node in nodes) {
                //if (node.count > 0) {
                    Draw3D.cube(node.bounds, Color.Red);
                //}
                node.draw();
            }
        }

        public void decimate() {
            if (!subdivided) return;

            subdivided = false;
            nodes = null;
        }

        public void subdivide() {
            if (subdivided) return;

            subdivided = true;
            nodes = new DynamicNode[2, 2, 2];

            for (byte z = 0; z < 2; z++) {
                for (byte y = 0; y < 2; y++) {
                    for (byte x = 0; x < 2; x++) {


                        var min = bounds.Min;
                        var max = bounds.Max;

                        var half = min + (max - min) / 2;

                        if (x == 0)
                            max.X = half.X;
                        else if (x == 1)
                            min.X = half.X;

                        if (y == 0)
                            max.Y = half.Y;
                        else if (y == 1)
                            min.Y = half.Y;

                        if (z == 0)
                            max.Z = half.Z;
                        else if (z == 1)
                            min.Z = half.Z;


                        nodes[x, y, z] = new DynamicNode(this, min, max);
                        //nodes[x, y, z].color = RNG.similar_color(color, 0.3f);
                    }
                }
            }


            bool hit = false;
            for (int i = 0; i < values.Count; i++) {

                for (byte z = 0; z < 2; z++) {
                    for (byte y = 0; y < 2; y++) {
                        for (byte x = 0; x < 2; x++) {
                            if (!hit && nodes[x,y,z].bounds.Contains(EngineState.world.current_map.game_objects[values[i].id].position) == ContainmentType.Contains) {
                                var id = values[i].id;
                                nodes[x, y, z].values.Add(values[i].id, values[i]);
                                values[i] = new octree_value() { id = id, next_subdiv_x = x, next_subdiv_y = y, next_subdiv_z = z };
                                hit = true;
                                goto bye;
                            }
                        }
                    }
                }
                bye:
                hit = false;
            }

            //if (node.bounds.Contains(EngineState.world.current_map.game_objects[values[i].id].position) == ContainmentType.Contains) {
            //node.values[i] = new octree_value() { id = values[i].id, next_subdiv_x = node.x, next_subdiv_y = node.y, next_subdiv_z = node.z };
            //nodes[node.x, node.y, node.z].values.Add(values[i]);
            //break;
            //}




        }
    }

}
