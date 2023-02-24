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

namespace MagpieBuild.TestActors {
    internal class gjkTestActor : object_info {
        public gjkTestActor(Vector3 position) : base(position) {
            init();
        }

        public gjkTestActor(Vector3 position, render_info renderinfo) : base(position, renderinfo) {
            init();
        }
        void init() {
            binds = new ControlBinds(
            (bind_type.digital, controller_type.keyboard, Keys.R, new string[] { "t_S" }),
            (bind_type.digital, controller_type.keyboard, Keys.Q, new string[] { "t_L" }),
            (bind_type.digital, controller_type.keyboard, Keys.E, new string[] { "t_R" }));
        }
        public int gjk_target_id = -1;
        public override void draw() {
            if (gjk_target_id > 0) {
                var mvhb = collision.hitbox;
                mvhb.draw(world);

                var sphb = EngineState.world.current_map.game_objects[gjk_target_id].collision.hitbox;
                sphb.draw(EngineState.world.current_map.game_objects[gjk_target_id].world);
                var spp = EngineState.world.current_map.game_objects[gjk_target_id].position;

                var c = position + ((spp - position) / 2);

                test_cr.draw(Vector3.Zero);

            }
            //Draw3D.line(c, c + )
            //Draw3D.xyz_cross(world.current_map.game_objects[moveid].test_cr.closest_point_A)
            base.draw();
        }
        public override void update() {
            lock (binds) {
                if (binds != null) {
                    binds.update();
                }
            }

            //collision snapshot
            if (binds.pressed("t_S")) {
                if (gjk_target_id > 0) { 
                    hitbox_collision me = (hitbox_collision)collision.hitbox;
                    hitbox_collision ts = (hitbox_collision)EngineState.world.current_map.game_objects[gjk_target_id].collision.hitbox;

                    var wa = world;
                    var wb = EngineState.world.current_map.game_objects[gjk_target_id].world;
                    int old_draw = test_cr.draw_simplex;
                    test_cr = MixedCollision.intersects(me.collision, ts.collision, wa, wb);
                    test_cr.draw_simplex = old_draw;
                }
            }

            if (binds.just_pressed("t_L")) {
                if (test_cr.draw_simplex > 0) {
                    test_cr.draw_simplex--;
                } else if (test_cr.draw_simplex < 0)
                    test_cr.draw_simplex = 0;
            }

            if (binds.just_pressed("t_R")) {
                if (test_cr.draw_simplex < test_cr.simplex_list.Count-1) {
                    test_cr.draw_simplex++;
                } else if (test_cr.draw_simplex > test_cr.simplex_list.Count-1){
                    test_cr.draw_simplex = test_cr.simplex_list.Count-1;
                }
            }

            Vector3 mv = Vector3.Zero;

            if (StaticControlBinds.pressed("t_forward")) {
                mv += Vector3.Forward;
            }
            if (StaticControlBinds.pressed("t_backward")) {
                mv += Vector3.Backward;
            }
            if (StaticControlBinds.pressed("t_left")) {
                mv += Vector3.Left;
            }
            if (StaticControlBinds.pressed("t_right")) {
                mv += Vector3.Right;
            }
            if (StaticControlBinds.pressed("t_up")) {
                mv += Vector3.Up;
            }
            if (StaticControlBinds.pressed("t_down")) {
                mv += Vector3.Down;
            }

            if (mv != Vector3.Zero)
                wants_movement = (Vector3.Normalize(mv) * (5f * (StaticControlBinds.pressed("shift") ? 0.2f : 1f)) * Clock.internal_frame_time_delta);
            
            base.update();
        }
    }
}
