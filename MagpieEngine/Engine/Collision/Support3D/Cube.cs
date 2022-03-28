using Magpie.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine.Collision.Support3D {
    public class Cube : shape3D {
        public Matrix orientation { get; set; } = Matrix.Identity;
        public Vector3 position { get; set; } = Vector3.Zero;
        public Vector3 start_point => position;
                
        public shape_type shape { get; } = shape_type.cube;

        public Vector3 A => obb.A;
        public Vector3 B => obb.B;
        public Vector3 C => obb.C;
        public Vector3 D => obb.D;
        public Vector3 E => obb.E;
        public Vector3 F => obb.F;
        public Vector3 G => obb.G;
        public Vector3 H => obb.H;
                
        OBB obb;
        public Vector3 half_scale;

        public AABB find_bounding_box() {
            return CollisionHelper.AABB_around_OBB(obb);
        }

        public Cube() {
            obb = new OBB(Vector3.Zero, Vector3.One * 0.5f);
        }
        public Cube(float half_scale) {
            obb = new OBB(Vector3.Zero, Vector3.One * half_scale);
            this.half_scale = Vector3.One * half_scale;
        }
        public Cube(Vector3 half_scale) {
            this.half_scale = half_scale;
            obb = new OBB(Vector3.Zero, half_scale);
        }
        
        public static Vector3 farthest_point(Cube cube, Vector3 direction, out int i) {
            Vector3 pos = Vector3.Zero;

            pos += Vector3.UnitX * (Vector3.Dot(direction, Vector3.UnitX) >= 0f ? cube.half_scale.X : -cube.half_scale.X);
            pos += Vector3.UnitY * (Vector3.Dot(direction, Vector3.UnitY) >= 0f ? cube.half_scale.Y : -cube.half_scale.Y);
            pos += Vector3.UnitZ * (Vector3.Dot(direction, Vector3.UnitZ) >= 0f ? cube.half_scale.Z : -cube.half_scale.Z);

            i = 0;
            if (pos.Z > 0) {
                if (pos.Y <= 0) {
                    if (pos.X <= 0) {
                        i = 6;
                    } else {
                        i = 7;
                    }

                } else if (pos.Y > 0) {
                    if (pos.X <= 0) {
                        i = 4;
                    } else {
                        i = 5;
                    }
                }
            } else if (pos.Z < 0) {
                if (pos.Y <= 0) {
                    if (pos.X <= 0) {
                        i = 2;
                    } else {
                        i = 3;
                    }
                } else {
                    if (pos.X <= 0) {
                        i = 0;
                    } else {
                        i = 1;
                    }
                }
            }

            return pos;
        }
        
        public void draw() {
            Draw3D.cube(EngineState.graphics_device,
                Vector3.Transform(A, orientation * Matrix.CreateTranslation(position)),
                Vector3.Transform(B, orientation * Matrix.CreateTranslation(position)),
                Vector3.Transform(C, orientation * Matrix.CreateTranslation(position)),
                Vector3.Transform(D, orientation * Matrix.CreateTranslation(position)),
                Vector3.Transform(E, orientation * Matrix.CreateTranslation(position)),
                Vector3.Transform(F, orientation * Matrix.CreateTranslation(position)),
                Vector3.Transform(G, orientation * Matrix.CreateTranslation(position)),
                Vector3.Transform(H, orientation * Matrix.CreateTranslation(position)),
                Color.MonoGameOrange, EngineState.camera.view, EngineState.camera.projection);
        }

    }
}
