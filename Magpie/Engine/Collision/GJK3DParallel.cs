using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magpie.Engine.Collision.Support3D;
using Magpie.Engine.WorldElements;
using Magpie.Graphics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using static Magpie.GJK;

namespace Magpie.Engine.Collision {
    public class GJK3DParallel {

        public struct gjk_solve {
            public Vector3
            A, B, C, D, E, F,
            T, N;

            public Vector3
            AB, BA, CB, BC, CA, AC,
            DB, BD, DC, CD, DA, AD,
            DD, B0;

            public float U, V;
            public float AS, BS, CS, PP;
            public float
            U_AB, V_AB, U_BC, V_BC, U_CA, V_CA,
            U_BD, V_BD, U_DC, V_DC, U_AD, V_AD;

            public float
            U_ABC, V_ABC, W_ABC, U_ADB, V_ADB, W_ADB,
            U_ACD, V_ACD, W_ACD, U_CBD, V_CBD, W_CBD;

            public float U_ABCD, V_ABCD, W_ABCD, X_ABCD;

            public float denom;
            public float volume;

            public simplex sim;
            public support sup;
            public gjk_result res;

            public bool solved;
        }


        float find_closest(Vector3 closest_to_this, params Vector3[] points) {
            float closest = float.MaxValue;
            Vector3 closest_point = Vector3.Zero;

            foreach (Vector3 v in points) {
                var d = Vector3.Distance(v, closest_to_this);

                if (d < closest) {
                    closest = d;
                    closest_point = v;
                }
            }

            return closest;
        }

