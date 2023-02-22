using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Magpie.Engine.Stages;
using Magpie.Graphics;
using Microsoft.Xna.Framework;

namespace Magpie.Engine {
    public class Octree {
        Node[,,] nodes;


        public BoundingBox bounds;


        public Octree(Vector3 min, Vector3 max) {
            bounds = new BoundingBox(min, max);

            nodes = new Node[2,2,2]; 

            for (int z = 0; z < 2; z++) {
                for (int y = 0; y < 2; y++) {
                    for (int x = 0; x < 2; x++) {
                        nodes[x, y, z] = new Node(null, min, max, x, y, z);
                    }
                }
            }

        }
        
        public void draw() {
            Draw3D.cube(bounds, Color.Purple);
            foreach (Node node in nodes) {
                node.draw();
            }
            
        }

    }

    public class Node {
        Node parent;

        Node[,,] nodes;

        bool subdivided = false;
        int x,y,z;

        public BoundingBox bounds;

        public Node(Node parent, Vector3 parent_min, Vector3 parent_max, int x, int y, int z) { 
            this.parent = parent;
            this.x = x; this.y = y; this.z = z;

            var min = parent_min;
            var max = parent_max;
            var half = min + ((max - min) / 2);

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

            this.bounds = new BoundingBox(min,max);

        }

        public void draw() {
            Draw3D.cube(bounds, Color.Red);

            if (subdivided)
            foreach (Node node in nodes) {
                Draw3D.cube(node.bounds, Color.MonoGameOrange);
            }
        }

        public void subdivide () {
            subdivided = true;
            nodes = new Node[2, 2, 2];

            for (int z = 0; z < 2; z++) {
                for (int y = 0; y < 2; y++) {
                    for (int x = 0; x < 2; x++) {
                        nodes[x, y, z] = new Node(this, bounds.Min, bounds.Max, x, y, z);
                    }
                }
            }
        }
    }
}
