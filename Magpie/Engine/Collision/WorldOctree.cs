using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Magpie.Engine.Collision.Octrees;
using Magpie.Engine.WorldElements;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Magpie.Engine.Collision {

    public class Octree {
        public class Node {
            BoundingBox _bounds;

            Vector3 _size;
            Vector3 _center_pos;

            void bounding_box(Vector3 min, Vector3 max) {
                _bounds = new BoundingBox(min, max);
                _size = max - min;
                _center_pos = min + (_size / 2);
            }

            public BoundingBox bounds { get => _bounds; set { bounding_box(value.Min, value.Max); } }
            public Vector3 size => _size;
            public Vector3 center => _center_pos;

            public List<int> contains_objects;

            int _path;
            int _parent;
            int _depth;

            public int path => _path;
            public int depth => _depth;

            public bool subdivided = false;

            public Node(int path, int parent, int depth) {
                this._path = path;
                this._parent = parent;
                this._depth = depth;
            }
        }

        Vector3 _min, _max, _size;
        BoundingBox _bounds;
        float _width, _height, _depth;

        int _subdivisions; public float subdivisions => _subdivisions;


        Dictionary<int, Node> nodes = new Dictionary<int, Node>();

        public Node get_node(int node_path) => nodes[node_path];

        public Octree(Vector3 min, Vector3 max, int subdivisions) {
            if (subdivisions < 1 || subdivisions > 7) throw new Exception();

            _min = min;
            _max = max;

            _size = max - min;

            _bounds = new BoundingBox(min, max);

            _width = _size.X;
            _height = _size.Y;
            _depth = _size.Z;

            _subdivisions = subdivisions;

            subdivide_all();
        }

        public ThreadedBindManager binds = new ThreadedBindManager();
        public void update_leaves_within_radius(float radius, Vector3 center) {
            List<int> leaf_ids;

            binds.update();

            if (binds.just_released("t_forward")) {
                walk_test_node = step_forward(walk_test_node);
            }
            if (binds.just_released("t_backward")) {
                walk_test_node = step_backward(walk_test_node);
            }

            if (binds.just_released("t_left")) {
                walk_test_node = step_left(walk_test_node);
            }
            if (binds.just_released("t_right")) {
                walk_test_node = step_right(walk_test_node);
            }

            if (binds.just_released("t_up")) {
                walk_test_node = step_up(walk_test_node);
            }
            if (binds.just_released("t_down")) {
                walk_test_node = step_down(walk_test_node);
            }

            if (binds.just_released("t_upper")) {
                move_up_one_level(ref walk_test_node);
            }

            if (binds.just_released("t_downer")) {


            }

            if (binds.just_released("test")) {
                walk_test_node = walk_test_node_corner;
            }
        }


        public int walk_test_node_corner = 0;        
        public int walk_test_node = 0;

        public Node[] get_all_nodes_at_path(int path) {
            if (path == 0 || !nodes[path].subdivided) return null;
            
            var l = new Node[8];
            int depth = get_path_depth(path) + 1;
            int c = 0;

            for (int z = 0; z < 2; z++) {
                for (int y = 0; y < 2; y++) {
                    for (int x = 0; x < 2; x++) {
                        int current_path = path | (1 << (4 * (depth)));
                        if (x > 0) enable_bit(ref current_path, (4 * (depth)) + 1);
                        if (y > 0) enable_bit(ref current_path, (4 * (depth)) + 2);
                        if (z > 0) enable_bit(ref current_path, (4 * (depth)) + 3);
                        l[c] = nodes[current_path];
                        c++;
                    }
                }
            }
            return l;
        }

        public List<int> raycast_node_intersections(Ray ray) {
            var l = get_all_nodes_at_path(0 | (1 << 0));
            List<int> hits = new List<int>();

            foreach(var node in l) {
                if (node.subdivided) {
                    if (node.bounds.Intersects(ray) != null) {
                        raycast_recurse(ref hits, ray, node.path);
                    }
                } else {
                    if (node.bounds.Intersects(ray) != null) {
                        hits.Add(node.path);
                    }
                }
            }


            return hits;
        }

        public void raycast_recurse(ref List<int> hits, Ray ray, int path) {
            var l = get_all_nodes_at_path(path);

            foreach (var node in l) {
                if (node.subdivided) {
                    if (node.bounds.Intersects(ray) != null) {
                        raycast_recurse(ref hits, ray, node.path);
                    }
                } else {
                    if (node.bounds.Intersects(ray) != null) {
                        hits.Add(node.path);
                    }
                }
            }
        }



        internal void draw_all_layers(int path) {
            int working_path = path;
            int depth = get_path_depth(path);
            int idepth = depth;

            while (depth > 0) {
                Draw3D.cube(nodes[working_path].bounds, Color.ForestGreen);
                move_up_one_level(ref working_path);
                depth--;
            }
        }

        public void draw_nodes() {
            //Draw3D.cube(Vector3.Zero, Vector3.One, Color.Red, Matrix.Identity);

            var ray_nodes = raycast_node_intersections(new Ray(EngineState.camera.position, EngineState.camera.direction * EngineState.camera.far_clip));

            //Draw3D.cube(nodes[ray_nodes[0]].bounds, Color.Red);
            foreach(int n in ray_nodes) {
                draw_all_layers(n);
                Draw3D.cube(nodes[n].bounds, Color.MonoGameOrange);
            }

            Draw3D.cube(_bounds, Color.MonoGameOrange);

            var nn = get_all_nodes_at_path(walk_test_node);

            if (nn == null) {
                Draw3D.cube(nodes[walk_test_node].bounds, Color.ForestGreen);
            } else {

                foreach (Node nn_node in nn) {
                    Draw3D.cube(nn_node.bounds, Color.HotPink);
                }

                draw_all_layers(walk_test_node);


                Draw3D.cube(nodes[walk_test_node].bounds, Color.ForestGreen);
            }


        }

        static Vector2 tl = Vector2.One * 30f;

        public void draw_info_2D() {
            var l = get_all_nodes_at_path(walk_test_node);
            StringBuilder sb = new StringBuilder();

            int c = 0;
            if (l != null) {
                foreach (Node n in l) {
                    sb.Append($"n{binary_string_short(n.path)}");
                    c++;
                    if (c == 4) sb.Append(",\n");
                    else if (c != 8) sb.Append(", ");
                }
            }

            Draw2D.text("pf",
$"{node_count_total} : {node_count_smallest}\n" +
                $"{binary_string(step_left(walk_test_node))}<-{binary_string(walk_test_node)}->{binary_string(step_right(walk_test_node))}\n" +
                $"{sb.ToString()}",
                Vector2.One * 7, Color.HotPink);
        }






        void subdivide_all() {
            BoundingBox parent_bounds = new BoundingBox(_min, _max);

            for (int z = 0; z < 2; z++) {
                for (int y = 0; y < 2; y++) {
                    for (int x = 0; x < 2; x++) {
                        int tmp_id = 0;

                        //set the first section to, say, 1101
                        //VVVV
                        //1101 0000 0000 0000 0000 0000 0000
                        //first bit of each new section always set to 1

                        tmp_id |= (1 << 0);
                        if (x > 0) tmp_id |= (1 << 1);
                        if (y > 0) tmp_id |= (1 << 2);
                        if (z > 0) tmp_id |= (1 << 3);

                        Vector3 s = new Vector3(x,y,z);

                        BoundingBox pb = new BoundingBox(
                            (_min + ((_size / 2) * s)),
                            (_min + ((_size / 2) * s)) + (_size/2f));

                        nodes.Add(tmp_id, new Node(tmp_id, 0, 0));
                        nodes[tmp_id].bounds = pb;
                        subdivide(tmp_id, 1, pb);

                        //     VVVV
                        //1101 0000 0000 0000 0000 0000 0000
                    }
                }
            }
        }
        internal void subdivide(int id, int current_depth, BoundingBox parent_bounds) {
            int tmp_id = id | (1 << (4 * (current_depth)));

            if (current_depth >= subdivisions) {
                if (walk_test_node == 0) walk_test_node = id;
                walk_test_node_corner = walk_test_node;
                nodes[id].contains_objects = new List<int>();
                return;
            }

            nodes[id].subdivided = true;

            //set first bit of section to 1
            //     V
            //1101 1000 0000 0000 0000 0000 0000 0000

            for (int z = 0; z < 2; z++) {
                for (int y = 0; y < 2; y++) {
                    for (int x = 0; x < 2; x++) {
                        int nid = tmp_id;

                        //      VVV
                        //1101 1000 0000 0000 0000 0000 0000 0000
                        if (x > 0) nid |= (1 << (4 * current_depth) + 1);
                        if (y > 0) nid |= (1 << (4 * current_depth) + 2);
                        if (z > 0) nid |= (1 << (4 * current_depth) + 3);

                        Vector3 s = new Vector3(x,y,z);

                        BoundingBox pb = new BoundingBox(
                            (parent_bounds.Min + (((parent_bounds.Max - parent_bounds.Min) / 2) * s)),
                            (parent_bounds.Min + (((parent_bounds.Max - parent_bounds.Min) / 2) * s)) + ((parent_bounds.Max - parent_bounds.Min) /2f));

                        nodes.Add(nid, new Node(nid, id, current_depth));
                        nodes[nid].bounds = pb;
                        subdivide(nid, current_depth + 1, pb);
                        //move on to next section
                        //          VVVV
                        //1101 1010 0000 0000 0000 0000 0000 0000
                    }
                }
            }
        }

        public bool isbitset(int path, int bit) => ((path & (1 << bit)) != 0);

        public string binary_string(int path) {
            StringBuilder sb = new StringBuilder();
            for (int s = 0; s < 32; s += 1) {
                if (isbitset(path, s)) sb.Append("1");
                else sb.Append("0");
            }
            return sb.ToString();
        }

        /// <summary>
        /// simply cuts trailing 0s
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string binary_string_short(int path) {
            StringBuilder sb = new StringBuilder();
            for (int s = 0; s < 32; s += 1) {
                
                if (s % 4 == 0) { if (!isbitset(path, s)) break; }
                if (isbitset(path, s)) sb.Append("1");
                else sb.Append("0");
            }
            return sb.ToString();
        }

        public void enable_bit(ref int path, int bit) => path |= (1 << bit);
        public void disable_bit(ref int path, int bit) => path &= ~(1 << bit);

        public bool x_at_depth(int path, int depth) => ((path & (1 << (depth * 4) + 1)) != 0);
        public bool y_at_depth(int path, int depth) => ((path & (1 << (depth * 4) + 2)) != 0);
        public bool z_at_depth(int path, int depth) => ((path & (1 << (depth * 4) + 3)) != 0);

        public int node_count_smallest => _subdivisions * 8;
        public int node_count_total => nodes.Count;
        public int node_count(int depth) => (int)Math.Pow(8, depth);

        public int get_path_depth(int path) {
            int depth = 0;
            for (int s = 0; s < 32; s += 4) {
                if (isbitset(path, s)) depth++;
                else break;
            }
            depth -= 1;

            if (depth < 0) depth = 0;
            return depth;
        }

        public int move_up_one_level(int path) {
            int p = path;
            int d = get_path_depth(path);

            disable_bit(ref p, (d*4));
            disable_bit(ref p, (d*4)+1);
            disable_bit(ref p, (d*4)+2);
            disable_bit(ref p, (d*4)+3);

            return p;
        }

        public void move_up_one_level(ref int path) {
            path = move_up_one_level(path);            
        }

        #region step in direction
        public int step_left(int path) {
            int tmp_path = path;
            int flipped = 0;

            //get the depth of the path
            int depth = get_path_depth(path);
            int idepth = depth;

            while (depth > 0) {
                //can move left in this branch, set x at this depth to 0 then exit
                if (x_at_depth(path, depth)) {
                    disable_bit(ref tmp_path, (depth * 4) + 1);
                    break;

                    //can't move left in this branch, set x at this depth to 1, then move up
                } else {
                    enable_bit(ref tmp_path, (depth * 4) + 1);
                    flipped++; depth--;
                }
            }

            if (tmp_path == 0 || idepth == flipped) return path;
            return tmp_path;
        }

        public int step_right(int path) {
            int tmp_path = path;
            int flipped = 0;

            int depth = 0;
            depth = get_path_depth(path);
            int idepth  = depth;

            while (depth > 0) {
                //can move right in this branch, set x at this depth to 1 then exit
                if (!x_at_depth(path, depth)) {
                    enable_bit(ref tmp_path, (depth * 4) + 1);
                    break;

                    //can't move right in this branch, set x at this depth to 0, then move up
                } else {
                    disable_bit(ref tmp_path, (depth * 4) + 1);
                    flipped++; depth--;
                }
            }

            if (tmp_path == 0 || idepth == flipped) return path;
            return tmp_path;
        }

        public int step_forward(int path) {
            int tmp_path = path;
            int flipped = 0;

            int depth = 0;
            depth = get_path_depth(path);
            int idepth  = depth;

            while (depth > 0) {
                if (!z_at_depth(path, depth)) {
                    enable_bit(ref tmp_path, (depth * 4) + 3);
                    break;

                } else {
                    disable_bit(ref tmp_path, (depth * 4) + 3);
                    flipped++; depth--;
                }
            }

            if (tmp_path == 0 || idepth == flipped) return path;
            return tmp_path;
        }

        public int step_backward(int path) {
            int tmp_path = path;
            int flipped = 0;

            int depth = 0;
            depth = get_path_depth(path);
            int idepth  = depth;

            while (depth > 0) {
                if (z_at_depth(path, depth)) {
                    disable_bit(ref tmp_path, (depth * 4) + 3);
                    break;

                } else {
                    enable_bit(ref tmp_path, (depth * 4) + 3);
                    flipped++; depth--;
                }
            }

            if (tmp_path == 0 || idepth == flipped) return path;
            return tmp_path;
        }

        public int step_up(int path) {
            int tmp_path = path;
            int flipped = 0;

            int depth = 0;
            depth = get_path_depth(path);
            int idepth  = depth;

            while (depth > 0) {
                if (!y_at_depth(path, depth)) {
                    enable_bit(ref tmp_path, (depth * 4) + 2);
                    break;

                } else {
                    disable_bit(ref tmp_path, (depth * 4) + 2);
                    flipped++; depth--;
                }
            }

            if (tmp_path == 0 || idepth == flipped) return path;
            return tmp_path;
        }

        public int step_down(int path) {
            int tmp_path = path;
            int flipped = 0;

            int depth = 0;
            depth = get_path_depth(path);
            int idepth  = depth;

            while (depth > 0) {
                if (y_at_depth(path, depth)) {
                    disable_bit(ref tmp_path, (depth * 4) + 2);
                    break;

                } else {
                    enable_bit(ref tmp_path, (depth * 4) + 2);
                    flipped++; depth--;
                }
            }

            if (tmp_path == 0 || idepth == flipped) return path;
            return tmp_path;
        }
        #endregion

        #region step in direction, in place
        public void step_left(ref int path) => path = step_left(path);
        public void step_right(ref int path) => path = step_right(path);

        public void step_forward(ref int path) => path = step_forward(path);
        public void step_backward(ref int path) => path = step_backward(path);

        public void step_up(ref int path) => path = step_up(path);
        public void step_down(ref int path) => path = step_down(path);
        #endregion

        #region step in direction, report OOB
        public int step_left(int path, out bool at_edge) {
            int np = step_left(path);
            at_edge = (np == path);
            return np;
        }
        public int step_right(int path, out bool at_edge) {
            int np = step_right(path);
            at_edge = (np == path);
            return np;
        }

        public int step_forward(int path, out bool at_edge) {
            int np = step_forward(path);
            at_edge = (np == path);
            return np;
        }
        public int step_backward(int path, out bool at_edge) {
            int np = step_backward(path);
            at_edge = (np == path);
            return np;
        }

        public int step_up(int path, out bool at_edge) {
            int np = step_up(path);
            at_edge = (np == path);
            return np;
        }
        public int step_down(int path, out bool at_edge) {
            int np = step_down(path);
            at_edge = (np == path);
            return np;
        }

        #endregion

    }
}
