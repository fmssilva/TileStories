using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TileStories.Tests
{
    // Effects domain (_2.2): proves that every effect config value changes what a REAL marker
    // does, not just that the components exist. Real POI_Marker prefab, real MarkerView.Initialise,
    // real Update ticks. Animated values are compared frame by frame against the spec formula
    // evaluated at that frame's own Time.time (a coroutine resumes after Update, so both see the
    // same Time.time). Structural "which effect is on" checks read the public state of the
    // effect components; the level matrix uses the real LivingRoom config plus a synthetic
    // sweep of every ripple x halo x pulse combination.
    public class MarkerEffectConfigTests
    {
        private const float Tolerance = 0.002f;

        private readonly List<GameObject> _spawned = new();
        private GameObject _prefab;

        [SetUp]
        public void SetUp()
        {
            _prefab = MarkerGalleryTestFixture.LoadPrefab();
            MarkerHierarchyResolver.ResetToDefaults();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _spawned)
                if (go != null) UnityEngine.Object.Destroy(go);
            _spawned.Clear();
            MarkerHierarchyResolver.ResetToDefaults();
        }

        // ---------------- helpers ----------------

        private static HierarchyLevelEntry Level(string key, string ripple, string halo, bool pulse,
            float revealDelay = 0f, float revealDuration = 0f) => new HierarchyLevelEntry
            {
                key = key, level_name = key, priority = 1, size_cm = 20f,
                ripple_effect = ripple, halo_effect = halo, pulse = pulse,
                reveal_delay_s = revealDelay, reveal_duration_s = revealDuration,
            };

        // Spawn one real marker on the given hierarchy level with the given wall effect defaults,
        // going through the same MarkerView.Initialise signature WallSession.SpawnPOIs uses.
        private GameObject Spawn(string levelKey, EffectDefaults defaults, out MarkerView view)
        {
            var go = UnityEngine.Object.Instantiate(_prefab);
            _spawned.Add(go);
            var anchor = go.GetComponent<POIAnchor>() ?? go.AddComponent<POIAnchor>();
            anchor.Initialise(new POIData
            {
                id = "fx_" + levelKey, name = "Effect " + levelKey, category = "religious",
                hierarchy_level_key = levelKey,
            });
            view = go.GetComponentInChildren<MarkerView>();
            Assert.IsNotNull(view, "POI_Marker prefab must carry a MarkerView.");
            view.Initialise(anchor, MarkerVisualSettings.Default(), MarkerEffectFlags.None, defaults);
            return go;
        }

        private static IEnumerator ForSeconds(float seconds, Action onEachFrame)
        {
            float end = Time.time + seconds;
            while (Time.time < end)
            {
                yield return null;
                onEachFrame();
            }
        }

        private static float BreatheWave(float time, float period) =>
            Mathf.Sin(time * Mathf.PI * 2f / period) * 0.5f + 0.5f;

        private static float SmoothStep01(float t) => t * t * (3f - 2f * t);

        // Spec of one ripple wave: its phase is the shared cycle shifted by the wave's delay.
        private static float RippleWaveFade(float time, float period, float delay, float waveAlpha)
        {
            float cycle = Mathf.Repeat(time / period, 1f);
            float t = Mathf.Repeat(cycle - delay, 1f);
            return Mathf.Lerp(waveAlpha, 0f, SmoothStep01(t));
        }

        private static Image FindImage(GameObject go, string childName)
        {
            var image = go.transform.Find(childName)?.GetComponent<Image>();
            Assert.IsNotNull(image, $"Child '{childName}' with an Image must exist on the marker.");
            return image;
        }

        // ---------------- config -> behaviour (first spawn) ----------------

        [UnityTest]
        public IEnumerator Pulse_AmplitudeAndPeriod_DriveTheRealSymbolScale_OnFirstSpawn()
        {
            MarkerHierarchyResolver.Configure(new[] { Level("p", "none", "none", pulse: true) });
            var defaults = new EffectDefaults();
            defaults.pulse.amplitude = 0.40f;   // compiled default is 0.18
            defaults.pulse.period = 0.5f;       // compiled default is 1.6

            var go = Spawn("p", defaults, out _);
            var symbol = (RectTransform)go.transform.Find("Symbol");
            float baseScale = symbol.localScale.x;
            Assert.Greater(baseScale, 0f, "Precondition: symbol has a real base scale.");

            int frames = 0;
            float maxSeen = 0f;
            yield return ForSeconds(1.2f, () =>
            {
                float expected = baseScale * (1f + 0.40f * BreatheWave(Time.time, 0.5f));
                Assert.AreEqual(expected, symbol.localScale.x, Tolerance,
                    "Pulse scale must follow the configured amplitude 0.40 / period 0.5s from the FIRST spawn.");
                maxSeen = Mathf.Max(maxSeen, symbol.localScale.x);
                frames++;
            });

            Assert.Greater(frames, 10, "Precondition: enough frames were sampled to mean something.");
            Assert.Greater(maxSeen, baseScale * 1.30f,
                "Peak must exceed what the compiled default amplitude (0.18) could ever reach.");
        }

        [UnityTest]
        public IEnumerator RippleRings_UseTheirOwnBlock_PeriodStaggerAlphasAndTint()
        {
            MarkerHierarchyResolver.Configure(new[] { Level("s", "ripple_rings", "none", pulse: false) });
            var defaults = new EffectDefaults();
            defaults.ripple_rings.period = 0.5f;
            defaults.ripple_rings.stagger = 0.10f;
            defaults.ripple_rings.inner_alpha = 0.90f;
            defaults.ripple_rings.middle_alpha = 0.60f;
            defaults.ripple_rings.outer_alpha = 0.30f;
            defaults.ripple_rings.tint_color_hex = "#FF0000";
            // The discs block must NOT leak into a rings marker.
            defaults.ripple_discs.period = 3f;
            defaults.ripple_discs.tint_color_hex = "#00FF00";

            var go = Spawn("s", defaults, out _);
            var inner = FindImage(go, "RippleInner");
            var middle = FindImage(go, "RippleMiddle");
            var outer = FindImage(go, "RippleOuter");
            Assert.AreEqual(MarkerRippleEffect.RippleStyle.Rings, go.GetComponent<MarkerRippleEffect>().CurrentStyle);

            int frames = 0;
            yield return ForSeconds(1.2f, () =>
            {
                Assert.IsTrue(inner.enabled && middle.enabled && outer.enabled, "All three waves must be drawn.");
                Assert.AreEqual(RippleWaveFade(Time.time, 0.5f, 0f, 0.90f), inner.color.a, Tolerance, "inner alpha/period");
                Assert.AreEqual(RippleWaveFade(Time.time, 0.5f, 0.10f, 0.60f), middle.color.a, Tolerance, "middle alpha/stagger");
                Assert.AreEqual(RippleWaveFade(Time.time, 0.5f, 0.20f, 0.30f), outer.color.a, Tolerance, "outer alpha/2x stagger");
                Assert.AreEqual(1f, inner.color.r, Tolerance, "tint red channel");
                Assert.AreEqual(0f, inner.color.g, Tolerance, "tint green channel (discs block must not leak)");
                Assert.AreEqual(0f, inner.color.b, Tolerance, "tint blue channel");
                frames++;
            });
            Assert.Greater(frames, 10, "Precondition: enough frames were sampled to mean something.");
        }

        [UnityTest]
        public IEnumerator RippleDiscs_UseTheirOwnBlock_NotTheRingsBlock()
        {
            MarkerHierarchyResolver.Configure(new[] { Level("d", "ripple_discs", "none", pulse: false) });
            var defaults = new EffectDefaults();
            defaults.ripple_rings.period = 3f;
            defaults.ripple_rings.inner_alpha = 0.10f;
            defaults.ripple_discs.period = 0.5f;
            defaults.ripple_discs.inner_alpha = 0.70f;
            defaults.ripple_discs.tint_color_hex = "#0000FF";

            var go = Spawn("d", defaults, out _);
            var inner = FindImage(go, "RippleInner");
            Assert.AreEqual(MarkerRippleEffect.RippleStyle.Discs, go.GetComponent<MarkerRippleEffect>().CurrentStyle);

            int frames = 0;
            yield return ForSeconds(1.0f, () =>
            {
                Assert.AreEqual(RippleWaveFade(Time.time, 0.5f, 0f, 0.70f), inner.color.a, Tolerance, "discs inner alpha/period");
                Assert.AreEqual(1f, inner.color.b, Tolerance, "discs tint");
                frames++;
            });
            Assert.Greater(frames, 8, "Precondition: enough frames were sampled to mean something.");
        }

        [UnityTest]
        public IEnumerator HaloRing_BreathesWithItsOwnAlphaAmplitudeAndTint()
        {
            MarkerHierarchyResolver.Configure(new[] { Level("a", "none", "halo_ring", pulse: false) });
            var defaults = new EffectDefaults();
            defaults.halo_ring.base_alpha = 0.60f;
            defaults.halo_ring.breathe_amplitude = 0.30f;
            defaults.halo_ring.period = 0.5f;
            defaults.halo_ring.tint_color_hex = "#0000FF";
            defaults.halo_disc.period = 5f;   // other halo blocks must not leak
            defaults.beacon.period = 5f;

            var go = Spawn("a", defaults, out _);
            var halo = go.GetComponent<MarkerHaloEffect>();
            var symbol = (RectTransform)go.transform.Find("Symbol");
            var image = FindImage(go, "Halo");
            float baseScale = symbol.localScale.x;
            Assert.AreEqual(MarkerHaloEffect.HaloVariant.Ring, halo.CurrentVariant);

            int frames = 0;
            yield return ForSeconds(1.2f, () =>
            {
                float expectedScale = baseScale * (1f + 0.30f * BreatheWave(Time.time, 0.5f));
                Assert.AreEqual(expectedScale, image.rectTransform.localScale.x, Tolerance, "breathe scale");
                Assert.AreEqual(0.60f, image.color.a, Tolerance, "breathe keeps a constant base alpha");
                Assert.AreEqual(1f, image.color.b, Tolerance, "tint blue channel");
                Assert.AreEqual(0f, image.color.r, Tolerance, "tint red channel");
                frames++;
            });
            Assert.Greater(frames, 10, "Precondition: enough frames were sampled to mean something.");
        }

        [UnityTest]
        public IEnumerator HaloDisc_BreathesWithItsOwnBlock()
        {
            MarkerHierarchyResolver.Configure(new[] { Level("dd", "none", "halo_disc", pulse: false) });
            var defaults = new EffectDefaults();
            defaults.halo_disc.base_alpha = 0.50f;
            defaults.halo_disc.breathe_amplitude = 0.20f;
            defaults.halo_disc.period = 0.5f;
            defaults.halo_ring.period = 5f;   // must not leak

            var go = Spawn("dd", defaults, out _);
            Assert.AreEqual(MarkerHaloEffect.HaloVariant.Disc, go.GetComponent<MarkerHaloEffect>().CurrentVariant);
            var symbol = (RectTransform)go.transform.Find("Symbol");
            var image = FindImage(go, "Halo");
            float baseScale = symbol.localScale.x;

            int frames = 0;
            yield return ForSeconds(1.0f, () =>
            {
                Assert.AreEqual(baseScale * (1f + 0.20f * BreatheWave(Time.time, 0.5f)), image.rectTransform.localScale.x, Tolerance, "disc breathe scale");
                Assert.AreEqual(0.50f, image.color.a, Tolerance, "disc alpha");
                frames++;
            });
            Assert.Greater(frames, 8, "Precondition: enough frames were sampled to mean something.");
        }

        [UnityTest]
        public IEnumerator Beacon_GrowsAndFadesBetweenItsConfiguredScales()
        {
            MarkerHierarchyResolver.Configure(new[] { Level("b", "none", "beacon", pulse: false) });
            var defaults = new EffectDefaults();
            defaults.beacon.base_alpha = 0.70f;
            defaults.beacon.period = 0.5f;
            defaults.beacon.start_scale = 1.2f;
            defaults.beacon.end_scale = 2.6f;
            defaults.halo_ring.period = 5f;   // must not leak

            var go = Spawn("b", defaults, out _);
            Assert.AreEqual(MarkerHaloEffect.HaloVariant.Beacon, go.GetComponent<MarkerHaloEffect>().CurrentVariant);
            var symbol = (RectTransform)go.transform.Find("Symbol");
            var image = FindImage(go, "Halo");
            float baseScale = symbol.localScale.x;

            int frames = 0;
            yield return ForSeconds(1.2f, () =>
            {
                float smooth = SmoothStep01(Mathf.Repeat(Time.time / 0.5f, 1f));
                Assert.AreEqual(baseScale * Mathf.Lerp(1.2f, 2.6f, smooth), image.rectTransform.localScale.x, Tolerance, "beacon scale");
                Assert.AreEqual(Mathf.Lerp(0.70f, 0f, smooth), image.color.a, Tolerance, "beacon alpha fades out");
                frames++;
            });
            Assert.Greater(frames, 10, "Precondition: enough frames were sampled to mean something.");
        }

        // ---------------- bad config must not break the marker ----------------

        [UnityTest]
        public IEnumerator NonPositivePeriods_NeverProduceNaN_OrHideTheMarker()
        {
            MarkerHierarchyResolver.Configure(new[]
            {
                Level("all", "ripple_rings", "halo_ring", pulse: true),
                Level("beacon", "none", "beacon", pulse: false),
            });
            var defaults = new EffectDefaults();
            defaults.pulse.period = 0f;
            defaults.ripple_rings.period = 0f;
            defaults.halo_ring.period = -1f;
            defaults.beacon.period = 0f;

            var allGo = Spawn("all", defaults, out _);
            var beaconGo = Spawn("beacon", defaults, out _);
            var symbol = (RectTransform)allGo.transform.Find("Symbol");
            var rippleInner = FindImage(allGo, "RippleInner");
            var haloAll = FindImage(allGo, "Halo");
            var haloBeacon = FindImage(beaconGo, "Halo");

            int frames = 0;
            yield return ForSeconds(0.5f, () =>
            {
                Assert.IsTrue(float.IsFinite(symbol.localScale.x) && symbol.localScale.x > 0f, "pulse scale finite and visible");
                Assert.IsTrue(float.IsFinite(rippleInner.color.a), "ripple alpha finite");
                Assert.IsTrue(float.IsFinite(rippleInner.rectTransform.localScale.x), "ripple scale finite");
                Assert.IsTrue(float.IsFinite(haloAll.rectTransform.localScale.x), "breathe scale finite");
                Assert.IsTrue(float.IsFinite(haloBeacon.rectTransform.localScale.x) && float.IsFinite(haloBeacon.color.a), "beacon finite");
                frames++;
            });
            Assert.Greater(frames, 5, "Precondition: enough frames were sampled to mean something.");
        }

        // ---------------- hierarchy level -> which effect runs ----------------

        // Independent oracle for the option strings (does not reuse the production parser).
        private static readonly Dictionary<string, MarkerRippleEffect.RippleStyle?> RippleOracle = new()
        {
            { "none", null },
            { "ripple_rings", MarkerRippleEffect.RippleStyle.Rings },
            { "ripple_discs", MarkerRippleEffect.RippleStyle.Discs },
        };

        private static readonly Dictionary<string, MarkerHaloEffect.HaloVariant?> HaloOracle = new()
        {
            { "none", null },
            { "halo_ring", MarkerHaloEffect.HaloVariant.Ring },
            { "halo_disc", MarkerHaloEffect.HaloVariant.Disc },
            { "beacon", MarkerHaloEffect.HaloVariant.Beacon },
        };

        private void AssertEffectsMatchLevel(GameObject go, string ripple, string halo, bool pulse, string context)
        {
            var rippleFx = go.GetComponent<MarkerRippleEffect>();
            var haloFx = go.GetComponent<MarkerHaloEffect>();
            var pulseFx = go.GetComponent<MarkerPulseEffect>();

            Assert.AreEqual(pulse, pulseFx.IsActive, $"{context}: pulse");

            var expectedRipple = RippleOracle[ripple];
            Assert.AreEqual(expectedRipple.HasValue, rippleFx.IsActive, $"{context}: ripple active");
            if (expectedRipple.HasValue)
                Assert.AreEqual(expectedRipple.Value, rippleFx.CurrentStyle, $"{context}: ripple style");

            var expectedHalo = HaloOracle[halo];
            Assert.AreEqual(expectedHalo.HasValue, haloFx.IsActive, $"{context}: halo active");
            if (expectedHalo.HasValue)
                Assert.AreEqual(expectedHalo.Value, haloFx.CurrentVariant, $"{context}: halo variant");
        }

        [UnityTest]
        public IEnumerator EveryLivingRoomHierarchyLevel_TurnsOnExactlyTheEffectsItAuthors()
        {
            WallConfigData config = null;
            yield return WallConfigLoader.LoadFromStreamingAssets("LivingRoom/config.json", c => config = c);
            Assert.IsNotNull(config, "LivingRoom config must load.");
            Assert.Greater(config.hierarchy_levels.Count, 0, "Precondition: the wall authors hierarchy levels.");
            MarkerHierarchyResolver.Configure(config.hierarchy_levels);

            int checkedLevels = 0;
            foreach (var level in config.hierarchy_levels)
            {
                var go = Spawn(level.key, config.effect_defaults, out _);
                yield return null;
                AssertEffectsMatchLevel(go, level.ripple_effect, level.halo_effect, level.pulse, "LivingRoom " + level.key);
                checkedLevels++;
            }
            Assert.AreEqual(config.hierarchy_levels.Count, checkedLevels, "Every authored level must have been checked.");
        }

        [UnityTest]
        public IEnumerator EveryRippleHaloPulseCombination_TurnsOnExactlyTheChosenEffects()
        {
            var ripples = new[] { "none", "ripple_rings", "ripple_discs" };
            var halos = new[] { "none", "halo_ring", "halo_disc", "beacon" };
            var levels = new List<HierarchyLevelEntry>();
            foreach (var r in ripples)
                foreach (var h in halos)
                    foreach (var p in new[] { false, true })
                        levels.Add(Level($"{r}|{h}|{p}", r, h, p));
            Assert.AreEqual(24, levels.Count, "Precondition: 3 ripple x 4 halo x 2 pulse.");
            MarkerHierarchyResolver.Configure(levels);

            foreach (var level in levels)
            {
                var go = Spawn(level.key, new EffectDefaults(), out _);
                yield return null;
                AssertEffectsMatchLevel(go, level.ripple_effect, level.halo_effect, level.pulse, level.key);
                UnityEngine.Object.Destroy(go);
            }
        }

        [UnityTest]
        public IEnumerator UnknownEffectStrings_AreIgnored_NotCrashing()
        {
            // Expectations first: the resolver warns while Configure parses the level.
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Unknown ripple_effect"));
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Unknown halo_effect"));
            MarkerHierarchyResolver.Configure(new[] { Level("typo", "ripple_squares", "confetti", pulse: false) });
            var go = Spawn("typo", new EffectDefaults(), out _);
            yield return null;
            AssertEffectsMatchLevel(go, "none", "none", false, "typo level");
        }

        // ---------------- master switch and per-effect switches ----------------

        [UnityTest]
        public IEnumerator EffectsEnabledFalse_SilencesEveryEffect_AndShrinksTheVisualRadius()
        {
            MarkerHierarchyResolver.Configure(new[] { Level("all", "ripple_discs", "halo_ring", pulse: true) });

            var on = Spawn("all", new EffectDefaults(), out var viewOn);
            var off = Spawn("all", new EffectDefaults { effects_enabled = false }, out var viewOff);
            yield return null;

            AssertEffectsMatchLevel(on, "ripple_discs", "halo_ring", true, "enabled");
            AssertEffectsMatchLevel(off, "none", "none", false, "effects_enabled=false");

            var symbolOff = (RectTransform)off.transform.Find("Symbol");
            float baseScale = symbolOff.localScale.x;
            yield return ForSeconds(0.5f, () =>
                Assert.AreEqual(baseScale, symbolOff.localScale.x, 0.0001f, "No pulse when effects are disabled."));

            Assert.AreEqual(viewOn.GetVisualRadiusWorld(), viewOff.GetVisualRadiusWorld() * 1.35f, 0.0001f,
                "Disabled effects must not inflate the displacement radius (the 1.35 animated envelope).");
        }

        [UnityTest]
        public IEnumerator DisablingOneEffect_SilencesOnlyThatEffect_OnEveryLevelThatRequestsIt()
        {
            MarkerHierarchyResolver.Configure(new[] { Level("all", "ripple_discs", "halo_ring", pulse: true) });
            var defaults = new EffectDefaults();
            defaults.halo_ring.enabled = false;

            var go = Spawn("all", defaults, out _);
            yield return null;
            AssertEffectsMatchLevel(go, "ripple_discs", "none", true, "halo_ring disabled");

            defaults = new EffectDefaults();
            defaults.pulse.enabled = false;
            defaults.ripple_discs.enabled = false;
            var go2 = Spawn("all", defaults, out _);
            yield return null;
            AssertEffectsMatchLevel(go2, "none", "halo_ring", false, "pulse + ripple_discs disabled");
        }

        // ---------------- re-initialise must not drift ----------------

        [UnityTest]
        public IEnumerator ReInitialiseMidAnimation_DoesNotDriftTheBaseScale()
        {
            MarkerHierarchyResolver.Configure(new[] { Level("p", "none", "none", pulse: true) });
            var defaults = new EffectDefaults();
            defaults.pulse.amplitude = 0.4f;
            defaults.pulse.period = 0.5f;

            var go = Spawn("p", defaults, out var view);
            var symbol = (RectTransform)go.transform.Find("Symbol");
            float baseScale = symbol.localScale.x;

            // Poll instead of a fixed wait: the wave phase depends on the absolute Time.time.
            float deadline = Time.time + 0.6f;
            while (symbol.localScale.x < baseScale * 1.05f && Time.time < deadline)
                yield return null;
            Assert.Greater(symbol.localScale.x, baseScale * 1.05f, "Precondition: pulse is mid-animation.");
            var anchor = go.GetComponent<POIAnchor>();
            view.Initialise(anchor, MarkerVisualSettings.Default(), MarkerEffectFlags.None, defaults);
            defaults.effects_enabled = false;
            view.Initialise(anchor, MarkerVisualSettings.Default(), MarkerEffectFlags.None, defaults);
            yield return null;

            Assert.AreEqual(baseScale, symbol.localScale.x, 0.0001f,
                "Re-initialising mid-pulse must not capture the animated scale as the new base.");
        }

        // ---------------- reveal timing ----------------

        [UnityTest]
        public IEnumerator RevealDelayAndDuration_OfTheLevel_DriveTheRealFadeIn()
        {
            MarkerHierarchyResolver.Configure(new[] { Level("r", "none", "none", pulse: false, revealDelay: 0.30f, revealDuration: 0.30f) });
            var go = Spawn("r", new EffectDefaults(), out var view);
            var group = go.GetComponent<CanvasGroup>();
            Assert.IsNotNull(group, "Marker root carries the CanvasGroup the reveal fades.");
            Assert.AreEqual(0.30f, view.RevealDurationSeconds, 0.0001f, "Level duration reaches the view.");

            yield return null;
            Assert.AreEqual(0f, group.alpha, 0.0001f, "Hidden right after spawn.");

            yield return new WaitForSeconds(0.15f);
            Assert.AreEqual(0f, group.alpha, 0.0001f, "Still hidden inside the 0.30s delay.");

            yield return new WaitForSeconds(0.30f);   // ~0.45s: halfway through the 0.30-0.60s fade
            Assert.Greater(group.alpha, 0.05f, "Fade has started after the delay.");
            Assert.Less(group.alpha, 0.95f, "Fade is not finished halfway through its duration.");

            yield return new WaitForSeconds(0.40f);   // ~0.85s: well past delay + duration
            Assert.AreEqual(1f, group.alpha, 0.0001f, "Fully visible after delay + duration.");
            Assert.AreEqual(1f, go.transform.localScale.x, 0.0001f, "Scale-in finished.");
        }
    }
}
