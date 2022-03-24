using Magpie.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Magpie.Engine.Collision.GJK3D;

namespace Magpie.Engine.Collision.Support3D {
    public class Tetrahedron : shape3D {
        public Vector3 A { get; set; } = Vector3.Zero + (Vector3.Right * 0);
        public Vector3 B { get; set; } = Vector3.Down + Vector3.Left + Vector3.Backward + (Vector3.Right * 0);
        public Vector3 C { get; set; } = Vector3.Down + Vector3.Right + Vector3.Backward + (Vector3.Right * 0);
        public Vector3 D { get; set; } = (Vector3.Down * 0.5f) + Vector3.Forward + (Vector3.Right * 0);

        public void draw() {
            Draw3D.xyz_cross(EngineState.graphics_device, A, 0.3f, Color.Red, EngineState.camera.view, EngineState.camera.projection);
            Draw3D.xyz_cross(EngineState.graphics_device, B, 0.3f, Color.Green, EngineState.camera.view, EngineState.camera.projection);
            Draw3D.xyz_cross(EngineState.graphics_device, C, 0.3f, Color.Blue, EngineState.camera.view, EngineState.camera.projection);
            Draw3D.xyz_cross(EngineState.graphics_device, D, 0.3f, Color.Purple, EngineState.camera.view, EngineState.camera.projection);

            Draw3D.line(EngineState.graphics_device, A, B, Color.Red, EngineState.camera.view, EngineState.camera.projection);
            Draw3D.line(EngineState.graphics_device, B, C, Color.Green, EngineState.camera.view, EngineState.camera.projection);
            Draw3D.line(EngineState.graphics_device, C, A, Color.Blue, EngineState.camera.view, EngineState.camera.projection);

            Draw3D.line(EngineState.graphics_device, A, D, Color.Purple, EngineState.camera.view, EngineState.camera.projection);
            Draw3D.line(EngineState.graphics_device, B, D, Color.Purple, EngineState.camera.view, EngineState.camera.projection);
            Draw3D.line(EngineState.graphics_device, C, D, Color.Purple, EngineState.camera.view, EngineState.camera.projection);

        }

        public Vector3 find_point_in_direction(Vector3 direction) {
            int ind = 0;
            return highest_dot(direction, out ind, A, B, C, D);
        }
    }
}
