using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine.Floors {
    public interface Floor {
        Vector3 position { get; set; }
        Matrix orientation { get; set; }

        float get_footing_height(Vector3 pos);
        Vector3 get_footing(float X, float Z);
        bool within_vertical_bounds(Vector3 pos);

        void Draw();
        void Update();
    }
}
