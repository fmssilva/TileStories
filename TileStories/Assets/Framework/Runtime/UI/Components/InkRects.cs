using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // One piece of ink: where it sits (in the space it was collected for), the colour it is drawn in, and whether it is a word (judged for
    // contrast) or a mark (an icon: part of the shape the shade hugs)
    public readonly struct Ink
    {
        public readonly Rect Rect;
        public readonly Color Colour;
        public readonly bool IsWord;

        public Ink(Rect rect, Color colour, bool isWord)
        {
            Rect = rect;
            Colour = colour;
            IsWord = isWord;
        }
    }

    // Where the INK of some UI sits (_3.3 P3c review, Words On Picture): not an element's box -- a title's label is the hero's full width --
    // but each word of each wrapped line, as wide as its measured run and as tall as its LETTERS (cap height to half the descent, from the
    // face's metrics; the whole line box when the face is unknown), placed as the label aligns it; and each icon's box. In the space of
    // `space` (the element Words On Picture draws on), so its layer and strength follow the words whatever their length, language or stop.
    public static class InkRects
    {
        // Every shown label's words and every shown icon (CardIcons' card-icon, a card-glyph) under `root`, into `into` -- except under an
        // element with its own solid background (a pill button): it already carries its words, a shade round it would only darken the picture
        public static void Collect(VisualElement root, VisualElement space, List<Ink> into)
        {
            if (root == null || space == null || into == null || !CarriesInk(root, space)) return;
            if (root is Label label)
            {
                OfLabel(label, space, into);
                return;
            }
            if (root.ClassListContains("card-icon") || root.ClassListContains("card-glyph"))
            {
                Add(root, root.contentRect, space, root.resolvedStyle.color, false, into);
                return;
            }
            foreach (var child in root.Children()) Collect(child, space, into);
        }

        // Every shown label with words under `root` whose ink Collect would hand (the same rule: not under its own solid ground), into `into`
        public static void ShownLabels(VisualElement root, List<Label> into)
        {
            if (root == null || into == null || !CarriesInk(root, null)) return;
            if (root is Label label)
            {
                if (!string.IsNullOrWhiteSpace(label.text)) into.Add(label);
                return;
            }
            foreach (var child in root.Children()) ShownLabels(child, into);
        }

        // Shown, and not an element with its own solid background (a pill button: it already carries its words) -- `space` itself excepted
        private static bool CarriesInk(VisualElement e, VisualElement space) =>
            Shown(e) && (e == space || e.resolvedStyle.backgroundColor.a < 0.5f);

        // One label's words: wrapped as the text engine wraps them (HeroTitleFitRule.LineWords with the label's own measure), the lines
        // sharing the label's height, each word at its letters' band in its line, placed by the label's alignment
        public static void OfLabel(Label label, VisualElement space, List<Ink> into)
        {
            if (label == null || string.IsNullOrWhiteSpace(label.text) || !Shown(label)) return;
            var box = label.contentRect;
            if (!(box.width > 0f) || !(box.height > 0f)) return;
            var lines = HeroTitleFitRule.LineWords(label.text, box.width,
                run => label.MeasureTextSize(run, 0f, VisualElement.MeasureMode.Undefined, 0f, VisualElement.MeasureMode.Undefined).x);
            if (lines.Count == 0) return;
            float lineHeight = box.height / lines.Count;
            var (bandTop, bandBottom) = LetterBand(label, lineHeight);
            var align = label.resolvedStyle.unityTextAlign;
            var colour = label.resolvedStyle.color;
            for (int i = 0; i < lines.Count; i++)
            {
                var words = lines[i];
                if (words.Count == 0) continue;
                float run = Mathf.Min(words[words.Count - 1].X + words[words.Count - 1].Width, box.width);
                float start = align is TextAnchor.UpperCenter or TextAnchor.MiddleCenter or TextAnchor.LowerCenter ? box.x + (box.width - run) * 0.5f
                    : align is TextAnchor.UpperRight or TextAnchor.MiddleRight or TextAnchor.LowerRight ? box.xMax - run
                    : box.x;
                float top = box.y + i * lineHeight;
                foreach (var (x, w) in words)
                    Add(label, new Rect(start + x, top + bandTop, Mathf.Min(w, box.width), bandBottom - bandTop), space, colour, true, into);
            }
        }

        // The letters' band inside one line box of `label` (from the line's top): the face's cap height above its baseline down to half its
        // descent, the line's spare height shared above and below the face's own; the whole box without a face to read
        private static (float Top, float Bottom) LetterBand(Label label, float lineHeight)
        {
            var font = label.resolvedStyle.unityFontDefinition.fontAsset;
            float size = label.resolvedStyle.fontSize;
            if (font == null || !(font.faceInfo.pointSize > 0f) || !(size > 0f)) return (0f, lineHeight);
            var face = font.faceInfo;
            float s = size / face.pointSize * face.scale;
            float ascent = face.ascentLine * s, descent = -face.descentLine * s;
            float baseline = (lineHeight - (ascent + descent)) * 0.5f + ascent;
            float top = Mathf.Clamp(baseline - face.capLine * s, 0f, lineHeight);
            float bottom = Mathf.Clamp(baseline + descent * 0.5f, top, lineHeight);
            return bottom - top > 1f ? (top, bottom) : (0f, lineHeight);
        }

        // A rectangle of `element`'s own space, carried into `space`
        private static void Add(VisualElement element, Rect local, VisualElement space, Color colour, bool isWord, List<Ink> into)
        {
            var min = element.ChangeCoordinatesTo(space, local.min);
            var max = element.ChangeCoordinatesTo(space, local.max);
            var rect = Rect.MinMaxRect(Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y), Mathf.Max(min.x, max.x), Mathf.Max(min.y, max.y));
            if (rect.width > 0f && rect.height > 0f && !float.IsNaN(rect.x)) into.Add(new Ink(rect, colour, isWord));
        }

        // Shown and laid out: displayed, visible, not faded out
        private static bool Shown(VisualElement e) =>
            e.resolvedStyle.display != DisplayStyle.None && e.resolvedStyle.visibility == Visibility.Visible && e.resolvedStyle.opacity > 0f;
    }
}
