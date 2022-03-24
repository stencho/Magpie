using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using static Magpie.Engine.Collision.GJK3D;

namespace Magpie.Engine.Collision.Support3D {
    public class Triangle : shape3D {
        public Vector3 A { get; set; } = Vector3.Zero;
        public Vector3 B { get; set; } = Vector3.Down + Vector3.Left;
        public Vector3 C { get; set; } = Vector3.Down + Vector3.Right;

        public void draw() {
            Draw3D.xyz_cross(EngineState.graphics_device, A, 0.3f, Color.Red, EngineState.camera.view, EngineState.camera.projection);
            Draw3D.xyz_cross(EngineState.graphics_device, B, 0.3f, Color.Green, EngineState.camera.view, EngineState.camera.projection);
            Draw3D.xyz_cross(EngineState.graphics_device, C, 0.3f, Color.Blue, EngineState.camera.view, EngineState.camera.projection);

            Draw3D.line(EngineState.graphics_device, A, B, Color.Red, EngineState.camera.view, EngineState.camera.projection);
            Draw3D.line(EngineState.graphics_device, B, C, Color.Green, EngineState.camera.view, EngineState.camera.projection);
            Draw3D.line(EngineState.graphics_device, C, A, Color.Blue, EngineState.camera.view, EngineState.camera.projection);
        }

        public Vector3 find_point_in_direction(Vector3 direction) {
            int ind = 0;
            return highest_dot(direction, out ind, A, B, C);
        }
    }
}
