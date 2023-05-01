using Magpie;
using Magpie.Engine;
using Magpie.Engine.Collision.Support3D;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Magpie.Engine.Controls;

namespace MagpieTestbed.TestActors {
    class MoveTestActor : Actor {
        public Vector3 position { get; set; } = new Vector3(4.5738883f, 14.473223f, 2.298569f);
        public Vector3 wants_movement { get; set; } = Vector3.Zero;
        public Vector3 wants_absolute_movement { get; set; } = Vector3.Zero;
        public bool request_absolute_move { get; set; } = false;
        public bool sweep_absolute_move { get; set; } = true;

        public Matrix world => Matrix.CreateTranslation(position);

        float movement_speed = 10f;

        XYPair last_mouse_pos = XYPair.Zero;

        public Shape3D collision { get; set; }
        public Shape3D sweep_collision { get; set; }

        public light[] lights { get; set; } = new light[1] {
            new light {
                type = LightType.POINT,
                color = Color.Blue,
                point_info = new point_info() {
                    radius = 5f
                }
            }
        };

        public MoveTestActor() {
            collision = new Capsule(1.85f, 1f);
            //collision = new PointSphere(1f);
        }

        Vector3 start_pos = Vector3.Zero;

        bool flip = true;
        public void Update() {
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
                wants_movement = (Vector3.Normalize(mv) * (movement_speed * (StaticControlBinds.pressed("shift") ? 0.2f:1f)) * Clock.internal_frame_time_delta);

            
            if (wants_movement != Vector3.Zero) {
                //this.position += wants_movement;
                //wants_movement = Vector3.Zero;
            }

        }
        public void unthreaded_update() {
            lights[0].position = position;
            lights[1].position = position;
        }
        public void debug_draw() {
            collision.draw(Matrix.CreateTranslation(position));
            //sweep_collision.draw();
            //if (sweep_collision.shape == shape_type.quad)
                //Draw3D.capsule(((Capsule)collision).A+ ((Quad)sweep_collision).B, ((Capsule)collision).B + ((Quad)sweep_collision).B, collision.radius, Color.Green);
        }
    }
}
