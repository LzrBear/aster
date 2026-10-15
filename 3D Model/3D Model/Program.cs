using System;

namespace Asteroids
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (asterGame game = new asterGame())
            {
                game.Run();
            }
        }
    }
#endif
}

