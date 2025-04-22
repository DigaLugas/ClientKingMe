using System;
using System.Collections.Generic;
using System.Linq;
using KingMeServer;

namespace ClientKingMe
{
    public class MCTSAgent
    {
        private readonly int _playerId;
        private readonly int _maxIterations;
        private readonly int _numThreads;
        private readonly MCTS.GameRules _gameRules;
        private readonly Random _random = new Random();

        public MCTSAgent(int playerId, int maxIterations = 5000, int numThreads = 4)
        {
            _playerId = playerId;
            _maxIterations = maxIterations;
            _numThreads = numThreads;
            _gameRules = new MCTS.GameRules();
        }

        public MCTS.GameMove MakeMove(MCTS.GameState gameState)
        {
            if (gameState.CurrentPhase == MCTS.GameState.GamePhase.Voting)
            {
                var player = gameState.Players.FirstOrDefault(p => p.Id == _playerId);
                var throneCharacter = gameState.Characters.FirstOrDefault(c => c.CurrentFloor == MCTS.Floor.Throne);

                if (throneCharacter != null && player != null || !player.HasNoVotes())
                {
                    if (player.FavoriteCharacters.Contains(throneCharacter.Id))
                        return new MCTS.VotingMove(true);
                    else
                        return new MCTS.VotingMove(false);
                }
            }

            var mcts = new MCTS.MCTSParallel(_gameRules, _maxIterations, 1.414, _numThreads);
            return mcts.FindBestMove(gameState, _playerId);
        }

        public List<MCTS.GameMove> GetValidMoves(MCTS.GameState gameState)
        {
            return _gameRules.GetValidMoves(gameState);
        }

        public void ApplyMove(MCTS.GameState gameState, MCTS.GameMove move)
        {
            _gameRules.ApplyMove(gameState, move);
        }

        public string GetMoveDescription(MCTS.GameState gameState, MCTS.GameMove move)
        {
            if (move is MCTS.PlacementMove placementMove)
            {
                var character = gameState.Characters.FirstOrDefault(c => c.Id == placementMove.CharacterId);
                return $"Colocar {character?.Name ?? "Personagem"} no andar {placementMove.TargetFloor}";
            }
            else if (move is MCTS.AscensionMove ascensionMove)
            {
                var character = gameState.Characters.FirstOrDefault(c => c.Id == ascensionMove.CharacterId);
                return $"Promover {character?.Name ?? "Personagem"} para o próximo andar";
            }
            else if (move is MCTS.VotingMove votingMove)
            {
                return $"Votar {(votingMove.VoteYes ? "SIM" : "NÃO")} para o rei";
            }

            return "Movimento desconhecido";
        }
    }
}