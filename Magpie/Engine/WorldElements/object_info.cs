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
            if (RNG.rng_float < 0.21f) {
                lights = new light[1] {
                    new light {
                        type = LightType.POINT,
                        color = RNG.random_opaque_color(),
                        point_info = new point_info() {
                            radius = (RNG.rng_float * 15f) + 2f,
                            position = collision.position
                        }
                    }/*
                    new light {
                        type = LightType.SPOT,
                        color = RNG.random_opaque_color(),
                        spot_info = new spot_info() {
                            position = collision.position,
                            orientation = Matrix.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(RNG.rng_float_neg_one_to_one * 180f))
                        }
                    }*/
                };
            }
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
            foreach (render_info ri in render) {               
                ri.world = Matrix.CreateTranslation(collision.position + ri.render_offset) * ri.orientation;
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
