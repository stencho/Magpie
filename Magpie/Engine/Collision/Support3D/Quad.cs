using Magpie.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magpie.Engine.Collision;
using Microsoft.Xna.Framework.Graphics;

namespace Magpie.Engine.Collision.Support3D {
    public class Quad : Shape3D {
        public Vector3 start_point => A;
        public Vector3 center => (A + B+C+D) / 4f;
        public shape_type shape { get; } = shape_type.quad;

        public Vector3 A;
        public Vector3 B;
        public Vector3 C;
        public Vector3 D;

        public Vector3 origin => (A + B + C + D) / 4f;

        public VertexBuffer debug_vertex_buffer => null;
        public IndexBuffer debug_index_buffer => null;

        public BoundingBox sweep_bounding_box(Matrix world, Vector3 sweep) {
            if (sweep != Vector3.Zero) {
                return CollisionHelper.BoundingBox_around_BoundingBoxes(
                    find_bounding_box(world),
                    find_bounding_box(world * Matrix.CreateTranslation(sweep))
                );
            } else {
                return find_bounding_box(world);
            }
        }
        public BoundingBox find_bounding_box(Matrix world) {
            return CollisionHelper.BoundingBox_around_points(
                Vector3.Transform(A, world), 
                Vector3.Transform(B, world), 
                Vector3.Transform(C, world), 
                Vector3.Transform(D, world)
                );
        }

        public Quad() {
            create(1, 1);
        }
        public Quad(float scale) {
            create(scale, scale);
        }
        public Quad(float scale_x, float scale_y) {
            create(scale_x, scale_y);
        }
        public Quad(Vector3 A, Vector3 B, Vector3 C, Vector3 D) {
            create(A, B, C, D);
        }

        public void create(float scale_x, float scale_y) {
            A = (Vector3.Left * 0.5f * scale_x) + (Vector3.Forward * 0.5f * scale_y);
            B = (Vector3.Right * 0.5f * scale_x) + (Vector3.Forward * 0.5f * scale_y);
            D = (Vector3.Left * 0.5f * scale_x) + (Vector3.Backward* 0.5f * scale_y);
            C = (Vector3.Right * 0.5f * scale_x) + (Vector3.Backward* 0.5f * scale_y);
        }

        public void create(Vector3 A, Vector3 B, Vector3 C, Vector3 D) {
            this.A = A;
            this.B = B;
            this.C = C;
            this.D = D;
        }

        public void draw(Matrix world) {

            //Draw3D.fill_quad(w, A, B, C, D, Color.White * 0.9f, EngineState.camera.view, EngineState.camera.projection);
            var wA = Vector3.Transform(A, world);
            var wB = Vector3.Transform(B, world);
            var wC = Vector3.Transform(C, world);
            var wD = Vector3.Transform(D, world);

            Draw3D.xyz_cross(wA, 5f, Color.DeepPink);
            Draw3D.xyz_cross(wB, 5f, Color.LightPink);
            Draw3D.xyz_cross(wC, 5f, Color.LightPink);
            Draw3D.xyz_cross(wD, 5f, Color.DeepPink);

            var n = Vector3.Normalize(Vector3.Cross(wA-wB,wA-wC));
            var c = ((wA+wB+wC+wD)/4);

            Draw3D.line(c, c + n, Color.Red);

            Draw3D.lines(Color.LightPink,
                Vector3.Transform(A, world),
                Vector3.Transform(B, world),
                Vector3.Transform(C, world),
                Vector3.Transform(D, world),
                Vector3.Transform(A, world));

            Draw3D.cube(find_bounding_box(world), Color.Magenta);
        }
        public Vector3 support(Vector3 direction, Vector3 sweep) {
            if (sweep != Vector3.Zero) {
                return Supports.Polyhedron(direction, A, B, C, D,
                    A + sweep, B + sweep, C + sweep, D + sweep);
            }
            return Supports.Quad(direction, A, B, C, D);
        }
    }
}