        Vector3 choose_better_start_capsule(Capsule capsule, Shape3D other_shape, Matrix w_a, Matrix w_b) {
            switch (other_shape.shape) {
                case shape_type.tri:
                    Triangle t = ((Triangle)other_shape);

                    var c_A = find_closest(Vector3.Transform(capsule.A, w_a * Matrix.Invert(w_b)), t.A, t.B, t.C);
                    var c_B = find_closest(Vector3.Transform(capsule.B, w_a * Matrix.Invert(w_b)), t.A, t.B, t.C);
                    var c_c = find_closest(Vector3.Transform(capsule.A + ((capsule.B - capsule.A) / 2), w_a * Matrix.Invert(w_b)), t.A, t.B, t.C);

                    if (c_c < c_A && c_c < c_B) {
                        return capsule.A + ((capsule.B - capsule.A) / 2);
                    }
                    if (c_A < c_B) {
                        return capsule.A;
                    } else {
                        return capsule.B;
                    }

                default:
                    return Vector3.Zero;
            }
        }
        public gjk_result gjk_intersects(Shape3D shape_A, Shape3D shape_B, Matrix w_a, Matrix w_b) {
            return gjk_intersects(shape_A, shape_B, w_a, w_b, Vector3.Zero, Vector3.Zero);
        }
        public gjk_result gjk_intersects(Shape3D shape_A, Shape3D shape_B, Matrix w_a, Matrix w_b, Vector3 sweep_A, Vector3 sweep_B) {
            if (shape_A == null || shape_B == null) throw new Exception();

            gjk_solve gjk = new gjk_solve();

            Vector3 sa = Vector3.Right;
            Vector3 sb = Vector3.Right;

            int t = 0;

            bool a_swept = (sweep_A != Vector3.Zero);
            bool b_swept = (sweep_B != Vector3.Zero);

            switch (shape_A.shape) {
                case shape_type.capsule:
                    gjk.sup.A = Vector3.Transform(choose_better_start_capsule((Capsule)shape_A, shape_B, w_a, w_b), w_a);                    
                    break;

                default:
                    gjk.sup.A = Vector3.Transform(shape_A.start_point, w_a);
                    break;
            }

            gjk.sup.B = Vector3.Transform(shape_B.start_point, w_b);

            gjk.res.shape_A = shape_A;
            gjk.res.shape_B = shape_B;

            gjk.res.world_A = w_a;
            gjk.res.world_B = w_b;


            do {                 
                t = GJK(ref gjk);

                switch (shape_A.shape) {
                    case shape_type.cube:
                        sa = Supports.Cube(Vector3.Transform(gjk.sup.DA, Matrix.Invert(w_a)), ((Cube)shape_A));
                        break;
                    case shape_type.polyhedron:
                        sa = Supports.Polyhedron(Vector3.Transform(gjk.sup.DA, Matrix.Invert(w_a)), ((Polyhedron)shape_A).verts.ToArray());
                        break;
                    case shape_type.quad:
                        sa = Supports.Quad(Vector3.Transform(gjk.sup.DA, Matrix.Invert(w_a)), ((Quad)shape_A).A, ((Quad)shape_A).B, ((Quad)shape_A).C, ((Quad)shape_A).D);
                        break;
                    case shape_type.tri:
                        sa = Supports.Tri(Vector3.Transform(gjk.sup.DA, Matrix.Invert(w_a)), ((Triangle)shape_A).A, ((Triangle)shape_A).B, ((Triangle)shape_A).C);                        
                        break;
                    case shape_type.capsule:
                        if (a_swept) {
                            sa = Supports.Quad(
                                Vector3.Transform(gjk.sup.DA, Matrix.Invert(w_a)),
                                ((Capsule)shape_A).A, ((Capsule)shape_A).B,
                                ((Capsule)shape_A).A + sweep_A, ((Capsule)shape_A).B + sweep_A);
                        } else {
                            sa = Supports.Line(Vector3.Transform(gjk.sup.DA, Matrix.Invert(w_a)), ((Capsule)shape_A).A, ((Capsule)shape_A).B);
                        }
                        break;
                    //case shape_type.point_capsule:
                    //    gjk.sup.vert_ID_A = Supports.Line(ref sa, Vector3.Transform(gjk.sup.DA, Matrix.Invert(w_a)), ((PointCapsule)shape_A).A, ((PointCapsule)shape_A).B);
                    //    break;
                    case shape_type.line:
                        sa = Supports.Line(Vector3.Transform(gjk.sup.DA, Matrix.Invert(w_a)), ((Line3D)shape_A).A, ((Line3D)shape_A).B);
                        break;
                    case shape_type.sphere:
                        if (a_swept) {
                            sa = Supports.Line(Vector3.Transform(gjk.sup.DA, Matrix.Invert(w_a)), ((Sphere)shape_A).P, ((Sphere)shape_A).P + sweep_A);
                        } else {
                            sa = Supports.Point(Vector3.Transform(gjk.sup.DA, Matrix.Invert(w_a)), ((Sphere)shape_A).P);
                        }
                        break;
                    //case shape_type.point_sphere:
                    //    gjk.sup.vert_ID_A = Supports.Point(ref sa, Vector3.Transform(gjk.sup.DA, Matrix.Invert(w_a)), ((PointSphere)shape_A).P);
                    //    break;
                    case shape_type.dummy: break;
                }

                switch (shape_B.shape) {
                    case shape_type.cube:
                        sb = Supports.Cube(Vector3.Transform(gjk.sup.DB, Matrix.Invert(w_b)), ((Cube)shape_B));
                        break;
                    case shape_type.polyhedron:
                        sb = Supports.Polyhedron(Vector3.Transform(gjk.sup.DB, Matrix.Invert(w_b)), ((Polyhedron)shape_B).verts.ToArray());
                        break;
                    case shape_type.quad:
                        sb = Supports.Quad(Vector3.Transform(gjk.sup.DB, Matrix.Invert(w_b)), ((Quad)shape_B).A, ((Quad)shape_B).B, ((Quad)shape_B).C, ((Quad)shape_B).D);
                        break;
                    case shape_type.tri:
                        sb = Supports.Tri(Vector3.Transform(gjk.sup.DB, Matrix.Invert(w_b)), ((Triangle)shape_B).A, ((Triangle)shape_B).B, ((Triangle)shape_B).C);
                        break;
                    case shape_type.capsule:
                        sb = Supports.Line(Vector3.Transform(gjk.sup.DB, Matrix.Invert(w_b)), ((Capsule)shape_B).A, ((Capsule)shape_B).B);
                        break;
                    //case shape_type.point_capsule:
                    //    gjk.sup.vert_ID_B = Supports.Line(ref sb, gjk.sup.DB, ((PointCapsule)shape_B).A, ((PointCapsule)shape_B).B);
                    //    break;
                    case shape_type.line:
                        sb = Supports.Line(Vector3.Transform(gjk.sup.DB, Matrix.Invert(w_b)), ((Line3D)shape_B).A, ((Line3D)shape_B).B);
                        break;
                    case shape_type.sphere:
                        sb = Supports.Point(Vector3.Transform(gjk.sup.DB, Matrix.Invert(w_b)), ((Sphere)shape_B).P);
                        break;
                    //case shape_type.point_sphere:
                    //    gjk.sup.vert_ID_B = Supports.Point(ref sb, gjk.sup.DB, ((PointSphere)shape_B).P);
                    //    break;
                    case shape_type.dummy: break;
                }

                gjk.sup.A = Vector3.Transform(sa, w_a);
                gjk.sup.B = Vector3.Transform(sb, w_b);

            } while (t == 1);

            gjk_check(ref gjk);

            float a_rad = 0, b_rad = 0;

            switch (shape_A.shape) {
                case shape_type.polyhedron:
                case shape_type.quad:
                case shape_type.tri:
                case shape_type.line:
                    //I sleep
                    break;

                // REAL SHIT?
                case shape_type.capsule:
                    a_rad = ((Capsule)shape_A).radius;
                    break;
                case shape_type.sphere:
                    a_rad = ((Sphere)shape_A).radius;
                    break;
            }

            switch (shape_B.shape) {
                case shape_type.polyhedron:
                case shape_type.quad:
                case shape_type.tri:
                case shape_type.line:
                    break;

                case shape_type.capsule:
                    b_rad = ((Capsule)shape_B).radius;
                    break;
                case shape_type.sphere:
                    b_rad = ((Sphere)shape_B).radius;
                    break;
            }

            if (a_rad + b_rad > gjk.res.distance) {
               // gjk_quadratic_distance_solve((a_rad>0?1:0)*epsilon, (b_rad > 0 ? 1 : 0) * epsilon, gjk, ref gjk.res);
                //gjk.res.hit = true;
            } else {


            }


            //if ((a_rad != 0 || b_rad != 0))
                //gjk_quadratic_distance_solve(a_rad, b_rad, gjk, ref gjk.res);
            gjk.solved = true;

            return gjk.res;
        }
        public void gjk_quadratic_distance_solve(float a_radius, float b_radius, gjk_solve gjk, ref gjk_result res) {
            float radius = a_radius + b_radius;
            float radius2 = radius * radius;

            if (res.distance_squared > epsilon && res.distance_squared > radius2) {

                res.distance_squared -= radius2;

                gjk.N = res.closest_point_B - res.closest_point_A;

                float L2 = Vector3.Dot(gjk.N, gjk.N);
                if (L2 != 0f) {
                    gjk.N *= InvSqrt(L2);
                }

                gjk.DA = gjk.N * a_radius;
                gjk.DB = gjk.N * b_radius;

                res.closest_point_A += gjk.DA;
                res.closest_point_B -= gjk.DB;

            } else {

                Vector3 P = res.closest_point_A + res.closest_point_B;

                res.closest_point_A = P * 0.5f;
                res.closest_point_B = res.closest_point_A;

                res.distance_squared = 0;

                res.hit = true;
            }
        }

