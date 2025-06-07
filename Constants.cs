// ============================
// File: Constants.cs (refatorado)
// ============================
using System;
using System.Collections.Generic;

namespace ClientKingMe
{
    public static class ApplicationConstants
    {
        public const string GroupName = "Monges de Cluny";
        public const string MusicFolderPath = "./Musicas";
        public const string ImagesFolderPath = "./Images";

        public static class GamePhases
        {
            public const string Positioning = "S";
            public const string Promotion = "P";
            public const string Voting = "V";

            public static readonly Dictionary<string, string> Names = new Dictionary<string, string>()
            {
                {Positioning, "posicionamento"},
                {Promotion, "promoção"},
                {Voting, "Votação"}
            };
        }

        public static readonly Dictionary<char, string> Characters = new Dictionary<char, string>()
        {
            {'A', "Adilson Konrad"}, {'B', "Beatriz Paiva"}, {'C', "Claro"},
            {'D', "Douglas Baquiao"}, {'E', "Eduardo Takeo"}, {'G', "Guilherme Rey"},
            {'H', "Heredia"}, {'K', "Karin"}, {'L', "Leonardo Takuno"},
            {'M', "Mario Toledo"}, {'Q', "Quintas"}, {'R', "Ranulfo"}, {'T', "Toshio"}
        };

        public static readonly Dictionary<char, int> CharacterMap = new Dictionary<char, int>()
        {
            {'A', 0}, {'B', 1}, {'C', 2}, {'D', 3}, {'E', 4},
            {'G', 5}, {'H', 6}, {'K', 7}, {'L', 8}, {'M', 9},
            {'Q', 10}, {'R', 11}, {'T', 12}
        };

        public static readonly List<char> AllCharacterCodes = new List<char>()
        {
            'A', 'B', 'C', 'D', 'E', 'G', 'H', 'K', 'L', 'M', 'Q', 'R', 'T'
        };

        public static class GameLimits
        {
            public const int MaxCharactersPerFloor = 4;
            public const int MaxRounds = 3;
            public const int MaxFloors = 6;
        }

        public static class ScoreValues
        {
            public const int ThroneScore = 10;
            public const int NoblesScore = 5;
            public const int DignitariesScore = 4;
            public const int OfficersScore = 3;
            public const int MerchantsScore = 2;
            public const int ArtisansScore = 1;
            public const int ServantsScore = 0;
        }

        public const int AI_CHECK_INTERVAL = 5000;
    }

    public static class Utils
    {
        public static class Game
        {
            public static List<char> GetAvailableCharacters(string boardState)
            {
                var charactersOnBoard = new HashSet<char>();
                var lines = boardState.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    if (parts.Length >= 2 && parts[1].Length > 0)
                        charactersOnBoard.Add(parts[1][0]);
                }

                return ApplicationConstants.AllCharacterCodes.FindAll(c => !charactersOnBoard.Contains(c));
            }

            public static bool IsMyTurn(string serverResponse, string playerId)
            {
                var lines = serverResponse.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                return lines.Length > 0 && lines[0].Split(',')[0] == playerId;
            }

            public static string GetCurrentPhase(string serverResponse)
            {
                var lines = serverResponse.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                return lines.Length > 0 && lines[0].Split(',').Length >= 4
                    ? lines[0].Split(',')[3]
                    : ApplicationConstants.GamePhases.Positioning;
            }
        }

        public static class Server
        {
            public static string SafeServerCall(Func<string> serverCall)
            {
                var response = serverCall();
                return ErrorHandler.HandleServerResponse(response) ? null : response;
            }
        }
    }
}
