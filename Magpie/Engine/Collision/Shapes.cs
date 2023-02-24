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
        cylinder,
        line,
        sphere,
        dummy
    }

    public interface Shape3D {
        Vector3 start_point { get; }
        Vector3 center { get; }

        BoundingBox find_bounding_box(Matrix world);
        BoundingBox sweep_bounding_box(Matrix world, Vector3 sweep);

        shape_type shape { get; }

        Vector3 support(Vector3 direction, Vector3 sweep);

        void draw(Matrix world);
    }  
}
