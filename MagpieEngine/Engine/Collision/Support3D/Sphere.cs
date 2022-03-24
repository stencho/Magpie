using Magpie.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Magpie.Engine.Collision.GJK3D;

namespace Magpie.Engine.Collision.Support3D {
    public class Sphere : shape3D {
        public Vector3 position { get; set; } = Vector3.Up * 2f + (Vector3.Right * 0);
        public float radius { get; set; } = 1f;

        public void draw() {
            Draw3D.sphere(EngineState.graphics_device, position, radius, Color.Red, EngineState.camera.view, EngineState.camera.projection);
        }

        public Vector3 find_point_in_direction(Vector3 direction) {
            return position + (Vector3.Normalize(direction) * (radius));
        }
    }
}
