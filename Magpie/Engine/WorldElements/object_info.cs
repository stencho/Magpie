using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Magpie.Engine.Collision;
using Magpie.Engine.Collision.Support3D;
using Magpie.Graphics;
using Microsoft.Xna.Framework;

namespace Magpie.Engine.WorldElements {
    public partial class object_info {
        public int id = -1;

        public Vector3 position = Vector3.Zero;
        public Vector3 wants_movement = Vector3.Zero;


        public Vector3 velocity_normal = Vector3.Zero;
        public float velocity = 0;

        public List<long> octree_base_nodes = new List<long>();

        public Vector3 scale = Vector3.One;

        public Matrix orientation = Matrix.Identity;

        public Matrix world = Matrix.Identity;

        public render_info render;

        public light[] lights;

        public collision_info collision;

        public Action update_action;
        public Action draw_action;

        public ThreadedBindManager binds = new ThreadedBindManager();

        public bool resting = false;
        public bool updated = false;

        public object_info(Vector3 position) {
            this.position = position;
            init(null, null);
        }
        public object_info(Vector3 position, render_info renderinfo) {
            this.position = position;
            init(renderinfo, new collision_info(new Sphere(1f)));
        }
        public object_info(Vector3 position, collision_info collision_info) {
            this.position = position;
            init(null, collision_info);
        }
        public object_info(Vector3 position, render_info renderinfo, collision_info collision_info) {
            this.position = position;
            init(renderinfo, collision_info);
        }

        void init(render_info render_info, collision_info collision_info) {
            this.render = render_info;
            this.collision = collision_info;

            world = Matrix.CreateScale(scale) * orientation * Matrix.CreateTranslation(position);
        }

        public bool in_frustum(BoundingFrustum frustum) {
            if (render != null) {
                if (render.in_frustum(frustum)) {
                    return true;
                }
            } else if (collision!= null) {
                if (frustum.Intersects(collision.movebox.find_bounding_box(world))) {
                    return true;
                }
            }
            
            return false;
        }

        public BoundingBox bounding_box() {
            if (collision != null) {
                if (collision.dynamic) {
                    return collision.movebox.sweep_bounding_box(world, wants_movement);
                } else {
                    return collision.movebox.find_bounding_box(world);
                }
            } else return new BoundingBox(Vector3.Zero, Vector3.Zero);
        }

        public virtual void pre_update() {
            binds.update();
        }


        public virtual void update() {

            //if (!resting)

            if (render != null)
                render.world = world;
            /*
            if (update_action != null) {
                update_action();
            }
            */
            updated = true;
        }

        public virtual void post_solve() {
            //UPDATE OCTREE POSITIONS HERE
            world = Matrix.CreateScale(scale) * orientation * Matrix.CreateTranslation(position);
            updated = false;
        }


        public virtual void draw() {
            if (render != null) { 
                render.prepass();

                render.draw();
            }
            if (collision != null) {
                collision.movebox.draw(world);
                Draw3D.cube(bounding_box(), Color.Red);
            }
            Draw3D.text_3D(EngineState.spritebatch, id.ToString(), "pf", bounding_box().Max, -EngineState.camera.direction, 1f, Color.Black);
            if (draw_action != null) draw_action();
        }
        public void draw_to_light(light light) {
            if (render != null) {
                render.draw_to_light(light);
            }
        }

    }
}
