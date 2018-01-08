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
            bool hadPreviousGame = false;
            if (this.game != null)
            {
                hadPreviousGame = true;
            }

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

            // show start message, if not show automatically by showing the form
            if (hadPreviousGame)
            {
                GameForm_Shown(null, null);
            }
        }

        private void PicClicked(object sender, EventArgs e)
        {
            // we know our sender
            PictureBox clickedPic = (PictureBox) sender;
            
            // find out position of button
            int[] pos = getPositionOfPic(clickedPic);

            if (this.game.CanSelectPicture(pos))
            {
                // change image
                if (this.game.CurrentPlayer == Player.PlayerX)
                {
                    clickedPic.Image = Properties.Resources.picX;
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

            this.ShowMessage("{0} won in {1}!", Program.FirstLetterToUpper(this.game.GetNameOfPlayer(whoWon)), whereWon);
        }

        public void EndGameStalemate()
        {
            this.ShowMessage("Uuups, we are in a stalemate! I am very sorry, but nobody won.");
        }

        private int[] getPositionOfPic(PictureBox elem)
        {
            // iterate through fields
            for (int column = 0; column < this.PicField.GetLength(0); column++)
            {
                for (int line = 0; line < this.PicField.GetLength(0); line++)
                {
                    Debug.WriteLine("{0} == {1} ??", elem, this.PicField[column, line]);
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
            MessageBox.Show(this, String.Format(message, contentArray));
        }

        /// <summary>
        /// show game beginener message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameForm_Shown(object sender, EventArgs e)
        {
            this.ShowMessage("{0} begins", Program.FirstLetterToUpper(this.game.GetNameOfPlayer(this.game.CurrentPlayer)));
        }
    }
}
