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
        private Button toggleAutoPlayButton;
        private Label aiStatusLabel;
        private const int AI_CHECK_INTERVAL = 5000;


        public GameSessionForm(Dictionary<string, string> valoresJogo)
        {
            this.designer = new DesignerConfigurator();
            InitializeComponent();
            ApplyCustomStyling();

            label7.Text = "";
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
                Location = new Point(button3.Location.X + button3.Width + 20, button3.Location.Y),
                AutoSize = true,
                ForeColor = Color.DarkGray
            };
            this.Controls.Add(aiStatusLabel);

            // Add toggle button for auto play
            toggleAutoPlayButton = new Button
            {
                Text = "Ativar Auto-Play",
                Location = new Point(button4.Location.X, button4.Location.Y + button4.Height + 7),
                Size = button4.Size
            };
            toggleAutoPlayButton.Click += ToggleAutoPlayButton_Click;
            this.Controls.Add(toggleAutoPlayButton);
            DesignerConfigurator.StyleButton(toggleAutoPlayButton, designer.primaryColor, designer.accentColor, 10);

        }

        private void AiMoveButton_Click(object sender, EventArgs e)
        {
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
                {
                    ErrorHandler.ShowWarning("Não é sua vez de jogar.");
                    return;
                }

                // Update game state data
                currentBoardState = turnInfo;
                if (firstLine.Length >= 4)
                {
                    currentGamePhase = firstLine[3];
                }

                // Get available characters if needed
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
                    ErrorHandler.ShowWarning("O AI não conseguiu determinar um movimento.");
                    return;
                }

                // Convert move to client format
                string moveData = gameStateAdapter.ConvertMoveToClientFormat(bestMove);

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
                    button4_Click(null, EventArgs.Empty);

                    string moveDescription = mctsAgent.GetMoveDescription(gameState, bestMove);
                    MessageBox.Show($"AI move: {moveDescription}", "AI Move", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError($"Erro ao fazer movimento AI: {ex.Message}");
            }
        }
        private void ToggleAutoPlayButton_Click(object sender, EventArgs e)
        {
            autoPlayEnabled = !autoPlayEnabled;

            if (autoPlayEnabled)
            {
                toggleAutoPlayButton.Text = "Desativar Auto-Play";
                aiStatusLabel.Text = "AI: Monitorando";
                aiStatusLabel.ForeColor = Color.Green;
                aiTimer.Start();

                // Check immediately if it's our turn
                AiTimer_Tick(null, EventArgs.Empty);
            }
            else
            {
                toggleAutoPlayButton.Text = "Ativar Auto-Play";
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

                // Verificar se houve mudança de fase
                string newGamePhase = firstLine.Length >= 4 ? firstLine[3] : ApplicationConstants.GamePhases.Positioning;
                if (newGamePhase != currentGamePhase && currentGamePhase != string.Empty)
                {
                    // Parar o autoplay quando mudar de fase
                    autoPlayEnabled = false;
                    toggleAutoPlayButton.Text = "Ativar Auto-Play";
                    aiStatusLabel.Text = "AI: Inativo (Fase mudou)";
                    aiStatusLabel.ForeColor = Color.DarkGray;
                    aiTimer.Stop();

                    // Notificar o usuário
                    MessageBox.Show($"Auto-play parado: Fase mudou de {gamePhaseNames[currentGamePhase]} para {gamePhaseNames[newGamePhase]}.",
                        "Mudança de Fase", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Atualizar a fase atual
                    currentGamePhase = newGamePhase;
                    return;
                }

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
                        UpdateCharacterList(charactersData);
                        UpdateCharacterListBox(charactersData);
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
                    button4_Click(null, EventArgs.Empty);

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
            DesignerConfigurator.StyleButton(button3, designer.primaryColor, designer.accentColor, 10);
            DesignerConfigurator.StyleButton(button4, designer.primaryColor, designer.accentColor, 10);
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

            // Atualizar apenas a exibição da carta do jogador
            UpdatePlayerCardDisplay(response);

            // Preencher a listBox com todos os personagens disponíveis no jogo
            if (listBox1.Items.Count == 0)
            {
                UpdateCharacterListBox("");
            }
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

        private void UpdateCharacterListBox(string charactersData)
        {
            listBox1.Items.Clear();

            // Preencher a listBox com todos os personagens do jogo
            foreach (var character in characterNames)
            {
                listBox1.Items.Add($"{character.Key} - {character.Value}");
            }

            if (listBox1.Items.Count > 0)
            {
                listBox1.SelectedIndex = 0;
            }

            // Também garantir que o combobox de andares está preenchido
            if (comboBox1.Items.Count == 0)
            {
                for (int i = 0; i < 6; i++)
                {
                    comboBox1.Items.Add(i);
                }
                comboBox1.SelectedIndex = 0;
            }
        }

        // Método para atualizar apenas a exibição da carta do jogador
        private void UpdatePlayerCardDisplay(string charactersData)
        {
            label7.Text = "";
            foreach (char c in charactersData.ToCharArray())
            {
                if (characterNames.ContainsKey(c))
                {
                    label7.Text += characterNames[c] + "\n";
                }
            }

            // Armazenar os personagens disponíveis para o AI
            availableCharacters = charactersData.ToCharArray().ToList();
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

                    if (!ErrorHandler.HandleServerResponse(charactersData))
                    {
                        UpdatePlayerCardDisplay(charactersData);
                    }
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
                button4_Click(null, EventArgs.Empty);
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
                    // Na fase de posicionamento, permitir posicionar qualquer personagem disponível
                    // Não verificamos se está na carta do jogador
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