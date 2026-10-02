using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The wall card's owners (_3.1 15.4.2), kept together so PoiCardHost only wires them: the ONE audio owner (with the mini-player that shows
    // it after the card closes, CardAudioCoordinator), the ONE video owner, the coordinator that keeps the two from sounding at once
    // (CardSoundRule), the 3D/360 preview slots and the AR placement. Blocks reach them through BlockBindContext. Each Use* swaps one owner
    // (the host's own, or a test's over a manual device) and lets the old one go first.
    public sealed class CardOwners
    {
        private readonly CardAudioPlayer _audioPlayer;
        private readonly CardVideoPlayer _videoPlayer;
        private readonly VisualElement _layer;
        private readonly Func<CardSettings> _settings;
        private readonly Func<bool> _cardOpen;
        private readonly Action<string> _openAudiosCard;
        private CardSoundCoordinator _sound;

        // The players that tick the audio / video owners, the card layer the mini-player sits in, the wall's card settings and whether the card
        // is open (Keep Audio Playing), and what a tap on the mini-player opens (that point's card)
        public CardOwners(CardAudioPlayer audioPlayer, CardVideoPlayer videoPlayer, VisualElement layer, Func<CardSettings> settings, Func<bool> cardOpen,
            Action<string> openAudiosCard)
        {
            _audioPlayer = audioPlayer;
            _videoPlayer = videoPlayer;
            _layer = layer;
            _settings = settings;
            _cardOpen = cardOpen;
            _openAudiosCard = openAudiosCard;
        }

        // The wall card's own owners: over `host`'s players and preview stage (added the first time), the wall's frame and tracker for AR
        // placement, and the card's media source as it is at each load (`media`); `clock` times the audio fades
        public static CardOwners ForWall(GameObject host, WallSession wall, VisualElement layer, Func<IMediaSource> media, Func<float> clock,
            Func<bool> cardOpen, Action<string> openAudiosCard)
        {
            Func<CardSettings> settings = () => wall != null ? wall.CardSettings : null;
            var audioPlayer = ComponentOn<CardAudioPlayer>(host);
            var videoPlayer = ComponentOn<CardVideoPlayer>(host);
            audioPlayer.PollForOutputLoss = () => settings()?.container.audio_android_output_poll ?? false;
            var owners = new CardOwners(audioPlayer, videoPlayer, layer, settings, cardOpen, openAudiosCard);
            owners.UseAudio(new CardAudioService(audioPlayer.CreateOutput(), media, clock, () => settings()?.container.audio_when_another_starts));
            owners.UseVideo(new CardVideoService(videoPlayer.CreateOutput(), media));
            owners.UsePreview(new CardPreviewService(ComponentOn<CardPreviewStage>(host), media));
            owners._arWall = new WallArSurface(wall);
            owners.UseArPlacement(new ArPlacementService(owners._arWall, media));
            return owners;
        }

        // `host`'s component of type T, added the first time it is asked for
        private static T ComponentOn<T>(GameObject host) where T : Component =>
            host.GetComponent<T>() != null ? host.GetComponent<T>() : host.AddComponent<T>();

        // The wall's own AR surface (ForWall), let go with the owners
        private WallArSurface _arWall;

        public CardAudioCoordinator AudioCoordinator { get; private set; }
        public ICardAudio Audio => AudioCoordinator?.Audio;
        public CardVideoService Video { get; private set; }
        public CardPreviewService Preview { get; private set; }
        public ArPlacementService ArPlacement { get; private set; }

        // Make `service` the audio owner: the old one stops and its mini-player goes, the player ticks the new one, a mini-player is drawn for it
        public void UseAudio(CardAudioService service)
        {
            if (AudioCoordinator != null)
            {
                AudioCoordinator.Shutdown();
                AudioCoordinator.Mini.Root.RemoveFromHierarchy();
            }
            _audioPlayer.Service = service;
            var mini = new MiniPlayerView(_layer, service);
            mini.OpenRequested += _openAudiosCard;
            AudioCoordinator = new CardAudioCoordinator(service, mini, _settings, _cardOpen);
            JoinTheTwoPlayers();
        }

        // Make `service` the video owner: the old one lets go of its clip, the player ticks the new one
        public void UseVideo(CardVideoService service)
        {
            Video?.CardClosed();
            _videoPlayer.Service = service;
            Video = service;
            JoinTheTwoPlayers();
        }

        // Make `service` the preview owner: the old one gives back every slot it still held
        public void UsePreview(CardPreviewService service)
        {
            Preview?.ReleaseAll();
            Preview = service;
        }

        // Make `service` the AR placement owner: the old one takes its model away first
        public void UseArPlacement(ArPlacementService service)
        {
            ArPlacement?.Dispose();
            ArPlacement = service;
        }

        // One sound coordinator over the audio and video owners the card has now (rebuilt when either is swapped)
        private void JoinTheTwoPlayers()
        {
            _sound?.Dispose();
            _sound = new CardSoundCoordinator(Audio, Video);
        }

        // The card is about to show `poiId` (before its blocks bind): a model placed for another point goes unless its block keeps it (Keep
        // Model On Switch); the same point (a live edit) keeps it
        public void PointChosen(string poiId) => ArPlacement?.SelectionChanged(poiId);

        // The card shows `poiId` now: audio and video learn the card's point (another point's video stops, the mini-player follows)
        public void CardShown(string poiId, CardStrings strings)
        {
            AudioCoordinator?.CardShown(poiId, strings);
            Video?.CardShown(poiId);
        }

        // The card closed: audio stays for the mini-player only with Keep Audio Playing, video and previews let go, a placed model goes unless kept
        public void CardClosed()
        {
            AudioCoordinator?.CardClosed();
            Video?.CardClosed();
            Preview?.ReleaseAll();
            ArPlacement?.SelectionChanged(null);
        }

        // The card was switched off (host disabled): no card and no mini-player is left, so nothing keeps playing or standing
        public void SwitchedOff()
        {
            AudioCoordinator?.Shutdown();
            Video?.CardClosed();
            Preview?.ReleaseAll();
            ArPlacement?.Remove();
        }

        // The host goes away: nothing it placed stays in the world, nothing listens to the wall's tracker
        public void Dispose()
        {
            ArPlacement?.Dispose();
            _arWall?.Dispose();
        }
    }
}
