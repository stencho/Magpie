using Magpie;
using Magpie.Engine;
using Magpie.Engine.Collision;
using Magpie.Engine.Collision.Support3D;
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

namespace MagpieTestbed.TestObjects {
    [Serializable]
    class TestSphere : GameObject {
        public Vector3 position { get; set; } = Vector3.Up * 3f;
        public Matrix orientation { get; set; } = Matrix.Identity;
        public Vector3 scale { get; set; } = Vector3.One;
        public Matrix world => Matrix.CreateScale(scale) * orientation * Matrix.CreateTranslation(position) ;

        public float radius = 1f;

        public light[] lights { get; set; }
        public BoundingBox bounds { get; set; }

        public string model { get; set; } = "sphere";
        public string[] textures { get; set; } = new string[] { "OnePXWhite" };

        public Shape3D collision { get; set; }
        public Shape3D sweep_collision { get; set; }

        public Map parent_map { get; set; }
        
        public float velocity { get; set; } = 0f;
        public Vector3 inertia_dir { get; set; } = Vector3.Zero;

        public string name { get; set; }

        public bool dead { get; set; } = false;

        public float distance_to_camera => Vector3.Distance(CollisionHelper.closest_point_on_AABB(EngineState.camera.position, bounds.Min, bounds.Max), EngineState.camera.position);

        public SceneRenderInfo render_info { get; set; }

        public TestSphere() {
            collision = new Sphere(1f);
            render_info = new SceneRenderInfo() {
                model = this.model,
                textures = this.textures,
                tint = Color.White,
                render = true
            };
        }

        public void Update() {
            render_info.model = this.model;
            textures = this.textures;
            bounds = CollisionHelper.BoundingBox_around_sphere((Sphere)collision, Matrix.Identity);
        }
    }
}
