using Magpie.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine.Collision.Support3D {
    public class Polyhedron : shape3D {
        public Matrix orientation { get; set; } = Matrix.Identity;
        public Vector3 position { get; set; } = Vector3.Zero;
        public Vector3 start_point => verts[0];
        
        public shape_type shape { get; } = shape_type.polyhedron;

        public List<Vector3> verts;
        
        public AABB find_bounding_box() {
            return new AABB();
        }

        public Polyhedron(params Vector3[] points) {
            if (points.Length < 1) throw new Exception();

            verts.AddRange(points);
        }

        public void draw() {
            foreach (Vector3 point in verts) {
                Draw3D.xyz_cross(Vector3.Transform(position + point, orientation), 0.1f, Color.Red);
            }
        }

    }
}
