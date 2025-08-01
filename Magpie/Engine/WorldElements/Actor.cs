﻿using Magpie.Engine.Brushes;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine {
    public interface Actor {
        Vector3 position { get; set; }
        Vector3 wants_movement { get; set; }

        bool request_absolute_move { get; set; }
        bool sweep_absolute_move { get; set; }
        Vector3 wants_absolute_movement { get; set; }

        Matrix world { get; }

        Shape3D collision { get; set; }
        Shape3D sweep_collision{ get; set; }
        light[] lights { get; set; }

        void Update();
        void unthreaded_update();
        void debug_draw();
    }
}
