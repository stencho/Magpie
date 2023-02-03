using Magpie.Engine.Stages;
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

        float velocity { get; set; }
        Vector3 inertia_dir { get; set; }

        BoundingBox bounds { get; set; }
        Matrix world { get; }

        bool dead { get; set; }

        //updating these to use a specific object type that holds shape3D lists and approximations like overall AABBs and cylinder type stuff
        //instead of just one shape, actual lists of bone-linked hitshapes woah

        Shape3D collision { get; set; }
        Shape3D sweep_collision { get; set; }

        //need to do the same for actors 
        //and maybe brushes could do the meme of having a list of brushes in them instead for easy environmental code traversal
        //instead of many many loops

        SceneRenderInfo render_info { get; set; }
        float distance_to_camera { get; }
        Map parent_map { get; set; }
        light[] lights { get; set; }
        string model { get; set; }
        string[] textures { get; set; }
        
        string name { get; set; }

        void Update();
    }
}
