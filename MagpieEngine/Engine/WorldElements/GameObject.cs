using Magpie.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine {
    public interface GameObject {
        Vector3 position { get; set; }
        Matrix orientation { get; set; }
        void Update();
        void Draw();        
    }
}
