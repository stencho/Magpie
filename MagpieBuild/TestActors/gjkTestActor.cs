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
using static Magpie.Engine.Collision.MixedCollision;

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
        void init() {
        }

        int selected_target = 0;
        public volatile Dictionary<int, collision_result> gjk_targets = new Dictionary<int, collision_result>();

        public override void draw() {
            Draw3D.xyz_cross(
                Vector3.Zero,
                1f, Color.Brown);

            //lock (gjk_targets) { 
                int i = -1;
                foreach (int gjkid in gjk_targets.Keys) {
                    
                    i++;
                    if (selected_target == -1 || selected_target == i) {

                        var mvhb = collision.hitbox;
                        mvhb.draw(world);

                        var sphb = EngineState.world.current_map.game_objects[gjkid].collision.hitbox;
                        sphb.draw(EngineState.world.current_map.game_objects[gjkid].world);
                        var spp = EngineState.world.current_map.game_objects[gjkid].position;

                        Draw3D.text_3D(EngineState.spritebatch, $"{selected_target.ToString()}", "pf", position + Vector3.Up, -EngineState.camera.direction, 1f, Color.Black);


                        var c = position + ((spp - position) / 2);

                        gjk_targets[gjkid].draw(c);

                        //Draw3D.xyz_cross(spp, 1f, Color.Green);               
                    }
                }
            //}
            //Draw3D.line(c, c + )
            //Draw3D.xyz_cross(position, 1f, Color.Green);
            base.draw();
        }

        double held_tick_time = 500;
        int held_tick_count = 0;

        public override void update() {
            binds.update();

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
            
            //lock (gjk_targets) {
            foreach (int gjkid in gjk_targets.Keys) {
                //collision snapshot
                if (binds.pressed("t_S")) {
                    hitbox_collision me = (hitbox_collision)collision.hitbox;
                    hitbox_collision ts = (hitbox_collision)EngineState.world.current_map.game_objects[gjkid].collision.hitbox;

                    var wa = world;
                    var wb = EngineState.world.current_map.game_objects[gjkid].world;
                    int old_draw =  gjk_targets[gjkid].draw_simplex;
                    bool old_draw_supp = gjk_targets[gjkid].draw_all_supports;

                    var i = MixedCollision.gjk_intersects(me.collision, ts.collision, wa, wb);


                    //if (old_draw >= gjk_targets[gjkid].simplex_list.Count - 1 || old_draw < 0)
                        old_draw = gjk_targets[gjkid].simplex_list.Count - 1;
                       

                    i.draw_simplex = old_draw;
                    i.draw_all_supports = old_draw_supp;
                    gjk_targets[gjkid] = i;
                }

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
                wants_movement = (Vector3.Normalize(mv) * (5f * (binds.pressed("shift") ? 0.2f : 1f)) * Clock.internal_frame_time_delta);
            
            base.update();
        }
    }
}
