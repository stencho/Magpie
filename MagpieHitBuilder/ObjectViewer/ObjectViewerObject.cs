using Magpie;
using Magpie.Engine;
using Magpie.Engine.Physics;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magpie.Engine.Collision;
using Magpie.Engine.Collision.Support3D;
using Magpie.Engine.Stages;

namespace MagpieHitBuilder.ObjectViewer {
    class ObjectViewerObject : GameObject {
        public Vector3 position { get; set; } = Vector3.Zero;

        Camera camera;

        public Vector3 camera_position => camera.position;

        public Matrix orientation { get; set; } = Matrix.Identity;
        public Vector3 scale { get; set; } = Vector3.One;

        public float velocity { get; set; } = 0f;
        public Vector3 inertia_dir { get; set; } = Vector3.Zero;
        public BoundingBox bounds { get; set; }

        public Matrix world => Matrix.CreateScale(scale) * orientation;

        public Shape3D collision { get; set; }
        public Shape3D sweep_collision { get; set; }

        public PhysicsInfo phys_info { get; set; }

        public string model { get; set; }
        public string[] textures { get; set; }

        public string name { get; set; }

        public bool dead { get; set; } = false;

        public Map parent_map { get; set; }

        public Color tint { get; set; }


        public CollisionList collisions { get; set; } = new CollisionList();
        /*
        public ObjectViewerObject(params (string, shape3D)[] collisions) {
            collisions.Add();
        }
        */

        public ObjectViewerObject() {
            //collisions.add("testcube", new Cube(10f));
            //collisions.add("testcube2", new Cube(50f));
        }

        public void draw() {

        }
        public void debug_draw() {

        }

        public void Update() {
        }
    }
}
