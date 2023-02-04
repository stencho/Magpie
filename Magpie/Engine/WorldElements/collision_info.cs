using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Magpie.Engine.WorldElements {
    public class collision_info {
        public Vector3 position;

        public Matrix orientation { 
            get => move_shape.orientation; 
            set => move_shape.orientation = value; 
        }

        public Matrix world = Matrix.Identity;

        public Shape3D move_shape;

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
            this.move_shape = move_shape;
            this.position = position;
        }

        public void draw_move_shapes() {
            move_shape.draw(position);            
        }

        public void draw_extra_collisions() {

        }

        public void internal_update() {

        }
    }
}
