using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using static System.Windows.Forms.DataFormats;
using static Magpie.Engine.Collision.Collision2D;

namespace Magpie.Engine.Collision.Solver {


    internal class NarrowPhaseSolver {
        public volatile ConcurrentQueue<narrow_queue_obj> queue = new ConcurrentQueue<narrow_queue_obj>();

        Thread solver_thread = null;

        BroadPhaseSolver parent_solver;

        public NarrowPhaseSolver(ref BroadPhaseSolver parent_solver) {
            this.parent_solver = parent_solver;

            this.solver_thread = new Thread(thread_loop);
            this.solver_thread.Start();
        }

        public bool working = false;

        void thread_loop() {

            while (EngineState.running) {

                if (queue.Count > 0) {
                    working = true;
                    narrow_queue_obj nqo;
                    if (!queue.TryDequeue(out nqo)) continue;
                    if (nqo.A == 0) continue;

                    World.internal_frame_probe.set("solving nqo " + nqo.A.ToString());

                    Shape3D shape_a = EngineState.world.current_map.game_objects[nqo.A].collision.movebox;

                    if (EngineState.world.current_map.game_objects[nqo.A].wants_movement.contains_nan()) {
                        EngineState.world.current_map.game_objects[nqo.A].wants_movement = Vector3.Zero;
                    }

                    iterate:
                    if (!EngineState.running) return;

                    bool hit = false;

                    foreach (int target in nqo.B) {
                        if (!EngineState.running) return;

                        Shape3D shape_b = EngineState.world.current_map.game_objects[target].collision.movebox;
                        collision_result 
                            result = GJK.gjk_intersects(
                                   shape_a, shape_b,
                                   EngineState.world.current_map.game_objects[nqo.A].world,
                                   EngineState.world.current_map.game_objects[target].world,
                                   EngineState.world.current_map.game_objects[nqo.A].wants_movement,
                                   EngineState.world.current_map.game_objects[target].wants_movement);


                        if (result.intersects) {
                            hit = true;

                            if (Vector3.Dot(EngineState.world.current_map.game_objects[nqo.A].wants_movement, result.penetration_scalar) < Math3D.epsilon) {
                                EngineState.world.current_map.game_objects[nqo.A].wants_movement += result.penetration_scalar;

                            } else {
                                result = GJK.swept_gjk_intersects_with_halving(
                                   shape_a, shape_b,
                                   EngineState.world.current_map.game_objects[nqo.A].world,
                                   EngineState.world.current_map.game_objects[target].world,
                                   EngineState.world.current_map.game_objects[nqo.A].wants_movement,
                                   EngineState.world.current_map.game_objects[target].wants_movement);

                                EngineState.world.current_map.game_objects[nqo.A].wants_movement = result.sweep_end + result.penetration_scalar + result.sweep_slide;
                            }

                            
                            if (EngineState.world.current_map.game_objects[nqo.A].wants_movement.contains_nan() ||
                                EngineState.world.current_map.game_objects[nqo.A].wants_movement.Length() < Math3D.epsilon) {
                                EngineState.world.current_map.game_objects[nqo.A].wants_movement = Vector3.Zero;
                                break;
                            }
                        } 
                    }
                    
                    EngineState.world.current_map.game_objects[nqo.A].collision.solve.solver_iterations++;

                    EngineState.world.current_map.game_objects[nqo.A].post_solve();

                    if (hit && EngineState.world.current_map.game_objects[nqo.A].collision.solve.solver_iterations < 4)
                        goto iterate;

                    EngineState.world.current_map.game_objects[nqo.A].position += EngineState.world.current_map.game_objects[nqo.A].wants_movement;
                    EngineState.world.current_map.game_objects[nqo.A].wants_movement = Vector3.Zero;

                    EngineState.world.current_map.game_objects[nqo.A].post_solve();


                } else { working = false; }
            }
        }

    }
}
