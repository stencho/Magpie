using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using static Magpie.Engine.Collision.GJK3D;

namespace Magpie.Engine.Collision.Support3D {
    public class Point3D : shape3D {
        public Vector3 position { get; set; } = Vector3.Zero + Vector3.Right * 4;

        public void draw() {
            Draw3D.xyz_cross(EngineState.graphics_device, position, 1f, Color.Red, EngineState.camera.view, EngineState.camera.projection);
        }

        public Vector3 find_point_in_direction(Vector3 direction, out int vert_ID) {
            vert_ID = 0;
            return position;
        }
    }
}
