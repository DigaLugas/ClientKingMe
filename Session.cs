using KingMeServer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ClientKingMe
{
    public partial class GameSessionForm : Form
    {
        private readonly Dictionary<char, string> characterNames = new Dictionary<char, string>()
        {
            {'A', "Adilson Konrad"},
            {'B', "Beatriz Paiva"},
            {'C', "Claro"},
            {'D', "Douglas Baquiao"},
            {'E', "Eduardo Takeo"},
            {'G', "Guilherme Rey"},
            {'H', "Heredia"},
            {'J', "Joker"},
            {'K', "Karin"},
            {'L', "Leonardo Takuno"},
            {'M', "Mario Toledo"},
            {'P', "Pedreiro"},
            {'Q', "Quintas"},
            {'R', "Ranulfo"},
            {'T', "Toshio"},
        };

        private readonly Dictionary<string, string> gamePhaseNames = new Dictionary<string, string>()
        {
            {"S", "posicionamento"},
            {"P", "promoção"},
            {"V", "Votação"},
        };

        // Renamed to be more descriptive, but kept original property
        public Dictionary<string, string> ValoresJogo { get; set; }
        public Dictionary<string, string> GameSessionData { get; internal set; }

        private readonly DesignerConfigurator designer;
        private readonly GameBoard gameBoard;

        public GameSessionForm(Dictionary<string, string> valoresJogo)
        {
            this.designer = new DesignerConfigurator();
            InitializeComponent();
            ApplyCustomStyling();

            label7.Text = "";
            this.ValoresJogo = valoresJogo;
            label8.Text = "";

            // Set player information labels
            label3.Text = ValoresJogo["nomePartida"];
            label4.Text = $"Id: {ValoresJogo["idJogador"]}";
            label5.Text = $"Nome: {ValoresJogo["nomeJogador"]}";
            label6.Text = $"Senha: {ValoresJogo["senhaJogador"]}";

            // Initialize game board
            gameBoard = new GameBoard(pictureBox1);
        }

        private void ApplyCustomStyling()
        {
            // Style labels
            Label[] labelsToStyle = {
                label1, label2, label3, label4,
                label5, label6, label8, label9
            };

            int[] fontSizes = { 13, 13, 10, 10, 10, 10, 9, 9 };

            for (int i = 0; i < labelsToStyle.Length; i++)
            {
                DesignerConfigurator.StyleLabel(
                    labelsToStyle[i],
                    designer.primaryColor,
                    fontSizes[i]
                );
            }

            // Style buttons - keeping original names
            DesignerConfigurator.StyleButton(button1, designer.primaryColor, designer.accentColor, 10);
            DesignerConfigurator.StyleButton(button2, designer.primaryColor, designer.accentColor, 10);
        }

        // Keep original method name to match the designer file
        private void button1_Click(object sender, EventArgs e)
        {
            string response = Jogo.Iniciar(
                Convert.ToInt32(ValoresJogo["idJogador"]),
                ValoresJogo["senhaJogador"]
            );

            ErrorHandler.HandleServerResponse(response);
        }

        // Keep original method name to match the designer file
        private void button2_Click(object sender, EventArgs e)
        {
            string response = Jogo.ListarCartas(
                Convert.ToInt32(ValoresJogo["idJogador"]),
                ValoresJogo["senhaJogador"]
            );

            if (ErrorHandler.HandleServerResponse(response))
                return;

            UpdateCharacterList(response);
        }

        private void UpdateCharacterList(string charactersData)
        {
            label7.Text = "";
            foreach (char c in charactersData.ToCharArray())
            {
                if (characterNames.ContainsKey(c))
                {
                    label7.Text += characterNames[c] + "\n";
                }
            }
        }

        // Keep original method name to match the designer file
        private void button4_Click(object sender, EventArgs e)
        {
            label8.Text = "";
            string response = Jogo.VerificarVez(
                Convert.ToInt32(ValoresJogo["idPartida"])
            );

            if (ErrorHandler.HandleServerResponse(response))
                return;

            // Update game board visualization
            gameBoard.ProcessBoardUpdate(response);

            // Process turn information
            ProcessTurnInformation(response);
        }

        private void ProcessTurnInformation(string response)
        {
            string[] lines = response.Split(
                new[] { "\r\n" },
                StringSplitOptions.RemoveEmptyEntries
            );

            if (lines.Length == 0)
                return;

            string[] firstLine = lines[0].Split(',');

            // Update phase information
            if (firstLine.Length >= 4 && gamePhaseNames.ContainsKey(firstLine[3]))
            {
                label9.Text = "Estamos na fase de " + gamePhaseNames[firstLine[3]];
            }

            // Check if it's current player's turn
            if (firstLine.Length >= 2 && firstLine[0] == ValoresJogo["idJogador"])
            {
                HandlePlayerTurn(firstLine);
            }
            else
            {
                DisplayCurrentPlayerTurn(firstLine[0]);
            }
        }

        private void HandlePlayerTurn(string[] turnData)
        {
            label8.Text = $"ID: {ValoresJogo["idJogador"]}, sua vez {ValoresJogo["nomeJogador"]}";

            // Handle voting phase
            if (turnData.Length >= 4 && turnData[3] == ApplicationConstants.GamePhases.Voting)
            {
                var voteResult = MessageBox.Show(
                    "Voce aceita o persoagem para ser o rei?",
                    "Votação",
                    MessageBoxButtons.YesNo
                );

                Jogo.Votar(
                    Convert.ToInt32(ValoresJogo["idJogador"]),
                    ValoresJogo["senhaJogador"],
                    voteResult == DialogResult.Yes ? "s" : "n"
                );
            }
        }

        private void DisplayCurrentPlayerTurn(string currentPlayerId)
        {
            var playersData = Jogo.ListarJogadores(
                Convert.ToInt32(ValoresJogo["idPartida"])
            );

            var players = playersData.Split('\n');

            foreach (var player in players)
            {
                if (string.IsNullOrWhiteSpace(player))
                    continue;

                string[] playerDetails = player.Split(',');

                if (playerDetails.Length >= 2 && playerDetails[0] == currentPlayerId)
                {
                    label8.Text = $"ID: {playerDetails[0]}, vez do {playerDetails[1]}";
                    break;
                }
            }
        }

        // Keep original method name to match the designer file
        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null || comboBox1.SelectedIndex == -1)
            {
                ErrorHandler.ShowWarning("Selecione um personagem e um andar");
                return;
            }

            try
            {
                PerformGameAction();
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError($"Erro ao processar movimento: {ex.Message}");
            }
        }

        private void PerformGameAction()
        {
            // Get current game phase
            var phaseData = Jogo.VerificarVez(
                Convert.ToInt32(ValoresJogo["idPartida"])
            );

            var lines = phaseData.Split(
                new[] { "\r\n" },
                StringSplitOptions.RemoveEmptyEntries
            );

            if (lines.Length == 0)
            {
                ErrorHandler.ShowError("Não foi possível obter o estado atual do jogo.");
                return;
            }

            var phaseInfo = lines[0].Split(',');
            string response = string.Empty;
            var characterCode = listBox1.SelectedItem.ToString().First();

            switch (phaseInfo[3])
            {
                case ApplicationConstants.GamePhases.Positioning:
                    response = Jogo.ColocarPersonagem(
                        Convert.ToInt32(ValoresJogo["idJogador"]),
                        ValoresJogo["senhaJogador"],
                        comboBox1.SelectedIndex,
                        Convert.ToString(characterCode)
                    );
                    break;

                case ApplicationConstants.GamePhases.Promotion:
                    response = Jogo.Promover(
                        Convert.ToInt32(ValoresJogo["idJogador"]),
                        ValoresJogo["senhaJogador"],
                        characterCode.ToString()
                    );
                    break;
            }

            if (string.IsNullOrEmpty(response))
            {
                ErrorHandler.ShowWarning("Nenhuma ação válida foi encontrada.");
                return;
            }

            if (!ErrorHandler.HandleServerResponse(response))
            {
                // Refresh the board if action was successful
                button4_Click(null, EventArgs.Empty);
            }
        }

        // Keep all original event handlers exactly as they were
        private void Form2_Load(object sender, EventArgs e) { }
        private void label7_Click(object sender, EventArgs e) { }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
        private void pictureBox1_Click_1(object sender, EventArgs e) { }
    }
}