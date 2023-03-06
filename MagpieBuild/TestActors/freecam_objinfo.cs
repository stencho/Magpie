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

namespace MagpieBuild.TestActors {
    public class freecam_objinfo : object_info {

        public volatile Camera cam;

        public freecam_objinfo(Vector3 position) : base(position) {
            init();
        }

        void init() {
            cam = new Camera();
        }

        float movement_speed = 12f;

        XYPair stored_pos;
        bool aiming = false;
        bool was_aiming = false;

        public override void post_solve() {
            cam.position = position;
            cam.update();
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
            if (binds.pressed("up")) {
                mv += Vector3.Up;
            }
            if (binds.pressed("down")) {
                mv += Vector3.Down;
            }

            if (mv != Vector3.Zero)
                wants_movement = Vector3.Normalize(mv) * movement_speed * (binds.pressed("ctrl") ? 0.3f : (binds.pressed("shift") ? 1f : 4f)) * Clock.internal_frame_time_delta;

            if (wants_movement != Vector3.Zero) {
                //this.position += wants_movement;
                //wants_movement = Vector3.Zero;
            }


            base.update();
        }
    }
}
