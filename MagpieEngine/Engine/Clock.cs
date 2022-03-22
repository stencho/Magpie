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

        private static int _frame_counter = 0;

        public static int frame_rate { get; set; } = 0;
        public static int frame_rate_immediate { get; set; } = 0;

        private static double _frame_rate_timer = 0;
        private static double _frame_rate_timer_i = 0;

        public static int frame_rate_r { get; set; } = 0;
        public static int frame_rate_immediate_r { get; set; } = 0;

        private static double _frame_rate_timer_r = 0;
        private static double _frame_rate_timer_r_i = 0;

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
                game.MaxElapsedTime = new TimeSpan((long)(TimeSpan.TicksPerMillisecond * (1000f / frame_limiter_fps / 3f)));
                game.TargetElapsedTime = new TimeSpan((long)(TimeSpan.TicksPerMillisecond * (1000f / frame_limiter_fps)));
            }
            set_gametime(gt);
            

            total_ms_ignore_pause += _gt.ElapsedGameTime.TotalMilliseconds;

            if (paused ){
                return;
            }
            
            total_ms += _gt.ElapsedGameTime.TotalMilliseconds;
        }

        public static void update_fps() {           
            _frame_rate_timer += _gt.ElapsedGameTime.TotalMilliseconds;
            _frame_rate_timer_i++;

            if (_frame_rate_timer > 1000.0) {
                frame_rate_immediate = (int)(_frame_rate_timer_i );
                frame_rate = frame_rate_immediate;
                _frame_rate_timer -= 1000.0;
                _frame_rate_timer_i = 0;
            }

            frame_count++;

        }
    }
}
