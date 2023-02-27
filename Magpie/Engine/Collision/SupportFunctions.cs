using Magpie.Engine;
using Magpie.Engine.Collision;
using Magpie.Engine.Collision.Support3D;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Magpie {
    public class Supports {        
        public static Vector3 Polyhedron(Vector3 direction, params Vector3[] verts) {
            return Math3D.highest_dot(verts, direction, out _);
        }

        public static Vector3 Line(Vector3 direction, Vector3 A, Vector3 B) {
            if (Vector3.Dot(A, direction) > Vector3.Dot(B, direction)) {
                return A;
            } else {
                return B;                
            }            
        }
        
        public static Vector3 Point(Vector3 direction, Vector3 P) {
            return P;
        }

        public static Vector3 Tri(Vector3 direction, Vector3 A, Vector3 B, Vector3 C) {

            return Math3D.highest_dot(new Vector3[3] { A, B, C }, direction, out _);
           
            //return CollisionHelper.triangle_farthest_point(A, B, C, direction);  

            /*
            var AB = B-A;
            var AC = C-A;
            var N = Vector3.Cross(AB, AC);

            float dot = Vector3.Dot(N, A);
            float inv = 1f / Vector3.Dot(N, direction);

            float u = Vector3.Dot(Vector3.Cross(direction, AC),A- C) * inv;
            float v = Vector3.Dot(Vector3.Cross(AB, direction),A- B) * inv;
            float w = 1f - u - v;

            support = w * A + v * B + u * C;
            */
        }

        public static Vector3 Quad(Vector3 direction, Vector3 A, Vector3 B, Vector3 C, Vector3 D) { 
            return Math3D.highest_dot(new Vector3[4] { A,B,C,D }, direction, out _);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Cube(Vector3 direction, Cube cube) {

            return Math3D.highest_dot(new Vector3[8] {
                cube.A,
                cube.B,
                cube.C,
                cube.D,
                cube.E,
                cube.F,
                cube.G,
                cube.H }, 
                direction, out _);            
        }
    }
}
