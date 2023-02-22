using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Magpie.Engine.Collision;
using Magpie.Engine.Collision.Support3D;
using Magpie.Graphics;
using Microsoft.Xna.Framework;

namespace Magpie.Engine.WorldElements {
    public class object_info {
        public Vector3 position {
            get => collision.position;
            set => collision.position = value;
        }

        public render_info[] render;
        public collision_info collision;
        public light[] lights;

        public ModelCollision[] testc;

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
            testc = new ModelCollision[((render_info_model)render[0]).model.Meshes.Count];

            int v = 0;
            
            foreach(var mesh in ((render_info_model)render[0]).model.Meshes) {
                
                testc[v] = new ModelCollision(
                    mesh.MeshParts[0].VertexBuffer,
                    mesh.MeshParts[0].IndexBuffer);
                v++;

            }

        }

        void init(render_info[] render_info, collision_info collision_info) {
            this.render = render_info;
            this.collision = collision_info;


        }

        public bool in_frustum(BoundingFrustum frustum) {
            foreach (render_info ri in render) {
                if (ri.in_frustum(frustum)) {
                    return true;
                }
            }

            return false;
        }

        public void update() {
            collision.world = Matrix.CreateTranslation(position) * collision.orientation;
            foreach (render_info ri in render) {               
                ri.world = Matrix.CreateScale(ri.scale) * ri.orientation * Matrix.CreateTranslation(position + ri.render_offset);
            }
        }


        public void draw() {
            foreach (render_info ri in render) {
                ri.prepass();
            }
            foreach (render_info ri in render) {

                ri.draw();
                
                if (testc != null) {
                    foreach (ModelCollision mc in testc) {
                        mc.draw(collision.world);
                    }
                }
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
