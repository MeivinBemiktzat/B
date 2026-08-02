using System;

namespace EscapeSpaceStation
{
    /// <summary>
    /// Application entry point. Boots the MonoGame Game1 instance.
    /// </summary>
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new Game1())
            {
                game.Run();
            }
        }
    }
}
