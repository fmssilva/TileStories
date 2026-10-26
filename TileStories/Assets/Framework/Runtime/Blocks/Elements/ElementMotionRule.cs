using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // How a row moves (_3.3 R17.3, R6, R11 v4), pure: every answer is a number the motion driver writes as a transform or an opacity, never a
    // layout property (the AR camera runs beside the card). A row stores ONE motion value (effect + strength); Auto plays the element's own
    // default (ElementKindDefinition.AutoMotion: pictures a subtle parallax, text a reveal), and an effect the element does not offer plays
    // that default too. The numbers are the prototype's magazine (_3.3.1 Story.dc.html) on R6's scale:
    //   - Reveal: the row fades in and rises RevealRise as its top crosses EnterLine of the view, over RevealDuration (slow, decelerating);
    //     rows that enter together follow one by one, Stagger apart
    //   - Slide in: the same, travelling in from the start side by SlideDistance
    //   - Parallax: a picture moves inside its clipped frame by ParallaxFactor of its distance to the view's centre (up to its frame's room)
    //   - Slow zoom / Pan: a picture pushes in to ZoomMax, or travels across its room, there and back once per ZoomPeriod / PanPeriod --
    //     moving by itself, so only ONE such row plays at a time in view (Mover) and it stops off-screen
    //   - Strong doubles a subtle effect's travel
    // Reduce Motion is not a choice per row but this rule: every effect is STILL -- the row simply there, a picture at rest in its frame --
    // which the Editor's Motion table shows read-only, so accessibility cannot be configured away.
    public static class ElementMotionRule
    {
        // What an effect becomes under Reduce Motion (one value today; the Editor's Motion table names it)
        public enum Reduced
        {
            // Nothing moves: the row is simply there (a still picture for parallax / slow zoom / pan, a revealed row for a reveal)
            Still,
        }

        // A row has entered the view when its top is above this share of the view's height (and it is not wholly above the view)
        public const float EnterLine = 0.92f;
        // Reveal / slide: the travel (subtle), how long it takes, and how far apart rows that enter together start
        public const float RevealRise = 16f;
        public const float SlideDistance = 24f;
        public const float RevealDuration = 0.36f;
        public const float Stagger = 0.06f;
        // At most this many steps of stagger: the sixth row entering together does not wait longer than the fifth
        public const int MaxStaggerSteps = 5;
        // Parallax: the picture's share of its distance to the view's centre (subtle; R6 M3 "a window with depth")
        public const float ParallaxFactor = 0.1f;
        // Slow zoom: the furthest push-in (subtle) and one there-and-back (s); the prototype's 14 s each way
        public const float ZoomMax = 1.06f;
        public const float ZoomPeriod = 28f;
        // Pan: the share of the picture's sideways room it travels (subtle), one there-and-back (s) -- the hero's pace
        public const float PanShare = 0.5f;
        public const float PanPeriod = HeroMotionRule.PanPeriod;
        // A strong effect travels this many times a subtle one
        public const float StrongFactor = 2f;

        // The effect a row plays: its stored one when its element offers it and it is not Auto, else the element's Auto
        public static string EffectOf(ElementRow row, ElementKindDefinition element)
        {
            if (element == null) return ElementOptions.MotionNone;
            string effect = row.Motion;
            return effect != ElementOptions.MotionAuto && element.OffersMotion(effect) ? effect : element.AutoMotion;
        }

        // What `effect` becomes when the visitor, the OS or the wall reduces motion: still, whatever it is
        public static Reduced ReducedOf(string effect) => Reduced.Still;

        // Whether an effect plays as the row enters the view (a one-off), moves with the scroll, or moves by itself
        public static bool Enters(string effect) => effect == ElementOptions.MotionReveal || effect == ElementOptions.MotionSlideIn;
        public static bool MovesPicture(string effect) =>
            effect == ElementOptions.MotionParallax || effect == ElementOptions.MotionSlowZoom || effect == ElementOptions.MotionPan;
        public static bool MovesByItself(string effect) => effect == ElementOptions.MotionSlowZoom || effect == ElementOptions.MotionPan;

        // How much further a strong effect travels
        public static float StrengthOf(string strength) => strength == ElementOptions.StrengthStrong ? StrongFactor : 1f;

        // Whether a row whose box runs from `top` to `bottom` (measured from the view's top) has entered a view `viewport` tall
        public static bool Entered(float top, float bottom, float viewport)
        {
            if (float.IsNaN(top) || float.IsNaN(bottom) || float.IsNaN(viewport) || viewport <= 0f) return false;
            return top < viewport * EnterLine && bottom > 0f;
        }

        // The wait before the `order`-th row entering at the same moment starts (0 for the first)
        public static float StaggerDelay(int order) => Mathf.Clamp(order, 0, MaxStaggerSteps) * Stagger;

        // The share of a reveal done `elapsed` seconds after it starts: 0 before, 1 once over, decelerating (ease-out cubic) in between
        public static float RevealProgress(float elapsed)
        {
            if (float.IsNaN(elapsed) || elapsed <= 0f) return 0f;
            float t = Mathf.Clamp01(elapsed / RevealDuration);
            float u = 1f - t;
            return 1f - u * u * u;
        }

        // Where an entering row is drawn at `progress` (0 = before it entered, 1 = at rest): its opacity and its offset from its place (panel
        // units, +y down). An effect that does not enter is at rest
        public static (float Opacity, Vector2 Offset) EnterPose(string effect, string strength, float progress)
        {
            if (!Enters(effect)) return (1f, Vector2.zero);
            float p = Mathf.Clamp01(progress);
            float far = (1f - p) * StrengthOf(strength);
            return effect == ElementOptions.MotionSlideIn
                ? (p, new Vector2(-SlideDistance * far, 0f))
                : (p, new Vector2(0f, RevealRise * far));
        }

        // A picture's offset inside its frame (+y down) for parallax: against its distance from the view's centre, kept inside the frame's
        // `room` (how far the picture overhangs its frame on each side). A picture at the view's centre sits at rest
        public static float ParallaxOffset(float rowCentre, float viewCentre, string strength, float room)
        {
            if (float.IsNaN(rowCentre) || float.IsNaN(viewCentre)) return 0f;
            float limit = float.IsNaN(room) ? 0f : Mathf.Max(0f, room);
            return Mathf.Clamp(-(rowCentre - viewCentre) * ParallaxFactor * StrengthOf(strength), -limit, limit);
        }

        // The share of the way there (0 = start, 1 = the far end) at `time` on a there-and-back of `period` seconds, eased at both ends
        public static float Phase(float time, float period)
        {
            if (float.IsNaN(time) || float.IsInfinity(time) || period <= 0f) return 0f;
            float t = Mathf.Repeat(time, period) / period;
            float there = t < 0.5f ? t * 2f : 2f - t * 2f;
            return there * there * (3f - 2f * there);
        }

        // A slow zoom's scale at `time` seconds into its motion (1 = fills the frame)
        public static float ZoomScale(float time, string strength) =>
            1f + (ZoomMax - 1f) * StrengthOf(strength) * Phase(time, ZoomPeriod);

        // A pan's sideways offset at `time` seconds into its motion: from +travel to -travel and back, travel = PanShare of the `room` the
        // picture overhangs its frame on each side (strong: all of it)
        public static float PanOffset(float time, string strength, float room)
        {
            float limit = float.IsNaN(room) ? 0f : Mathf.Max(0f, room);
            float travel = Mathf.Min(limit, limit * PanShare * StrengthOf(strength));
            return Mathf.Lerp(travel, -travel, Phase(time, PanPeriod));
        }

        // Which ONE of the rows that move by themselves plays now (R6: one moving thing in view): the one in view (`visible` > 0) whose
        // centre is nearest the view's centre (`distance`); the first of equals. -1 when none is in view
        public static int Mover(IReadOnlyList<(bool MovesByItself, float Visible, float Distance)> rows)
        {
            int best = -1;
            float nearest = float.MaxValue;
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                if (!row.MovesByItself || !(row.Visible > 0f) || float.IsNaN(row.Distance)) continue;
                float d = Mathf.Abs(row.Distance);
                if (d < nearest)
                {
                    nearest = d;
                    best = i;
                }
            }
            return best;
        }
    }
}
