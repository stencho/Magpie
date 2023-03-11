using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magpie.Engine.Collision.Support3D;
using Magpie.Engine.Collision;
using Magpie.Engine;
using Magpie.Engine.WorldElements;
using Microsoft.Xna.Framework;
using Magpie;
using Microsoft.Xna.Framework.Input;
using Magpie.Graphics;
using static Magpie.Engine.Collision.Collision2D;

namespace MagpieBuild.TestActors {
    internal class gjkTestActor : object_info {
        public gjkTestActor(Vector3 position) : base(position) {
            init();
        }

        public gjkTestActor(Vector3 position, render_info renderinfo) : base(position, renderinfo) {
            init();
        }
        public gjkTestActor(Vector3 position, collision_info collision_info) : base(position, collision_info) {
            init();
        }

        public gjkTestActor(Vector3 position, render_info renderinfo, collision_info collision_info) : base(position, renderinfo, collision_info) {
            init();
        }

        void init() {
            this.collision.dynamic = true;
        }

        int selected_target = 0;
        public volatile Dictionary<int, collision_result> gjk_targets = new Dictionary<int, collision_result>();
        collision_result sweep_test;
        public override void draw() {

            base.draw();

            //EngineState.world.current_map.octree.draw();

            Draw3D.xyz_cross(
                Vector3.Zero,
                1f, Color.Brown);

            var mvhb = collision.movebox;
            mvhb.draw(world);
            mvhb.draw(world * Matrix.CreateTranslation(last_mov));

            //lock (gjk_targets) { 
            int i = -1;

            foreach (int gjkid in gjk_targets.Keys) {

                i++;
                if (selected_target == -1 || selected_target == i) {


                    var sphb = EngineState.world.current_map.game_objects[gjkid].collision.movebox;
                    sphb.draw(EngineState.world.current_map.game_objects[gjkid].world);

                    //Draw3D.text_3D(EngineState.spritebatch, $"{selected_target.ToString()}", "pf", position + Vector3.Up, -EngineState.camera.direction, 1f, Color.Black);



                    //Draw3D.xyz_cross(spp, 1f, Color.Green);               
                }
            }
            i = -1;
            foreach (int gjkid in gjk_targets.Keys) {

                i++;
                if (selected_target == -1 || selected_target == i) {

                    var spp = EngineState.world.current_map.game_objects[gjkid].position;

                    //Draw3D.text_3D(EngineState.spritebatch, $"{selected_target.ToString()}", "pf", position + Vector3.Up, -EngineState.camera.direction, 1f, Color.Black);


                    var c = position + ((spp - position) / 2);

                    gjk_targets[gjkid].draw(c);

                    //Draw3D.xyz_cross(spp, 1f, Color.Green);               
                }
            }

            //if (sweep_test.intersects) {
                sweep_test.draw(Vector3.Zero);
                //Draw3D.sprite_line(Vector3.Transform(sweep_test.closest_A, sweep_test.end_simplex.A_transform * Matrix.CreateTranslation(sweep_test.end_simplex.sweep_A)), sweep_test.end_simplex.closest_B, 0.2f, Color.Pink);

            //}
            

            //}
            //Draw3D.line(c, c + )
            //Draw3D.xyz_cross(position, 1f, Color.Green);
        }

