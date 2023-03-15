using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magpie.Engine.Collision.Solver;
using Magpie.Engine.Stages;
using Magpie.Engine.WorldElements;
using Microsoft.Xna.Framework;

namespace Magpie.Engine.Collision.Solver {
    public struct narrow_queue_obj {
        public int A;
        public List<int> B;

        public narrow_queue_obj(int a, List<int> b) {
            A = a;
            B = b;
        }
    }

    public struct contact_point {
        public int id;
        public Vector3 contact;
        public Vector3 normal;
        //public Vector3 tangent;
    }

    public class solve_result {
        public List<(int id, collision_result result)> collision_steps = new List<(int id, collision_result result)>();

        public bool solved = false;
        public int solver_iterations = 0;

        public void reset() {
            collision_steps.Clear();
            solved = false; solver_iterations = 0;
        }
    }

    public class CollisionSolver {
        BroadPhaseSolver broad;
        NarrowPhaseSolver narrow;

        public CollisionSolver() {
            broad = new BroadPhaseSolver();
            narrow = new NarrowPhaseSolver(ref broad);

            broad.set_output_solver(ref narrow);
        }

        public bool solving = false;

        public void solve() {
            World.internal_frame_probe.set("solve start");
            while (EngineState.drawing) { }
            solving = true;
            foreach (var obj in EngineState.world.current_map.game_objects.Keys) {
                if (EngineState.world.current_map.game_objects[obj].dynamic) {
                    if (EngineState.world.current_map.game_objects[obj].collision != null) {
                        EngineState.world.current_map.game_objects[obj].collision.solve.reset();
                        EngineState.world.current_map.game_objects[obj].collision.contact_points.Clear();
                        broad.queue.Enqueue(obj);
                    } else {
                        EngineState.world.current_map.game_objects[obj].position += EngineState.world.current_map.game_objects[obj].wants_movement;
                        EngineState.world.current_map.game_objects[obj].wants_movement = Vector3.Zero;
                        EngineState.world.current_map.game_objects[obj].post_solve();
                    }
                }
            }
            while (broad.queue.Count > 0 || narrow.queue.Count > 0 || broad.working || narrow.working) { }

            solving = false;
            World.internal_frame_probe.set("solve end");
        }

    }
}
