using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Magpie.Engine.Floors {
    class SegmentedHeightfield : Floor {
        public Vector3 position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Matrix orientation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Draw() {
            throw new NotImplementedException();
        }

        public Vector3 get_footing(float X, float Z) {
            throw new NotImplementedException();
        }

        public float get_footing_height(float X, float Z) {
            throw new NotImplementedException();
        }

        public void Update() {
            throw new NotImplementedException();
        }

        public bool within_vertical_bounds(Vector2 XZ) {
            throw new NotImplementedException();
        }
    }
}
