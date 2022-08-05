using Magpie;
using Magpie.Engine;
using Magpie.Engine.Collision.Support3D;
using Magpie.Engine.Physics;
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
        public Vector3 wants_absolute_movement { get; set; } = Vector3.Zero;
        public bool request_absolute_move { get; set; } = false;
        public bool sweep_absolute_move { get; set; } = true;

        public Matrix world => Matrix.CreateTranslation(position);

        float movement_speed = 10f;

        XYPair last_mouse_pos = XYPair.Zero;

        public Shape3D collision { get; set; }
        public Shape3D sweep_collision { get; set; }

        public PhysicsInfo phys_info { get; set; } = PhysicsInfo.default_static();



        public MoveTestActor() {
            collision = new Capsule(1.85f, 1f);
            phys_info.stick_to_ground = true;
        }

        Vector3 start_pos = Vector3.Zero;

        bool flip = true;
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
            if (bind_pressed("t_up")) {
                mv += Vector3.Up;
            }
            if (bind_pressed("t_down")) {
                mv += Vector3.Down;
            }

            if (bind_just_pressed("test_sweep")) {
                start_pos = position;
            }
            if (bind_just_released("test_sweep")) {
                flip = true;

                position = start_pos;
                wants_movement = Vector3.Zero;

            }

            if (bind_pressed("test_sweep")) {
                request_absolute_move = true;
                sweep_absolute_move = true;
                if (flip) {
                    wants_absolute_movement = start_pos + (Vector3.Right * 20) + (Vector3.Forward * 20) + (Vector3.Down  * 50);

                } else {
                    wants_absolute_movement = start_pos;
                }

                flip = !flip;
            } else {

                if (mv != Vector3.Zero)
                    wants_movement = (Vector3.Normalize(mv) * movement_speed * Clock.frame_time_delta);

            }

        }
        public void debug_draw() {
            collision.draw();
            //sweep_collision.draw();
            //if (sweep_collision.shape == shape_type.quad)
                //Draw3D.capsule(((Capsule)collision).A+ ((Quad)sweep_collision).B, ((Capsule)collision).B + ((Quad)sweep_collision).B, collision.radius, Color.Green);
        }
    }
}
