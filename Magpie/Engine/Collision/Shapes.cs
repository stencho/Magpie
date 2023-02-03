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
        Matrix orientation { get; set; }
        Vector3 position { get; set; }
        Vector3 start_point { get; }

        BoundingBox find_bounding_box();

        shape_type shape { get; }

        float radius { get; set; }

        //VertexBuffer debug_vertex_buffer { get; }
        //IndexBuffer debug_index_buffer { get; }

        void draw(Vector3 offset);
    }  
}