        public void gjk_check(ref gjk_solve solve) {
            //res = new result();
            solve.res.iterations = solve.sim.iterations;
            solve.res.hit = solve.sim.hit;

            solve.denom = 0f;
            for (int i = 0; i < solve.sim.count; ++i) {
                solve.denom += solve.sim.bc[i];
            }
            solve.denom = 1f / solve.denom;

            switch (solve.sim.count) {
                default: throw new Exception();

                //you know the drill by now, point, line, tri, tet
                case 1:
                    solve.res.closest_point_A = solve.sim.verts[0].A;
                    solve.res.closest_point_B = solve.sim.verts[0].B;
                    break;

                case 2:
                    solve.AS = solve.denom * solve.sim.bc[0];
                    solve.BS = solve.denom * solve.sim.bc[1];

                    solve.A = solve.sim.verts[0].A * solve.AS;
                    solve.B = solve.sim.verts[1].A * solve.BS;
                    solve.C = solve.sim.verts[0].B * solve.AS;
                    solve.D = solve.sim.verts[1].B * solve.BS;

                    solve.res.closest_point_A = solve.A + solve.B;
                    solve.res.closest_point_B = solve.C + solve.D;
                    break;

                case 3:
                    solve.AS = solve.denom * solve.sim.bc[0];
                    solve.BS = solve.denom * solve.sim.bc[1];
                    solve.CS = solve.denom * solve.sim.bc[2];

                    solve.A = solve.sim.verts[0].A * solve.AS;
                    solve.B = solve.sim.verts[1].A * solve.BS;
                    solve.C = solve.sim.verts[2].A * solve.CS;

                    solve.D = solve.sim.verts[0].B * solve.AS;
                    solve.E = solve.sim.verts[1].B * solve.BS;
                    solve.F = solve.sim.verts[2].B * solve.CS;

                    solve.res.closest_point_A = solve.A + solve.B + solve.C;
                    solve.res.closest_point_B = solve.D + solve.E + solve.F;
                    break;

                case 4:
                    solve.A = solve.sim.verts[0].A * (solve.denom * solve.sim.bc[0]);
                    solve.B = solve.sim.verts[1].A * (solve.denom * solve.sim.bc[1]);
                    solve.C = solve.sim.verts[2].A * (solve.denom * solve.sim.bc[2]);
                    solve.D = solve.sim.verts[3].A * (solve.denom * solve.sim.bc[3]);

                    solve.res.closest_point_A = solve.A + solve.B + solve.C + solve.D;
                    solve.res.closest_point_B = solve.res.closest_point_A;
                    break;
            }

            if (!solve.res.hit) {
                solve.res.distance_squared = Vector3.DistanceSquared(solve.res.closest_point_A, solve.res.closest_point_B);
                solve.res.distance = Vector3.Distance(solve.res.closest_point_A, solve.res.closest_point_B);
            } else {
                solve.res.distance_squared = 0;
                solve.res.distance = 0;
            }

            solve.res.last_simplex = solve.sim;
        }

