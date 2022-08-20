using Magpie;
using Magpie.Engine;
using Magpie.Engine.Collision;
using Magpie.Engine.Collision.Support3D;
using Magpie.Engine.Physics;
using Magpie.Engine.Stages;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Magpie.Engine.Controls;
using static Magpie.Engine.DigitalControlBindings;
using static Magpie.GJK;

namespace MagpieTestbed.TestObjects {
    [Serializable]
    class TestPoo : GameObject {
        public Vector3 position { get; set; } = Vector3.Up * 3f;
        public Matrix orientation { get; set; } = Matrix.Identity;
        public Vector3 scale { get; set; } = Vector3.One;
        public Matrix world => Matrix.CreateScale(scale) * orientation * Matrix.CreateTranslation(position) ;

        public float radius = 1f;

        public BoundingBox bounds { get; set; }

        public string model { get; set; } = "sphere";
        public string[] textures { get; set; } = new string[] { "OnePXWhite" };

        public string name { get; set; }

        public Shape3D collision { get; set; }
        public Shape3D sweep_collision { get; set; }

        public PhysicsInfo phys_info { get; set; } = PhysicsInfo.default_grav_only();

        public float velocity { get; set; } = 0f;
        public Vector3 inertia_dir { get; set; } = Vector3.Zero;

        public bool dead { get; set; } = false;

        public Map parent_map { get; set; }

        double lifetime = 0;

        public Color tint { get; set; } = Color.SaddleBrown;
        
        public TestPoo() {
             collision = new Sphere(radius);
        }

        Vector3 last_pos;
        public void Update() {
            last_pos = this.position;

            this.position += inertia_dir * velocity * Clock.frame_time_delta;
            collision.position = this.position;

            if (velocity > 0) {
                foreach (GameObject go in parent_map.objects.Values) {
                    if (go.name.StartsWith("test_cube")) {
                        gjk_result r = gjk_intersects(collision, go.collision, world, go.world);
                        if (r.distance < 0.3f || r.hit) {
                            velocity = 0;
                            this.position = last_pos;
                            collision.position = last_pos;
                        }
                    }
                }
            }

            if (lifetime > 20000 || this.position.Y <= 49f || this.position.X < -26f || this.position.X > 26f || this.position.Z > 51f || this.position.Z < -1f)
                dead = true;
            
            bounds = CollisionHelper.BoundingBox_around_sphere((Sphere)collision, world);
            lifetime += Clock.frame_time_delta_ms;
        }
    }
}
