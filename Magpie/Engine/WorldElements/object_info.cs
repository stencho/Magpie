using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Magpie.Engine.Collision.Support3D;
using Magpie.Graphics;
using Microsoft.Xna.Framework;

namespace Magpie.Engine.WorldElements {
    public class object_info {
        public render_info[] render;
        public collision_info collision;
        public light[] lights;
        
        public object_info(Vector3 position) {
            init(new render_info[] {
                    new render_info_model("sphere")
                 },
                 new collision_info(new Sphere(1f), position)
            );
        }
        public object_info(Vector3 position, render_info renderinfo) {
            init(new render_info[] {
                    renderinfo
                 },
                 new collision_info(new Sphere(1f), position)
            );
        }

        void init(render_info[] render_info, collision_info collision_info) {
            this.render = render_info;
            this.collision = collision_info;
            

        }


        void fit_bound_sphere() {
            foreach (render_info ri in render) {
                ri.render_bounds = new BoundingSphere(collision.position + ri.render_offset, 1f);
            }
        }

        public bool in_frustum(BoundingFrustum frustum) {
            foreach (render_info ri in render) {

            }
            return false;
        }

        public void update() {
            collision.world = Matrix.CreateTranslation(collision.position) * collision.orientation;
            foreach (render_info ri in render) {               
                ri.world = Matrix.CreateScale(ri.scale) * ri.orientation * Matrix.CreateTranslation(collision.position + ri.render_offset);
            }
        }

        public void draw() {
            foreach (render_info ri in render) {


                ri.draw();
                
                //this.collision.draw_move_shapes();
            }
        }
        public void draw_to_light(light light) {
            foreach (render_info ri in render) {
                ri.draw_to_light(light);
            }
        }

    }
}
