using System;
using System.Collections.Generic;
using System.Linq;
using KingMeServer;

namespace ClientKingMe
{
    public class GameStateAdapter
    {
        private readonly MCTS.GameRules _gameRules = new MCTS.GameRules();

        public MCTS.GameState CreateGameState(Dictionary<string, string> gameSessionData, string gamePhase,
            List<char> availableCharacters, string boardState, int numPlayers, string playerCardsInfo = null)
        {
            var gameState = new MCTS.GameState(numPlayers);

            int currentPlayerId = int.Parse(gameSessionData["idJogador"]);
            gameState.CurrentPlayerIndex = currentPlayerId % numPlayers;

            // Configurar jogadores com personagens favoritos reais
            SetupPlayersWithRealFavorites(gameState, currentPlayerId, playerCardsInfo, numPlayers);

            SetGamePhase(gameState, gamePhase);
            ProcessBoardState(gameState, boardState);

            foreach (var character in gameState.Characters)
            {
                if (character.CurrentFloor == MCTS.Floor.Servants && gameState.CurrentPhase != MCTS.GameState.GamePhase.Placement)
                {
                    character.IsEliminated = true;
                }
            }

            return gameState;
        }

        private void SetupPlayersWithRealFavorites(MCTS.GameState gameState, int currentPlayerId,
            string playerCardsInfo, int numPlayers)
        {
            gameState.Players.Clear();

            // Determinar número de votos "não" baseado no número de jogadores
            int noVotes = GetNoVotesForPlayerCount(numPlayers);

            for (int i = 0; i < numPlayers; i++)
            {
                List<int> favoriteCharacters;

                if (i == currentPlayerId && !string.IsNullOrEmpty(playerCardsInfo))
                {
                    // Usar personagens reais do jogador atual
                    favoriteCharacters = ParsePlayerCards(playerCardsInfo);
                }
                else
                {
                    // Gerar aleatoriamente para outros jogadores
                    favoriteCharacters = _gameRules.GenerateRandomFavorites();
                }

                gameState.Players.Add(new MCTS.Player(i, favoriteCharacters, noVotes));
            }
        }

        private int GetNoVotesForPlayerCount(int numPlayers)
        {
            switch (numPlayers)
            {
                case 2:
                case 3: return 4;
                case 4: return 3;
                case 5:
                case 6: return 2;
                default: throw new ArgumentException("Número de jogadores inválido");
            }
        }

        private List<int> ParsePlayerCards(string playerCardsInfo)
        {
            var favoriteIds = new List<int>();

            if (string.IsNullOrEmpty(playerCardsInfo))
                return _gameRules.GenerateRandomFavorites();

            try
            {
                // Assumindo que playerCardsInfo contém códigos de personagens separados por vírgula ou quebra de linha
                var lines = playerCardsInfo.Split(new[] { "\r\n", "\n", "," }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();

                    // Se a linha contém um código de personagem
                    if (trimmedLine.Length > 0)
                    {
                        char characterCode = trimmedLine[0];
                        var characterDef = ApplicationConstants.GetByCode(characterCode);

                        if (characterDef != null && !favoriteIds.Contains(characterDef.Id))
                        {
                            favoriteIds.Add(characterDef.Id);
                        }
                    }
                }

                // Se não conseguimos parsear 6 personagens, completar com aleatórios
                while (favoriteIds.Count < 6)
                {
                    var randomId = new Random().Next(13);
                    if (!favoriteIds.Contains(randomId))
                        favoriteIds.Add(randomId);
                }

                // Se temos mais de 6, pegar apenas os primeiros 6
                return favoriteIds.Take(6).ToList();
            }
            catch (Exception)
            {
                // Em caso de erro no parsing, usar personagens aleatórios
                return _gameRules.GenerateRandomFavorites();
            }
        }

        private void SetGamePhase(MCTS.GameState gameState, string phase)
        {
            if (phase == ApplicationConstants.GamePhases.Positioning)
                gameState.CurrentPhase = MCTS.GameState.GamePhase.Placement;
            else if (phase == ApplicationConstants.GamePhases.Promotion)
                gameState.CurrentPhase = MCTS.GameState.GamePhase.Ascension;
            else if (phase == ApplicationConstants.GamePhases.Voting)
                gameState.CurrentPhase = MCTS.GameState.GamePhase.Voting;
            else
                gameState.CurrentPhase = MCTS.GameState.GamePhase.Placement;
        }

        private void ProcessBoardState(MCTS.GameState gameState, string boardState)
        {
            string[] lines = boardState.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length >= 2 && int.TryParse(parts[0], out int floor))
                {
                    char characterCode = parts[1][0];
                    var def = ApplicationConstants.CharacterDefinitions.FirstOrDefault(d => d.Code == characterCode);
                    if (def != null)
                    {
                        var character = gameState.Characters.FirstOrDefault(c => c.Id == def.Id);
                        if (character != null)
                            character.CurrentFloor = (MCTS.Floor)floor;
                    }
                }
            }
        }

        public string ConvertMoveToClientFormat(MCTS.GameMove move)
        {
            if (move is MCTS.PlacementMove)
            {
                var p = (MCTS.PlacementMove)move;
                return ((int)p.TargetFloor).ToString() + "," + GetCharacterCode(p.CharacterId);
            }
            else if (move is MCTS.AscensionMove)
            {
                var a = (MCTS.AscensionMove)move;
                return GetCharacterCode(a.CharacterId).ToString();
            }
            else if (move is MCTS.VotingMove)
            {
                var v = (MCTS.VotingMove)move;
                return v.VoteYes ? "s" : "n";
            }

            return "";
        }

        private char GetCharacterCode(int characterId)
        {
            return ApplicationConstants.GetById(characterId).Code;
        }

        // Método para obter informações dos personagens do jogador do servidor
        public string GetPlayerCardsFromServer(Dictionary<string, string> gameSessionData)
        {
            try
            {
                int playerId = int.Parse(gameSessionData["idJogador"]);
                string password = gameSessionData["senhaJogador"];

                return Utils.Server.SafeServerCall(() => Jogo.ListarCartas(playerId, password));
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError($"Erro ao obter cartas do jogador: {ex.Message}");
                return null;
            }
        }
    }
}