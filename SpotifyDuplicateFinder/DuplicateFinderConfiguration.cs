namespace SpotifyDuplicateFinder
{
    public struct DuplicateFinderConfiguration
    {
        public int MinimumMatches { get; set; }
        public int MinimumPhraseLength { get; set; }
    }
}