        double held_tick_time = 500;
        int held_tick_count = 0;
        float velocity = 0f;
        Vector3 saved_pos = Vector3.Zero;
        Vector3 last_mov = Vector3.Zero;
        public override void update() {
            Vector3 mv = Vector3.Zero;

            if (binds.pressed("t_forward")) {
                mv += Vector3.Forward;
            }
            if (binds.pressed("t_backward")) {
                mv += Vector3.Backward;
            }
            if (binds.pressed("t_left")) {
                mv += Vector3.Left;
            }
            if (binds.pressed("t_right")) {
                mv += Vector3.Right;
            }
            if (binds.pressed("t_up")) {
                mv += Vector3.Up;
            }
            if (binds.pressed("t_down")) {
                mv += Vector3.Down;
            }

            if (mv != Vector3.Zero)
                wants_movement += (Vector3.Normalize(mv) * (13f * (binds.pressed("shift") ? 50f : 1f)) * Clock.internal_frame_time_delta);

            //wants_movement += Vector3.Down * 9.81f * Clock.internal_frame_time_delta;

            //wants_movement = Vector3.Zero;

            if (binds.pressed("speenL")) {
                this.orientation *= Matrix.CreateFromAxisAngle(Vector3.Up, -1f * Clock.internal_frame_time_delta);

            }
            if (binds.pressed("speenR")) {
                this.orientation *= Matrix.CreateFromAxisAngle(Vector3.Up, 1f * Clock.internal_frame_time_delta);
            }


            if (binds.just_pressed("t_L") && binds.pressed("shift")) {
                if (selected_target > -1)
                    selected_target--;
                else if (selected_target < -1)
                    selected_target = -1;
            }


            if (binds.just_pressed("t_R") && binds.pressed("shift")) {
                if (selected_target < gjk_targets.Count - 1)
                    selected_target++;
                else if (selected_target > gjk_targets.Count - 1)
                    selected_target = gjk_targets.Count - 1;
            }

            Vector3 shortest_sweep = wants_movement;
            int shortest_id = -1;

            int st = 0;
            //lock (gjk_targets) {
            foreach (int gjkid in gjk_targets.Keys) {
                if (binds.pressed("t_S") && st == 0) {
                    var me = collision.movebox;
                    var ts = EngineState.world.current_map.game_objects[gjkid].collision.movebox;
                    var wa = world;
                    var wb = EngineState.world.current_map.game_objects[gjkid].world;

                    sweep_test = GJK.swept_gjk_intersects_with_halving(me, ts, wa, wb,
                        Vector3.Down * 13f * 135f * Clock.internal_frame_time_delta, Vector3.Zero);

                    if (binds.pressed("shift")) {
                        position = Vector3.Left * 5f + (Vector3.Up * 5f);
                    }
                }
                st++;
                /*
                var me = collision.movebox;
                var ts = EngineState.world.current_map.game_objects[gjkid].collision.movebox;
                var wa = world;
                var wb = EngineState.world.current_map.game_objects[gjkid].world;

                //collision snapshot
                if (binds.pressed("t_S") && st == selected_target) {

                    sweep_test = GJK.swept_gjk_intersects_with_halving(me, ts, wa, wb, 
                        Vector3.Down * 13f * 135f * Clock.internal_frame_time_delta, Vector3.Zero);
    
                    if (binds.pressed("shift")) {
                        position = Vector3.Left * 5f + (Vector3.Up * 5f);
                    }
                }
                st++;
                int old_draw =  gjk_targets[gjkid].draw_simplex;
                bool old_draw_supp = gjk_targets[gjkid].draw_all_supports;

                var i = gjk_targets[gjkid] = GJK.gjk_intersects(me, ts, wa, wb);

                if (gjk_targets[gjkid].intersects) {
                    var p = gjk_targets[gjkid].penetration * Vector3.Normalize(gjk_targets[gjkid].penetration_normal);

                    var dist = gjk_targets[gjkid].end_simplex.sweep_A.Length();

                    if (dist < shortest_sweep.Length()) {
                        shortest_sweep = Vector3.Normalize(wants_movement) * dist;
                        shortest_id = gjkid;
                    }
                    if (!p.contains_nan() && p != Vector3.Zero)
                        wants_movement += p;
                    

                    
                }

                if (old_draw >= gjk_targets[gjkid].simplex_list.Count - 1 || old_draw < 0)
                    old_draw = gjk_targets[gjkid].simplex_list.Count - 1;


                i.draw_simplex = old_draw;
                i.draw_all_supports = old_draw_supp;

                gjk_targets[gjkid] = i;
                */
                var t = gjk_targets[gjkid];

                if (binds.pressed("t_L")) {
                    var h = binds.held_time("t_L");
                    if (binds.just_pressed("t_L")) {
                        if (t.draw_simplex > 0)
                            t.draw_simplex--;
                        else if (t.draw_simplex < 0)
                            t.draw_simplex = 0;
                    } else if (held_tick_count < h / held_tick_time) {
                        held_tick_count = (int)(h / held_tick_time);

                        if (t.draw_simplex > 0)
                            t.draw_simplex--;
                        else if (t.draw_simplex < 0)
                            t.draw_simplex = 0;

                    } else {
                        held_tick_count = 0;
                    }
                } else if (binds.pressed("t_R")) {
                    var h = binds.held_time("t_R");

                    if (binds.just_pressed("t_R")) {
                        if (t.draw_simplex < t.simplex_list.Count - 1)
                            t.draw_simplex++;
                        else if (t.draw_simplex > t.simplex_list.Count - 1)
                            t.draw_simplex = t.simplex_list.Count - 1;
                    } else if (held_tick_count < h / held_tick_time) {
                        held_tick_count = (int)(h / held_tick_time);

                        if (t.draw_simplex < t.simplex_list.Count - 1)
                            t.draw_simplex++;
                        else if (t.draw_simplex > t.simplex_list.Count - 1)
                            t.draw_simplex = t.simplex_list.Count - 1;
                    } else {
                        held_tick_count = 0;
                    }
                }


                if (binds.just_pressed("t_supp")) {
                    t.draw_all_supports = !t.draw_all_supports;
                }

                gjk_targets[gjkid] = t;
                //}

            }

            last_mov = wants_movement;


            base.update();
        }
    }
}
