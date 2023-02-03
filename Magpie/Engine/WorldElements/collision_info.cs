using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Magpie.Engine.WorldElements {
    public class collision_info {
        public Vector3 position;

        public Shape3D[] move_shapes;

        public bool dynamic = true;
        public bool enabled = true;        
        public bool gravity = true;

        public bool collides_with_move_shapes = true;
        public bool collides_with_hitboxes = true;

        public Vector3 velocity_normal = Vector3.Zero;
        public float velocity = 0;

        List<(Shape3D shape, 
              int bone_index, 
              Vector3 offset)> hitboxes = new List<(Shape3D shape, int bone_index, Vector3 offset)>();
        
        public collision_info(Shape3D move_shape, Vector3 position) {
            this.move_shapes = new Shape3D[1] { move_shape };
            this.position = position;
        }

        public collision_info(Shape3D[] move_shapes, Vector3 position) {
            this.move_shapes = move_shapes;
            this.position = position;
        }

        public void draw_move_shapes() {
            foreach(Shape3D s in move_shapes) {
                s.draw(position);
            }
        }

        public void draw_extra_collisions() {

        }

        public void internal_update() {

        }
    }
}
