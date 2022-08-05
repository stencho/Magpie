using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine {
    public static class Clock {
        private static GameTime _gt = new GameTime();
        public static GameTime game_time => _gt;

        public static int frame_rate { get; set; } = 0;
        public static int frame_rate_immediate { get; set; } = 0;

        private static double _frame_rate_timer = 0;
        private static double _frame_rate_timer_i = 0;

        public static int frame_rate_r { get; set; } = 0;
        
        public static double total_ms { get; private set; } = 0;
        public static double total_ms_ignore_pause { get; private set; } = 0;


        public static bool paused { get; set; } = false;

        public static long frame_count { get; private set; } = 0;
        public static long frame_count_r { get; private set; } = 0;

        //FRAME LIMITER
        public static float frame_limiter_fps = 0;
        //public static float minimum_tolerated_fps = (frame_limiter_fps > 0 ? frame_limiter_fps / 3 : 20);

        public static TimeSpan frame_time_delta_ts => new TimeSpan((long)(_gt.ElapsedGameTime.TotalSeconds * 1000));
                        
        //(float)(_gt.ElapsedGameTime.TotalMilliseconds <= 1000f / //minimum_tolerated_fps 
        //: 1000f / minimum_tolerated_fps / 1000f);

        public static float frame_time_delta => paused ? 0 :
                (float)_gt.ElapsedGameTime.TotalSeconds;
        // (float)(_gtr.ElapsedGameTime.TotalMilliseconds <= //1000f / minimum_tolerated_fps
        // : 1000f / minimum_tolerated_fps / 1000f);

        public static float frame_time_delta_ms => paused ? 0 :
                     (float)_gt.ElapsedGameTime.TotalMilliseconds;
        //(float)(_gt.ElapsedGameTime.TotalMilliseconds <= 1000f / minimum_tolerated_fps 
        //  : 1000f / minimum_tolerated_fps);


        public static void set_gametime(GameTime gt) {
            _gt = gt;
        }


        public static void update(GameTime gt, Game game) {
            game.IsFixedTimeStep = frame_limiter_fps > 0;
            if (frame_limiter_fps > 0) {
                game.TargetElapsedTime = new TimeSpan(0, 0, 0, 0, (int)(1000 / frame_limiter_fps));
                game.MaxElapsedTime = new TimeSpan(0,0,0,0,(int)(1000 / frame_limiter_fps));
            }
            set_gametime(gt);
            

            total_ms_ignore_pause += _gt.ElapsedGameTime.TotalMilliseconds;

            if (paused ){
                return;
            }
            
            total_ms += _gt.ElapsedGameTime.TotalMilliseconds;
        }

        public static int FPS_buffer_length = 10;
        static double[] FPS_immediate_buffer = new double[FPS_buffer_length];

        public static void update_fps() {           
            _frame_rate_timer += _gt.ElapsedGameTime.TotalMilliseconds;
            _frame_rate_timer_i++;
            
            for (int i = 0; i< FPS_buffer_length-1; i++) {
                FPS_immediate_buffer[i + 1] = FPS_immediate_buffer[i];
            }
            FPS_immediate_buffer[0] = (1000 / Clock.frame_time_delta_ms);

            frame_rate_immediate = (int)(FPS_immediate_buffer.Aggregate((a, b) => a + b) / (double)FPS_buffer_length);

            if (_frame_rate_timer >= 1000.0) {
                frame_rate = (int)(_frame_rate_timer_i );
                
                _frame_rate_timer -= 1000.0;
                _frame_rate_timer_i = 0;
            }

            frame_count++;

        }
    }
}
