using Magpie;
using Magpie.Engine;
using Magpie.Engine.Brushes;
using Magpie.Engine.Collision.Support3D;
using Magpie.Engine.Physics;
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

using Magpie.Engine.DigitalBinds;
using static Magpie.Engine.InstancedBinds;
using Magpie.Engine.AnalogBinds;

namespace MagpieTestbed.TestActors {
    [Serializable]
    class FreeCamActor : Actor {
        public Camera cam;
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

        public PhysicsInfo phys_info { get; set; } = PhysicsInfo.default_static();

        bool camera_enabled = false;
        XYPair last_mouse_pos = XYPair.Zero;

        public InstancedBinds binds = new InstancedBinds(
            (bind_type.digital, digital_bind_type.keyboard, Keys.W, new string[] { "forward" }),
            (bind_type.digital, digital_bind_type.keyboard, Keys.A, new string[] { "left" }),
            (bind_type.digital, digital_bind_type.keyboard, Keys.S, new string[] { "backward" }),
            (bind_type.digital, digital_bind_type.keyboard, Keys.D, new string[] { "right" }),

            (bind_type.digital, digital_bind_type.keyboard, Keys.Space, new string[] { "up" }),
            (bind_type.digital, digital_bind_type.keyboard, Keys.C, new string[] { "down" }),

            (bind_type.digital, digital_bind_type.keyboard, Keys.LeftShift, new string[] { "boost" }),

            (bind_type.digital, digital_bind_type.mouse, MouseButtons.Left, new string[] { "fire", "picker" }),
            (bind_type.digital, digital_bind_type.mouse, MouseButtons.Right, new string[] { "mouse_aim", "context_menu" })//,

            //(bind_type.analog, analog_bind_type.mouse_delta_axis,   MouseAxis.X,            new string[] { "mouse_x" }),
            //(bind_type.analog, analog_bind_type.mouse_delta_axis,   MouseAxis.Y,            new string[] { "mouse_y" })
            );

        public FreeCamActor() {
            cam = new Camera();
            collision = new Sphere(1f);
            EngineState.player_binds_one = binds;
        }
        

        
        public void Update() {
            binds.update();
            if (binds.held("mouse_aim")) {
                if (binds.just_held("mouse_aim")) {
                    last_mouse_pos = mouse_position;
                }

                if (!binds.just_held("mouse_aim")) {
                    camera_enabled = true;
                    EngineState.game.IsMouseVisible = false;
                    mouse_lock = true;
                }

            } else if (binds.just_released("mouse_aim") || (camera_enabled && !EngineState.is_active && EngineState.was_active)) {
                camera_enabled = false;
                EngineState.game.IsMouseVisible = true;
                mouse_lock = false;
                Mouse.SetPosition(last_mouse_pos.X, last_mouse_pos.Y);                
            }

            if (camera_enabled && !binds.just_held("mouse_aim") && !binds.released("mouse_aim")) {


                cam.orientation *= Matrix.CreateRotationY(mouse_delta.X / (EngineState.resolution.X * mouse_multi));
                                               
                //first person camera pitch
                //clamped from -0.9 to 0.9 by way of only allowing the mouse to move up/down if it's below/above those values
                if (cam.orientation.Forward.Y < .98f && mouse_delta.Y > 0) {
                    cam.orientation *= Matrix.CreateFromAxisAngle(cam.orientation.Right, mouse_delta.Y / (EngineState.resolution.Y * mouse_multi));
                } else if (cam.orientation.Forward.Y > -.98f && mouse_delta.Y < 0) {                    
                    cam.orientation *= Matrix.CreateFromAxisAngle(cam.orientation.Right, mouse_delta.Y / (EngineState.resolution.Y * mouse_multi));
                } 
            }

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
                wants_movement = Vector3.Normalize(mv) * movement_speed * (binds.pressed("shift") ? 6f : 1f) * Clock.frame_time_delta;

            cam.position = position;
            cam.update();
            cam.update_projection(EngineState.resolution);
        }
        public void debug_draw() { }
    }
}
