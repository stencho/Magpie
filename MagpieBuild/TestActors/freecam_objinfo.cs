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
using Magpie.Graphics.Lights;

namespace MagpieBuild.TestActors {
    public class freecam_objinfo : object_info_dynamic {

        public volatile Camera cam;

        public override bool dynamic => true;

        public freecam_objinfo(Vector3 position) : base(position) {
            init();
        }


        void init() {
            collision = new collision_info(new Capsule(1.85f, 1f));
            collision.parent = this;

            cam = new Camera();
            //gravity = false;

            lights = new light[2] {
            new light {
                type = LightType.POINT,
                color = Color.Blue,
                point_info = new point_info() {
                    radius = 2f
                }
            },

            new light {
                type = LightType.SPOT,
                color = Color.Red,
                spot_info = new spot_info()
            }
        };
        }

        float movement_speed = 12f;

        XYPair stored_pos;
        bool aiming = false;
        bool was_aiming = false;

        public override void post_solve() {
            cam.position = position + ((Capsule)collision.movebox).B;

            lights[0].position = position + (cam.orientation.Right * 0.5f) + (cam.orientation.Down * 0.4f) + (cam.orientation.Forward * 0.6f);
            lights[1].position = position + (cam.orientation.Right * 0.5f) + (cam.orientation.Down * 0.4f) + (cam.orientation.Forward * 0.5f);

            lights[1].spot_info.orientation = cam.orientation * Matrix.CreateFromAxisAngle(cam.orientation.Up, MathHelper.ToRadians(5f));

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
                mouse_cursor = false;
                mouse_lock = true;
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

                mouse_lock = false;
                mouse_cursor = true;
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
                //gravity_current = -0.8f;
            }
            if (binds.pressed("down")) {
                mv += Vector3.Down;
            }

            if (mv != Vector3.Zero)
                wants_movement = Vector3.Normalize(mv) * movement_speed * (binds.pressed("ctrl") ? 0.3f : (binds.pressed("shift") ? 1f : 4f)) * Clock.internal_frame_time_delta;


            if (binds.just_pressed("click")) {
                var no = new object_info_dynamic(cam.position + cam.direction,
                    new render_info_model("cube", "trumpmap"),
                    new collision_info(new Sphere(1f))
                    );
                no.wants_movement = cam.direction * 10f;

               // var noid = EngineState.world.current_map.spawn_object(no);
                /*
                var o = EngineState.world.current_map.spawn_object(
                    new object_info_dynamic(position + (cam.direction * 5f),
                    new render_info_model("sphere", "trumpmap"),
                    new collision_info(
                        //new Sphere(1f)
                        !binds.pressed("shift") ? new Sphere(1f) : new Cube(1f)
                        )));
                */
            }

            if (wants_movement != Vector3.Zero) {
                //this.position += wants_movement;
                //wants_movement = Vector3.Zero;
            }


            base.update();
        }
    }
}
