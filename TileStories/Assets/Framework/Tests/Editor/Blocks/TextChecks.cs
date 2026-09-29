using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // The rule for text the DEVELOPER reads in the POI Editor (labels, (i) help, where-notes, warnings, guides): ASCII only
    // (20-code-quality.md 2.1). Written once so every Editor text test asks the same question, and so a test can prove an
    // accented help text still fails. Public: an app's test assembly reaches it (like CardStringTableChecks).
    public static class EditorTextChecks
    {
        // The first character of `text` outside printable ASCII (a newline is allowed), or null when the text is clean
        public static string FirstProblem(string text)
        {
            if (text == null) return "null";
            foreach (char c in text)
                if (c != '\n' && (c < ' ' || c > '~')) return "character code " + (int)c;
            return null;
        }

        public static void AssertAscii(string text, string what) =>
            Assert.IsNull(FirstProblem(text), what + " must be ASCII only (an Editor text)");
    }

    // The rule for text a VISITOR reads (the card's string tables, a wall's Card Texts, POI card content, glossary): real
    // language, so accents and c-cedillas are right and welcome (20-code-quality.md 2.1 exception). What it must be is VALID:
    // composed (NFC: a tilde stored as its own combining mark draws badly in some fonts), no control characters (a newline is
    // a paragraph break), no replacement character (a sign the text was decoded with the wrong encoding) and no lone surrogate.
    public static class VisitorTextChecks
    {
        // Why `text` is not valid visitor text, or null when it is
        public static string Problem(string text)
        {
            if (text == null) return "null";
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '�') return "replacement character at " + i + " (wrong encoding)";
                if (char.IsControl(c) && c != '\n') return "control character code " + (int)c + " at " + i;
                if (char.IsHighSurrogate(c) && !(i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))) return "lone surrogate at " + i;
                if (char.IsLowSurrogate(c) && !(i > 0 && char.IsHighSurrogate(text[i - 1]))) return "lone surrogate at " + i;
            }
            if (!text.IsNormalized(NormalizationForm.FormC)) return "not in normalisation form C (a letter and its accent stored apart)";
            return null;
        }

        public static void AssertValid(string text, string what) => Assert.IsNull(Problem(text), what + ": " + Problem(text) + " in \"" + text + "\"");

        // Every LocalizedEntry value found anywhere under `root` (lists and TileStories classes are walked by reflection), with
        // where it was found ("pois[3].card.blocks[7]..."): what the shipped-content test judges, so a field added later is covered
        public static List<(string Where, string Lang, string Value)> LocalizedValues(object root, string rootName)
        {
            var found = new List<(string, string, string)>();
            Walk(root, rootName, found, 0);
            return found;
        }

        private static void Walk(object node, string path, List<(string, string, string)> found, int depth)
        {
            if (node == null || depth > 24) return;
            if (node is LocalizedEntry entry)
            {
                found.Add((path, entry.lang, entry.value));
                return;
            }
            var type = node.GetType();
            if (node is IList list)
            {
                for (int i = 0; i < list.Count; i++) Walk(list[i], path + "[" + i + "]", found, depth + 1);
                return;
            }
            if (type.Namespace != "TileStories") return;
            foreach (var f in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                Walk(f.GetValue(node), path + "." + f.Name, found, depth + 1);
        }
    }
}
