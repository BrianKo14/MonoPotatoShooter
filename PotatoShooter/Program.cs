using System;
using System.Windows.Forms;

namespace PotatoShooter
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            //using (var game = new Game1())
            //game.Run();
            using (var start = new Start())
                start.ShowDialog();//Application.Run(start);
        }
    }
#endif
}
