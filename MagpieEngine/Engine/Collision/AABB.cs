using Magpie.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie {
    public struct AABB {
        Vector3 _min;
        Vector3 _max;

        public Vector3 min { get { return _min; } set { _min = value; monogame_bb.Min = _min; } }
        public Vector3 max { get { return _max; } set { _max = value; monogame_bb.Max = _max; } }


        public AABB(Vector3 min, Vector3 max) {
            this._min = min;
            this._max = max;

            monogame_bb = new BoundingBox(min, max);
        }

        public Vector3 origin => ((_min + _max) / 2f);
        public Vector3 half_scale => _max - origin;
        public Vector3 scale => _max - _min;

        public Vector3 A => origin + half_scale;
        public Vector3 B => origin + (half_scale * (Vector3.One - (Vector3.UnitX * 2)));
        public Vector3 C => origin + (half_scale * (Vector3.One - (Vector3.UnitY * 2)));
        public Vector3 D => origin + (half_scale * (Vector3.One - (Vector3.UnitX * 2) - (Vector3.UnitY * 2)));
        public Vector3 E => origin + (half_scale * (Vector3.One - (Vector3.UnitZ * 2)));
        public Vector3 F => origin + (half_scale * (Vector3.One - (Vector3.UnitX * 2) - (Vector3.UnitZ * 2)));
        public Vector3 G => origin + (half_scale * (Vector3.One - (Vector3.UnitZ * 2) - (Vector3.UnitY * 2)));
        public Vector3 H => origin + (half_scale * (Vector3.One - (Vector3.UnitX * 2) - (Vector3.UnitY * 2) - (Vector3.UnitZ * 2)));

        public Vector3 Top => origin + (Vector3.Up * half_scale.Y);

        public BoundingBox monogame_bb;

        public static AABB operator +(AABB A, Vector3 B) {
            return new AABB { _min = A._min + B, _max = A._max + B };
        }

        public static AABB operator -(AABB A, Vector3 B) {
            return new AABB { _min = A._min - B, _max = A._max - B };
        }

        public void draw(GraphicsDevice gd, Vector3 offset, Color color, Matrix view, Matrix projection) {
            Draw3D.cube(gd, A+ offset, B+ offset, C+ offset, D+ offset, E+ offset, F+ offset, G+ offset, H+ offset, color, view, projection);
        }        
    }
}
