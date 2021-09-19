using SpotifyAPI.Web;

namespace SpotifyDuplicateFinder
{
    public record Match(SimplePlaylist Playlist, FullTrack Track1, FullTrack Track2);
}