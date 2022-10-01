using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magpie.Engine;
using Magpie.Engine.Collision.Support3D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Magpie.Graphics.Instancing;

namespace Magpie.Graphics.Particles {
    public class PointCloud {
        public Vector3[] points;
        public InstanceData[] vert_positions;
        float[] distances;
        
        int _point_count;
        public int point_count => _point_count;

        public PointCloud(int points) {
            this.points = new Vector3[points];
            vert_positions = new InstanceData[points];
            distances = new float[points];
            _point_count = points;

            create_random_within_sphere(Vector3.Up * 5f, 3f);
        }

        public void draw_debug() {

            for (int i = 0; i < points.Length; i++) {
                //Draw3D.xyz_cross(points[i], 0.1f, Color.Red);
                //Draw3D.line(points[i], points[i] + (vert_positions[i].normal * 3), Color.DeepPink);
            }
        }

        void create_random_within_sphere(Vector3 center, float radius) {
            for (int i = 0; i < points.Length; i++) {
                points[i] = center + new Vector3(
                    RNG.rng_float_neg_one_to_one * radius,
                    RNG.rng_float_neg_one_to_one * radius,
                    RNG.rng_float_neg_one_to_one * radius);
            }
        }
        public DynamicVertexBuffer instance_buffer;
        Matrix mtmp = Matrix.Identity;
        public void GenerateWorldMatrixBuffer() {
            //for (int i = 0; i < _point_count; i++) {
            int i = 0;
            foreach(Vector3 p in points.OrderByDescending(a => Vector3.Distance(EngineState.camera.position, a))) { 
                mtmp = Matrix.CreateBillboard(p, EngineState.camera.position, EngineState.camera.orientation.Up, EngineState.camera.orientation.Forward);

                mtmp.r1(out vert_positions[i].r1.X, out vert_positions[i].r1.Y, out vert_positions[i].r1.Z, out vert_positions[i].r1.W);
                mtmp.r2(out vert_positions[i].r2.X, out vert_positions[i].r2.Y, out vert_positions[i].r2.Z, out vert_positions[i].r2.W);
                mtmp.r3(out vert_positions[i].r3.X, out vert_positions[i].r3.Y, out vert_positions[i].r3.Z, out vert_positions[i].r3.W);
                mtmp.r4(out vert_positions[i].r4.X, out vert_positions[i].r4.Y, out vert_positions[i].r4.Z, out vert_positions[i].r4.W);

                mtmp = Matrix.Transpose(Matrix.Invert(mtmp));
                mtmp.r1(out vert_positions[i].r1_IT.X, out vert_positions[i].r1_IT.Y, out vert_positions[i].r1_IT.Z, out vert_positions[i].r1_IT.W);
                mtmp.r2(out vert_positions[i].r2_IT.X, out vert_positions[i].r2_IT.Y, out vert_positions[i].r2_IT.Z, out vert_positions[i].r2_IT.W);
                mtmp.r3(out vert_positions[i].r3_IT.X, out vert_positions[i].r3_IT.Y, out vert_positions[i].r3_IT.Z, out vert_positions[i].r3_IT.W);
                mtmp.r4(out vert_positions[i].r4_IT.X, out vert_positions[i].r4_IT.Y, out vert_positions[i].r4_IT.Z, out vert_positions[i].r4_IT.W);


                vert_positions[i].normal = Vector3.Normalize(mtmp.Forward);

                i++;
            }            

            instance_buffer = new DynamicVertexBuffer(EngineState.graphics_device, InstanceDataDec.VertexDeclaration, vert_positions.Length, BufferUsage.WriteOnly);
            instance_buffer.SetData(vert_positions);

            
        }

    }
}
