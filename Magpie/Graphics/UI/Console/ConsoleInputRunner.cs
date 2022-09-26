using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleInput;
using CSScriptLib;
using Magpie.Engine;

namespace Magpie.Graphics.UI {
    public static class ConsoleInputRunner {
        public static string code_current(ConsoleInputHandler cih) { return preamble + cih.current_input + postamble; }
        public static string code_previous(ConsoleInputHandler cih) { return preamble + cih.previous_input + postamble; }

        public static string preamble = @"
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using Magpie;
using Magpie.Engine;
using Magpie.Graphics;
using Magpie.Graphics.UI;

using static Magpie.EngineState;
using static Magpie.Engine.Log;

public class Script { 
    public void Func() {
";
        public static string postamble = @"    
}}
";

        public static void run_console_input(ConsoleInputHandler handler) {
            string input = handler.previous_input;

            input = input.TrimEnd();

            if (!input.EndsWith(";")) {
                input = input + ";";
            }

            var console_script = preamble + input + postamble;

            try {
                dynamic script = CSScript.RoslynEvaluator.LoadCode(console_script);
                
                script.Func();

                

            } catch (Exception ex) {
                string message = ex.Message.Remove(0, ex.Message.IndexOf(":") + 1);
                message = message.Remove(0, message.IndexOf(":") + 1);
                message = message.Remove(0, message.IndexOf(":") + 2);
                message = message.Replace("\n", "");

                Log.log($"[script error] {message}");
            }
        }        
    }
}
