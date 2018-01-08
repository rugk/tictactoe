using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TickTackTo
{
    public partial class GameForm : Form
    {
        private Game game;

        private Timer messageTimeoutTimer;

        // box 
        public PictureBox[,] PicField { get; set; } = new PictureBox[3, 3];

        public GameForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Prepares the UI/form to start with a game.
        /// </summary>
        /// <param name="gameInstance"></param>
        public void StartGame(Game gameInstance)
        {
            this.game = gameInstance;

            // initialize/reset PicField
            PicField = new PictureBox[3, 3] {
                { pic11, pic12, pic13 },
                { pic21, pic22, pic23 },
                { pic31, pic32, pic33 },
            };

            // reset each image to an empty element if needed
            foreach (var picEl in PicField)
            {
                picEl.Image = null;
            }

            // set player state
            this.SetPlayerState(this.game.CurrentPlayer);

            // show begin message
            this.ShowMessage("{0} begins", Program.FirstLetterToUpper(this.game.GetNameOfPlayer(this.game.CurrentPlayer)));
        }

        private void PicClicked(object sender, EventArgs e)
        {
            // we know our sender
            PictureBox clickedPic = (PictureBox) sender;
            
            // find out position of button
            int[] pos = GetPositionOfPic(clickedPic);

            if (this.game.CanSelectPicture(pos))
            {
                // change image
                if (this.game.CurrentPlayer == Player.PlayerX)
                {
                    clickedPic.Image = Properties.Resources.cross;
                }
                else
                {
                    clickedPic.Image = Properties.Resources.picO;
                }

                // change internal data
                this.game.SelectPicture(pos);
                this.game.SwitchPlayer();
            }
            else
            {
                this.ShowMessage("{0}, please click on an empty field!", Program.FirstLetterToUpper(this.game.GetNameOfPlayer(this.game.CurrentPlayer)));
            }
        }

        /// <summary>
        /// Changes the state of the UI to indicate that it is the turn of the passed player.
        /// </summary>
        /// <param name="newPlayer"></param>
        public void SetPlayerState(Player newPlayer)
        {
            // set label
            statusMessage.Text = Program.FirstLetterToUpper(this.game.GetNameOfPlayer(newPlayer));

            if (newPlayer == Player.PlayerO)
            {
                // change cursor for each item
                foreach (var picEl in PicField)
                {
                    picEl.Cursor = Program.ConvertCursor(Properties.Resources.cursor_circle);
                }
            }
            else if (newPlayer == Player.PlayerX)
            {
                // change cursor for each item
                foreach (var picEl in PicField)
                {
                    picEl.Cursor = Program.ConvertCursor(Properties.Resources.cursor_cross);
                }
            }
            else
            {
                throw new ArgumentException("player argument invalid");
            }
        }

        public void EndGameWinner(Player whoWon, int? line, int? column, int? diagonal)
        {
            string whereWon = "";

            if (line != null)
            {
                whereWon += String.Format("line {0}, ", line + 1);
            }
            if (column != null)
            {
                whereWon += String.Format("column {0}, ", column + 1);
            }
            if (diagonal != null)
            {
                whereWon += "diagonal direction, ";
            }

            // remove last two characters, which were appended
            Debug.WriteLine(whereWon);
            whereWon = whereWon.Substring(0, whereWon.Length - 2);
            Debug.WriteLine(whereWon);

            // replace last comma with and for good style
            whereWon = Program.ReplaceLastOccurrence(whereWon, ", ", " and ");

            this.ShowMessageConfirm("You won!", "{0} won in {1}!", Program.FirstLetterToUpper(this.game.GetNameOfPlayer(whoWon)), whereWon);
        }

        public void EndGameStalemate()
        {
            this.ShowMessageConfirm("Uups...", "Uuups, we are in a stalemate! I am very sorry, but nobody won.");
        }

        private int[] GetPositionOfPic(PictureBox elem)
        {
            // iterate through fields
            for (int column = 0; column < this.PicField.GetLength(0); column++)
            {
                for (int line = 0; line < this.PicField.GetLength(0); line++)
                {
                    // and compare PictureBox to see whether it's the one, which we need
                    if (elem.Equals(this.PicField[column, line]))
                    {
                        Debug.WriteLine("getPositionOfPic: line {0}, column {1} = {2}", line, column, this.PicField[column, line]);
                        return new int[2] { line, column };
                    }
                }
            }

            throw new ArgumentOutOfRangeException("could not find picture");
        }

        /// <summary>
        /// Custom message function, so message can be shown in a different/other way in the future
        /// </summary>
        /// <param name="message">the message to show</param>
        /// <param name="contentArray">all params to pass to String.Format</param>
        public void ShowMessage(string message, params object[] contentArray)
        {
            string formattedString = String.Format(message, contentArray);

            Debug.WriteLine("Show message: " + formattedString);

            // stop old timer, if neded, so that only new timer counts and the message is not hidden in the middle of nothing
            if (messageTimeoutTimer != null)
            {
                messageTimeoutTimer.Stop();
            }

            // create timer for hiding message
            messageTimeoutTimer = new Timer();
            messageTimeoutTimer.Interval = (3 * 1000); // 3 sec
            messageTimeoutTimer.Tick += (object sender, EventArgs e) =>
            {
                // hide text
                bigMessageLabel.Text = "";
            };


            // Set text.
            bigMessageLabel.Text = formattedString;

            // start timeout timer
            messageTimeoutTimer.Start();
        }

        /// <summary>
        /// Shows a message, which the user has to confirm, i.e. click on OK.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="contentArray"></param>
        public void ShowMessageConfirm(string title, string message, params object[] contentArray)
        {
            string formattedString = String.Format(message, contentArray);

            Debug.WriteLine("Show message: " + title + " - " + formattedString);

            MessageBox.Show(this, formattedString, title);
        }

        private void bigMessageLabel_Click(object sender, EventArgs e)
        {

        }
    }
}
