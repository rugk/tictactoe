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
using TicTacToe.Properties;

namespace TicTacToe
{
    public partial class GameForm : Form
    {
        private Game game;

        private Timer messageTimeoutTimer;
        private RadioButton[] playerRadioButtons;

        // box 
        public PictureBox[,] PicField { get; set; } = new PictureBox[3, 3];

        public GameForm()
        {
            InitializeComponent();

            this.playerRadioButtons = groupStartPlayer.Controls.OfType<RadioButton>().ToArray();
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
            this.ShowMessage("{0} begins", Program.FirstLetterToUpper(this.GetNameOfPlayer(this.game.CurrentPlayer)));
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

                // disable start player selection if game began
                if (this.game.GameFieldsUsed == 1)
                {
                    this.ChangeStateOfPlayerRadio(false);
                }

                // finally check whether the game ended
                game.CheckGameEnd();
            }
            else
            {
                this.ShowMessage("{0}, please click on an empty field!", Program.FirstLetterToUpper(this.GetNameOfPlayer(this.game.CurrentPlayer)));
            }
        }

        /// <summary>
        /// Changes the state of the UI to indicate that it is the turn of the passed player.
        /// </summary>
        /// <param name="newPlayer"></param>
        public void SetPlayerState(Player newPlayer)
        {
            // set label
            this.SetStatusMessage(Program.FirstLetterToUpper(this.GetNameOfPlayer(newPlayer)));

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

            // enable controls for start player again
            this.ChangeStateOfPlayerRadio(true);

            this.ShowMessageConfirm("You won!", "{0} won in {1}!", MessageBoxIcon.Information, Program.FirstLetterToUpper(this.GetNameOfPlayer(whoWon)), whereWon);
        }

        public void EndGameStalemate()
        {
            // enable controls for start player again
            this.ChangeStateOfPlayerRadio(true);

            this.ShowMessageConfirm("Uups...", "Uuups, we are in a stalemate! I am very sorry, but nobody won.", MessageBoxIcon.Information);
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
                // stop itself, so only run once
                ((System.Windows.Forms.Timer) sender).Stop();
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
        public void ShowMessageConfirm(string title, string message, MessageBoxIcon messageBoxIcon, params object[] contentArray)
        {
            string formattedString = String.Format(message, contentArray);

            Debug.WriteLine("Show message: " + title + " - " + formattedString);

            MessageBox.Show(this, formattedString, title, MessageBoxButtons.OK, messageBoxIcon);
        }
        public void ShowMessageConfirm(string title, string message, params object[] contentArray)
        {
            // use no icon by default
            ShowMessageConfirm(title, message, MessageBoxIcon.None, contentArray);
        }

        public void SetStatusMessage(string message, params object[] contentArray)
        {
            string formattedString = String.Format(message, contentArray);

            Debug.WriteLine("Modify status message: " + formattedString);

            statusMessage.Text = formattedString;
        }

        public string GetNameOfPlayer(Player player)
        {
            // also possible: return player.ToString();

            Debug.WriteLine("request name for player: {0}", player);

            switch (player)
            {
                // break not needed as early return is used here and so the break would never be reached anyway
                case Player.PlayerX:
                    return (inputPlayer1.Text != "" ? inputPlayer1.Text : "player 1");
                case Player.PlayerO:
                    return (inputPlayer2.Text != "" ? inputPlayer2.Text : "player 2");
                // PlayerNull and any other value does not need to return something useful
                case Player.PlayerNull:
                default:
                    return "unknown player";
            }
        }

        private void PlayerNamesChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("ChangePlayerNames");

            // update radio buttons
            radioStartPlayer1.Text = Program.FirstLetterToUpper(this.GetNameOfPlayer(Player.PlayerX));
            radioStartPlayer2.Text = Program.FirstLetterToUpper(this.GetNameOfPlayer(Player.PlayerO));

            // trigger update of player state
            this.SetPlayerState(this.game.CurrentPlayer);
        }

        /// <summary>
        /// Return the selected start player.
        /// </summary>
        /// <returns>Player</returns>
        public Player GetStartPlayer()
        {
            RadioButton checkedButton = groupStartPlayer.Controls.OfType<RadioButton>()
                                        .FirstOrDefault(r => r.Checked);

            if (checkedButton == radioStartPlayer1)
            {
                return Player.PlayerX;
            }
            else if (checkedButton == radioStartPlayer2)
            {
                return Player.PlayerO;
            }

            throw new InvalidOperationException("could not find selected player");
        }

        /// <summary>
        /// Sets a player. If PlayerNull is given, it chooses a random start player.
        /// </summary>
        /// <param name="player"></param>
        public void SetStartPlayer(Player player)
        {
            // remove handler to prevent that it is fired
            foreach (var radioButton in this.playerRadioButtons)
            {
                radioButton.CheckedChanged -= RadioStartPlayerChanged;
            }

            switch (player)
            {
                case Player.PlayerX:
                    radioStartPlayer1.Checked = true;
                    break;
                case Player.PlayerO:
                    radioStartPlayer2.Checked = true;
                    break;
                case Player.PlayerNull:
                    // choose random player if needed
                    int randomPlayer = Program.Random.Next(1, 3); // random number 1 or 2
                    Debug.WriteLine("Chose random start player: {0}", randomPlayer);

                    // run itself again with choosen startplayer value
                    this.SetStartPlayer((Player)randomPlayer);
                    break;
                default:
                    throw new ArgumentException("invalid player");
            }

            // reattach handler
            foreach (var radioButton in this.playerRadioButtons)
            {
                radioButton.CheckedChanged += RadioStartPlayerChanged;
            }
        }

        public void ChangeStateOfPlayerRadio(bool newState)
        {
            groupStartPlayer.Enabled = newState;
        }

        private void RadioStartPlayerChanged(object sender, EventArgs e)
        {
            // restart game with new startplayer
            this.game.StartGame();
            this.StartGame(this.game);
        }

        private void ButtonRandomStartPlayerChanged(object sender, EventArgs e)
        {
            this.SetStartPlayer(Player.PlayerNull);

            // disable button for some time to give feedback to the user that the choosing happened
            Timer buttonTimeoutTimer = new Timer();
            buttonTimeoutTimer.Interval = 750;

            // enable visual effects
            buttonRandomStartPlayer.Enabled = false;
            switch (this.GetStartPlayer())
            {
                case Player.PlayerX:
                    radioStartPlayer1.ForeColor = System.Drawing.Color.Red;
                    break;
                case Player.PlayerO:
                    radioStartPlayer2.ForeColor = System.Drawing.Color.Red;
                    break;
            }

            // define function to disable them and start Timer
            buttonTimeoutTimer.Tick += (object senderTimer, EventArgs eTimer) =>
            {
                // stop itself, so only run once
                ((System.Windows.Forms.Timer)senderTimer).Stop();

                // disable visual effects
                buttonRandomStartPlayer.Enabled = true;
                foreach (var radioButton in this.playerRadioButtons)
                {
                    radioButton.ForeColor = System.Drawing.SystemColors.ControlText;
                }
            };
            buttonTimeoutTimer.Start();

            // manually trigger change event as it is disabled in SetStartPlayer()
            RadioStartPlayerChanged(sender, e);
        }
    }
}
