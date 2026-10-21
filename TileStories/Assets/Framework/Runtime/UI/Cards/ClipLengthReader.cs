using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace TileStories
{
    // The length in seconds of the audio or video clip a block names (ReadingTimeRule.ClipField): what a contents list and the menus' time
    // chips need to give an audio guide or a video its minutes. It loads the clip through the media source it is given -- the caller's own
    // scope, so the clip is released with the caller -- and remembers each length by path. A missing field, file or clip reads as 0.
    public sealed class ClipLengthReader
    {
        private readonly IMediaSource _media;
        private readonly string _language;
        private readonly string _fallbackLanguage;
        private readonly Dictionary<string, float> _seconds = new();

        public ClipLengthReader(IMediaSource media, string language, string fallbackLanguage)
        {
            _media = media;
            _language = language;
            _fallbackLanguage = fallbackLanguage;
        }

        // The length of the clip in `clipField` of this block (the path is read in the card's language), once per path
        public float Seconds(BlockInstanceData instance, BlockFieldDefinition clipField)
        {
            string path = new BlockFieldReader(instance, _language, _fallbackLanguage).ValidAsset(clipField.Key, clipField.Media);
            if (path.Length == 0) return 0f;
            if (_seconds.TryGetValue(path, out float known)) return known;
            float seconds = 0f;
            if (clipField.Media == MediaKind.Audio)
            {
                var clip = _media?.Load<AudioClip>(path);
                seconds = clip != null ? clip.length : 0f;
            }
            else if (clipField.Media == MediaKind.Video)
            {
                var clip = _media?.Load<VideoClip>(path);
                seconds = clip != null ? (float)clip.length : 0f;
            }
            _seconds[path] = seconds;
            return seconds;
        }

        // The block's whole minutes by ReadingTimeRule: its Reading Time field, or its clip's length rounded up (0 = none)
        public int MinutesOf(BlockKindDefinition kind, BlockInstanceData instance)
        {
            var clipField = ReadingTimeRule.ClipField(kind);
            float seconds = clipField != null ? Seconds(instance, clipField) : 0f;
            return ReadingTimeRule.Minutes(kind, instance, seconds);
        }

        // Forget the lengths read (a new card)
        public void Clear() => _seconds.Clear();
    }
}
