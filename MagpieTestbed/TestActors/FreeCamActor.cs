using Magpie;
using Magpie.Engine;
using Magpie.Engine.Floors;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Magpie.Engine.Controls;
using static Magpie.Engine.DigitalControlBindings;

namespace MagpieTestbed.TestActors {
    [Serializable]
    class FreeCamActor : Actor {
        public Camera cam;
        public Vector3 position { get; set; } = Vector3.Zero;
        public Vector3 wants_movement { get; set; } = (Vector3.Backward + Vector3.Up) * 2;

        float movement_speed = 12f;
        float mouse_multi = 3f;


        bool camera_enabled = false;
        XYPair last_mouse_pos = XYPair.Zero;

        public FreeCamActor() {
            cam = new Camera();
        }

        public void Update() {

            if (bind_pressed("click_right")) {
                if (bind_just_pressed("click_right")) {
                    last_mouse_pos = mouse_position;
                }

                if (!bind_just_pressed("click_right")) {
                    camera_enabled = true;
                    EngineState.game.IsMouseVisible = false;

                    mouse_lock = true;
                }

            } else if (bind_just_released("click_right") || (camera_enabled && !EngineState.is_active && EngineState.was_active)) {
                camera_enabled = false;
                EngineState.game.IsMouseVisible = true;
                mouse_lock = false;
                Mouse.SetPosition(last_mouse_pos.X, last_mouse_pos.Y);                
            }

            if (camera_enabled && !bind_just_pressed("click_right") && !bind_released("click_right")) {
                cam.orientation *= Matrix.CreateRotationY(mouse_delta.X / (EngineState.resolution.X / mouse_multi));
                                               
                //first person camera pitch
                //clamped from -0.9 to 0.9 by way of only allowing the mouse to move up/down if it's below/above those values
                if (cam.orientation.Forward.Y < .98f && mouse_delta.Y > 0) {
                    cam.orientation *= Matrix.CreateFromAxisAngle(cam.orientation.Right, mouse_delta.Y / (EngineState.resolution.Y / mouse_multi));
                } else if (cam.orientation.Forward.Y > -.98f && mouse_delta.Y < 0) {                    
                    cam.orientation *= Matrix.CreateFromAxisAngle(cam.orientation.Right, mouse_delta.Y / (EngineState.resolution.Y / mouse_multi));
                } 
            }

            Vector3 mv = Vector3.Zero;

            if (bind_pressed("forward")) {
                mv += Vector3.Cross(Vector3.Up, cam.orientation.Right);
            }
            if (bind_pressed("backward")) {
                mv += -Vector3.Cross(Vector3.Up, cam.orientation.Right);
            }
            if (bind_pressed("left")) {
                mv += cam.orientation.Left;
            }
            if (bind_pressed("right")) {
                mv += cam.orientation.Right;
            }
            if (bind_pressed("up")) {
                mv += Vector3.Up;
            }
            if (bind_pressed("down")) {
                mv += Vector3.Down;
            }

            if (mv != Vector3.Zero)
                wants_movement = Vector3.Normalize(mv) * movement_speed * Clock.frame_time_delta;

            cam.position = position;
            cam.update();    
        }
        public void Draw() { }
    }
}
