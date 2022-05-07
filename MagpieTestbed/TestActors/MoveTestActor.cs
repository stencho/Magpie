using Magpie;
using Magpie.Engine;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Magpie.Engine.Controls;
using static Magpie.Engine.DigitalControlBindings;

namespace MagpieTestbed.TestActors {
    class MoveTestActor : Actor {
        public Vector3 position { get; set; } = Vector3.Zero;
        public Vector3 wants_movement { get; set; } = Vector3.Zero;

        float movement_speed = 12f;

        XYPair last_mouse_pos = XYPair.Zero;

        public MoveTestActor() {
        }

        public void Update() {
            
            Vector3 mv = Vector3.Zero;
            
            if (bind_pressed("t_forward")) {
                mv += Vector3.Forward;
            }
            if (bind_pressed("t_backward")) {
                mv += Vector3.Backward;
            }
            if (bind_pressed("t_left")) {
                mv += Vector3.Left;
            }
            if (bind_pressed("t_right")) {
                mv += Vector3.Right;
            }

            if (mv != Vector3.Zero)
                wants_movement =(Vector3.Normalize(mv) * movement_speed * Clock.frame_time_delta);

        }
        public void Draw() {
            Draw3D.sphere(position + (Vector3.Up * 1), 1f, Color.LightGreen);
        }
    }
}
