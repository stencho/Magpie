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


        public struct collision_result {
            public bool solved = false;
            public int id_A, id_B;

            public int closest_iteration;
            public Vector3 closest_A;
            public Vector3 closest_B;

            public float distance = float.MaxValue;

            public float distance_to_zero_A = float.MaxValue;
            public float distance_to_zero_B = float.MaxValue;


            public float penetration;

            public bool intersects;

            public List<gjk_simplex> simplex_list = new List<gjk_simplex>();
            public int draw_simplex;
            public bool draw_all_supports = true;

            public bool save_simplices = true;
            bool draw_simple = false;

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

                closest_iteration = 0;

                id_A = -1;
                id_B = -1;

                closest_A = Vector3.Zero;
                closest_B = Vector3.Zero;
            }

            public void draw(Vector3 world_pos) {
                if (simplex_list == null) return;
                if (save_simplices == false || draw_simple) {
                    Draw3D.line(closest_A, closest_B, Color.Pink);
                    Draw3D.xyz_cross(closest_A, 0.2f, Color.HotPink);
                    Draw3D.xyz_cross(closest_B, 0.2f, Color.HotPink);
                } else {
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

                        //Draw3D.line(simplex.closest_A, simplex.closest_B, Color.Red);

                        Draw3D.line(closest_A, closest_B, Color.Pink);
                        Draw3D.xyz_cross(closest_A, 0.2f, Color.HotPink);
                        Draw3D.xyz_cross(closest_B, 0.2f, Color.HotPink);
                    }
                    if (draw_all_supports && simplex_list.Count > 1 && draw_simplex <= simplex_list.Count - 1) {
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
                        for (int i = 0; i < draw_simplex; i++) {
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

        }

        #region GJK

        const int max_iterations = 20;

        public enum spoint { A=0, B=1, C=2, D=3 }

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

            public Vector3 A => supports[A_index].P; 
            public Vector3 B => supports[B_index].P;
            public Vector3 C => supports[C_index].P; 
            public Vector3 D => supports[D_index].P;

            internal int A_index => (int)stage;
            internal int B_index => (int)stage - 1;
            internal int C_index => (int)stage - 2;
            internal int D_index => (int)stage - 3;


            public float A_bary => supports[A_index].barycentric; 
            public float B_bary => supports[B_index].barycentric;
            public float C_bary => supports[C_index].barycentric; 
            public float D_bary => supports[D_index].barycentric;

            public gjk_support A_support => supports[A_index]; 
            public gjk_support B_support => supports[B_index];
            public gjk_support C_support => supports[C_index]; 
            public gjk_support D_support => supports[D_index];


            public Vector3 AO => -A; public Vector3 BO => -B;
            public Vector3 CO => -C; public Vector3 DO => -D;

            public Vector3 AB => B - A; public Vector3 AC => C - A; public Vector3 AD => D - A;
            public Vector3 BA => A - B; public Vector3 BC => C - B; public Vector3 BD => D - B;
            public Vector3 CD => D - C; public Vector3 DB => B - D;

            public Vector3 ABC => Vector3.Cross(AB, AC);
            public Vector3 ADB => Vector3.Cross(AD, AB);
            public Vector3 ACD => Vector3.Cross(AC, AD);
            public Vector3 BCD => Vector3.Cross(BC, BD);

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
                    case spoint.A: return A_index;
                    case spoint.B: return B_index;
                    case spoint.C: return C_index;
                    case spoint.D: return D_index;
                }
                return -1;
            }

            Vector3 spoint_value(spoint p) {
                switch (p) {
                    case spoint.A: return supports[A_index].P;
                    case spoint.B: return supports[B_index].P;
                    case spoint.C: return supports[C_index].P;
                    case spoint.D: return supports[D_index].P;
                }
                return Vector3.Zero;
            }

            float spoint_bary(spoint P) {
                switch (P) {
                    case spoint.A: return supports[A_index].barycentric;
                    case spoint.B: return supports[B_index].barycentric;
                    case spoint.C: return supports[C_index].barycentric;
                    case spoint.D: return supports[D_index].barycentric;
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
            public void set_bary(float U) {
                supports[A_index].barycentric = U;
            }
            public void set_bary_line(float U, float V) {
                supports[A_index].barycentric = U;
                supports[B_index].barycentric = V;
            }
            public void set_bary_tri(float U, float V, float W) {
                supports[A_index].barycentric = U;
                supports[B_index].barycentric = V;
                supports[C_index].barycentric = W;
            }
            public void set_bary_line((float U, float V) uv) {
                supports[A_index].barycentric = uv.U;
                supports[B_index].barycentric = uv.V;
            }
            
            
            public void set_bary_line() {
                var bc = CollisionHelper.line_barycentric(Vector3.Zero, A, B);
                supports[A_index].barycentric = bc.U;
                supports[B_index].barycentric = bc.V;
            }
            public void set_bary_tri() {
                var bc = CollisionHelper.triangle_barycentric(Vector3.Zero, A, B, C);
                supports[A_index].barycentric = bc.u;
                supports[B_index].barycentric = bc.v;
                supports[C_index].barycentric = bc.w;
            }

            public float get_denom() {
                float denom = 0f;
                switch (stage) {                    
                    case simplex_stage.point:
                        denom = 1f;
                        break;
                    case simplex_stage.line:
                        denom = A_support.barycentric + B_support.barycentric;
                        break;
                    case simplex_stage.triangle:
                        denom = A_support.barycentric + B_support.barycentric + C_support.barycentric;
                        break;
                    case simplex_stage.tetrahedron:
                        denom = A_support.barycentric + B_support.barycentric + C_support.barycentric + D_support.barycentric;
                        break;
                }
                denom = 1f / denom;
                return denom;
            }

            public void move_to_stage(spoint A) {
                var s = new gjk_support[4];
                s[0] = supports[spoint_index(A)];
                supports = s;
                stage = simplex_stage.point;
                s[0].barycentric = 1f;
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

            public void set_dir_to_inverse_closest() {
                switch (stage) {
                    case simplex_stage.point:
                        direction = AO;
                        break;
                    case simplex_stage.line:
                        if (Vector3.Distance(Vector3.Zero, A) < Vector3.Distance(Vector3.Zero, B))
                            direction = AO;
                        else
                            direction = BO;
                        break;
                    case simplex_stage.triangle:
                        if (Vector3.Distance(Vector3.Zero, A) < Vector3.Distance(Vector3.Zero, B))
                            if (Vector3.Distance(Vector3.Zero, A) < Vector3.Distance(Vector3.Zero, C))
                                direction = AO;
                            else
                                direction = CO;
                        else
                            direction = BO;
                        break;
                    case simplex_stage.tetrahedron:
                        var a = Vector3.Distance(Vector3.Zero, A);

                        if (a < Vector3.Distance(Vector3.Zero, B))
                            if (a < Vector3.Distance(Vector3.Zero, C))
                                if (a < Vector3.Distance(Vector3.Zero, D))
                                    direction = AO;
                                else
                                    direction = DO;
                            else
                                direction = CO;
                        else
                            direction = BO;
                        break;
                }
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

                if ((int)stage >= 0) sb.Append($"[A] {A.ToXString()} [b] {supports[A_index].barycentric}\n");
                if ((int)stage >= 1) sb.Append($"[B] {B.ToXString()} [b] {supports[B_index].barycentric}\n");
                if ((int)stage >= 2) sb.Append($"[C] {C.ToXString()} [b] {supports[C_index].barycentric}\n");
                if ((int)stage >= 3) sb.Append($"[D] {D.ToXString()} [b] {supports[D_index].barycentric}\n");

                return sb.ToString();

            }

            public void draw() {
                Draw3D.xyz_cross(A, 0.2f, Color.Red);
                if ((int)stage > 0) Draw3D.xyz_cross(B, 0.2f, Color.Green);
                if ((int)stage > 1) Draw3D.xyz_cross(C, 0.2f, Color.Blue);
                if ((int)stage > 2) Draw3D.xyz_cross(D, 0.2f, Color.Yellow);

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

        public static collision_result gjk_intersects(Shape3D shape_A, Shape3D shape_B, Matrix w_a, Matrix w_b) {
            collision_result result = new collision_result();

            gjk_simplex simplex = new gjk_simplex();

            Vector3 scale_a; Quaternion rot_a;
            Vector3 scale_b; Quaternion rot_b;

            w_a.Decompose(out scale_a, out rot_a, out _);
            w_b.Decompose(out scale_b, out rot_b, out _);

            simplex.A_transform =  Matrix.CreateFromQuaternion(rot_a);
            simplex.B_transform =  Matrix.CreateFromQuaternion(rot_b);

            //simplex.direction = w_b.Translation - w_a.Translation;
            simplex.direction = Vector3.One;

            simplex.add_new_point(
                Vector3.Transform(shape_A.support(Vector3.Transform(simplex.direction, Matrix.Invert(simplex.A_transform)), Vector3.Zero), (w_a)),
                Vector3.Transform(shape_B.support(Vector3.Transform(-simplex.direction, Matrix.Invert(simplex.B_transform)), Vector3.Zero), (w_b)));

            simplex.supports[0].barycentric = 1f;

            gjk_closest_point_calc(ref simplex, ref result, w_a, w_b);

            simplex.direction = simplex.AO;

            result.save_simplex(simplex, "Begin");


            int iteration = 1;

            while (iteration < max_iterations) {
                simplex.add_new_point(
                    Vector3.Transform(shape_A.support(Vector3.Transform(simplex.direction, Matrix.Invert(simplex.A_transform)), Vector3.Zero), (w_a)),
                    Vector3.Transform(shape_B.support(Vector3.Transform(-simplex.direction, Matrix.Invert(simplex.B_transform)), Vector3.Zero), (w_b)) 
                    );

                simplex.iteration = iteration;


                    ///////////////////////////////////////////////////////////
                if (simplex.stage == simplex_stage.line) { // *** LINE ***

                    gjk_closest_point_calc(ref simplex, ref result, w_a, w_b);
                    result.save_simplex(simplex, "Create Line");
                     
                    
                    if (Vector3.Distance(simplex.A, simplex.B) <= Math3D.big_epsilon) {                        
                        simplex.move_to_stage(spoint.B);
                        gjk_closest_point_calc(ref simplex, ref result, w_a, w_b);
                        result.save_simplex(simplex, "New line point in exact same position");
                        break;
                    }

                    if (CollisionHelper.line_closest_point(simplex.A, simplex.B, Vector3.Zero).Length() <= Math3D.big_epsilon) {
                        result.intersects = true;
                        simplex.set_bary_line();
                        result.save_simplex(simplex, "Hit on Line");
                        break;
                    }

                    //origin between A and B
                    if (simplex.same_dir_as_AO(simplex.AB)) {
                        simplex.early_exit_reason = "Origin betweeen A and B";
                        simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AB, simplex.AO), simplex.AB);

                    } else {
                        simplex.direction = simplex.AO;
                        simplex.move_to_stage(spoint.A);
                        result.save_simplex(simplex, "origin past A");
                    }

                    ///////////////////////////////////////////////////////////
                } else if (simplex.stage == simplex_stage.triangle) { // *** TRIANGLE ***

                    result.save_simplex(simplex, "Create Triangle");


                    if (Vector3.Distance(simplex.A, simplex.B) <= Math3D.big_epsilon) {
                        simplex.move_to_stage(spoint.B, spoint.C);
                        gjk_closest_point_calc(ref simplex, ref result, w_a, w_b);
                        result.save_simplex(simplex, "New tri point in exact same position");
                        break;
                    } else if (Vector3.Distance(simplex.A, simplex.C) <= Math3D.big_epsilon) {
                        simplex.move_to_stage(spoint.B, spoint.C);
                        gjk_closest_point_calc(ref simplex, ref result, w_a, w_b);
                        result.save_simplex(simplex, "New tri point in exact same position");
                        break;
                    }


                    //On the ABC x AC plane, so origin could be closest to either AC or A
                    if (simplex.same_dir_as_AO(Vector3.Cross(simplex.ABC, simplex.AC))) {

                        if (simplex.same_dir_as_AO(simplex.AC)) {
                            simplex.early_exit_reason = "Tri -> AC";
                            simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AC, simplex.AO), simplex.AC);
                            
                            simplex.move_to_stage(spoint.A, spoint.C);

                        } else {
                            if (simplex.same_dir_as_AO(simplex.AB)) {
                                simplex.early_exit_reason = "Tri -> AB1";
                                simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AB, simplex.AO), simplex.AB);

                                simplex.move_to_stage(spoint.A, spoint.B);


                            } else {
                                simplex.early_exit_reason = "Tri -> A1";
                                simplex.direction = simplex.AO;

                                simplex.move_to_stage(spoint.A);
                            }

                        }
                    } else {
                        //On the AB x ABC plane, so we're either on AB or A
                        if (simplex.same_dir_as_AO(Vector3.Cross(simplex.AB, simplex.ABC))) {
                            if (simplex.same_dir_as_AO(simplex.AB)) {
                                simplex.early_exit_reason = "Tri -> AB2";
                                simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AB, simplex.AO), simplex.AB);

                                simplex.move_to_stage(spoint.A, spoint.B);


                            } else {
                                simplex.early_exit_reason = "Tri -> A1";

                                simplex.direction = simplex.AO; 
                                simplex.move_to_stage(spoint.A);
                            }


                        } else { // within plane
                            if (CollisionHelper.triangle_closest_point(simplex.A, simplex.B, simplex.C, Vector3.Zero).Length() <= Math3D.big_epsilon) {
                                result.intersects = true;

                                gjk_closest_point_calc(ref simplex, ref result, w_a, w_b);
                                result.save_simplex(simplex, "Hit on Triangle");
                                break;
                            }
                            
                            if (simplex.same_dir_as_AO(simplex.ABC)) {
                                simplex.early_exit_reason = "Tri -> ABC";
                                simplex.direction = simplex.ABC;
                            } else {
                                simplex.early_exit_reason = "Tri -> ACB";
                                simplex.direction = -simplex.ABC;
                                simplex.move_to_stage(spoint.A, spoint.C, spoint.B);
                            }
                        }
                    }


                    ///////////////////////////////////////////////////////////
                } else if (simplex.stage == simplex_stage.tetrahedron) { // *** TETRAHEDRON ***

                    result.save_simplex(simplex, "Create Tetrahedron");

                    
                    if (Vector3.Distance(simplex.A, simplex.B) <= Math3D.big_epsilon) {
                        simplex.move_to_stage(spoint.B, spoint.C, spoint.D);
                        gjk_closest_point_calc(ref simplex, ref result, w_a, w_b);
                        result.save_simplex(simplex, "New line point in exact same position");
                        break;

                    } else if (Vector3.Distance(simplex.A, simplex.C) <= Math3D.big_epsilon) {
                        simplex.move_to_stage(spoint.B, spoint.C, spoint.D);
                        gjk_closest_point_calc(ref simplex, ref result, w_a, w_b);
                        result.save_simplex(simplex, "New line point in exact same position");
                        break;                    

                    } else if (Vector3.Distance(simplex.A, simplex.D) <= Math3D.big_epsilon) {
                        simplex.move_to_stage(spoint.B, spoint.C, spoint.D);
                        gjk_closest_point_calc(ref simplex, ref result, w_a, w_b);
                        result.save_simplex(simplex, "New line point in exact same position");
                        break;
                    }
                    

                    bool ABC = simplex.same_dir_as_AO(simplex.ABC);
                    bool ACD = simplex.same_dir_as_AO(simplex.ACD);
                    bool ADB = simplex.same_dir_as_AO(simplex.ADB);
                    bool BCD = simplex.same_dir_as_AO(simplex.BCD);

                    if (ABC && ADB && ACD) {
                        simplex.early_exit_reason = "Tetra -> A";

                        simplex.direction = simplex.AO;

                        simplex.move_to_stage(spoint.A);

                    } else {
                        if (ABC && ADB) {
                            simplex.early_exit_reason = "Tetra -> AB";
                            simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AB, simplex.AO), simplex.AB);
                            //simplex.direction = -(simplex.A + ((simplex.B - simplex.A) / 2));

                            simplex.move_to_stage(spoint.A, spoint.B);

                        } else if (ABC && ACD) {
                            simplex.early_exit_reason = "Tetra -> AC";
                            simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AC, simplex.AO), simplex.AC);
                            //simplex.direction = -(simplex.A + ((simplex.C - simplex.A) / 2));

                            simplex.move_to_stage(spoint.A, spoint.C);

                        } else if (ACD && ADB) {
                            simplex.early_exit_reason = "Tetra -> AD";
                            simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AD, simplex.AO), simplex.AD);
                            //simplex.direction = -(simplex.A + ((simplex.D - simplex.A) / 2));

                            simplex.move_to_stage(spoint.A, spoint.D);

                        } else {
                            //face
                            if (ABC) {
                                simplex.early_exit_reason = "Tetra -> ABC";
                                simplex.direction = simplex.ABC;

                                simplex.move_to_stage(spoint.A, spoint.B, spoint.C);

                            } else if (ACD) {
                                simplex.early_exit_reason = "Tetra -> ACD";
                                simplex.direction = simplex.ACD;

                                simplex.move_to_stage(spoint.A, spoint.C, spoint.D);

                            } else if (ADB) {
                                simplex.early_exit_reason = "Tetra -> ADB";
                                simplex.direction = simplex.ADB;

                                simplex.move_to_stage(spoint.A, spoint.D, spoint.B);

                            } else {

                                if (!ABC && !ACD && !ADB && !BCD) {
                                    result.intersects = true;
                                    result.save_simplex(simplex, "Intersection found");
                                    break;
                                }

                                //trying to go backwards??                                
                                simplex.early_exit_reason = "oh no";
                                break;
                            }
                        }
                    }

                }

                gjk_closest_point_calc(ref simplex, ref result, w_a, w_b);

                result.save_simplex(ref simplex);
                iteration++;
            }


            result.solved = true;
            return result;
        }

        static void gjk_closest_point_calc(ref gjk_simplex simplex, ref collision_result result, Matrix w_a, Matrix w_b) {

            Vector3 closest_A = Vector3.Zero;
            Vector3 closest_B = Vector3.Zero;

            switch (simplex.stage) {
                case simplex_stage.line:
                    simplex.set_bary_line();
                    break;
                case simplex_stage.triangle:

                    simplex.set_bary_tri();
                    break;
                case simplex_stage.tetrahedron:
                    var bary = CollisionHelper.tetrahedron_barycentric(Vector3.Zero, simplex.A, simplex.B, simplex.C, simplex.D);

                    simplex.set_bary(spoint.A, bary.U);
                    simplex.set_bary(spoint.B, bary.V);
                    simplex.set_bary(spoint.C, bary.W);
                    simplex.set_bary(spoint.D, bary.Z);
                    break;
            }

            float d = float.MaxValue;
            float dot = float.MaxValue;
            float denom = simplex.get_denom();

            switch (simplex.stage) {
                case simplex_stage.point:
                    closest_A = simplex.A_support.A_support;
                    closest_B = simplex.A_support.B_support;

                    break;
                case simplex_stage.line:

                    var l_AS = denom * simplex.A_bary;
                    var l_BS = denom * simplex.B_bary;

                    closest_A = (simplex.A_support.A_support * l_AS) + (simplex.B_support.A_support * l_BS);
                    closest_B = (simplex.A_support.B_support * l_AS) + (simplex.B_support.B_support * l_BS);


                    dot = Vector3.Dot(closest_A, closest_B);
                    break;
                case simplex_stage.triangle:
                    //closest_A = CollisionHelper.triangle_closest_point(simplex.A_support.A_support, simplex.B_support.A_support, simplex.C_support.A_support, Vector3.Zero);
                    //closest_B = CollisionHelper.triangle_closest_point(simplex.A_support.B_support, simplex.B_support.B_support, simplex.C_support.B_support, Vector3.Zero);

                    var t_AS = denom * simplex.A_bary;
                    var t_BS = denom * simplex.B_bary;
                    var t_CS = denom * simplex.C_bary;

                    closest_A = (simplex.A_support.A_support * t_AS) + (simplex.B_support.A_support * t_BS) + (simplex.C_support.A_support * t_CS);
                    closest_B = (simplex.A_support.B_support * t_AS) + (simplex.B_support.B_support * t_BS) + (simplex.C_support.B_support * t_CS);

                    //closest_A = Vector3.Transform(closest_A, (simplex.A_transform));
                    //closest_B = Vector3.Transform(closest_B, (simplex.B_transform));

                    dot = Vector3.Dot(closest_A, closest_B);
                    break;
                case simplex_stage.tetrahedron:

                    closest_A =
                        (simplex.A_support.A_support * (denom * simplex.A_bary)) +
                        (simplex.B_support.A_support * (denom * simplex.B_bary)) +
                        (simplex.C_support.A_support * (denom * simplex.C_bary)) +
                        (simplex.D_support.A_support * (denom * simplex.D_bary));

                    //closest_A = Vector3.Transform(closest_A, (simplex.A_transform));

                    closest_B = closest_A;
                    break;

            }

            d = Vector3.Distance(closest_A, closest_B);

            var da = closest_A.Length();
            var db = closest_B.Length();

            simplex.closest_A = closest_A;
            simplex.closest_B = closest_B;

            if (da < result.distance_to_zero_A) {
                result.distance_to_zero_A = da; 
            }
            if (db < result.distance_to_zero_B) {
                result.distance_to_zero_B = db; 
            }

            if (d < result.distance && d > 0) {
                result.distance = d;

                result.closest_A = closest_A;
                result.closest_B = closest_B;

                result.closest_iteration = simplex.iteration;
            }

        }
    }

    #endregion
}
