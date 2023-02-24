using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magpie.Engine.Collision;
using Magpie.Engine.Collision.Support3D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static System.Net.Mime.MediaTypeNames;
using static Magpie.GJK;

namespace Magpie.Engine.WorldElements {
    public enum collision_type {
        none,

        hitbox,
        hitbox_collection,

        mesh,
        mesh_collection,

        movebox_and_hitboxes,
        //movebox_and_mesh,
        //movebox_and_meshes
    }

    public interface collision_interface {
        public collision_type collision_type { get; }
        public void draw(Matrix world);
        public BoundingBox find_bounding_box(Matrix world);
        public BoundingBox find_bounding_box_swept(Matrix world, Vector3 sweep);
    }

    public class dummy_collision : collision_interface {
        public collision_type collision_type => collision_type.none;

        public dummy_collision(Shape3D collision) {  }

        public BoundingBox find_bounding_box(Matrix world) {
            return new BoundingBox(Vector3.Zero, Vector3.Zero);
        }
        public BoundingBox find_bounding_box_swept(Matrix world, Vector3 sweep) {
            return new BoundingBox(Vector3.Zero, Vector3.Zero);
        }

        public void draw(Matrix world) {}
    }

    public class hitbox_collision : collision_interface {
        public collision_type collision_type => collision_type.hitbox;

        public Shape3D collision;
        public hitbox_collision(Shape3D collision) { this.collision = collision; }

        public BoundingBox find_bounding_box(Matrix world) {
            return collision.find_bounding_box(world);
        }
        public BoundingBox find_bounding_box_swept(Matrix world, Vector3 sweep) {
            return collision.sweep_bounding_box(world, sweep);
        }

        public void draw(Matrix world) {
            collision.draw(world);
        }
    }
    public class hitbox_collection_collision : collision_interface {
        public collision_type collision_type => collision_type.hitbox_collection;

        public Shape3D[] collision;

        public hitbox_collection_collision(params Shape3D[] collision) { this.collision = collision; }

        public BoundingBox find_bounding_box(Matrix world) {
            return CollisionHelper.BoundingBox_around_Shapes(world, collision);
        }
        public BoundingBox find_bounding_box_swept(Matrix world, Vector3 sweep) {
            return CollisionHelper.BoundingBox_around_Shapes(world, collision);
        }

        public void draw(Matrix world) {
            foreach (Shape3D shape in collision) {
                shape.draw(world);
            }
        }
    }

    public class mesh_collision : collision_interface {
        public collision_type collision_type => collision_type.mesh;

        public ModelCollision collision;

        public mesh_collision(ModelCollision collision) { this.collision = collision; }

        public BoundingBox find_bounding_box(Matrix world) {
            return collision.get_bounds(world);
        }
        public BoundingBox find_bounding_box_swept(Matrix world, Vector3 sweep) {
            return collision.get_bounds(world);
        }

        public void draw(Matrix world) {
            collision.draw(world);
        }
    }
    public class mesh_collection_collision : collision_interface {
        public collision_type collision_type => collision_type.mesh_collection;

        public ModelCollision[] collision;

        public mesh_collection_collision(params ModelCollision[] collision) { this.collision = collision; }

        public BoundingBox find_bounding_box(Matrix world) {
            return CollisionHelper.BoundingBox_around_ModelCollisions(world, collision);
        }
        public BoundingBox find_bounding_box_swept(Matrix world, Vector3 sweep) {
            return CollisionHelper.BoundingBox_around_ModelCollisions(world, collision);
        }

        public void draw(Matrix world) {
            foreach (ModelCollision mc in collision) {
                mc.draw(world);
            }
        }
    }

    public class movebox_and_hitbox_collection_collision : collision_interface {
        public collision_type collision_type => collision_type.movebox_and_hitboxes;

        public Shape3D movebox;
        public Shape3D[] collision;

        public movebox_and_hitbox_collection_collision(Shape3D movebox, params Shape3D[] collision) { this.collision = collision; this.movebox = movebox; }

        public BoundingBox find_bounding_box(Matrix world) {
            return CollisionHelper.BoundingBox_around_BoundingBoxes(
                movebox.find_bounding_box(world),
                CollisionHelper.BoundingBox_around_Shapes(world, collision)
                );
        }
        public BoundingBox find_bounding_box_swept(Matrix world, Vector3 sweep) {
            if (sweep != Vector3.Zero) {
                return CollisionHelper.BoundingBox_around_BoundingBoxes(
                    movebox.find_bounding_box(world),
                    movebox.find_bounding_box(world * Matrix.CreateTranslation(sweep)),
                    CollisionHelper.BoundingBox_around_Shapes(world, collision),
                    CollisionHelper.BoundingBox_around_Shapes(world * Matrix.CreateTranslation(sweep), collision)
                    );
            }
            return CollisionHelper.BoundingBox_around_BoundingBoxes(
                    movebox.find_bounding_box(world),
                    CollisionHelper.BoundingBox_around_Shapes(world, collision));
        }

        public void draw(Matrix world) {
            foreach (Shape3D shape in collision) {
                shape.draw(world);
            }
        }
    }



    public class collision_info {
        public collision_interface hitbox = null;

        public bool dynamic = true;
        public bool enabled = true;        
        public bool gravity = true;

        public Vector3 velocity_normal = Vector3.Zero;
        public float velocity = 0;

        public volatile List<gjk_result> gjk_results = new List<gjk_result>();
        public volatile bool doing_collisions = false;

        public collision_info(Shape3D shape) {
            hitbox = new hitbox_collision(shape);
        }
        public collision_info(params Shape3D[] shape) {
            hitbox = new hitbox_collection_collision(shape);
        }
        public collision_info(VertexBuffer vb, IndexBuffer ib) {
            hitbox = new mesh_collision(new ModelCollision(vb, ib, -1, -1));
        }
        public collision_info(string model_name) {
            load_hitbox_from_model(ContentHandler.resources[model_name].value_gfx);
        }
        public collision_info(Model model) {
            load_hitbox_from_model(model);
        }

        void load_hitbox_from_model(Model model) {
            int c = 0;

            foreach (var mesh in model.Meshes) {
                foreach (var meshpart in mesh.MeshParts) {
                    c++;
                }                
            }

            hitbox = new mesh_collection_collision(new ModelCollision[c]);

            int m = 0;
            int mp = 0;
            c = 0;
            foreach (var mesh in model.Meshes) {
                mp = 0;
                foreach(var meshpart in mesh.MeshParts) {
                    ((mesh_collection_collision)hitbox).collision[c] = new ModelCollision(
                        meshpart.VertexBuffer,
                        meshpart.IndexBuffer,
                        m,mp);

                    mp++;
                }
                m++;
            }
        }

        public void draw_move_shapes(Matrix world) {
            hitbox.draw(world);            
        }

        public void draw_extra_collisions() {

        }

        public void internal_update() {

        }
    }
}
