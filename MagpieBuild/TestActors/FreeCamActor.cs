using Magpie;
using Magpie.Engine;
using Magpie.Engine.Brushes;
using Magpie.Engine.Collision.Support3D;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Magpie.Engine.Controls;
//using static Magpie.Engine.DigitalControlBindings;

using static Magpie.Engine.ControlBinds;
using Magpie.Graphics.Lights;
using Magpie.Engine.WorldElements;

namespace MagpieTestbed.TestActors {
    [Serializable]
    class FreeCamActor : Actor {
        public volatile Camera cam;
        public Vector3 position { get; set; } = Vector3.Zero;
        public Vector3 wants_movement { get; set; } = (Vector3.Backward + Vector3.Up) * 2;
        public Matrix world => Matrix.CreateTranslation(position);

        public Vector3 wants_absolute_movement { get; set; } = Vector3.Zero;
        public bool request_absolute_move { get; set; } = false;
        public bool sweep_absolute_move { get; set; } = false;

        float movement_speed = 12f;
        float mouse_multi = 0.6f;

        public Shape3D collision { get; set; }
        public Shape3D sweep_collision { get; set; }

        bool camera_enabled = false;
        XYPair last_mouse_pos = XYPair.Zero;

        public light[] lights { get; set; } = new light[2]{ 
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
        public FreeCamActor() {
            cam = new Camera();
            collision = new Sphere(1f);
            //EngineState.player_binds_one = binds;
        }

        bool was_locked = false;
        XYPair stored_pos;

        bool aiming = false;
        bool was_aiming = false;
        ThreadBindManager binds = new ThreadBindManager();
        public void Update() {
            binds.update();
            /*
            if (binds.held("mouse_aim")) {
                if (binds.just_held("mouse_aim")) {
                    last_mouse_pos = mouse_position;
                }

                if (!binds.held("mouse_aim")) {
                    camera_enabled = true;
                    EngineState.game.IsMouseVisible = false;
                    mouse_lock = true;
                }

            } else if (binds.released("mouse_aim") || (camera_enabled && !EngineState.is_active && EngineState.was_active)) {
                camera_enabled = false;
                EngineState.game.IsMouseVisible = true;
                mouse_lock = false;
                Mouse.SetPosition(last_mouse_pos.X, last_mouse_pos.Y);                
            }

            if (camera_enabled && binds.held("mouse_aim") && !binds.released("mouse_aim")) {


                cam.orientation *= Matrix.CreateRotationY(mouse_delta.X / (EngineState.resolution.X * mouse_multi));
                                               
                //first person camera pitch
                //clamped from -0.9 to 0.9 by way of only allowing the mouse to move up/down if it's below/above those values
                if (cam.orientation.Forward.Y < .98f && mouse_delta.Y > 0) {
                    cam.orientation *= Matrix.CreateFromAxisAngle(cam.orientation.Right, mouse_delta.Y / (EngineState.resolution.Y * mouse_multi));
                } else if (cam.orientation.Forward.Y > -.98f && mouse_delta.Y < 0) {                    
                    cam.orientation *= Matrix.CreateFromAxisAngle(cam.orientation.Right, mouse_delta.Y / (EngineState.resolution.Y * mouse_multi));
                } 
            }
            */
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
                cam.orientation *= Matrix.CreateRotationY(mouse_delta_internal.X / (EngineState.resolution.X * mouse_multi));

                if (cam.orientation.Forward.Y < .98f && mouse_delta_internal.Y > 0) {
                    cam.orientation *= Matrix.CreateFromAxisAngle(cam.orientation.Right, mouse_delta_internal.Y / (EngineState.resolution.Y * mouse_multi));
                } else if (cam.orientation.Forward.Y > -.98f && mouse_delta_internal.Y < 0) {
                    cam.orientation *= Matrix.CreateFromAxisAngle(cam.orientation.Right, mouse_delta_internal.Y / (EngineState.resolution.Y * mouse_multi));
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
        }

        public void unthreaded_update() {
            cam.position = position;

            lights[0].position = position + (cam.orientation.Right * 0.5f) + (cam.orientation.Down * 0.4f) + (cam.orientation.Forward * 0.6f);
            lights[1].position = position + (cam.orientation.Right * 0.5f) + (cam.orientation.Down * 0.4f) + (cam.orientation.Forward * 0.5f);

            lights[1].spot_info.orientation = cam.orientation * Matrix.CreateFromAxisAngle(cam.orientation.Up, MathHelper.ToRadians(5f));

        }

        public void debug_draw() { }
    }
}
