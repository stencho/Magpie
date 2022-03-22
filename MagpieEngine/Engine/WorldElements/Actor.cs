using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine {
    public interface Actor {
        Vector3 position { get; set; }
        void Update();
        void Draw();
    }
}
