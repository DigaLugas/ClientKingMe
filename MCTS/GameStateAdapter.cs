using System;
using System.Collections.Generic;
using System.Linq;
using KingMeServer;

namespace ClientKingMe
{
    public class GameStateAdapter
    {
        private readonly MCTS.GameRules _gameRules = new MCTS.GameRules();

        public MCTS.GameState CreateGameState(Dictionary<string, string> gameSessionData, string gamePhase, List<char> availableCharacters, string boardState)
        {
            int numPlayers = 4;

            var gameState = new MCTS.GameState(numPlayers);
            _gameRules.SetupGame(gameState, numPlayers);

            int currentPlayerId = int.Parse(gameSessionData["idJogador"]);
            gameState.CurrentPlayerIndex = currentPlayerId % numPlayers;

            SetGamePhase(gameState, gamePhase);
            ProcessBoardState(gameState, boardState);
            foreach (var character in gameState.Characters)
            {
                character.IsEliminated = false;
            }

            return gameState;
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
                    int characterId;
                    if (ApplicationConstants.CharacterMap.TryGetValue(characterCode, out characterId))
                    {
                        var character = gameState.Characters.FirstOrDefault(c => c.Id == characterId);
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
            foreach (var kvp in ApplicationConstants.CharacterMap)
            {
                if (kvp.Value == characterId)
                    return kvp.Key;
            }
            return '?';
        }
    }
}
