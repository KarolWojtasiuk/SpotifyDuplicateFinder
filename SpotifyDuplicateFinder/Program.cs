using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace SpotifyDuplicateFinder
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                new Option<string>(
                        "--auth-token",
                        "Spotify WebApi token, grab temporary one from `https://developer.spotify.com/console/get-current-user-playlists/`, no additional scopes are required")
                    { IsRequired = true },
                new Option<int>(
                    "--minimum-matches",
                    () => 3,
                    "Minimum matches required for a positive result"),
                new Option<int>(
                    "--minimum-phrase-length",
                    () => 3,
                    "Minimum phrase length for a match")
            };

            rootCommand.Handler = CommandHandler.Create<string, int, int>(Run);
            await rootCommand.InvokeAsync(args);
        }

        private static async Task Run(string authToken, int minimumMatches, int minimumPhraseLength)
        {
            if (String.IsNullOrWhiteSpace(authToken)) throw new ArgumentNullException(nameof(authToken));

            var configuration = new DuplicateFinderConfiguration
            {
                MinimumMatches = minimumMatches,
                MinimumPhraseLength = minimumPhraseLength
            };

            var duplicateFinder = new SpotifyDuplicateFinder(authToken);
            var duplicates = await duplicateFinder.FindDuplicates(configuration);
            duplicates.ForEach(PrintResult);
        }


        private static void PrintResult(Match match)
        {
            Console.Write("Found duplicate in playlist ");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"\"{match.Playlist.Name}\": ");

            Console.ResetColor();
            Console.Write("{ ");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(match.Track1.Name);

            Console.ResetColor();
            Console.Write(", ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(match.Track2.Name);

            Console.ResetColor();
            Console.Write(" }");

            Console.WriteLine();
        }
    }
}