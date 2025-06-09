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
                return new MCTS.VotingMove(true); // Fallback

            // Se não há personagem no trono, votar sim para acelerar o jogo
            if (throneCharacter == null)
                return new MCTS.VotingMove(true);

            // Análise estratégica da votação
            var votingAnalysis = AnalyzeVotingScenarios(gameState, player, throneCharacter);
            
            // Se não temos votos "não", só podemos votar sim
            if (!player.HasNoVotes())
                return new MCTS.VotingMove(true);

            // Decidir baseado na análise estratégica
            if (votingAnalysis.ShouldVoteNo)
                return new MCTS.VotingMove(false);
            else
                return new MCTS.VotingMove(true);
        }

        private VotingAnalysis AnalyzeVotingScenarios(MCTS.GameState gameState, MCTS.Player player, MCTS.Character throneCharacter)
        {
            var analysis = new VotingAnalysis();

            // Calcular pontos se votarmos SIM (personagem fica no trono)
            int pointsIfVoteYes = CalculateRoundPoints(gameState, player, throneCharacter, true);
            
            // Calcular pontos se votarmos NÃO (personagem vai para servos)
            int pointsIfVoteNo = CalculateRoundPoints(gameState, player, throneCharacter, false);

            analysis.PointsIfVoteYes = pointsIfVoteYes;
            analysis.PointsIfVoteNo = pointsIfVoteNo;
            analysis.PointsDifference = pointsIfVoteNo - pointsIfVoteYes;

            // Análise de oportunidade futura
            analysis.FutureOpportunityValue = AnalyzeFutureOpportunities(gameState, player);

            // Análise competitiva (verificar se outros jogadores se beneficiam mais)
            analysis.CompetitiveAdvantage = AnalyzeCompetitivePosition(gameState, player, throneCharacter);

            // Decisão final baseada em múltiplos fatores
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
                    // Se é o personagem no trono
                    if (throneCharacterStays)
                        totalPoints += ApplicationConstants.ScoreValues.ThroneScore; // 10 pontos
                    else
                        totalPoints += ApplicationConstants.ScoreValues.ServantsScore; // 0 pontos
                }
                else
                {
                    // Outros personagens mantêm sua posição
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

            // Se ainda há rodadas restantes, considere as oportunidades futuras
            if (gameState.CurrentRound < 3)
            {
                // Verificar quantos dos nossos personagens favoritos estão em posições baixas
                // e poderiam se beneficiar de mais tempo para subir
                foreach (var favoriteId in player.FavoriteCharacters)
                {
                    var character = gameState.Characters.FirstOrDefault(c => c.Id == favoriteId);
                    if (character != null && !character.IsEliminated)
                    {
                        // Personagens em andares baixos têm mais potencial de crescimento
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

            // Verificar se outros jogadores se beneficiam mais do personagem no trono
            foreach (var otherPlayer in gameState.Players.Where(p => p.Id != player.Id))
            {
                if (otherPlayer.FavoriteCharacters.Contains(throneCharacter.Id))
                {
                    // Outro jogador também se beneficia - isso diminui nosso incentivo para votar sim
                    competitiveAdvantage -= 5; // Penalidade por beneficiar oponentes
                }
            }

            // Se somos os únicos a nos beneficiar, aumenta o incentivo para votar sim
            if (player.FavoriteCharacters.Contains(throneCharacter.Id) && competitiveAdvantage == 0)
            {
                competitiveAdvantage += 3; // Bônus por vantagem exclusiva
            }

            return competitiveAdvantage;
        }

        private bool ShouldVoteNoBasedOnAnalysis(VotingAnalysis analysis, int currentRound)
        {
            // Limiar de decisão baseado na rodada atual
            double decisionThreshold = currentRound == 3 ? 3 : 5; // Mais conservador na última rodada

            // Fatores que influenciam a decisão
            double totalValue = analysis.PointsDifference + 
                               analysis.FutureOpportunityValue + 
                               analysis.CompetitiveAdvantage;

            // Decidir votar NÃO se o valor total for positivo e acima do limiar
            return totalValue > decisionThreshold;
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

        // Classe auxiliar para análise de votação
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