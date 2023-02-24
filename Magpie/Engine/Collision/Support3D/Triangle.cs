﻿using Magpie.Graphics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine.Collision.Support3D {
    public class Triangle : Shape3D {

        public Vector3 start_point => A;
        public Vector3 center => (A + B+C) / 8f;
        public shape_type shape { get; } = shape_type.tri;

        public Vector3 A;
        public Vector3 B;
        public Vector3 C;

        public Vector3 normal => CollisionHelper.triangle_normal(A, B, C);


        public BoundingBox sweep_bounding_box(Matrix world, Vector3 sweep) {

            if (sweep != Vector3.Zero) {
                return CollisionHelper.BoundingBox_around_points(
                    Vector3.Transform(A, world),
                    Vector3.Transform(B, world),
                    Vector3.Transform(C, world),
                    Vector3.Transform(A + sweep, world),
                    Vector3.Transform(B + sweep, world),
                    Vector3.Transform(C + sweep, world));
            } else return find_bounding_box(world);
        }

        public BoundingBox find_bounding_box(Matrix world) {
            return CollisionHelper.BoundingBox_around_points(
                Vector3.Transform(A, world), 
                Vector3.Transform(B, world), 
                Vector3.Transform(C, world));
        }

        public Triangle() {
            create(1, 1);
        }
        public Triangle(float scale) {
            create(scale, scale);
        }
        public Triangle(float scale_x, float scale_y) {
            create(scale_x, scale_y);
        }

        public void create(float scale_x, float scale_y) {
            A = (Vector3.Up * 0.5f * scale_y);
            B = (Vector3.Left * 0.5f * scale_x) + (Vector3.Down * 0.5f * scale_y);
            C = (Vector3.Right * 0.5f * scale_x) + (Vector3.Down * 0.5f * scale_y);
        }

        public Triangle(Vector3 A, Vector3 B, Vector3 C) {
            this.A = A;
            this.B = B;
            this.C = C;
        }

        public void draw(Matrix world) {
            Draw3D.fill_tri(world, A, B, C, Color.White * 0.9f);

            Draw3D.lines(Color.MonoGameOrange,
                Vector3.Transform(A, world),
                Vector3.Transform(B, world),
                Vector3.Transform(C, world),
                Vector3.Transform(A, world));
        }

        public Vector3 support(Vector3 direction, Vector3 sweep) {
            if (sweep != Vector3.Zero) {
                return Supports.Polyhedron(direction, A,B,C,A+sweep,B+sweep,C+sweep);
            }
            return Supports.Tri(direction, A,B,C);
        }
    }

}
