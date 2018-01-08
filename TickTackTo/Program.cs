using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TickTackTo
{
    static class Program
    {
        const 

        /// <summary>
        /// The main entry got the application
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // create window
            GameForm gameWindow = new GameForm();

            Debug.WriteLine("Start app.");

            Game newGame = new Game(gameWindow);
            newGame.StartGame(Player.PlayerX);
            gameWindow.StartGame(newGame);

            // show form
            Application.Run(gameWindow);
        }
        /// <summary>
        /// Helper function, thanks https://stackoverflow.com/a/4135491
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

        /// <summary>
        /// replace the last occurence of a string, thanks https://stackoverflow.com/a/14826068
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="Find"></param>
        /// <param name="Replace"></param>
        /// <returns></returns>
        public static string ReplaceLastOccurrence(string Source, string Find, string Replace)
        {
            int place = Source.LastIndexOf(Find);

            if (place == -1)
                return Source;

            string result = Source.Remove(place, Find.Length).Insert(place, Replace);
            return result;
        }
    }
}
