using System;

namespace MG2DSDFTool
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var game = new SDFTool_game())
                game.Run();
        }
    }
}
