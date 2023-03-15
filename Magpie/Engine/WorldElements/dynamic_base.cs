using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Magpie.Engine.WorldElements {
    public  class object_info_dynamic : object_info {
        public override bool dynamic => true;
        public object_info_dynamic(Vector3 position) : base(position) {
        }

        public object_info_dynamic(Vector3 position, render_info renderinfo) : base(position, renderinfo) {
        }

        public object_info_dynamic(Vector3 position, collision_info collision_info) : base(position, collision_info) {
        }

        public object_info_dynamic(Vector3 position, render_info renderinfo, collision_info collision_info) : base(position, renderinfo, collision_info) {
        }

        public bool has_footing() {
            lock (collision.contact_points) {
                foreach (var cp in collision.contact_points) {
                    if (Vector3.Dot(cp.normal, Vector3.Up) >= .2f) {
                        return true;
                    }
                }
            }
            return false;
        }

        public override void update() {
            if (gravity) {
                if (gravity_current < 53f) {
                    gravity_current += 9f * Clock.internal_frame_time_delta;
                }
                if (has_footing() && gravity_current > 0f) gravity_current = 0f;


                wants_movement += Vector3.Down * gravity_current;
            }
            base.update();
        }
    }
}
