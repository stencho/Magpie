using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            public BoundingBox bounds;

            public List<int> contains_objects;

            int path;
            int parent;
            int depth;

            public bool subdivided = false;

            public Node(int path, int parent, int depth) {
                this.path = path;
                this.parent = parent;
                this.depth = depth;
            }
        }

        Vector3 _min, _max, _size;
        float _width, _height, _depth;

        int _subdivisions; public float subdivisions => _subdivisions;


        Dictionary<int, Node> nodes = new Dictionary<int, Node>();

        public Node get_node(int node_path) => nodes[node_path];
        
        public Octree(Vector3 min, Vector3 max, int subdivisions) {
            if (subdivisions < 1 || subdivisions > 7) throw new Exception();

            _min = min;
            _max = max;
            _size = max - min;

            _width = _size.X;
            _height = _size.Y;
            _depth = _size.Z;

            _subdivisions = subdivisions;

            subdivide_all();
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
                            (_min + ((_max / 2) * s)),
                            (_min + ((_max / 2) * s)) + (_size/2f));

                        nodes.Add(tmp_id, new Node(tmp_id, 0, 0));
                        subdivide(tmp_id, 1, pb);

                        //     VVVV
                        //1101 0000 0000 0000 0000 0000 0000
                    }
                }
            }
        }
        internal void subdivide(int id, int current_depth, BoundingBox parent_bounds) {
            if (current_depth > subdivisions) {
                nodes[id].contains_objects = new List<int>();
                return;
            }

            nodes[id].subdivided = true;

            //set first bit of section to 1
            //     V
            //1101 1000 0000 0000 0000 0000 0000 0000
            int tmp_id = id | (1 << (4 * (current_depth)));

            for (int z = 0; z < 2; z++) {
                for (int y = 0; y < 2; y++) {
                    for (int x = 0; x < 2; x++) {
                        int nid = tmp_id;

                        //      VVV
                        //1101 1000 0000 0000 0000 0000 0000 0000
                        if (x > 0) nid |= (1 << (4*current_depth)+1);
                        if (y > 0) nid |= (1 << (4*current_depth)+2);
                        if (z > 0) nid |= (1 << (4*current_depth)+3);

                        Vector3 s = new Vector3(x,y,z);

                        BoundingBox pb = new BoundingBox(
                            (parent_bounds.Min + ((parent_bounds.Max / 2) * s)),
                            (parent_bounds.Min + ((parent_bounds.Max / 2) * s)) + ((parent_bounds.Max - parent_bounds.Min) /2f));

                        nodes.Add(nid, new Node(nid, id, current_depth));
                        subdivide(nid, current_depth + 1, pb);
                        //move on to next section
                        //          VVVV
                        //1101 1010 0000 0000 0000 0000 0000 0000
                    }
                }
            }
        }

        public bool isbitset(int path, int bit) => ((path & (1 << bit)) == 1);

        public bool x_at_depth(int path, int depth) => ((path & (1 << (depth * 4) + 1)) == 1);
        public bool y_at_depth(int path, int depth) => ((path & (1 << (depth * 4) + 2)) == 1);
        public bool z_at_depth(int path, int depth) => ((path & (1 << (depth * 4) + 3)) == 1);

        public bool smallest_node_x_value(int path) => ((path & (1 << (_subdivisions * 4) + 1)) == 1);
        public bool smallest_node_y_value(int path) => ((path & (1 << (_subdivisions * 4) + 2)) == 1);
        public bool smallest_node_z_value(int path) => ((path & (1 << (_subdivisions * 4) + 3)) == 1);

        public int node_count_smallest => _subdivisions * 8;
        public int node_count_total => (int)Math.Pow(8, _subdivisions); 

        #region step in direction
        public int step_left(int path) {
            int tmp_path = path;

            //find depth of path
            int depth = 0;
            for (int s = 0; s < 32; s+=4) {
                if (isbitset(path, s)) depth++;
                else break;
            }

            while (depth > 0) {
                //can move left in this branch, set x at this depth to 0 then exit
                if (x_at_depth(path, depth)) {
                    tmp_path &= (0 << (depth * 4) + 1);
                    break;

                //can't move left in this branch, set x at this depth to 1, then move up
                } else {
                    tmp_path &= (1 << (depth * 4) + 1);
                    depth--;
                }
            }

            return tmp_path;
        }
        public int step_left_at_depth(int path, int depth) {
            int tmp_path = path;

            int dd = depth;
            while (dd > 0) {
                //can move left in this branch, set x at this depth to 0 then exit
                if (x_at_depth(path, dd)) {
                    tmp_path &= (0 << (dd * 4) + 1);
                    break;

                //can't move left in this branch, set x at this depth to 1, then move up
                } else {
                    tmp_path &= (1 << (dd * 4) + 1);
                    dd--;
                }
            }

            return tmp_path;
        }

        public int step_right(int path) {
            int tmp_path = path;

            //find depth of path
            int depth = 0;
            for (int s = 0; s < 32; s += 4) {
                if (isbitset(path, s)) depth++;
                else break;
            }

            while (depth > 0) {
                //can move right in this branch, set x at this depth to 1 then exit
                if (!x_at_depth(path, depth)) {
                    tmp_path &= (1 << (depth * 4) + 1);
                    break;

                //can't move right in this branch, set x at this depth to 0, then move up
                } else {
                    tmp_path &= (0 << (depth * 4) + 1);
                    depth--;
                }
            }

            return tmp_path;
        }
        public int step_right_at_depth(int path, int depth) {
            int tmp_path = path;

            int dd = depth;
            while (dd > 0) {
                //can move right in this branch, set x at this depth to 1 then exit
                if (!x_at_depth(path, dd)) {
                    tmp_path &= (1 << (dd * 4) + 1);
                    break;

                //can't move right in this branch, set x at this depth to 0, then move up
                } else {
                    tmp_path &= (0 << (dd * 4) + 1);
                    dd--;
                }
            }

            return tmp_path;
        }

        public int step_forward(int path) {
            int tmp_path = path;

            int depth = 0;
            for (int s = 0; s < 32; s += 4) {
                if (isbitset(path, s)) depth++;
                else break;
            }

            while (depth > 0) {
                if (!z_at_depth(path, depth)) {
                    tmp_path &= (1 << (depth * 4) + 1);
                    break; 

                } else {
                    tmp_path &= (0 << (depth * 4) + 1);
                    depth--;
                }
            }

            return tmp_path;
        }
        public int step_forward_at_depth(int path, int depth) {
            int tmp_path = path;

            int dd = depth;
            while (dd > 0) {
                if (!z_at_depth(path, dd)) {
                    tmp_path &= (1 << (dd * 4) + 1);
                    break;

                } else {
                    tmp_path &= (0 << (dd * 4) + 1);
                    dd--;
                }
            }

            return tmp_path;
        }

        public int step_backward(int path) {
            int tmp_path = path;

            int depth = 0;
            for (int s = 0; s < 32; s += 4) {
                if (isbitset(path, s)) depth++;
                else break;
            }

            while (depth > 0) {                
                if (z_at_depth(path, depth)) {
                    tmp_path &= (0 << (depth * 4) + 1);
                    break;

                } else {
                    tmp_path &= (1 << (depth * 4) + 1);
                    depth--;
                }
            }

            return tmp_path;
        }
        public int step_backward_at_depth(int path, int depth) {
            int tmp_path = path;

            int dd = depth;
            while (dd > 0) {
                if (z_at_depth(path, dd)) {
                    tmp_path &= (0 << (dd * 4) + 1);
                    break;

                } else {
                    tmp_path &= (1 << (dd * 4) + 1);
                    dd--;
                }
            }

            return tmp_path;
        }

        public int step_up(int path) {

            int tmp_path = path;

            int depth = 0;
            for (int s = 0; s < 32; s += 4) {
                if (isbitset(path, s)) depth++;
                else break;
            }

            while (depth > 0) {
                if (!y_at_depth(path, depth)) {
                    tmp_path &= (1 << (depth * 4) + 1);
                    break;

                } else {
                    tmp_path &= (0 << (depth * 4) + 1);
                    depth--;
                }
            }

            return tmp_path;
        }
        public int step_up_at_depth(int path, int depth) {
            int tmp_path = path;

            int dd = depth;
            while (dd > 0) {
                if (!y_at_depth(path, dd)) {
                    tmp_path &= (1 << (dd * 4) + 1);
                    break;

                } else {
                    tmp_path &= (0 << (dd * 4) + 1);
                    dd--;
                }
            }

            return tmp_path;
        }

        public int step_down(int path) {
            int tmp_path = path;

            int depth = 0;
            for (int s = 0; s < 32; s += 4) {
                if (isbitset(path, s)) depth++;
                else break;
            }

            while (depth > 0) {
                if (y_at_depth(path, depth)) {
                    tmp_path &= (0 << (depth * 4) + 1);
                    break;

                } else {
                    tmp_path &= (1 << (depth * 4) + 1);
                    depth--;
                }
            }

            return tmp_path;
        }
        public int step_down_at_depth(int path, int depth) {
            int tmp_path = path;

            int dd = depth;
            while (dd > 0) {
                if (y_at_depth(path, dd)) {
                    tmp_path &= (0 << (dd * 4) + 1);
                    break;

                } else {
                    tmp_path &= (1 << (dd * 4) + 1);
                    dd--;
                }
            }

            return tmp_path;
        }
        #endregion

        public void update_leaves_within_radius(float radius, Vector3 center) {
            int possibility_mask = 0;
            List<int> leaf_ids = new List<int>();



        }
        static Vector3 info_cam_pos = Vector3.Up + (Vector3.Backward * 1.3f) + (Vector3.Right * 0.9f);
        static Camera info_cam = new Camera(info_cam_pos, Matrix.CreateLookAt(info_cam_pos, Vector3.Zero, Vector3.Up));
        public void draw_info_2D() {
            Draw2D.text("pf", $"{node_count_total} : {node_count_smallest}", Vector2.One * 7, Color.Red);

            var cam = EngineState.camera;
            info_cam.update();
            EngineState.camera = info_cam;

            Draw3D.cube(-Vector3.One / 2, Vector3.One / 2, Color.Pink, Matrix.Identity);


            EngineState.camera = cam;
        }

    }

}
