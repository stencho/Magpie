using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magpie.Engine.Collision;
using Magpie.Engine.Collision.Support3D;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Magpie.Engine.Controls;
using static Magpie.Engine.DigitalControlBindings;


namespace Magpie.Engine.Brushes {
    public class FloorPlane : Brush {
        public BrushType type => BrushType.PLANE;

        public Vector3 position { get; set; } = Vector3.Zero;
        public Vector2 size { get; set; } = Vector2.One * 50f;
        public Matrix orientation { get; set; } = Matrix.Identity;
        public Matrix world => orientation * Matrix.CreateTranslation(position);

        public Vector3 A => (Vector3.Forward * size.Y * 0.5f) + (Vector3.Left * size.X * 0.5f)  ;
        public Vector3 B => (Vector3.Forward * size.Y * 0.5f) + (Vector3.Right * size.X * 0.5f) ;
        public Vector3 C => (Vector3.Backward * size.Y * 0.5f) + (Vector3.Right * size.X * 0.5f);
        public Vector3 D => (Vector3.Backward * size.Y * 0.5f) + (Vector3.Left * size.X * 0.5f) ;

        public Vector3 center => (A + B + C + D) / 4f;

        public Shape3D collision { get; set; }

        static ushort[] q_indices = { 0, 1, 2, 2, 3, 0 };
        public static VertexPositionNormalTexture[] quad = new VertexPositionNormalTexture[4] {
                new VertexPositionNormalTexture(new Vector3(-1, 1, 0) , -Vector3.UnitZ, new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(1, 1, 0)  , -Vector3.UnitZ, new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3(1, -1, 0) , -Vector3.UnitZ, new Vector2(1, 1)),
                new VertexPositionNormalTexture(new Vector3(-1, -1, 0), -Vector3.UnitZ, new Vector2(0, 1))
            };

        public IndexBuffer index_buffer { get; set; }
        public VertexBuffer vertex_buffer { get; set; }

        public string texture { get; set; } =  "zerocool_sharper";

        public Vector3 movement_vector { get; set; } = Vector3.Zero;
        public Vector3 final_position { get; set; }

        public BoundingBox bounds => throw new NotImplementedException();

        public float distance_to_camera => throw new NotImplementedException();

        public SceneRenderInfo render_info { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public FloorPlane() {

            quad = new VertexPositionNormalTexture[4] {
                new VertexPositionNormalTexture(A, Vector3.Up, new Vector2(0, 0)),
                new VertexPositionNormalTexture(B, Vector3.Up, new Vector2(1, 0)),
                new VertexPositionNormalTexture(C, Vector3.Up, new Vector2(1, 1)),
                new VertexPositionNormalTexture(D, Vector3.Up, new Vector2(0, 1))
            };

            if (index_buffer == null) {
                index_buffer = new IndexBuffer(EngineState.graphics_device, IndexElementSize.SixteenBits, q_indices.Length, BufferUsage.None);
                index_buffer.SetData<ushort>(q_indices);
                vertex_buffer = new VertexBuffer(EngineState.graphics_device, VertexPositionNormalTexture.VertexDeclaration, quad.Length, BufferUsage.None);
                vertex_buffer.SetData<VertexPositionNormalTexture>(quad);
            }

            collision = new Quad(size.X, size.Y);
            collision.radius = 0.02f;

        }

        public void Update() {
            // bounds = CollisionHelper.find_bounding_box_around_points(A, B, C, D);
            //bounds = BoundingBox.CreateFromPoints(new Vector3[] {A,B,C,D});
            collision.position = position;
            collision.orientation = orientation;
        }

        public Vector3 get_footing(float X, float Z) {
            throw new NotImplementedException();
        }

        //add normal to this
        public float get_footing_height(Vector3 pos) {
            Vector3 hit = Vector3.Zero;
            //this feels hacky but fuck it works
            Collision.Raycasting.ray_intersects_quad(pos + (Vector3.Up * float.MaxValue), Vector3.Down, A,B,C,D, out hit, out _);            
            return hit.Y;
        }

        public Vector3 ensure_position_on_floor(Vector3 position) {
            return Vector3.Zero;
        }

        public bool within_vertical_bounds(Vector3 pos) {            
            return Math2D.point_within_polygon(
                pos.XZ(),
                A.XZ(),B.XZ(),C.XZ(),D.XZ());
        }

        public void debug_draw() {
            //Draw3D.cube((bounds.Min + bounds.Max) / 2, (bounds.Max - bounds.Min) / 2f, Color.MediumPurple, Matrix.Identity, EngineState.camera.view, EngineState.camera.projection);
            collision.draw();
        }
    }
}
