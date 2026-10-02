using System;
using System.Collections.Generic;

namespace TileStories
{
    // When the Editor draws a block field (_3.1 step 15.1): a small declarative condition over the block's OWN values -- the look it is
    // drawn in, another field's option, or whether another field has words. A field without one is always drawn. Hiding is drawing only:
    // a hidden field keeps its value (saved, and shown again when the condition holds again). FieldVisibilityRule evaluates it; a kind
    // declares it on BlockFieldDefinition.ShownWhen, so an app's kind gets the same behaviour with no Editor code.
    public sealed class FieldShownWhen
    {
        public enum Test
        {
            // The block's resolved look is one of Values
            Looks,
            // The effective option of the Choice field FieldKey is one of Values
            Choice,
            // The text field FieldKey has words in some language
            Filled,
        }

        public Test Kind { get; private set; }
        // Choice / Filled: the other field's key -- a block field for a block field, a field of the SAME row for an Items sub-field
        public string FieldKey { get; private set; }
        // Looks: the variants; Choice: the option values
        public IReadOnlyList<string> Values { get; private set; }

        public static FieldShownWhen Looks(params string[] variants) => new() { Kind = Test.Looks, Values = variants };

        public static FieldShownWhen Choice(string fieldKey, params string[] values) => new() { Kind = Test.Choice, FieldKey = fieldKey, Values = values };

        public static FieldShownWhen Filled(string fieldKey) => new() { Kind = Test.Filled, FieldKey = fieldKey, Values = Array.Empty<string>() };

        // Whether the condition holds: `variant` is the look the block is drawn in, `choiceOf(key)` a Choice field's effective option, and
        // `filled(key)` whether a text field has words
        public bool Holds(string variant, Func<string, string> choiceOf, Func<string, bool> filled)
        {
            switch (Kind)
            {
                case Test.Looks: return Contains(Values, variant);
                case Test.Choice: return Contains(Values, choiceOf?.Invoke(FieldKey));
                default: return filled != null && filled(FieldKey);
            }
        }

        private static bool Contains(IReadOnlyList<string> list, string value)
        {
            if (list == null || value == null) return false;
            for (int i = 0; i < list.Count; i++)
                if (list[i] == value) return true;
            return false;
        }
    }
}
