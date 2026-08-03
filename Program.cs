using System;
using System.IO;

namespace EscapeSpaceStation
{
    /// <summary>
    /// Application entry point. Boots the MonoGame Game1 instance.
    ///
    /// Because the project is built as WinExe (no console window), any
    /// unhandled exception during startup would otherwise just silently
    /// close the process with no visible error. To make failures diagnosable,
    /// this wraps startup in a try/catch that writes a crash log next to the
    /// executable and shows a native message box with the error.
    /// </summary>
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                using (var game = new Game1())
                {
                    game.Run();
                }
            }
            catch (Exception ex)
            {
                string logPath = Path.Combine(AppContext.BaseDirectory, "crash_log.txt");
                try
                {
                    File.WriteAllText(logPath, ex.ToString());
                }
                catch { /* if we can't even write the log, still try to show the message box */ }

                System.Windows.Forms.MessageBox.Show(
                    "המשחק נתקל בשגיאה בעת ההפעלה:\n\n" + ex.Message +
                    "\n\nפרטים מלאים נשמרו בקובץ crash_log.txt לצד קובץ ה-exe.",
                    "Escape Space Station - שגיאה",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }
}
