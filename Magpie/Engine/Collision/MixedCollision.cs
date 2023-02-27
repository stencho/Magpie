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
    public class MixedCollision {

        const int max_iterations = 30;

        public enum simplex_stage {
            empty = -1,
            point=0,
            line=1,
            triangle=2,
            tetrahedron=3
        }


        public struct gjk_support {
            public Vector3 A_support;
            public Vector3 B_support;

            public Vector3 P;

            public float barycentric;

            public gjk_support() {
                A_support = Vector3.Zero;
                B_support = Vector3.Zero;
                P = Vector3.Zero;
                barycentric = 0f;
            }

            public gjk_support(Vector3 a, Vector3 b, Vector3 p) {
                A_support = a;
                B_support = b;
                P = p;
                barycentric = 0f;
            }
        }

        public enum spoint { A=0, B=1, C=2, D=3 }
        public struct gjk_simplex {

            public Vector3 direction;

            public Vector3 closest_A = Vector3.Zero;
            public Vector3 closest_B = Vector3.Zero;

            public simplex_stage stage = simplex_stage.empty;

            public Matrix A_transform;
            public Matrix B_transform;

            public gjk_support[] supports = new gjk_support[4];

            public bool hit;

            public int iteration = 0;

            public string early_exit_reason;

            internal int A_index => (int)stage;
            internal int B_index => (int)stage - 1;
            internal int C_index => (int)stage - 2;
            internal int D_index => (int)stage - 3;

            public Vector3 A => supports[A_index].P;
            public Vector3 B => supports[B_index].P;
            public Vector3 C => supports[C_index].P;
            public Vector3 D => supports[D_index].P;

            public float A_bary => supports[A_index].barycentric;
            public float B_bary => supports[B_index].barycentric;
            public float C_bary => supports[C_index].barycentric;
            public float D_bary => supports[D_index].barycentric;

            public gjk_support A_support => supports[A_index];
            public gjk_support B_support => supports[B_index];
            public gjk_support C_support => supports[C_index];
            public gjk_support D_support => supports[D_index];


            public Vector3 AO => -A;
            public Vector3 BO => -B;
            public Vector3 CO => -C;
            public Vector3 DO => -D;

            public Vector3 AB => B - A;
            public Vector3 AC => C - A;
            public Vector3 AD => D - A;
            public Vector3 BA => A - B;
            public Vector3 BC => C - B;
            public Vector3 BD => D - B;
            public Vector3 CD => D - C;
            public Vector3 DB => B - D;
            public Vector3 ABC => Vector3.Cross(AB, AC);
            public Vector3 ABD => Vector3.Cross(AB, AD);
            public Vector3 ADB => Vector3.Cross(AD, AB);
            public Vector3 ACD => Vector3.Cross(AC, AD);
            public Vector3 BCD => Vector3.Cross(BD, BC);

            public void add_new_point(Vector3 A_sup, Vector3 B_sup) {

                if ((int)stage < (int)simplex_stage.tetrahedron)
                    stage = (simplex_stage)((int)stage + 1);
                else
                    return;

                var P = A_sup - B_sup;
                supports[(int)stage] = new gjk_support(A_sup, B_sup, P);

            }

            int spoint_index(spoint p) {
                switch (p) {
                    case spoint.A:
                        return A_index;
                    case spoint.B:
                        return B_index;
                    case spoint.C:
                        return C_index;
                    case spoint.D:
                        return D_index;
                }
                return -1;
            }

            Vector3 spoint_value(spoint p) {
                switch (p) {
                    case spoint.A:
                        return supports[A_index].P;
                    case spoint.B:
                        return supports[B_index].P;
                    case spoint.C:
                        return supports[C_index].P;
                    case spoint.D:
                        return supports[D_index].P;
                }
                return Vector3.Zero;
            }

            float spoint_bary(spoint P) {
                switch (P) {
                    case spoint.A:
                        return supports[A_index].barycentric;
                    case spoint.B:
                        return supports[B_index].barycentric;
                    case spoint.C:
                        return supports[C_index].barycentric;
                    case spoint.D:
                        return supports[D_index].barycentric;
                }
                return 0f;
            }

            public void set_bary(spoint P, float bary) {
                switch (P) {
                    case spoint.A:
                        supports[A_index].barycentric = bary;
                        break;
                    case spoint.B:
                        supports[B_index].barycentric = bary;
                        break;
                    case spoint.C:
                        supports[C_index].barycentric = bary;
                        break;
                    case spoint.D:
                        supports[D_index].barycentric = bary;
                        break;
                }
            }

            public float get_denom() {
                float d = 0f;
                for (int i = 0; i < (int)stage; ++i) {
                    d += supports[i].barycentric;
                }
                return d;
            }

            public void move_to_stage(spoint A) {
                var s = new gjk_support[4];
                s[0] = supports[spoint_index(A)];
                supports = s;
                stage = simplex_stage.point;

            }
            public void move_to_stage(spoint A, spoint B) {
                var s = new gjk_support[4];
                s[1] = supports[spoint_index(A)];
                s[0] = supports[spoint_index(B)];
                supports = s;
                stage = simplex_stage.line;

            }
            public void move_to_stage(spoint A, spoint B, spoint C) {
                var s = new gjk_support[4];
                s[2] = supports[spoint_index(A)];
                s[1] = supports[spoint_index(B)];
                s[0] = supports[spoint_index(C)];
                supports = s;
                stage = simplex_stage.triangle;
            }

            public bool same_dir_as_AO(Vector3 P) {
                return Math3D.same_dir(P, AO);
            }


            public gjk_simplex() {
                A_transform = Matrix.Identity;
                B_transform = Matrix.Identity;

                direction = Vector3.Zero;

                early_exit_reason = "";
                hit = false;
            }

            public gjk_simplex copy() {
                return new gjk_simplex() {
                    stage = stage,

                    A_transform = A_transform,
                    B_transform = B_transform,

                    closest_A = closest_A,
                    closest_B = closest_B,

                    direction = direction,

                    early_exit_reason = early_exit_reason,
                    hit = hit,
                    iteration = iteration,
                    supports = supports
                };
            }

            public bool farthest_tet(spoint P) {
                if (stage != simplex_stage.tetrahedron) return false;
                var pos = spoint_value(P);

                for (int i = 0; i < 4; i++) {
                    if (i == spoint_index(P))
                        continue;

                    if (!Math3D.same_dir(pos - supports[i].P, pos)) {
                        return false;
                    }

                }

                return true;
            }

            public string get_info() {
                StringBuilder sb = new StringBuilder();

                if ((int)stage >= 0)
                    sb.Append($"[A] {A.ToXString()} [b] {supports[A_index].barycentric}\n");
                if ((int)stage >= 1)
                    sb.Append($"[B] {B.ToXString()} [b] {supports[B_index].barycentric}\n");
                if ((int)stage >= 2)
                    sb.Append($"[C] {C.ToXString()} [b] {supports[C_index].barycentric}\n");
                if ((int)stage >= 3)
                    sb.Append($"[D] {D.ToXString()} [b] {supports[D_index].barycentric}\n");

                return sb.ToString();

            }

            public void draw() {
                Draw3D.xyz_cross(A, 0.2f, Color.Red);
                if ((int)stage > 0)
                    Draw3D.xyz_cross(B, 0.2f, Color.Green);
                if ((int)stage > 1)
                    Draw3D.xyz_cross(C, 0.2f, Color.Blue);
                if ((int)stage > 2)
                    Draw3D.xyz_cross(D, 0.2f, Color.Yellow);

                var mid = A;

                switch (stage) {
                    case simplex_stage.point: break;

                    case simplex_stage.line:
                        mid = (A + B) / 2f;

                        Draw3D.line(A, B, Color.HotPink);

                        Draw3D.line(
                            supports[(int)spoint.A].A_support,
                            supports[(int)spoint.B].A_support,
                            Color.HotPink);
                        Draw3D.line(
                            supports[(int)spoint.A].B_support,
                            supports[(int)spoint.B].B_support,
                            Color.HotPink);

                        break;

                    case simplex_stage.triangle:
                        mid = (A + B + C) / 3f;

                        Draw3D.lines(Color.HotPink, A, B, C, A);

                        Draw3D.lines(Color.HotPink,
                            supports[(int)spoint.A].A_support,
                            supports[(int)spoint.B].A_support,
                            supports[(int)spoint.C].A_support,
                            supports[(int)spoint.A].A_support);

                        Draw3D.lines(Color.HotPink,
                            supports[(int)spoint.A].B_support,
                            supports[(int)spoint.B].B_support,
                            supports[(int)spoint.C].B_support,
                            supports[(int)spoint.A].B_support);
                        break;
                    case simplex_stage.tetrahedron:
                        mid = (B + C + D) / 3f;

                        Draw3D.lines(Color.HotPink, C, B, D, C);

                        Draw3D.line(B, A, Color.Purple);
                        Draw3D.line(C, A, Color.Purple);
                        Draw3D.line(D, A, Color.Purple);


                        Draw3D.lines(Color.HotPink,
                            supports[(int)spoint.C].A_support,
                            supports[(int)spoint.B].A_support,
                            supports[(int)spoint.D].A_support,
                            supports[(int)spoint.C].A_support);

                        Draw3D.lines(Color.HotPink,
                            supports[(int)spoint.C].B_support,
                            supports[(int)spoint.B].B_support,
                            supports[(int)spoint.D].B_support,
                            supports[(int)spoint.C].B_support);


                        Draw3D.line(supports[(int)spoint.B].A_support, supports[(int)spoint.A].A_support, Color.Purple);
                        Draw3D.line(supports[(int)spoint.C].A_support, supports[(int)spoint.A].A_support, Color.Purple);
                        Draw3D.line(supports[(int)spoint.D].A_support, supports[(int)spoint.A].A_support, Color.Purple);

                        Draw3D.line(supports[(int)spoint.B].B_support, supports[(int)spoint.A].B_support, Color.Purple);
                        Draw3D.line(supports[(int)spoint.C].B_support, supports[(int)spoint.A].B_support, Color.Purple);
                        Draw3D.line(supports[(int)spoint.D].B_support, supports[(int)spoint.A].B_support, Color.Purple);


                        var m = (A + B + C) / 3f;
                        Draw3D.line(m, m + Vector3.Normalize(ABC), Color.Red);

                        m = (A + D + B) / 3f;
                        Draw3D.line(m, m + Vector3.Normalize(ADB), Color.Green);

                        m = (A + C + D) / 3f;
                        Draw3D.line(m, m + Vector3.Normalize(ACD), Color.Blue);


                        break;
                }

                Draw3D.arrow(mid, mid + direction, 0.2f, Color.HotPink);

            }
        }

        public struct collision_result {
            public int id_A, id_B;

            public float distance = float.MaxValue;
            public float distance_lowest_dot = float.MaxValue;
            public float penetration;

            public bool intersects;

            public List<gjk_simplex> simplex_list = new List<gjk_simplex>();
            public int draw_simplex;
            public bool draw_all_supports = true;

            public bool save_simplices = true;

            public void save_simplex(ref gjk_simplex simplex) {
                if (!save_simplices) return;

                simplex_list.Add(simplex.copy());
                simplex.early_exit_reason = "";
            }
            public void save_simplex(gjk_simplex simplex, string reason) {
                if (!save_simplices) return;

                var gs = simplex.copy();
                gs.early_exit_reason = reason;
                simplex_list.Add(gs);
            }

            public collision_result() {
                distance = float.MaxValue;
                penetration = 0;

                intersects = false;

                simplex_list = new List<gjk_simplex>();
                draw_simplex = 0;

                id_A = -1;
                id_B = -1;
            }

            public void set_draw_simplex(int simplex) {
                draw_simplex = simplex;
            }

            public void draw(Vector3 world_pos) {
                if (simplex_list == null) return;

                if (simplex_list != null && draw_simplex > -1 && draw_simplex < simplex_list.Count) {
                    gjk_simplex simplex = simplex_list[draw_simplex];
                    Draw3D.text_3D(EngineState.spritebatch,
                        $"iter {simplex.iteration} | {draw_simplex + 1}/{simplex_list.Count} [{simplex.stage.ToString()}] {(intersects ? "[hit]" : "")}\n" +
                        $"{simplex.early_exit_reason}\n" +
                        $"[dir] {simplex.direction.ToXString()}\n" +
                        $"[dist] {Vector3.Distance(simplex.closest_A, simplex.closest_B)} [{distance}]\n" +
                        $"[denom] {simplex.get_denom()}\n" +
                        $"{simplex.get_info()}",

                        "pf", world_pos + Vector3.Down * 3f, EngineState.camera.direction, 1f, Color.Black);

                    //Draw3D.line(world_pos, world_pos + (simplex.direction) * 0.5f, Color.HotPink);
                    //Draw3D.arrow(world_pos + simplex.A, world_pos + simplex.B, 0.1f, Color.HotPink);

                    simplex.draw();

                    Draw3D.xyz_cross(simplex.closest_A, 0.5f, Color.Red);
                    Draw3D.xyz_cross(simplex.closest_B, 0.5f, Color.GreenYellow);
                }
                if (draw_all_supports && simplex_list.Count > 1) {
                    int pc = 0;
                    for (int ci = 0; ci < simplex_list.Count; ci++) {
                        gjk_simplex s = simplex_list[ci];
                        switch (s.stage) {
                            case simplex_stage.line: pc += 2; break;
                            case simplex_stage.triangle: pc += 3; break;
                            case simplex_stage.tetrahedron: pc += 3; break;
                        }
                    }
                    float col_multi = 0f;
                    for (int i = 0; i < simplex_list.Count; i++) {
                        gjk_simplex s = simplex_list[i];

                        switch (s.stage) {
                            case simplex_stage.line:
                                Draw3D.line(s.supports[s.A_index].A_support, s.supports[s.B_index].A_support, Color.ForestGreen);
                                Draw3D.line(s.supports[s.A_index].B_support, s.supports[s.B_index].B_support, Color.ForestGreen);
                                break;
                            case simplex_stage.triangle:
                                Draw3D.lines(Color.ForestGreen, s.supports[s.A_index].A_support, s.supports[s.B_index].A_support, s.supports[s.C_index].A_support);
                                Draw3D.lines(Color.ForestGreen, s.supports[s.A_index].B_support, s.supports[s.B_index].B_support, s.supports[s.C_index].B_support);
                                break;
                            case simplex_stage.tetrahedron:
                                Draw3D.lines(Color.ForestGreen, s.supports[s.A_index].A_support, s.supports[s.B_index].A_support, s.supports[s.C_index].A_support, s.supports[s.D_index].A_support);
                                Draw3D.lines(Color.ForestGreen, s.supports[s.A_index].B_support, s.supports[s.B_index].B_support, s.supports[s.C_index].B_support, s.supports[s.D_index].B_support);
                                break;
                        }
                    }
                }
            }

        }



        public static collision_result intersects(Shape3D shape_A, Shape3D shape_B, Matrix w_a, Matrix w_b) {
            collision_result result = new collision_result();

            gjk_simplex simplex = new gjk_simplex();

            Vector3 scale_a; Quaternion rot_a;
            Vector3 scale_b; Quaternion rot_b;

            w_a.Decompose(out scale_a, out rot_a, out _);
            w_b.Decompose(out scale_b, out rot_b, out _);

            simplex.A_transform = Matrix.CreateScale(scale_a) * Matrix.CreateFromQuaternion(rot_a);
            simplex.B_transform = Matrix.CreateScale(scale_b) * Matrix.CreateFromQuaternion(rot_b);

            simplex.direction = w_a.Translation - w_b.Translation;

            simplex.add_new_point(
                Vector3.Transform(shape_A.support(Vector3.Transform(simplex.direction, Matrix.Invert(simplex.A_transform)), Vector3.Zero), (w_a)),
                Vector3.Transform(shape_B.support(Vector3.Transform(-simplex.direction, Matrix.Invert(simplex.B_transform)), Vector3.Zero), (w_b)));

            result.save_simplex(simplex, "Begin");

            simplex.set_bary(spoint.A, 1f);

            simplex.direction = simplex.AO;

            int iteration = 1;

            while (iteration < max_iterations) {
                simplex.add_new_point(
                    Vector3.Transform(shape_A.support(Vector3.Transform(simplex.direction, Matrix.Invert(simplex.A_transform)), Vector3.Zero), (w_a)),
                    Vector3.Transform(shape_B.support(Vector3.Transform(-simplex.direction, Matrix.Invert(simplex.B_transform)), Vector3.Zero), (w_b)));

                simplex.iteration = iteration;

                if (simplex.stage == simplex_stage.line) { // *** LINE ***
                    result.save_simplex(simplex, "Create Line");

                    if (CollisionHelper.line_closest_point(simplex.A, simplex.B, Vector3.Zero).Length() <= Math3D.epsilon) {
                        result.intersects = true;
                        result.save_simplex(simplex, "Hit on Line");
                        break;
                    }

                    //origin between A and B
                    if (simplex.same_dir_as_AO(simplex.AB)) {
                        simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AB, simplex.AO), simplex.AB);

                        result.save_simplex(simplex, "Origin betweeen A and B");


                    } else {
                        //simplex.move_to_stage(spoint.A);
                        simplex.direction = simplex.AO;

                        result.save_simplex(simplex, "Origin past A");
                    }

                    simplex.set_bary(spoint.A, Vector3.Dot(simplex.B, simplex.BA));
                    simplex.set_bary(spoint.B, Vector3.Dot(simplex.A, simplex.AB));

                    ///////////////////////////////////////////////////////////

                } else if (simplex.stage == simplex_stage.triangle) { // *** TRIANGLE ***
                    result.save_simplex(simplex, "Create Triangle");

                    //On the ABC x AC plane, so origin could be closest to either AC or A
                    if (simplex.same_dir_as_AO(Vector3.Cross(simplex.ABC, simplex.AC))) {

                        if (simplex.same_dir_as_AO(simplex.AC)) {
                            simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AC, simplex.AO), simplex.AC);
                            simplex.move_to_stage(spoint.A, spoint.C);

                            result.save_simplex(simplex, "AC ->");

                        } else {
                            if (simplex.same_dir_as_AO(simplex.AB)) {
                                simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AB, simplex.AO), simplex.AB); //simplex.AO;
                                simplex.move_to_stage(spoint.A, spoint.B);

                                result.save_simplex(simplex, "AB1 ->");

                            } else {
                                simplex.direction = simplex.AO;
                                simplex.move_to_stage(spoint.A);

                                result.save_simplex(simplex, "A1 ->");
                            }

                        }
                    } else {
                        //On the AB x ABC plane, so we're either on AB or A
                        if (simplex.same_dir_as_AO(Vector3.Cross(simplex.AB, simplex.ABC))) {
                            if (simplex.same_dir_as_AO(simplex.AB)) {
                                simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AB, simplex.AO), simplex.AB); //simplex.AO;

                                simplex.move_to_stage(spoint.A, spoint.B);

                                result.save_simplex(simplex, "AB2 ->");

                            } else {
                                simplex.direction = simplex.AO;
                                simplex.move_to_stage(spoint.A);

                                result.save_simplex(simplex, "A1 ->");
                            }


                        } else { // within plane
                            if (CollisionHelper.triangle_closest_point_alternative(simplex.A, simplex.B, simplex.C, Vector3.Zero).Length() <= Math3D.epsilon) {
                                result.intersects = true;
                                result.save_simplex(simplex, "Hit on Triangle");
                                break;
                            }

                            if (simplex.same_dir_as_AO(simplex.ABC)) {
                                simplex.direction = simplex.ABC;

                                result.save_simplex(simplex, "ABC ->");
                            } else {
                                simplex.direction = -simplex.ABC;
                                simplex.move_to_stage(spoint.A, spoint.C, spoint.B);

                                result.save_simplex(simplex, "ACB ->");
                            }
                        }
                    }



                    ///////////////////////////////////////////////////////////
                } else if (simplex.stage == simplex_stage.tetrahedron) { // *** TETRAHEDRON ***
                    result.save_simplex(simplex, "Create Tetrahedron");


                    if (simplex.same_dir_as_AO(simplex.ABC)) {
                        var ac = simplex.same_dir_as_AO(Vector3.Cross(simplex.ABC, simplex.AC));
                        var ab = simplex.same_dir_as_AO(Vector3.Cross(simplex.AB, simplex.ABC));

                        if (ac && ab) {
                            break;

                            simplex.direction = simplex.AO;
                            simplex.move_to_stage(spoint.A);

                            result.save_simplex(simplex, "ABC -> A ->");

                        } else if (ab) {

                            simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AB, simplex.AO), simplex.AB);
                            simplex.move_to_stage(spoint.A, spoint.B);

                            result.save_simplex(simplex, "ABC -> AB ->");
                        } else if (ac) {

                            simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AC, simplex.AO), simplex.AC);
                            simplex.move_to_stage(spoint.A, spoint.C);

                            result.save_simplex(simplex, "ABC -> AC ->");
                        } else {
                            simplex.direction = simplex.ABC;
                            simplex.move_to_stage(spoint.A, spoint.B, spoint.C);

                            result.save_simplex(simplex, "ABC -> ABC ->");
                        }

                    } else if (simplex.same_dir_as_AO(simplex.ADB)) {
                        var bd = simplex.same_dir_as_AO(Vector3.Cross(simplex.BD, simplex.ADB));
                        var ba = simplex.same_dir_as_AO(Vector3.Cross(simplex.BA, simplex.ADB));

                        if (bd && ba) {

                            simplex.direction = simplex.BO;
                            simplex.move_to_stage(spoint.B);

                            result.save_simplex(simplex, "ADB -> B ->");

                        } else if (bd) {
                            simplex.direction = Vector3.Cross(Vector3.Cross(simplex.BD, simplex.AO), simplex.BD);
                            simplex.move_to_stage(spoint.B, spoint.D);

                            result.save_simplex(simplex, "ADB -> BD ->");

                        } else if (ba) {
                            simplex.direction = Vector3.Cross(Vector3.Cross(simplex.BA, simplex.AO), simplex.BA);
                            simplex.move_to_stage(spoint.B, spoint.A);

                            result.save_simplex(simplex, "ADB -> BA ->");
                        } else {
                            simplex.direction = simplex.ADB;
                            simplex.move_to_stage(spoint.A, spoint.D, spoint.B);

                            result.save_simplex(simplex, "ADB -> ADB ->");
                        }

                    } else if (simplex.same_dir_as_AO(simplex.ACD)) {
                        var ad = simplex.same_dir_as_AO(Vector3.Cross(simplex.ACD, simplex.AD));
                        var ac = simplex.same_dir_as_AO(Vector3.Cross(simplex.AC, simplex.ACD));
                        var cd = !simplex.same_dir_as_AO(Vector3.Cross(simplex.CD, simplex.ACD));

                        if (ad && ac) {

                            simplex.direction = simplex.CO;
                            simplex.move_to_stage(spoint.C);

                            result.save_simplex(simplex, "ACD -> C ->");
                        } else if (ad && cd) {

                            simplex.direction = simplex.DO;
                            simplex.move_to_stage(spoint.D);

                            result.save_simplex(simplex, "ACD -> D ->");


                        } else if (ad) {
                            simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AD, simplex.AO), simplex.AD);
                            simplex.move_to_stage(spoint.A, spoint.D);
                            result.save_simplex(simplex, "ACD -> AD ->");

                        } else if (ac) {

                            simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AC, simplex.AO), simplex.AC);
                            simplex.move_to_stage(spoint.A, spoint.C);
                            result.save_simplex(simplex, "ACD -> AC ->");

                        } else if (cd) {
                            simplex.direction = Vector3.Cross(Vector3.Cross(simplex.CD, simplex.CO), simplex.CD);
                            simplex.move_to_stage(spoint.C, spoint.D);
                            result.save_simplex(simplex, "ACD -> CD ->");
                        } else {

                            simplex.direction = simplex.ACD;
                            simplex.move_to_stage(spoint.A, spoint.C, spoint.D);
                            result.save_simplex(simplex, "ACD -> ACD ->");
                        }

                    } else {

                        if (CollisionHelper.point_inside_tetrahedron(simplex.A, simplex.B, simplex.C, simplex.D, Vector3.Zero)) {
                            result.intersects = true;
                            result.save_simplex(simplex, "Done");
                            break;
                        }
                        result.save_simplex(simplex, "Oh no");
                        break;
                    }

                }

                Vector3 closest_A = Vector3.Zero;
                Vector3 closest_B = Vector3.Zero;
                float d = float.MaxValue;
                float dot = float.MaxValue;
                float denom = simplex.get_denom();

                if (simplex.stage != simplex_stage.tetrahedron) {
                    switch (simplex.stage) {
                        case simplex_stage.point:
                            closest_A = simplex.A_support.A_support;
                            closest_B = simplex.A_support.B_support;

                            d = Vector3.Distance(closest_A, closest_B);

                            break;
                        case simplex_stage.line:

                            var AS = denom * simplex.A_bary;
                            var BS = denom * simplex.B_bary;

                            closest_A = (simplex.A_support.A_support * AS) + (simplex.B_support.A_support * BS);
                            closest_B = (simplex.A_support.B_support * AS) + (simplex.B_support.B_support * BS);


                            d = Vector3.Distance(closest_A, closest_B);
                            dot = Vector3.Dot(closest_A, closest_B);
                            break;
                        case simplex_stage.triangle:
                            closest_A = CollisionHelper.triangle_closest_point(simplex.A_support.A_support, simplex.B_support.A_support, simplex.C_support.A_support, Vector3.Zero);
                            closest_B = CollisionHelper.triangle_closest_point(simplex.A_support.B_support, simplex.B_support.B_support, simplex.C_support.B_support, Vector3.Zero);

                            d = Vector3.Distance(closest_A, closest_B);
                            dot = Vector3.Dot(closest_A, closest_B);
                            break;
                        case simplex_stage.tetrahedron:
                            d = float.MaxValue;
                            break;

                    }

                    simplex.closest_A = closest_A;
                    simplex.closest_B = closest_B;

                    if (d < result.distance && d > 0) {
                        result.distance = d;

                    }
                }
                iteration++;
            }



            return result;
        }
    }
}