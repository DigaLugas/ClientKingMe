using System;
using System.Collections.Generic;
using System.Linq;

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
                {Voting, "votação"}
            };
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

        public const int AI_CHECK_INTERVAL = 4000;

        public static List<CharacterDefinition> CharacterDefinitions = new List<CharacterDefinition>
        {
            new CharacterDefinition(0, 'A', "Alighiero o Escudeiro"),
            new CharacterDefinition(1, 'B', "Beatrice a Encantadora"),
            new CharacterDefinition(2, 'C', "Clemente o Sargento"),
            new CharacterDefinition(3, 'D', "Dario o Antiquário"),
            new CharacterDefinition(4, 'E', "Ernesto o Duque"),
            new CharacterDefinition(5, 'G', "Fiorello o Pintor"),
            new CharacterDefinition(6, 'H', "Gavino o Paladino"),
            new CharacterDefinition(7, 'K', "Irina a Fazendeira"),
            new CharacterDefinition(8, 'L', "Leonardo o Mensageiro"),
            new CharacterDefinition(9, 'M', "Merlino o Vidente"),
            new CharacterDefinition(10, 'Q', "Natale o Guardião"),
            new CharacterDefinition(11, 'R', "Odessa a Condessa"),
            new CharacterDefinition(12, 'T', "Piero o Cozinheiro")
        };

        public static CharacterDefinition GetById(int id) =>
            CharacterDefinitions.FirstOrDefault(cd => cd.Id == id);

        public static CharacterDefinition GetByCode(char code) =>
            CharacterDefinitions.FirstOrDefault(cd => cd.Code == code);
    }

    public class CharacterDefinition
    {
        public int Id { get; }
        public char Code { get; }
        public string Name { get; }

        public CharacterDefinition(int id, char code, string name)
        {
            Id = id;
            Code = code;
            Name = name;
        }
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

                return ApplicationConstants.CharacterDefinitions
                    .Select(c => c.Code)
                    .Where(c => !charactersOnBoard.Contains(c))
                    .ToList();
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
