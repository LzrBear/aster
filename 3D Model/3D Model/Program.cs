using System;

namespace Asteroids
{
    static class Program
    {
        static void Main(string[] args)
        {
            using (asterGame game = new asterGame())
            {
                game.Run();
            }
        }
    }
}

