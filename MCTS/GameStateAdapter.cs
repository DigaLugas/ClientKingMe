using System;
using System.Collections.Generic;
using System.Linq;
using KingMeServer;

namespace ClientKingMe
{
    public class GameStateAdapter
    {
        private readonly Dictionary<char, string> _characterNames = new Dictionary<char, string>()
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

        private readonly Dictionary<char, int> _characterMap = new Dictionary<char, int>()
        {
            {'A', 0}, {'B', 1}, {'C', 2}, {'D', 3}, {'E', 4},
            {'G', 5}, {'H', 6}, {'K', 7}, {'L', 8}, {'M', 9},
            {'Q', 10}, {'R', 11}, {'T', 12}
        };

        public MCTS.GameState CreateGameState(Dictionary<string, string> gameSessionData, string gamePhase, List<char> availableCharacters, string boardState)
        {
            // Determine número de jogadores (simplificado, ajuste conforme necessário)
            int numPlayers = 4; // Valor padrão para 4 jogadores

            // Criar estado do jogo
            var gameState = new MCTS.GameState(numPlayers);
            _gameRules.SetupGame(gameState, numPlayers);

            // Definir jogador atual
            int currentPlayerId = int.Parse(gameSessionData["idJogador"]);
            gameState.CurrentPlayerIndex = currentPlayerId % numPlayers;

            // Definir fase do jogo
            SetGamePhase(gameState, gamePhase);

            // Processar estado do tabuleiro
            ProcessBoardState(gameState, boardState);

            // Processar personagens disponíveis
            ProcessAvailableCharacters(gameState, availableCharacters);

            return gameState;
        }

        private readonly MCTS.GameRules _gameRules = new MCTS.GameRules();

        private void SetGamePhase(MCTS.GameState gameState, string phase)
        {
            switch (phase)
            {
                case ApplicationConstants.GamePhases.Positioning:
                    gameState.CurrentPhase = MCTS.GameState.GamePhase.Placement;
                    break;
                case ApplicationConstants.GamePhases.Promotion:
                    gameState.CurrentPhase = MCTS.GameState.GamePhase.Ascension;
                    break;
                case ApplicationConstants.GamePhases.Voting:
                    gameState.CurrentPhase = MCTS.GameState.GamePhase.Voting;
                    break;
                default:
                    gameState.CurrentPhase = MCTS.GameState.GamePhase.Placement;
                    break;
            }
        }

        private void ProcessBoardState(MCTS.GameState gameState, string boardState)
        {
            string[] lines = boardState.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] parts = line.Split(',');

                if (parts.Length >= 2 && int.TryParse(parts[0], out int floor))
                {
                    char characterCode = parts[1][0];

                    if (_characterMap.TryGetValue(characterCode, out int characterId))
                    {
                        var character = gameState.Characters.FirstOrDefault(c => c.Id == characterId);
                        if (character != null)
                        {
                            character.CurrentFloor = (MCTS.Floor)floor;
                        }
                    }
                }
            }
        }

        private void ProcessAvailableCharacters(MCTS.GameState gameState, List<char> availableCharacters)
        {
            // Marca como eliminados os personagens que não estão disponíveis
            foreach (var character in gameState.Characters)
            {
                character.IsEliminated = true;
            }

            // Marca como disponíveis os personagens da lista
            foreach (char charCode in availableCharacters)
            {
                if (_characterMap.TryGetValue(charCode, out int characterId))
                {
                    var character = gameState.Characters.FirstOrDefault(c => c.Id == characterId);
                    if (character != null)
                    {
                        character.IsEliminated = false;
                    }
                }
            }
        }

        public string ConvertMoveToClientFormat(MCTS.GameMove move)
        {
            if (move is MCTS.PlacementMove placementMove)
            {
                char characterCode = GetCharacterCode(placementMove.CharacterId);
                int floor = (int)placementMove.TargetFloor;
                return $"{floor},{characterCode}";
            }
            else if (move is MCTS.AscensionMove ascensionMove)
            {
                char characterCode = GetCharacterCode(ascensionMove.CharacterId);
                return characterCode.ToString();
            }
            else if (move is MCTS.VotingMove votingMove)
            {
                return votingMove.VoteYes ? "s" : "n";
            }

            return string.Empty;
        }

        private char GetCharacterCode(int characterId)
        {
            return _characterMap.FirstOrDefault(x => x.Value == characterId).Key;
        }
    }
}