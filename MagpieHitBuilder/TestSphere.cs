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

namespace MagpieHitBuilder.TestObjects {
    [Serializable]
    class TestSphere : GameObject {
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

        public PhysicsInfo phys_info { get; set; } = PhysicsInfo.default_static();

        public Map parent_map { get; set; }

        public float velocity { get; set; } = 0f;
        public Vector3 inertia_dir { get; set; } = Vector3.Zero;

        public Color tint { get; set; }

        public bool dead { get; set; } = false;

        public TestSphere() {
             collision = new Cube(scale);
        }

        public void Update() {
            bounds = CollisionHelper.BoundingBox_around_OBB((Cube)collision, world);
        }
    }
}
