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
        Vector3 scale { get; set; }
        BoundingBox bounds { get; set; }
        Matrix world { get; }

        shape3D collision { get; set; }

        string model { get; set; }
        string[] textures { get; set; }

        void Update();
    }
}
