using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magpie.Engine;
using Magpie;
using Magpie.Engine.WorldElements;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using static Magpie.Engine.Controls;
using static Magpie.Engine.ControlBinds;
using Microsoft.Xna.Framework.Input;
using Magpie.Engine.Collision.Support3D;

namespace MagpieBuild.TestActors {
    public class freecam_objinfo : object_info_dynamic {

        public volatile Camera cam;

        public override bool dynamic => true;

        public freecam_objinfo(Vector3 position) : base(position) {
            init();
        }

        void init() {
            collision = new collision_info(new Capsule(1.85f, 1f));
            cam = new Camera();
            //gravity = false;
        }

        float movement_speed = 12f;

        XYPair stored_pos;
        bool aiming = false;
        bool was_aiming = false;

        public override void post_solve() {
            cam.position = position + ((Capsule)collision.movebox).B;

            cam.update();

            base.post_solve();
        }

        public override void update() {

            if (binds.just_pressed("click_right") && EngineState.is_active && EngineState.was_active) {
                EngineState.game.IsMouseVisible = false;
                aiming = true;
            } else if (binds.just_released("click_right") || !EngineState.is_active) {
                aiming = false;
            }

            if (aiming && !was_aiming) {
                stored_pos = Controls.mouse_position;
                enable_mouse_cursor = false;
                enable_mouse_lock = true;
            } else if (aiming) {
                cam.orientation *= Matrix.CreateRotationY(mouse_delta_internal.X / (EngineState.resolution.X));

                if (cam.orientation.Forward.Y < .98f && mouse_delta_internal.Y > 0) {
                    cam.orientation *= Matrix.CreateFromAxisAngle(cam.orientation.Right, mouse_delta_internal.Y / (EngineState.resolution.Y));
                } else if (cam.orientation.Forward.Y > -.98f && mouse_delta_internal.Y < 0) {
                    cam.orientation *= Matrix.CreateFromAxisAngle(cam.orientation.Right, mouse_delta_internal.Y / (EngineState.resolution.Y));
                }
            } else if (!aiming && was_aiming) {
                //if (was_locked == true)
                Mouse.SetPosition(stored_pos.X, stored_pos.Y);

                enable_mouse_lock = false;
                enable_mouse_cursor = true;
                //was_locked = false;
            }

            was_aiming = aiming;

            Vector3 mv = Vector3.Zero;

            if (binds.pressed("forward")) {
                mv += Vector3.Cross(Vector3.Up, cam.orientation.Right);
            }
            if (binds.pressed("backward")) {
                mv += -Vector3.Cross(Vector3.Up, cam.orientation.Right);
            }
            if (binds.pressed("left")) {
                mv += cam.orientation.Left;
            }
            if (binds.pressed("right")) {
                mv += cam.orientation.Right;
            }
            if (binds.just_pressed("up") && has_footing()) {
                //mv += Vector3.Up;
                gravity_current = -0.7f;
            }                
            if (binds.pressed("down")) {
                mv += Vector3.Down;
            }

            if (mv != Vector3.Zero)
                wants_movement = Vector3.Normalize(mv) * movement_speed * (binds.pressed("ctrl") ? 0.3f : (binds.pressed("shift") ? 1f : 4f)) * Clock.internal_frame_time_delta;


            if (binds.just_pressed("ui_select")) {
                var o = EngineState.world.current_map.spawn_object(
                    new object_info_dynamic(position + (cam.direction * 5f),
                    new render_info_model("sphere", "trumpmap"),
                    new collision_info(
                        //new Sphere(1f)
                        !binds.pressed("shift") ? new Sphere(1f) : new Cube(1f)
                        )));

            }

            if (wants_movement != Vector3.Zero) {
                //this.position += wants_movement;
                //wants_movement = Vector3.Zero;
            }


            base.update();
        }
    }
}