        public int GJK(ref gjk_solve solve) {

            //drop out if we've passed the max iterations
            if (solve.sim.max_iterations > 0 && solve.sim.iterations >= max_iterations)
                return 0;

            //just starting this test, set things up
            if (solve.sim.count == 0) {
                solve.sim = new simplex {
                    iterations = 0,
                    hit = false,
                    count = 0,
                    max_iterations = max_iterations,
                    distance = float.MaxValue,
                    bc = new float[4],
                    verts = new simplex.vert[4]
                };
            }

            solve.sim.verts[solve.sim.count].A = solve.sup.A;
            solve.sim.verts[solve.sim.count].B = solve.sup.B;
            solve.sim.verts[solve.sim.count].P = solve.sup.B - solve.sup.A;

            solve.sim.bc[solve.sim.count] = 1f;

            solve.sim.count++;

            switch (solve.sim.count) {
                case 1: break;

                case 2:
                    //line simplex

                    solve.A = solve.sim.verts[0].P;
                    solve.B = solve.sim.verts[1].P;

                    solve.AB = solve.B - solve.A; solve.BA = solve.A - solve.B;

                    solve.U = Vector3.Dot(solve.B, solve.AB);
                    solve.V = Vector3.Dot(solve.A, solve.BA);

                    //voronoi region A
                    if (solve.V <= 0f) {
                        solve.sim.bc[0] = 1f; solve.sim.count = 1;
                        break;
                    }
                    //region B
                    if (solve.U <= 0f) {
                        solve.sim.verts[0] = solve.sim.verts[1];
                        solve.sim.bc[0] = 1f; solve.sim.count = 1;
                        break;
                    }

                    solve.sim.bc[0] = solve.U;
                    solve.sim.bc[1] = solve.V;

                    solve.sim.count = 2;

                    break;

                case 3:
                    solve.A = solve.sim.verts[0].P;
                    solve.B = solve.sim.verts[1].P;
                    solve.C = solve.sim.verts[2].P;

                    solve.AB = solve.A - solve.B; solve.BA = solve.B - solve.A;
                    solve.BC = solve.B - solve.C; solve.CB = solve.C - solve.B;
                    solve.CA = solve.C - solve.A; solve.AC = solve.A - solve.C;

                    solve.U_AB = Vector3.Dot(solve.B, solve.BA); solve.V_BC = Vector3.Dot(solve.B, solve.BC);
                    solve.V_AB = Vector3.Dot(solve.A, solve.AB); solve.U_CA = Vector3.Dot(solve.A, solve.AC);
                    solve.U_BC = Vector3.Dot(solve.C, solve.CB); solve.V_CA = Vector3.Dot(solve.C, solve.CA);

                    //region A
                    if (solve.V_AB <= 0f && solve.U_CA <= 0f) {
                        solve.sim.bc[0] = 1f; solve.sim.count = 1;
                        break;
                    }

                    //region B
                    if (solve.U_AB <= 0f && solve.V_BC <= 0f) {
                        solve.sim.verts[0] = solve.sim.verts[1];
                        solve.sim.bc[0] = 1f; solve.sim.count = 1;
                        break;
                    }

                    //region C
                    if (solve.U_BC <= 0f && solve.V_CA <= 0f) {
                        solve.sim.verts[0] = solve.sim.verts[2];
                        solve.sim.bc[0] = 1f; solve.sim.count = 1;
                        break;
                    }

                    //fractional area calculation
                    solve.N = Vector3.Cross(solve.BA, solve.CA);

                    solve.U_ABC = Vector3.Dot(Vector3.Cross(solve.B, solve.C), solve.N);
                    solve.V_ABC = Vector3.Dot(Vector3.Cross(solve.C, solve.A), solve.N);
                    solve.W_ABC = Vector3.Dot(Vector3.Cross(solve.A, solve.B), solve.N);

                    //region AB
                    if (solve.U_AB > 0f && solve.V_AB > 0f && solve.W_ABC <= 0f) {
                        solve.sim.bc[0] = solve.U_AB;
                        solve.sim.bc[1] = solve.V_AB;
                        solve.sim.count = 2;
                        break;
                    }

                    //region BC
                    if (solve.U_BC > 0f && solve.V_BC > 0f && solve.U_ABC <= 0f) {
                        solve.sim.verts[0] = solve.sim.verts[1];
                        solve.sim.verts[1] = solve.sim.verts[2];
                        solve.sim.bc[0] = solve.U_BC;
                        solve.sim.bc[1] = solve.V_BC;
                        solve.sim.count = 2;
                        break;
                    }

                    //region CA
                    if (solve.U_CA > 0f && solve.V_CA > 0f && solve.V_ABC <= 0f) {
                        solve.sim.verts[1] = solve.sim.verts[0];
                        solve.sim.verts[0] = solve.sim.verts[2];
                        solve.sim.bc[0] = solve.U_CA;
                        solve.sim.bc[1] = solve.V_CA;
                        solve.sim.count = 2;
                        break;
                    }

                    //region ABC
                    if (!(solve.U_ABC > 0f && solve.V_ABC > 0f && solve.W_ABC > 0f)) {

                        //throw new Exception("what the fuck man");
                        solve.sim.count = 2;
                        break;
                    }

                    solve.sim.bc[0] = solve.U_ABC;
                    solve.sim.bc[1] = solve.V_ABC;
                    solve.sim.bc[2] = solve.W_ABC;

                    solve.sim.count = 3;
                    break;
                //}

                case 4:
                    //tetrahedron simplex
                    solve.A = solve.sim.verts[0].P;
                    solve.B = solve.sim.verts[1].P;
                    solve.C = solve.sim.verts[2].P;
                    solve.D = solve.sim.verts[3].P;

                    solve.AB = solve.A - solve.B; solve.BA = solve.B - solve.A;
                    solve.BC = solve.B - solve.C; solve.CB = solve.C - solve.B;
                    solve.CA = solve.C - solve.A; solve.AC = solve.A - solve.C;
                    solve.DB = solve.D - solve.B; solve.BD = solve.B - solve.D;
                    solve.DC = solve.D - solve.C; solve.CD = solve.C - solve.D;
                    solve.DA = solve.D - solve.A; solve.AD = solve.A - solve.D;

                    solve.U_AB = Vector3.Dot(solve.B, solve.BA); solve.V_AB = Vector3.Dot(solve.A, solve.AB);
                    solve.U_BC = Vector3.Dot(solve.C, solve.CB); solve.V_BC = Vector3.Dot(solve.B, solve.BC);
                    solve.U_CA = Vector3.Dot(solve.A, solve.AC); solve.V_CA = Vector3.Dot(solve.C, solve.CA);

                    solve.U_BD = Vector3.Dot(solve.D, solve.DB); solve.V_BD = Vector3.Dot(solve.B, solve.BD);
                    solve.U_DC = Vector3.Dot(solve.C, solve.CD); solve.V_DC = Vector3.Dot(solve.D, solve.DC);
                    solve.U_AD = Vector3.Dot(solve.D, solve.DA); solve.V_AD = Vector3.Dot(solve.A, solve.AD);
                    //region A
                    if (solve.V_AB <= 0f && solve.U_CA <= 0f && solve.V_AD <= 0f) {
                        solve.sim.bc[0] = 1f; solve.sim.count = 1;
                        break;
                    }

                    //region B
                    if (solve.U_AB <= 0f && solve.V_BC <= 0f && solve.V_BD <= 0f) {
                        solve.sim.verts[0] = solve.sim.verts[1];
                        solve.sim.bc[0] = 1f; solve.sim.count = 1;
                        break;
                    }

                    //region C
                    if (solve.U_BC <= 0f && solve.V_CA <= 0f && solve.U_DC <= 0f) {
                        solve.sim.verts[0] = solve.sim.verts[2];
                        solve.sim.bc[0] = 1f; solve.sim.count = 1;
                        break;
                    }

                    //region D
                    if (solve.U_BD <= 0f && solve.V_DC <= 0f && solve.U_AD <= 0f) {
                        solve.sim.verts[0] = solve.sim.verts[3];
                        solve.sim.bc[0] = 1f; solve.sim.count = 1;
                        break;
                    }

                    //fractional area calculation
                    solve.N = Vector3.Cross(solve.DA, solve.BA);

                    solve.U_ADB = Vector3.Dot(solve.N, Vector3.Cross(solve.D, solve.B));
                    solve.V_ADB = Vector3.Dot(solve.N, Vector3.Cross(solve.B, solve.A));
                    solve.W_ADB = Vector3.Dot(solve.N, Vector3.Cross(solve.A, solve.D));

                    solve.N = Vector3.Cross(solve.CA, solve.DA);

                    solve.U_ACD = Vector3.Dot(solve.N, Vector3.Cross(solve.C, solve.D));
                    solve.V_ACD = Vector3.Dot(solve.N, Vector3.Cross(solve.D, solve.A));
                    solve.W_ACD = Vector3.Dot(solve.N, Vector3.Cross(solve.A, solve.C));

                    solve.N = Vector3.Cross(solve.BC, solve.DC);

                    solve.U_CBD = Vector3.Dot(solve.N, Vector3.Cross(solve.B, solve.D));
                    solve.V_CBD = Vector3.Dot(solve.N, Vector3.Cross(solve.D, solve.C));
                    solve.W_CBD = Vector3.Dot(solve.N, Vector3.Cross(solve.C, solve.B));

                    solve.N = Vector3.Cross(solve.BA, solve.CA);

                    solve.U_ABC = Vector3.Dot(solve.N, Vector3.Cross(solve.B, solve.C));
                    solve.V_ABC = Vector3.Dot(solve.N, Vector3.Cross(solve.C, solve.A));
                    solve.W_ABC = Vector3.Dot(solve.N, Vector3.Cross(solve.A, solve.B));

                    //test edges

                    //AB
                    if (solve.W_ABC <= 0f && solve.V_ADB <= 0f && solve.U_AB > 0f && solve.V_AB > 0f) {
                        solve.sim.bc[0] = solve.U_AB;
                        solve.sim.bc[1] = solve.V_AB;
                        solve.sim.count = 2;
                        break;
                    }

                    //BC
                    if (solve.U_ABC <= 0f && solve.W_CBD <= 0f && solve.U_BC > 0f && solve.V_BC > 0f) {
                        solve.sim.verts[0] = solve.sim.verts[1];
                        solve.sim.verts[1] = solve.sim.verts[2];
                        solve.sim.bc[0] = solve.U_BC;
                        solve.sim.bc[1] = solve.V_BC;
                        solve.sim.count = 2;
                        break;
                    }

                    //CA
                    if (solve.V_ABC <= 0f && solve.W_CBD <= 0f && solve.U_CA > 0f && solve.V_CA > 0f) {
                        solve.sim.verts[1] = solve.sim.verts[0];
                        solve.sim.verts[0] = solve.sim.verts[2];
                        solve.sim.bc[0] = solve.U_CA;
                        solve.sim.bc[1] = solve.V_CA;
                        solve.sim.count = 2;
                        break;
                    }

                    //DC
                    if (solve.V_CBD <= 0f && solve.U_ACD <= 0f && solve.U_DC > 0f && solve.V_DC > 0f) {
                        solve.sim.verts[0] = solve.sim.verts[3];
                        solve.sim.verts[1] = solve.sim.verts[2];
                        solve.sim.bc[0] = solve.U_DC;
                        solve.sim.bc[1] = solve.V_DC;
                        solve.sim.count = 2;
                        break;
                    }

                    //AD
                    if (solve.V_ACD <= 0f && solve.W_ADB <= 0f && solve.U_AD > 0f && solve.V_AD > 0f) {
                        solve.sim.verts[1] = solve.sim.verts[3];
                        solve.sim.bc[0] = solve.U_AD;
                        solve.sim.bc[1] = solve.V_AD;
                        solve.sim.count = 2;
                        break;
                    }

                    //BD
                    if (solve.U_CBD <= 0f && solve.U_ADB <= 0f && solve.U_BD > 0f && solve.V_BD > 0f) {
                        solve.sim.verts[0] = solve.sim.verts[1];
                        solve.sim.verts[1] = solve.sim.verts[3];
                        solve.sim.bc[0] = solve.U_BD;
                        solve.sim.bc[1] = solve.V_BD;
                        solve.sim.count = 2;
                        break;
                    }

                    //fractional volume calc
                    solve.denom = box(solve.CB, solve.AB, solve.DB);
                    solve.volume = (solve.denom == 0) ? 1f : 1f / solve.denom;

                    solve.U_ABCD = box(solve.C, solve.D, solve.B) * solve.volume;
                    solve.V_ABCD = box(solve.C, solve.A, solve.D) * solve.volume;
                    solve.W_ABCD = box(solve.D, solve.A, solve.B) * solve.volume;
                    solve.X_ABCD = box(solve.B, solve.A, solve.C) * solve.volume;

                    ////////
                    Vector3 PA, PB, PC, PD;
                    PA = solve.sim.verts[0].P * (solve.denom * solve.U_ABCD);
                    PB = solve.sim.verts[1].P * (solve.denom * solve.V_ABCD);
                    PC = solve.sim.verts[2].P * (solve.denom * solve.W_ABCD);
                    PD = solve.sim.verts[3].P * (solve.denom * solve.X_ABCD);
                    
                    Vector3 point = PA + PB + PC + PD;

                    if (Vector3.Dot(point, point) >= epsilon * epsilon) {
                        solve.sim.bc[0] = solve.U_ABCD;
                        solve.sim.bc[1] = solve.V_ABCD;
                        solve.sim.bc[2] = solve.W_ABCD;
                        solve.sim.bc[3] = solve.X_ABCD;
                        solve.sim.count = 4;
                        break;
                        //return 1;
                    }
                    ////////////////

                    //ABC
                    if (solve.X_ABCD <= 0f && solve.U_ABC > 0f && solve.V_ABC > 0f && solve.W_ABC > 0f) {
                        solve.sim.bc[0] = solve.U_ABC;
                        solve.sim.bc[1] = solve.V_ABC;
                        solve.sim.bc[2] = solve.W_ABC;
                        solve.sim.count = 3;
                        break;
                    }

                    //CBD
                    if (solve.U_ABCD <= 0f && solve.U_CBD > 0f && solve.V_CBD > 0f && solve.W_CBD > 0F) {
                        solve.sim.verts[0] = solve.sim.verts[2];
                        solve.sim.verts[2] = solve.sim.verts[3];
                        solve.sim.bc[0] = solve.U_CBD;
                        solve.sim.bc[1] = solve.V_CBD;
                        solve.sim.bc[2] = solve.W_CBD;
                        solve.sim.count = 3;
                        break;
                    }

                    //ACD
                    if (solve.V_ABCD <= 0f && solve.U_ACD > 0f && solve.V_ACD > 0f && solve.W_ACD > 0f) {
                        solve.sim.verts[1] = solve.sim.verts[2];
                        solve.sim.verts[2] = solve.sim.verts[3];
                        solve.sim.bc[0] = solve.U_ACD;
                        solve.sim.bc[1] = solve.V_ACD;
                        solve.sim.bc[2] = solve.W_ACD;
                        solve.sim.count = 3;
                        break;
                    }

                    //ADB
                    if (solve.W_ABCD <= 0f && solve.U_ADB > 0f && solve.V_ADB > 0f && solve.W_ACD > 0f) {
                        solve.sim.verts[2] = solve.sim.verts[1];
                        solve.sim.verts[1] = solve.sim.verts[3];
                        solve.sim.bc[0] = solve.U_ADB;
                        solve.sim.bc[1] = solve.V_ADB;
                        solve.sim.bc[2] = solve.W_ADB;
                        solve.sim.count = 3;
                        break;
                    }

                    //ABCD
                    if (!(solve.U_ABCD > 0f && solve.V_ABCD > 0f && solve.W_ABCD > 0f && solve.X_ABCD > 0f)) {
                        //throw new Exception("what the fuck man");
                        solve.sim.count = 3;
                        break;
                    }

                    solve.sim.bc[0] = solve.U_ABCD;
                    solve.sim.bc[1] = solve.V_ABCD;
                    solve.sim.bc[2] = solve.W_ABCD;
                    solve.sim.bc[3] = solve.X_ABCD;

                    solve.sim.count = 4;

                    break;
            }

            //test whether origin is enclosed by tetrahedron
            if (solve.sim.count == 4) {
                solve.sim.hit = true;
                return 0;
            }

            Vector3 P = Vector3.Zero;
            solve.denom = 0f;

            for (int i = 0; i < solve.sim.count; ++i) {
                solve.denom += solve.sim.bc[i];
            }
            solve.denom = 1f / solve.denom;

            switch (solve.sim.count) {
                //point
                case 1:
                    P = solve.sim.verts[0].P;
                    break;

                //line
                case 2:
                    solve.A = solve.sim.verts[0].P * (solve.denom * solve.sim.bc[0]);
                    solve.B = solve.sim.verts[1].P * (solve.denom * solve.sim.bc[1]);
                    P = solve.A + solve.B;
                    break;

                //triangle
                case 3:
                    solve.A = solve.sim.verts[0].P * (solve.denom * solve.sim.bc[0]);
                    solve.B = solve.sim.verts[1].P * (solve.denom * solve.sim.bc[1]);
                    solve.C = solve.sim.verts[2].P * (solve.denom * solve.sim.bc[2]);
                    P = solve.A + solve.B + solve.C;
                    break;
            }

            solve.PP = Vector3.Dot(P, P);

            if (solve.PP >= solve.sim.distance) return 0;
            solve.sim.distance = solve.PP;

            //change search direction
            solve.DD = Vector3.Zero;

            switch (solve.sim.count) {
                case 1:
                    //point
                    solve.DD = solve.sim.verts[0].P * -1;
                    break;
                case 2:
                    //line
                    solve.BA = solve.sim.verts[1].P - solve.sim.verts[0].P;
                    solve.B0 = solve.sim.verts[1].P * -1;
                    solve.T = Vector3.Cross(solve.BA, solve.B0);
                    solve.DD = Vector3.Cross(solve.T, solve.BA);
                    break;
                case 3:
                    //tri
                    solve.AB = solve.sim.verts[1].P - solve.sim.verts[0].P;
                    solve.AC = solve.sim.verts[2].P - solve.sim.verts[0].P;
                    solve.N = Vector3.Cross(solve.AB, solve.AC);
                    if (Vector3.Dot(solve.N, solve.sim.verts[0].P) <= 0f) {
                        solve.DD = solve.N;
                    } else {
                        solve.DD = solve.N * -1;
                    }
                    break;
            }

            if (Vector3.Dot(solve.DD, solve.DD) < epsilon)
                return 0;

            solve.sup.DA = solve.DD * -1f;
            solve.sup.DB = solve.DD;

            return 1;
        }
    }
}
