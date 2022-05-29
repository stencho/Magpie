using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Magpie.Engine.Collision.Support3D {
    class DummySupport : shape3D {
        public Matrix orientation { get; set; } = Matrix.Identity;
        public Vector3 position { get; set; } = Vector3.Zero;

        public Vector3 start_point => Vector3.Zero;

        public shape_type shape => shape_type.dummy;

        public float radius { get; set; } = 0f;

        public void draw() {}

        public BoundingBox find_bounding_box() {
            return new BoundingBox(Vector3.Zero, Vector3.Zero);
        }
    }
}
