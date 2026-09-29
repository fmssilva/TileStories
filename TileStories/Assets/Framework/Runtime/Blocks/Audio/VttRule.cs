using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TileStories
{
    // WebVTT captions (_3.1 step 9A), pure: Parse reads the file's text into cues, TextAt says which caption line belongs to a time of
    // the clip. Reads what a captions file of a narration needs: the WEBVTT header, optional cue ids, "hh:mm:ss.mmm --> hh:mm:ss.mmm"
    // (the hours optional, a comma accepted for the dot), cue settings after the end time (ignored), a cue's several lines joined with a
    // space, <tags> dropped and the few HTML entities decoded. NOTE / STYLE / REGION blocks and a cue that cannot be read are skipped,
    // never an exception: a broken captions file shows no line, it does not stop the audio.
    public static class VttRule
    {
        // One caption: shown from Start (inclusive) to End (exclusive), in seconds of the clip
        public readonly struct Cue
        {
            public readonly float Start;
            public readonly float End;
            public readonly string Text;

            public Cue(float start, float end, string text)
            {
                Start = start;
                End = end;
                Text = text;
            }
        }

        // The cues of a WebVTT text, ordered by start time (an empty list for text that is not WebVTT)
        public static IReadOnlyList<Cue> Parse(string vtt)
        {
            var cues = new List<Cue>();
            if (string.IsNullOrWhiteSpace(vtt)) return cues;
            string text = vtt.TrimStart('﻿').Replace("\r\n", "\n").Replace('\r', '\n');
            var blocks = text.Split(new[] { "\n\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            if (blocks.Length == 0 || !blocks[0].TrimStart().StartsWith("WEBVTT")) return cues;
            // - the first block is the header (it may hold lines after WEBVTT): every later block is a cue or a comment
            for (int b = 1; b < blocks.Length; b++)
            {
                var lines = blocks[b].Split('\n');
                string first = lines[0].TrimStart();
                if (first.StartsWith("NOTE") || first.StartsWith("STYLE") || first.StartsWith("REGION")) continue;
                int timing = -1;
                for (int i = 0; i < lines.Length && i < 2; i++)
                    if (lines[i].Contains("-->")) { timing = i; break; }
                if (timing < 0) continue;
                if (!TryTimes(lines[timing], out float start, out float end) || end <= start) continue;
                var body = new StringBuilder();
                for (int i = timing + 1; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (line.Length == 0) continue;
                    if (body.Length > 0) body.Append(' ');
                    body.Append(line);
                }
                cues.Add(new Cue(start, end, Clean(body.ToString())));
            }
            // - a stable sort by start: cues are normally in order already, a hand-edited file may not be
            for (int i = 1; i < cues.Count; i++)
            {
                var cue = cues[i];
                int j = i - 1;
                while (j >= 0 && cues[j].Start > cue.Start) { cues[j + 1] = cues[j]; j--; }
                cues[j + 1] = cue;
            }
            return cues;
        }

        // The caption line at `time` seconds: the latest-starting cue that covers it, "" between cues
        public static string TextAt(IReadOnlyList<Cue> cues, float time)
        {
            int at = IndexAt(cues, time);
            return at >= 0 ? cues[at].Text : "";
        }

        // The index of the cue showing at `time`, or -1
        public static int IndexAt(IReadOnlyList<Cue> cues, float time)
        {
            if (cues == null) return -1;
            for (int i = cues.Count - 1; i >= 0; i--)
                if (time >= cues[i].Start && time < cues[i].End) return i;
            return -1;
        }

        // "00:01:02.500 --> 00:01:05.000 align:start" -> 62.5, 65
        private static bool TryTimes(string line, out float start, out float end)
        {
            start = end = 0f;
            int arrow = line.IndexOf("-->", System.StringComparison.Ordinal);
            if (arrow < 0) return false;
            string left = line.Substring(0, arrow).Trim();
            string[] right = line.Substring(arrow + 3).Trim().Split(new[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
            return right.Length > 0 && TryTime(left, out start) && TryTime(right[0], out end);
        }

        // "mm:ss.mmm" or "hh:mm:ss.mmm" in seconds
        private static bool TryTime(string stamp, out float seconds)
        {
            seconds = 0f;
            string[] parts = stamp.Replace(',', '.').Split(':');
            if (parts.Length < 2 || parts.Length > 3) return false;
            double total = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                if (!double.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out double value) || value < 0) return false;
                total = total * 60 + value;
            }
            seconds = (float)total;
            return true;
        }

        // Drop <tags> and decode the entities a caption file may hold
        private static string Clean(string text)
        {
            var sb = new StringBuilder();
            bool inTag = false;
            foreach (char c in text)
            {
                if (c == '<') inTag = true;
                else if (c == '>' && inTag) inTag = false;
                else if (!inTag) sb.Append(c);
            }
            return sb.ToString().Replace("&lt;", "<").Replace("&gt;", ">").Replace("&nbsp;", " ").Replace("&amp;", "&").Trim();
        }
    }
}
