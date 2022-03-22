using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magpie.Graphics;
using Microsoft.Xna.Framework;

using static Magpie.Engine.Controls;
using static Magpie.Engine.DigitalControlBindings;


namespace Magpie.Engine.Floors {
    public class FloorPlane : Floor {
        public Vector3 position { get; set; } = Vector3.Zero;
        public Vector2 size { get; set; } = Vector2.One * 50f;
        public Matrix orientation { get; set; } = Matrix.Identity;

        public Vector3 A => position + Vector3.Transform((Vector3.Forward * size.Y * 0.5f) + (Vector3.Left * size.X * 0.5f)  , orientation);
        public Vector3 B => position + Vector3.Transform((Vector3.Forward * size.Y * 0.5f) + (Vector3.Right * size.X * 0.5f) , orientation);
        public Vector3 C => position + Vector3.Transform((Vector3.Backward * size.Y * 0.5f) + (Vector3.Right * size.X * 0.5f), orientation);
        public Vector3 D => position + Vector3.Transform((Vector3.Backward * size.Y * 0.5f) + (Vector3.Left * size.X * 0.5f) , orientation);

        public void Draw() {
            Draw3D.fill_quad(EngineState.graphics_device, Matrix.Identity,
                A,B,C,D,
                Color.White, EngineState.camera.view, EngineState.camera.projection);
            Draw3D.xyz_cross(EngineState.graphics_device, Vector3.Transform(Vector3.Zero, orientation * Matrix.CreateTranslation(position)), 1f, Color.Pink, EngineState.camera.view, EngineState.camera.projection);
        }

        public void Update() {
        }

        public Vector3 get_footing(float X, float Z) {
            throw new NotImplementedException();
        }

        public float get_footing_height(Vector3 pos) {
            Vector3 hit = Vector3.Zero;
            //this feels hacky but fuck it works
            Collision.Raycasting.ray_intersects_quad(pos + (Vector3.Up * float.MaxValue), Vector3.Down, A,B,C,D, out hit, out _);            
            return hit.Y;
        }

        public Vector3 ensure_position_on_floor(Vector3 position) {
            return Vector3.Zero;
        }

        public Vector3 testpos = Vector3.Zero;
        public Vector3 test_A, test_B, test_C, test_D;
        public Vector3 test_t_A, test_t_B, test_t_C, test_t_D;

        public bool within_vertical_bounds(Vector3 pos) {            
            return Math2D.point_within_polygon(
                pos.XZ(),
                A.XZ(),B.XZ(),C.XZ(),D.XZ());
        }
    }
}
