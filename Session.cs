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
            {'K', "Karin"},
            {'L', "Leonardo Takuno"},
            {'M', "Mario Toledo"},
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

        // MCTS components
        private readonly MCTSAgent mctsAgent;
        private readonly GameStateAdapter gameStateAdapter;
        private List<char> availableCharacters = new List<char>();
        private string currentBoardState = string.Empty;
        private string currentGamePhase = ApplicationConstants.GamePhases.Positioning;

        private System.Windows.Forms.Timer aiTimer;
        private bool autoPlayEnabled = false;
        private Label aiStatusLabel;
        private const int AI_CHECK_INTERVAL = 5000;


        public GameSessionForm(Dictionary<string, string> valoresJogo)
        {
            this.designer = new DesignerConfigurator();
            InitializeComponent();
            ApplyCustomStyling();

            //label7.Text = "";
            this.ValoresJogo = valoresJogo;
            label8.Text = "";

            aiTimer = new System.Windows.Forms.Timer();
            aiTimer.Interval = AI_CHECK_INTERVAL;
            aiTimer.Tick += AiTimer_Tick;


            // Set player information labels
            label3.Text = ValoresJogo["nomePartida"];
            label4.Text = $"Id: {ValoresJogo["idJogador"]}";
            label5.Text = $"Nome: {ValoresJogo["nomeJogador"]}";
            label6.Text = $"Senha: {ValoresJogo["senhaJogador"]}";

            // Initialize game board
            gameBoard = new GameBoard(pictureBox1);

            // Initialize MCTS components
            mctsAgent = new MCTSAgent(int.Parse(ValoresJogo["idJogador"]), 2000);
            gameStateAdapter = new GameStateAdapter();


            aiStatusLabel = new Label
            {
                Text = "AI: Inativo",
                Location = new Point(button2.Location.X, button2.Location.Y + button2.Size.Height + 20),
                AutoSize = true,
                ForeColor = Color.DarkGray
            };
            this.Controls.Add(aiStatusLabel);

        }
        private void button2_Click(object sender, EventArgs e)
        {
            autoPlayEnabled = !autoPlayEnabled;

            if (autoPlayEnabled)
            {
                button2.Text = "Desativar Auto-Play";
                aiStatusLabel.Text = "AI: Monitorando";
                aiStatusLabel.ForeColor = Color.Green;
                aiTimer.Start();

                // Check immediately if it's our turn
                AiTimer_Tick(null, EventArgs.Empty);
            }
            else
            {
                button2.Text = "Ativar Auto-Play";
                aiStatusLabel.Text = "AI: Inativo";
                aiStatusLabel.ForeColor = Color.DarkGray;
                aiTimer.Stop();
            }
        }

        private void AiTimer_Tick(object sender, EventArgs e)
        {
            if (!autoPlayEnabled)
                return;

            try
            {
                // Check if it's our turn
                string turnInfo = Jogo.VerificarVez(Convert.ToInt32(ValoresJogo["idPartida"]));
                if (ErrorHandler.HandleServerResponse(turnInfo))
                    return;

                string[] lines = turnInfo.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 0)
                    return;

                string[] firstLine = lines[0].Split(',');
                if (firstLine.Length < 2 || firstLine[0] != ValoresJogo["idJogador"])
                    return; // Not our turn

                // It's our turn! Execute AI move
                aiStatusLabel.Text = "AI: Pensando...";
                aiStatusLabel.ForeColor = Color.Blue;

                // Update UI to reflect current game state
                currentBoardState = turnInfo;
                if (firstLine.Length >= 4)
                {
                    currentGamePhase = firstLine[3];
                }

                // Make sure we have available characters
                if (availableCharacters.Count == 0)
                {
                    string charactersData = Jogo.ListarCartas(
                        Convert.ToInt32(ValoresJogo["idJogador"]),
                        ValoresJogo["senhaJogador"]
                    );

                    if (!ErrorHandler.HandleServerResponse(charactersData))
                    {
                        availableCharacters = charactersData.ToCharArray().ToList();
                    }
                }

                // Execute AI move with a slight delay to make it visible in the UI
                this.BeginInvoke(new Action(() => {
                    ExecuteAiMove();
                }));
            }
            catch (Exception ex)
            {
                aiStatusLabel.Text = "AI: Erro";
                aiStatusLabel.ForeColor = Color.Red;
                ErrorHandler.ShowError($"Erro no timer AI: {ex.Message}");
            }
        }

        private void ExecuteAiMove()
        {
            try
            {
                // Obter a lista atualizada de personagens já colocados no tabuleiro
                string boardState = Jogo.VerificarVez(
                    Convert.ToInt32(ValoresJogo["idPartida"])
                );

                if (ErrorHandler.HandleServerResponse(boardState))
                    return;

                currentBoardState = boardState;

                // Na fase de posicionamento, precisamos obter todos os personagens
                // que ainda não estão no tabuleiro
                if (currentGamePhase == ApplicationConstants.GamePhases.Positioning)
                {
                    // Dicionário para armazenar os personagens já colocados no tabuleiro
                    var charactersOnBoard = new HashSet<char>();

                    string[] lines = boardState.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        string[] parts = line.Split(',');
                        if (parts.Length >= 2 && parts[1].Length > 0)
                        {
                            char characterCode = parts[1][0];
                            charactersOnBoard.Add(characterCode);
                        }
                    }

                    // Lista de todos os personagens disponíveis
                    var allCharacters = new List<char>() { 'A', 'B', 'C', 'D', 'E', 'G', 'H', 'K', 'L', 'M', 'Q', 'R', 'T' };

                    // Personagens disponíveis são todos que ainda não estão no tabuleiro
                    availableCharacters = allCharacters.Where(c => !charactersOnBoard.Contains(c)).ToList();
                }

                // Create game state for MCTS
                var gameState = gameStateAdapter.CreateGameState(
                    ValoresJogo,
                    currentGamePhase,
                    availableCharacters,
                    currentBoardState
                );

                // Get best move from MCTS
                var bestMove = mctsAgent.MakeMove(gameState);
                if (bestMove == null)
                {
                    aiStatusLabel.Text = "AI: Sem movimentos";
                    aiStatusLabel.ForeColor = Color.Orange;

                    // CORREÇÃO: Adicionar log detalhado para debug do problema
                    string debugInfo = $"Fase: {currentGamePhase}, Personagens disponíveis: {availableCharacters.Count}, " +
                                       $"BoardState lines: {currentBoardState.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Length}";
                    ErrorHandler.ShowWarning($"O AI não conseguiu determinar um movimento. Info: {debugInfo}");
                    return;
                }

                // Convert move to client format
                string moveData = gameStateAdapter.ConvertMoveToClientFormat(bestMove);
                string moveDescription = mctsAgent.GetMoveDescription(gameState, bestMove);

                // Display what the AI is doing
                aiStatusLabel.Text = $"AI: {moveDescription}";

                // Execute the move
                string response = string.Empty;

                switch (currentGamePhase)
                {
                    case ApplicationConstants.GamePhases.Positioning:
                        string[] parts = moveData.Split(',');
                        if (parts.Length >= 2)
                        {
                            int floor = int.Parse(parts[0]);
                            string character = parts[1];
                            response = Jogo.ColocarPersonagem(
                                Convert.ToInt32(ValoresJogo["idJogador"]),
                                ValoresJogo["senhaJogador"],
                                floor,
                                character
                            );
                        }
                        break;

                    case ApplicationConstants.GamePhases.Promotion:
                        response = Jogo.Promover(
                            Convert.ToInt32(ValoresJogo["idJogador"]),
                            ValoresJogo["senhaJogador"],
                            moveData
                        );
                        break;

                    case ApplicationConstants.GamePhases.Voting:
                        response = Jogo.Votar(
                            Convert.ToInt32(ValoresJogo["idJogador"]),
                            ValoresJogo["senhaJogador"],
                            moveData
                        );
                        break;
                }

                if (!ErrorHandler.HandleServerResponse(response))
                {
                    // Update the view after move
                    updateGameBoard();

                    // Indicate success
                    aiStatusLabel.Text = "AI: Movimento concluído";
                    aiStatusLabel.ForeColor = Color.Green;
                }
                else
                {
                    aiStatusLabel.Text = "AI: Erro no movimento";
                    aiStatusLabel.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                aiStatusLabel.Text = "AI: Erro";
                aiStatusLabel.ForeColor = Color.Red;
                ErrorHandler.ShowError($"Erro ao fazer movimento AI: {ex.Message}");
            }
        }

        // Add a method to handle form closing and cleanup
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Stop the timer to prevent issues
            if (aiTimer != null)
            {
                aiTimer.Stop();
                aiTimer.Dispose();
            }

            base.OnFormClosing(e);
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

        // Método para atualizar apenas a exibição da carta do jogador
        
        // Keep original method name to match the designer file
        private void updateGameBoard()
        {
            label8.Text = "";
            string response = Jogo.VerificarVez(
                Convert.ToInt32(ValoresJogo["idPartida"])
            );

            if (ErrorHandler.HandleServerResponse(response))
                return;

            // Store current board state for AI
            currentBoardState = response;

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
                string previousPhase = currentGamePhase;
                currentGamePhase = firstLine[3];
                label9.Text = "Estamos na fase de " + gamePhaseNames[firstLine[3]];

                // Se entramos na fase de posicionamento, atualizar carta do jogador
                if (currentGamePhase == ApplicationConstants.GamePhases.Positioning &&
                    previousPhase != ApplicationConstants.GamePhases.Positioning)
                {
                    // Buscar cartas do jogador
                    string charactersData = Jogo.ListarCartas(
                        Convert.ToInt32(ValoresJogo["idJogador"]),
                        ValoresJogo["senhaJogador"]
                    );
                }
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
                    "Você aceita o personagem para ser o rei?",
                    "Votação",
                    MessageBoxButtons.YesNo
                );

                Jogo.Votar(
                    Convert.ToInt32(ValoresJogo["idJogador"]),
                    ValoresJogo["senhaJogador"],
                    voteResult == DialogResult.Yes ? "s" : "n"
                );

                // Refresh the board after voting
                updateGameBoard();
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

    }
}