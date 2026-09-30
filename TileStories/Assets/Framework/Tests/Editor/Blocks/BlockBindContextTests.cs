using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // BlockBindContext.ForBlock (_3.1 10A-fix): BlockStackView.Bind used to rebuild each block's own BlockBindContext by
    // copying fields one by one, so a field added later (Preview, in 10A.2b.3) was silently dropped until someone
    // remembered the copy. ForBlock now copies through reflection instead, so this test -- which itself walks every
    // public field by reflection, never by name -- proves every one of them actually reaches the block's own copy.
    public class BlockBindContextTests
    {
        // A block never calls these; they only need to exist as distinct, checkable references.
        private sealed class StubHost : IBlockHost
        {
            public SheetStopRule.Stop Stop => SheetStopRule.Stop.Half;
            public void ShowOnWall() { }
            public void SelectPoi(string poiId) { }
            public bool TryGetViewer(out UnityEngine.Vector3 wallPosition) { wallPosition = default; return false; }
            public void OpenUrl(string url) { }
            public void OpenTakeover(string name, int pageCount, int startPage, TakeoverPageDrawer drawPage) { }
        }

        private sealed class StubAudio : ICardAudio
        {
            public CardAudioState State => CardAudioState.Idle;
            public AudioTrack Current => null;
            public float Position => 0f;
            public float Length => 0f;
            public float Speed => 1f;
            public IReadOnlyList<AudioTrack> Queue { get; } = Array.Empty<AudioTrack>();
            public event Action Changed { add { } remove { } }
            public bool IsCurrent(AudioTrack track) => false;
            public bool IsQueued(AudioTrack track) => false;
            public void Toggle(AudioTrack track) { }
            public void Pause() { }
            public void Resume() { }
            public void Seek(float seconds) { }
            public void SetSpeed(float speed) { }
            public void UpdateTitle(AudioTrack track) { }
            public void Stop() { }
        }

        private sealed class StubVideo : ICardVideo
        {
            public CardVideoState State => CardVideoState.Idle;
            public VideoTrack Current => null;
            public bool PlayingLoop => false;
            public float Position => 0f;
            public float Length => 0f;
            public bool HasFrame => false;
            public UnityEngine.Texture Texture => null;
            public event Action Changed { add { } remove { } }
            public bool IsCurrent(VideoTrack track) => false;
            public void Toggle(VideoTrack track) { }
            public void Pause() { }
            public void Resume() { }
            public void Seek(float seconds) { }
            public void Stop() { }
            public void PlayLoop(VideoTrack loop) { }
            public void StopLoop(VideoTrack loop) { }
            public void CardShown(string poiId) { }
            public void CardClosed() { }
        }

        private static readonly FieldInfo[] Fields = typeof(BlockBindContext).GetFields(BindingFlags.Public | BindingFlags.Instance);

        [Test]
        public void ForBlock_CopiesEveryPublicField_ExceptMediaAndVariant_WhichItOverrides()
        {
            var source = new BlockBindContext
            {
                Poi = new POIData(),
                Taxonomy = new WallConfigData(),
                Variant = "stack_level_variant",
                Language = "pt",
                FallbackLanguage = "en",
                Media = new ResourcesMediaSource("stack_level_media"),
                Strings = new CardStrings(Array.Empty<CardStringEntry>(), Array.Empty<CardStringEntry>(), Array.Empty<CardStringEntry>(), "pt", "en"),
                MarkerLook = new MarkerVisualSettings(),
                Glossary = new CardGlossary(Array.Empty<GlossaryEntry>(), "pt", "en"),
                Host = new StubHost(),
                State = new CardLocalState(new MemoryCardStateStore(), "wall_under_test"),
                Events = new LogCardEvents(),
                Services = new CardServices(),
                Audio = new StubAudio(),
                Video = new StubVideo(),
                Preview = new CardPreviewService(new ManualPreviewStage(), () => null),
                ReduceMotion = true,
            };

            var blockMedia = new ResourcesMediaSource("this_blocks_own_scoped_media");
            var copy = source.ForBlock(blockMedia, "this_blocks_own_variant");

            Assert.AreSame(blockMedia, copy.Media, "Media is the one field ForBlock is asked to override");
            Assert.AreEqual("this_blocks_own_variant", copy.Variant, "Variant is the other field ForBlock overrides");

            int checkedFields = 0;
            foreach (var field in Fields)
            {
                if (field.Name is nameof(BlockBindContext.Media) or nameof(BlockBindContext.Variant)) continue;
                checkedFields++;
                Assert.AreEqual(field.GetValue(source), field.GetValue(copy),
                    $"BlockBindContext.{field.Name} did not reach the block's own copy");
            }
            Assert.Greater(checkedFields, 10, "sanity: this really walked most of BlockBindContext's fields, not an empty loop");
        }
    }
}
