using Magpie.Engine.Collision.Support3D;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie {
    public class Supports {
        public static Vector3 highest_dot(Vector3[] verts, Vector3 direction, out int index, out float dot) {
            dot = float.MinValue; index = -1;
            Vector3 v = Vector3.Zero;

            for (int i = 0; i < verts.Length; i++) {
                float d = Vector3.Dot(verts[i], direction);

                if (d > dot) {
                    index = i;
                    dot = d;
                    v = verts[i];
                }
            }

            return v;
        }
        
        public static int Polyhedron(ref Vector3 support, Vector3 direction, params Vector3[] verts) {
            int i = 0;
            support = highest_dot(verts, direction, out i, out _);
            return i;
        }

        public static int Line(ref Vector3 support,  Vector3 direction, Vector3 A, Vector3 B) {
            int i = 0;

            if (Vector3.Dot(A, direction) > Vector3.Dot(B, direction)) {
                support = A;
            } else {
                support = B;
                i = 1;
            }

            return i;
        }
        
        public static int Point(ref Vector3 support, Vector3 direction, Vector3 P) {
            support = P;
            return 0;
        }

        public static int Tri(ref Vector3 support,  Vector3 direction, Vector3 A, Vector3 B, Vector3 C) {
            int i = 0; 
            support = highest_dot(new Vector3[3] { A, B, C }, direction, out i, out _);
            return i;
        }

        public static int Quad(ref Vector3 support, Vector3 direction, Vector3 A, Vector3 B, Vector3 C, Vector3 D, Quad data) {
            int i = 0;
            support = highest_dot(new Vector3[4] { A,B,C,D }, direction, out i, out _);
            return i;
        }

        public static int Cube(ref Vector3 support, Vector3 direction, Cube cube) {
            int i = 0;

            support = highest_dot(new Vector3[8] {
                Vector3.Transform(cube.A, Matrix.Invert(cube.orientation)),
                Vector3.Transform(cube.B, Matrix.Invert(cube.orientation)),
                Vector3.Transform(cube.C, Matrix.Invert(cube.orientation)),
                Vector3.Transform(cube.D, Matrix.Invert(cube.orientation)),
                Vector3.Transform(cube.E, Matrix.Invert(cube.orientation)),
                Vector3.Transform(cube.F, Matrix.Invert(cube.orientation)),
                Vector3.Transform(cube.G, Matrix.Invert(cube.orientation)),
                Vector3.Transform(cube.H, Matrix.Invert(cube.orientation)) }, 
                direction, out i, out _);
            
            return i;
        }
    }
}
