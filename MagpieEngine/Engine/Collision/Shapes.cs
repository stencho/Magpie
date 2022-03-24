using Magpie;
using Magpie.Engine;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Magpie.GJK;

namespace Magpie {
    public enum shape_type {
        cube,
        polyhedron,
        quad,
        tri,
        capsule,
        line,
        sphere
    }

    public interface shape3D {
        Matrix orientation { get; set; }
        Vector3 position { get; set; }
        Vector3 start_point { get; }

        AABB find_bounding_box();
        
        shape_type shape { get; }
             
        void draw();
    }  
}
