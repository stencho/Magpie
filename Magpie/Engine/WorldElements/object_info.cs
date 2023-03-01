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
using static Magpie.Engine.Collision.MixedCollision;

namespace Magpie.Engine.WorldElements {
    public partial class object_info {
        public int id = -1;

        public Vector3 position = Vector3.Zero;
        public Vector3 wants_movement = Vector3.Zero;

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

        void init(render_info render_info, collision_info collision_info) {
            this.render = render_info;
            this.collision = collision_info;
        }

        public bool in_frustum(BoundingFrustum frustum) {
            if (render != null) {
                if (render.in_frustum(frustum)) {
                    return true;
                }
            } else if (collision!= null) {
                if (frustum.Intersects(collision.hitbox.find_bounding_box(world))) {
                    return true;
                }
            }
            
            return false;
        }


        public virtual void update() {
            if (!resting)
                world = Matrix.CreateScale(scale) * orientation * Matrix.CreateTranslation(position);

            if (render != null)
                render.world = world;
            /*
            if (update_action != null) {
                update_action();
            }
            */
        }


        public virtual void draw() {
            if (render != null) { 
                render.prepass();

                render.draw();
            }
            if (draw_action != null) draw_action();
        }
        public void draw_to_light(light light) {
            if (render != null) {
                render.draw_to_light(light);
            }
        }

    }
}
