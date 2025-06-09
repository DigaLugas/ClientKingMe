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
                return MakeStrategicVotingDecision(gameState);
            }

            var mcts = new MCTS.MCTSParallel(_gameRules, _maxIterations, 1.414, _numThreads);
            return mcts.FindBestMove(gameState, _playerId);
        }

        private MCTS.GameMove MakeStrategicVotingDecision(MCTS.GameState gameState)
        {
            var player = gameState.Players.FirstOrDefault(p => p.Id == _playerId);
            var throneCharacter = gameState.Characters.FirstOrDefault(c => c.CurrentFloor == MCTS.Floor.Throne);

            if (player == null)
                return new MCTS.VotingMove(true);

            if (throneCharacter == null)
                return new MCTS.VotingMove(true);

            var votingAnalysis = AnalyzeVotingScenarios(gameState, player, throneCharacter);
            
            if (!player.HasNoVotes())
                return new MCTS.VotingMove(true);

            if (votingAnalysis.ShouldVoteNo)
                return new MCTS.VotingMove(false);
            else
                return new MCTS.VotingMove(true);
        }

        private VotingAnalysis AnalyzeVotingScenarios(MCTS.GameState gameState, MCTS.Player player, MCTS.Character throneCharacter)
        {
            var analysis = new VotingAnalysis();

            int pointsIfVoteYes = CalculateRoundPoints(gameState, player, throneCharacter, true);
            
            int pointsIfVoteNo = CalculateRoundPoints(gameState, player, throneCharacter, false);

            analysis.PointsIfVoteYes = pointsIfVoteYes;
            analysis.PointsIfVoteNo = pointsIfVoteNo;
            analysis.PointsDifference = pointsIfVoteNo - pointsIfVoteYes;

            analysis.FutureOpportunityValue = AnalyzeFutureOpportunities(gameState, player);

            analysis.CompetitiveAdvantage = AnalyzeCompetitivePosition(gameState, player, throneCharacter);

            analysis.ShouldVoteNo = ShouldVoteNoBasedOnAnalysis(analysis, gameState.CurrentRound);

            return analysis;
        }

        private int CalculateRoundPoints(MCTS.GameState gameState, MCTS.Player player, MCTS.Character throneCharacter, bool throneCharacterStays)
        {
            int totalPoints = 0;

            foreach (var favoriteId in player.FavoriteCharacters)
            {
                var character = gameState.Characters.FirstOrDefault(c => c.Id == favoriteId);
                if (character == null || character.IsEliminated) continue;

                if (character.Id == throneCharacter.Id)
                {
                    if (throneCharacterStays)
                        totalPoints += ApplicationConstants.ScoreValues.ThroneScore;
                    else
                        totalPoints += ApplicationConstants.ScoreValues.ServantsScore;
                }
                else
                {
                    totalPoints += GetPointsForFloor(character.CurrentFloor);
                }
            }

            return totalPoints;
        }

        private int GetPointsForFloor(MCTS.Floor floor)
        {
            switch (floor)
            {
                case MCTS.Floor.Throne: return ApplicationConstants.ScoreValues.ThroneScore;
                case MCTS.Floor.Nobles: return ApplicationConstants.ScoreValues.NoblesScore;
                case MCTS.Floor.Dignitaries: return ApplicationConstants.ScoreValues.DignitariesScore;
                case MCTS.Floor.Officers: return ApplicationConstants.ScoreValues.OfficersScore;
                case MCTS.Floor.Merchants: return ApplicationConstants.ScoreValues.MerchantsScore;
                case MCTS.Floor.Artisans: return ApplicationConstants.ScoreValues.ArtisansScore;
                case MCTS.Floor.Servants:
                default: return ApplicationConstants.ScoreValues.ServantsScore;
            }
        }

        private double AnalyzeFutureOpportunities(MCTS.GameState gameState, MCTS.Player player)
        {
            double futureValue = 0;

            if (gameState.CurrentRound < 3)
            {
                foreach (var favoriteId in player.FavoriteCharacters)
                {
                    var character = gameState.Characters.FirstOrDefault(c => c.Id == favoriteId);
                    if (character != null && !character.IsEliminated)
                    {
                        if (character.CurrentFloor <= MCTS.Floor.Merchants)
                            futureValue += (3 - (int)character.CurrentFloor) * 0.5;
                    }
                }
            }

            return futureValue;
        }

        private double AnalyzeCompetitivePosition(MCTS.GameState gameState, MCTS.Player player, MCTS.Character throneCharacter)
        {
            double competitiveAdvantage = 0;

            foreach (var otherPlayer in gameState.Players.Where(p => p.Id != player.Id))
            {
                if (otherPlayer.FavoriteCharacters.Contains(throneCharacter.Id))
                {
                    competitiveAdvantage -= 5;
                }
            }

            if (player.FavoriteCharacters.Contains(throneCharacter.Id) && competitiveAdvantage == 0)
            {
                competitiveAdvantage += 3;
            }

            return competitiveAdvantage;
        }

        private bool ShouldVoteNoBasedOnAnalysis(VotingAnalysis analysis, int currentRound)
        {
            double decisionThreshold = currentRound == 3 ? 3 : 5;

            double totalValue = analysis.PointsDifference + 
                               analysis.FutureOpportunityValue + 
                               analysis.CompetitiveAdvantage;

            return totalValue > decisionThreshold;
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

        private class VotingAnalysis
        {
            public int PointsIfVoteYes { get; set; }
            public int PointsIfVoteNo { get; set; }
            public int PointsDifference { get; set; }
            public double FutureOpportunityValue { get; set; }
            public double CompetitiveAdvantage { get; set; }
            public bool ShouldVoteNo { get; set; }
        }
    }
}