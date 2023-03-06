using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Magpie.Graphics;
using Microsoft.Xna.Framework;

namespace Magpie.Engine.Collision {
    public struct collision_result {
        public bool solved = false;
        public int id_A, id_B;

        public int closest_iteration;
        public Vector3 closest_A;
        public Vector3 closest_B;
        public Vector3 AB => closest_B - closest_A;

        public float distance = float.MaxValue;

        public float distance_to_zero_A = float.MaxValue;
        public float distance_to_zero_B = float.MaxValue;

        public float penetration;
        public Vector3 penetration_normal;

        public bool intersects;

        public gjk_simplex end_simplex;
        public List<gjk_simplex> simplex_list = new List<gjk_simplex>();

        public bool draw_all_supports = true;

        public bool save_simplices = true;
        public int draw_simplex = 0;
        public polytope polytope;

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


            closest_iteration = 0;

            id_A = -1;
            id_B = -1;

            closest_A = Vector3.Zero;
            closest_B = Vector3.Zero;
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

            if (polytope != null)
                polytope.draw();
            Draw3D.sprite_line(world_pos, world_pos + (penetration_normal * penetration), 0.02f, Color.Red);
        }
    }
}
