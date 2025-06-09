// ============================
// File: Session.cs (refatorado para C# 7.3)
// ============================
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
        public Dictionary<string, string> ValoresJogo { get; set; }
        public Dictionary<string, string> GameSessionData { get; internal set; }

        private readonly DesignerConfigurator designer = new DesignerConfigurator();
        private readonly GameBoard gameBoard;
        private readonly MCTSAgent mctsAgent;
        private readonly GameStateAdapter gameStateAdapter = new GameStateAdapter();

        private List<char> availableCharacters = new List<char>();
        private string currentBoardState = string.Empty;
        private string currentGamePhase = ApplicationConstants.GamePhases.Positioning;

        private readonly Timer aiTimer;
        private bool autoPlayEnabled = true;
        private readonly Label aiStatusLabel;

        public GameSessionForm(Dictionary<string, string> valoresJogo)
        {
            InitializeComponent();
            ApplyCustomStyling();
            ValoresJogo = valoresJogo;
            ExibirInfoJogador();

            gameBoard = new GameBoard(pictureBox1);
            mctsAgent = new MCTSAgent(int.Parse(valoresJogo["idJogador"]), 2000);

            aiStatusLabel = new Label
            {
                Text = "AI: Monitorando",
                Location = new Point(button1.Location.X, button1.Location.Y + button1.Size.Height + 20),
                AutoSize = true,
                ForeColor = Color.Green
            };
            Controls.Add(aiStatusLabel);

            aiTimer = new Timer { Interval = ApplicationConstants.AI_CHECK_INTERVAL };
            aiTimer.Tick += AiTimer_Tick;
            aiTimer.Start();
            AiTimer_Tick(null, EventArgs.Empty);
        }

        private void ExibirInfoJogador()
        {
            label3.Text = ValoresJogo["nomePartida"];
            label4.Text = "Id: " + ValoresJogo["idJogador"];
            label5.Text = "Nome: " + ValoresJogo["nomeJogador"];
            label6.Text = "Senha: " + ValoresJogo["senhaJogador"];
            label8.Text = "";
        }

        private void AiTimer_Tick(object sender, EventArgs e)
        {
            if (!autoPlayEnabled) return;

            try
            {
                var turnInfo = Utils.Server.SafeServerCall(() => Jogo.VerificarVez(int.Parse(ValoresJogo["idPartida"])));
                if (turnInfo == null || !Utils.Game.IsMyTurn(turnInfo, ValoresJogo["idJogador"])) return;

                aiStatusLabel.Text = "AI: Pensando...";
                aiStatusLabel.ForeColor = Color.Blue;

                AtualizarEstadoDoJogo(turnInfo);

                BeginInvoke((Action)(() => ExecuteAiMove()));
            }
            catch (Exception ex)
            {
                aiStatusLabel.Text = "AI: Erro";
                aiStatusLabel.ForeColor = Color.Red;
                ErrorHandler.ShowError("Erro no timer AI: " + ex.Message);
            }
        }

        private void AtualizarEstadoDoJogo(string turnInfo)
        {
            currentBoardState = turnInfo;
            currentGamePhase = Utils.Game.GetCurrentPhase(turnInfo);

            if (currentGamePhase == ApplicationConstants.GamePhases.Positioning)
                availableCharacters = Utils.Game.GetAvailableCharacters(currentBoardState);
        }

        // Adicionar estas variáveis à classe GameSessionForm:
        private string playerCardsInfo = string.Empty;
        private bool playerCardsLoaded = false;

        // Modificar o método ExecuteAiMove():
        private void ExecuteAiMove()
        {
            try
            {
                // Garantir que temos as cartas do jogador antes de fazer movimentos
                if (!playerCardsLoaded)
                {
                    LoadPlayerCards();
                }

                var lista = Jogo.ListarJogadores(int.Parse(ValoresJogo["idPartida"]));
                int numPlayers = lista.Split('\n').Count(l => !string.IsNullOrWhiteSpace(l));

                // Passar as informações das cartas do jogador para o GameState
                var gameState = gameStateAdapter.CreateGameState(
                    ValoresJogo,
                    currentGamePhase,
                    availableCharacters,
                    currentBoardState,
                    numPlayers,
                    playerCardsInfo  // ← Nova informação das cartas reais do jogador
                );

                var bestMove = mctsAgent.MakeMove(gameState);
                if (bestMove == null)
                {
                    aiStatusLabel.Text = "AI: Sem movimentos";
                    aiStatusLabel.ForeColor = Color.Orange;
                    ErrorHandler.ShowWarning("O AI não conseguiu determinar um movimento.");
                    return;
                }

                string moveData = gameStateAdapter.ConvertMoveToClientFormat(bestMove);
                string moveDescription = mctsAgent.GetMoveDescription(gameState, bestMove);

                // Mostrar análise da decisão se for votação
                if (bestMove is MCTS.VotingMove votingMove)
                {
                    string voteReason = GetVotingReason(gameState, votingMove);
                    aiStatusLabel.Text = $"AI: {moveDescription} - {voteReason}";
                }
                else
                {
                    aiStatusLabel.Text = "AI: " + moveDescription;
                }

                var response = ExecutarMovimento(moveData);
                if (response != null)
                {
                    AtualizarTabuleiro();
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
                ErrorHandler.ShowError("Erro ao fazer movimento AI: " + ex.Message);
            }
        }

        // Novo método para carregar as cartas do jogador:
        private void LoadPlayerCards()
        {
            try
            {
                playerCardsInfo = gameStateAdapter.GetPlayerCardsFromServer(ValoresJogo);
                playerCardsLoaded = !string.IsNullOrEmpty(playerCardsInfo);
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError($"Erro ao carregar cartas do jogador: {ex.Message}");
                playerCardsLoaded = false;
            }
        }

        private string ExecutarMovimento(string moveData)
        {
            int idJogador = int.Parse(ValoresJogo["idJogador"]);
            string senha = ValoresJogo["senhaJogador"];

            if (currentGamePhase == ApplicationConstants.GamePhases.Positioning)
            {
                return Utils.Server.SafeServerCall(() =>
                {
                    var parts = moveData.Split(',');
                    if (parts.Length >= 2)
                        return Jogo.ColocarPersonagem(idJogador, senha, int.Parse(parts[0]), parts[1]);
                    return null;
                });
            }
            else if (currentGamePhase == ApplicationConstants.GamePhases.Promotion)
            {
                return Utils.Server.SafeServerCall(() => Jogo.Promover(idJogador, senha, moveData));
            }
            else if (currentGamePhase == ApplicationConstants.GamePhases.Voting)
            {
                return Utils.Server.SafeServerCall(() => Jogo.Votar(idJogador, senha, moveData));
            }

            return null;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (aiTimer != null)
            {
                aiTimer.Stop();
                aiTimer.Dispose();
            }
            base.OnFormClosing(e);
        }

        private void ApplyCustomStyling()
        {
            Label[] labels = { label1, label2, label3, label4, label5, label6, label8, label9 };
            int[] sizes = { 13, 13, 10, 10, 10, 10, 9, 9 };

            for (int i = 0; i < labels.Length; i++)
            {
                DesignerConfigurator.StyleLabel(labels[i], designer.primaryColor, sizes[i]);
            }

            DesignerConfigurator.StyleButton(button1, designer.primaryColor, designer.accentColor, 10);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Utils.Server.SafeServerCall(() => Jogo.Iniciar(
                int.Parse(ValoresJogo["idJogador"]), ValoresJogo["senhaJogador"]));
        }

        private void AtualizarTabuleiro()
        {
            label8.Text = "";
            var response = Utils.Server.SafeServerCall(() =>
                Jogo.VerificarVez(int.Parse(ValoresJogo["idPartida"])));

            if (response == null) return;

            currentBoardState = response;
            gameBoard.ProcessBoardUpdate(response);
            ProcessarTurno(response);
        }

        private string GetVotingReason(MCTS.GameState gameState, MCTS.VotingMove votingMove)
        {
            var player = gameState.Players.FirstOrDefault(p => p.Id == int.Parse(ValoresJogo["idJogador"]));
            var throneCharacter = gameState.Characters.FirstOrDefault(c => c.CurrentFloor == MCTS.Floor.Throne);

            if (player == null || throneCharacter == null)
                return "Situação indefinida";

            bool isMyFavorite = player.FavoriteCharacters.Contains(throneCharacter.Id);

            if (votingMove.VoteYes)
            {
                if (isMyFavorite)
                    return "Meu personagem favorito vencerá";
                else
                    return "Melhor cenário para minha estratégia";
            }
            else
            {
                if (isMyFavorite)
                    return "Estratégia de longo prazo mais vantajosa";
                else
                    return "Eliminar personagem adversário";
            }
        }

        // Modificar o método ProcessarTurno para recarregar cartas quando necessário:
        private void ProcessarTurno(string response)
        {
            var lines = response.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0) return;

            var firstLine = lines[0].Split(',');
            if (firstLine.Length >= 4)
            {
                string faseAnterior = currentGamePhase;
                currentGamePhase = firstLine[3];

                if (ApplicationConstants.GamePhases.Names.ContainsKey(currentGamePhase))
                    label9.Text = "Estamos na fase de " + ApplicationConstants.GamePhases.Names[currentGamePhase];

                // Recarregar cartas quando entrar na fase de posicionamento
                if (currentGamePhase == ApplicationConstants.GamePhases.Positioning && faseAnterior != ApplicationConstants.GamePhases.Positioning)
                {
                    playerCardsLoaded = false; // Forçar recarregamento das cartas
                    LoadPlayerCards();
                }
            }

            ExibirTurnoJogadorAtual(firstLine[0]);
        }

        private void ExibirTurnoJogadorAtual(string idAtual)
        {
            var jogadores = Jogo.ListarJogadores(int.Parse(ValoresJogo["idPartida"])).Split('\n');

            foreach (var j in jogadores)
            {
                var dados = j.Split(',');
                if (dados.Length >= 2 && dados[0] == idAtual)
                {
                    label8.Text = "ID: " + dados[0] + ", vez do " + dados[1];
                    break;
                }
            }
        }
    }
}
