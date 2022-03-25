using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Magpie.Engine.Controls;
using static Magpie.Engine.DigitalControlBindings;


namespace Magpie.Engine.Floors {
    public class FloorPlane : Floor {
        public Vector3 position { get; set; } = Vector3.Zero;
        public Vector2 size { get; set; } = Vector2.One * 50f;
        public Matrix orientation { get; set; } = Matrix.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(45f));

        public Vector3 A => position + Vector3.Transform((Vector3.Forward * size.Y * 0.5f) + (Vector3.Left * size.X * 0.5f)  , orientation);
        public Vector3 B => position + Vector3.Transform((Vector3.Forward * size.Y * 0.5f) + (Vector3.Right * size.X * 0.5f) , orientation);
        public Vector3 C => position + Vector3.Transform((Vector3.Backward * size.Y * 0.5f) + (Vector3.Right * size.X * 0.5f), orientation);
        public Vector3 D => position + Vector3.Transform((Vector3.Backward * size.Y * 0.5f) + (Vector3.Left * size.X * 0.5f) , orientation);

        static ushort[] q_indices = { 0, 1, 2, 2, 3, 0 };
        public static VertexPositionNormalTexture[] quad = new VertexPositionNormalTexture[4] {
                new VertexPositionNormalTexture(new Vector3(-1, 1, 0), -Vector3.UnitZ, new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(1, 1, 0), -Vector3.UnitZ, new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3(1, -1, 0), -Vector3.UnitZ, new Vector2(1, 1)),
                new VertexPositionNormalTexture(new Vector3(-1, -1, 0), -Vector3.UnitZ, new Vector2(0, 1))
            };

        IndexBuffer index_buffer;
        VertexBuffer vertex_buffer;

        public FloorPlane() {

            quad = new VertexPositionNormalTexture[4] {
                new VertexPositionNormalTexture(A, -Vector3.UnitZ, new Vector2(0, 0)),
                new VertexPositionNormalTexture(B, -Vector3.UnitZ, new Vector2(1, 0)),
                new VertexPositionNormalTexture(C, -Vector3.UnitZ, new Vector2(1, 1)),
                new VertexPositionNormalTexture(D, -Vector3.UnitZ, new Vector2(0, 1))
            };

            if (index_buffer == null) {
                index_buffer = new IndexBuffer(EngineState.graphics_device, IndexElementSize.SixteenBits, q_indices.Length, BufferUsage.None);
                index_buffer.SetData<ushort>(q_indices);
                vertex_buffer = new VertexBuffer(EngineState.graphics_device, VertexPositionNormalTexture.VertexDeclaration, quad.Length, BufferUsage.None);
                vertex_buffer.SetData<VertexPositionNormalTexture>(quad);
            }


        }

        public void Draw() {
            Draw3D.draw_buffers_diffuse_texture(EngineState.graphics_device, vertex_buffer, index_buffer, Draw3D.tum, Color.White, orientation * Matrix.CreateTranslation(position), EngineState.camera.view, EngineState.camera.projection);
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

        public void draw_depth(DynamicLight light) {
            Draw3D.draw_buffers_depth(light, orientation * Matrix.CreateTranslation(position), vertex_buffer, index_buffer);
        }
    }
}
