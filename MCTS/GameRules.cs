using System;
using System.Collections.Generic;
using System.Linq;

namespace MCTS
{
    public class GameRules
    {
        private readonly Random _random = new Random();

        public List<GameMove> GetValidMoves(GameState state)
        {
            if (state.CurrentPhase == GameState.GamePhase.Placement)
                return GetPlacementMoves(state);
            else if (state.CurrentPhase == GameState.GamePhase.Ascension)
                return GetAscensionMoves(state);
            else if (state.CurrentPhase == GameState.GamePhase.Voting)
                return GetVotingMoves(state);

            return new List<GameMove>();
        }

        private List<GameMove> GetPlacementMoves(GameState state)
        {
            var moves = new List<GameMove>();
            var available = state.GetCharactersAvailableForPlacement();

            foreach (var character in available)
            {
                for (int floor = 1; floor <= 4; floor++)
                {
                    if (state.CountCharactersOnFloor((Floor)floor) < 4)
                        moves.Add(new PlacementMove(character.Id, (Floor)floor));
                }
            }

            return moves;
        }

        private List<GameMove> GetAscensionMoves(GameState state)
        {
            var moves = new List<GameMove>();
            var available = state.GetCharactersAvailableForAscension();

            foreach (var character in available)
            {
                Floor target = character.CurrentFloor + 1;
                if (state.CountCharactersOnFloor(target) < 4 || target == Floor.Throne)
                    moves.Add(new AscensionMove(character.Id));
            }

            return moves;
        }

        private List<GameMove> GetVotingMoves(GameState state)
        {
            var moves = new List<GameMove>();
            moves.Add(new VotingMove(true));
            if (state.GetCurrentPlayer().HasNoVotes())
                moves.Add(new VotingMove(false));
            return moves;
        }

        public void ApplyMove(GameState state, GameMove move)
        {
            if (move is PlacementMove)
                ApplyPlacementMove(state, (PlacementMove)move);
            else if (move is AscensionMove)
                ApplyAscensionMove(state, (AscensionMove)move);
            else if (move is VotingMove)
                ApplyVotingMove(state, (VotingMove)move);
            else
                throw new ArgumentException("Tipo de movimento inválido");
        }

        private void ApplyPlacementMove(GameState state, PlacementMove move)
        {
            var character = state.Characters.First(c => c.Id == move.CharacterId);
            character.CurrentFloor = move.TargetFloor;
            state.CharactersPlacedThisRound++;
            state.NextPlayer();

            if (state.IsPlacementPhaseComplete())
                state.TransitionToNextPhase();
        }

        private void ApplyAscensionMove(GameState state, AscensionMove move)
        {
            var character = state.Characters.First(c => c.Id == move.CharacterId);
            Floor target = character.CurrentFloor + 1;

            if (target != Floor.Throne && state.CountCharactersOnFloor(target) >= 4)
                throw new InvalidOperationException("Não há espaço no andar de destino");

            character.CurrentFloor = target;

            if (target == Floor.Throne)
                state.CurrentPhase = GameState.GamePhase.Voting;
            else
                state.NextPlayer();
        }

        private void ApplyVotingMove(GameState state, VotingMove move)
        {
            var player = state.GetCurrentPlayer();

            if (!move.VoteYes && !player.HasNoVotes())
                throw new InvalidOperationException("Jogador não tem votos 'Não' disponíveis");

            if (!move.VoteYes)
            {
                player.UseNoVote();
                var throneChar = state.Characters.FirstOrDefault(c => c.CurrentFloor == Floor.Throne);
                if (throneChar != null)
                {
                    throneChar.IsEliminated = true;
                    throneChar.CurrentFloor = Floor.Servants;
                }
                state.NextPlayer();
                state.CurrentPhase = GameState.GamePhase.Ascension;
            }
            else
            {
                foreach (var p in state.Players)
                    p.Score += p.CalculateScore(state.Characters);

                state.TransitionToNextPhase();
            }
        }
    }

    public abstract class GameMove { }

    public class PlacementMove : GameMove
    {
        public int CharacterId { get; }
        public Floor TargetFloor { get; }

        public PlacementMove(int characterId, Floor targetFloor)
        {
            CharacterId = characterId;
            TargetFloor = targetFloor;
        }
    }

    public class AscensionMove : GameMove
    {
        public int CharacterId { get; }

        public AscensionMove(int characterId)
        {
            CharacterId = characterId;
        }
    }

    public class VotingMove : GameMove
    {
        public bool VoteYes { get; }

        public VotingMove(bool voteYes)
        {
            VoteYes = voteYes;
        }
    }
}
