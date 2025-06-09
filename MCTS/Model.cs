using ClientKingMe;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCTS
{
    public enum Floor
    {
        Servants = 0,
        Artisans = 1,
        Merchants = 2,
        Officers = 3,
        Dignitaries = 4,
        Nobles = 5,
        Throne = 6
    }

    public class Character
    {
        public int Id { get; }
        public string Name { get; }
        public Floor CurrentFloor { get; set; }
        public bool IsEliminated { get; set; }

        public Character(int id, string name)
        {
            Id = id;
            Name = name;
            CurrentFloor = Floor.Servants;
            IsEliminated = false;
        }

        public Character Clone()
        {
            return new Character(Id, Name)
            {
                CurrentFloor = CurrentFloor,
                IsEliminated = IsEliminated
            };
        }
    }

    public class Player
    {
        public int Id { get; }
        public List<int> FavoriteCharacters { get; }
        public int YesVotes { get; set; }
        public int NoVotes { get; set; }
        public int Score { get; set; }

        public Player(int id, List<int> favoriteCharacters, int noVotes)
        {
            Id = id;
            FavoriteCharacters = favoriteCharacters;
            YesVotes = 1;
            NoVotes = noVotes;
            Score = 0;
        }

        public Player Clone()
        {
            var player = new Player(Id, new List<int>(FavoriteCharacters), NoVotes)
            {
                YesVotes = YesVotes,
                Score = Score
            };
            return player;
        }

        public bool HasNoVotes()
        {
            return NoVotes > 0;
        }

        public void UseNoVote()
        {
            if (NoVotes <= 0)
                throw new InvalidOperationException("Não há votos 'Não' disponíveis");
            NoVotes--;
        }

        public int CalculateScore(List<Character> characters)
        {
            int roundScore = 0;
            foreach (var id in FavoriteCharacters)
            {
                var character = characters.FirstOrDefault(c => c.Id == id);
                if (character != null && !character.IsEliminated)
                {
                    switch (character.CurrentFloor)
                    {
                        case Floor.Throne:
                            roundScore += ApplicationConstants.ScoreValues.ThroneScore;
                            break;
                        case Floor.Nobles:
                            roundScore += ApplicationConstants.ScoreValues.NoblesScore;
                            break;
                        case Floor.Dignitaries:
                            roundScore += ApplicationConstants.ScoreValues.DignitariesScore;
                            break;
                        case Floor.Officers:
                            roundScore += ApplicationConstants.ScoreValues.OfficersScore;
                            break;
                        case Floor.Merchants:
                            roundScore += ApplicationConstants.ScoreValues.MerchantsScore;
                            break;
                        case Floor.Artisans:
                            roundScore += ApplicationConstants.ScoreValues.ArtisansScore;
                            break;
                        case Floor.Servants:
                        default:
                            roundScore += ApplicationConstants.ScoreValues.ServantsScore;
                            break;
                    }
                }
            }
            return roundScore;
        }
    }

    public class GameState
    {
        public List<Character> Characters { get; }
        public List<Player> Players { get; }
        public int CurrentRound { get; set; }
        public int CurrentPlayerIndex { get; set; }
        public GamePhase CurrentPhase { get; set; }
        public int CharactersPlacedThisRound { get; set; }
        public int CharactersToPlacePerPlayer { get; }

        public enum GamePhase
        {
            Placement,
            Ascension,
            Voting,
            EndRound
        }

        public GameState(int numPlayers)
        {
            Characters = InitializeCharacters();
            Players = new List<Player>();
            CurrentRound = 1;
            CurrentPlayerIndex = 0;
            CurrentPhase = GamePhase.Placement;
            CharactersPlacedThisRound = 0;


            switch (numPlayers)
            {
                case 2: CharactersToPlacePerPlayer = 6; break;
                case 3: CharactersToPlacePerPlayer = 4; break;
                case 4: CharactersToPlacePerPlayer = 3; break;
                case 5:
                case 6: CharactersToPlacePerPlayer = 2; break;
                default: throw new ArgumentException("Número de jogadores inválido");
            }
        }

        private List<Character> InitializeCharacters()
        {
            return ApplicationConstants.CharacterDefinitions
                .Select(cd => new Character(cd.Id, cd.Name))
                .ToList();
        }


        public bool IsGameOver()
        {
            return CurrentRound > 3;
        }

        public GameState Clone()
        {
            var clonedState = new GameState(Players.Count)
            {
                CurrentRound = CurrentRound,
                CurrentPlayerIndex = CurrentPlayerIndex,
                CurrentPhase = CurrentPhase,
                CharactersPlacedThisRound = CharactersPlacedThisRound
            };

            clonedState.Characters.Clear();
            foreach (var character in Characters)
            {
                clonedState.Characters.Add(character.Clone());
            }

            clonedState.Players.Clear();
            foreach (var player in Players)
            {
                clonedState.Players.Add(player.Clone());
            }

            return clonedState;
        }

        public bool IsPlacementPhaseComplete()
        {
            return CharactersPlacedThisRound >= Players.Count * CharactersToPlacePerPlayer;
        }

        public int CountCharactersOnFloor(Floor floor)
        {
            return Characters.Count(c => c.CurrentFloor == floor && !c.IsEliminated);
        }

        public List<Character> GetCharactersAvailableForPlacement()
        {
            return Characters.Where(c => c.CurrentFloor == Floor.Servants && !c.IsEliminated).ToList();
        }

        public List<Character> GetCharactersAvailableForAscension()
        {
            return Characters.Where(c => !c.IsEliminated && c.CurrentFloor < Floor.Throne &&
                                          CountCharactersOnFloor((Floor)((int)c.CurrentFloor + 1)) < 4).ToList();
        }

        public Player GetCurrentPlayer()
        {
            return Players[CurrentPlayerIndex];
        }

        public void NextPlayer()
        {
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;
        }

        public void TransitionToNextPhase()
        {
            switch (CurrentPhase)
            {
                case GamePhase.Placement:
                    CurrentPhase = GamePhase.Ascension;
                    break;
                case GamePhase.Ascension:

                    CurrentPhase = GamePhase.EndRound;
                    break;
                case GamePhase.Voting:

                    CurrentPhase = GamePhase.Ascension;
                    break;
                case GamePhase.EndRound:
                    CurrentRound++;
                    if (CurrentRound <= 3)
                    {
                        ResetForNewRound();
                        CurrentPhase = GamePhase.Placement;
                    }
                    break;
            }
        }

        private void ResetForNewRound()
        {
            foreach (var character in Characters)
            {
                character.CurrentFloor = Floor.Servants;
                character.IsEliminated = false;
            }
            CharactersPlacedThisRound = 0;
            CurrentPlayerIndex = 0;
        }
    }
}