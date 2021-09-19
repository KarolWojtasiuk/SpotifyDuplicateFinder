using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpotifyAPI.Web;

namespace SpotifyDuplicateFinder
{
    public class SpotifyDuplicateFinder
    {
        private readonly SpotifyClient _client;

        public SpotifyDuplicateFinder(string authToken)
        {
            _client = new SpotifyClient(authToken);
        }

        public async Task<List<Match>> FindDuplicates(DuplicateFinderConfiguration configuration)
        {
            var results = new List<Match>();
            await foreach (var playlist in _client.Paginate(await _client.Playlists.CurrentUsers()))
                results.AddRange(await ProcessPlaylist(playlist, configuration));

            return results;
        }

        private async Task<List<Match>> ProcessPlaylist(SimplePlaylist playlist, DuplicateFinderConfiguration configuration)
        {
            var items = await _client.Playlists.GetItems(playlist.Id);
            var tracks = (await _client.PaginateAll(items))
                .Select(i => i.Track)
                .Cast<FullTrack>()
                .ToList();

            var results = new List<Match>();
            foreach (var track in tracks)
            {
                var duplicates = tracks.Where(t => ComparePhrases(GetPhrases(t.Name), GetPhrases(track.Name), configuration));
                foreach (var duplicate in duplicates)
                    if (duplicate != track
                        && !results.Any(r => r.Playlist == playlist && r.Track1 == track && r.Track2 == duplicate)
                        && !results.Any(r => r.Playlist == playlist && r.Track1 == duplicate && r.Track2 == track))
                        results.Add(new Match(playlist, track, duplicate));
            }

            return results;
        }

        private static IEnumerable<string> GetPhrases(string title)
        {
            var ignoredPhrases = new[] { "radio", "edit", "mix", "version", "original", "single", "feat.", "(feat." };

            return title.Split(' ')
                .Select(p => p.ToLower().Replace(" ", String.Empty))
                .Where(p => p.All(Char.IsLetterOrDigit)
                            && !ignoredPhrases.Any(i => i.Equals(p)))
                .Distinct()
                .ToList();
        }

        private static bool ComparePhrases(IEnumerable<string> a, IEnumerable<string> b, DuplicateFinderConfiguration configuration)
        {
            var matches = a.Where(phrase => b.Any(p => p.Equals(phrase, StringComparison.InvariantCulture))).ToList();
            return matches.Count >= configuration.MinimumMatches && matches.Any(m => m.Length >= configuration.MinimumPhraseLength);
        }
    }
}