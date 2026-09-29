using System.Globalization;

namespace TileStories
{
    // A Time field's value (_3.1 step 9B: a video chapter's start), pure: the config stores it as the developer typed it, one value for every
    // language, and this rule is the ONE reading of it -- plain seconds ("90", "12.5"), minutes:seconds ("1:30") or hours:minutes:seconds
    // ("1:02:03"). The seconds and minutes after a colon are below 60 and only the last part may have a fraction; anything else is not a
    // time (a row with it is incomplete: BlockFieldReader), never an exception.
    public static class TimeCodeRule
    {
        public static bool TryParse(string text, out float seconds)
        {
            seconds = 0f;
            if (string.IsNullOrWhiteSpace(text)) return false;
            string[] parts = text.Trim().Split(':');
            if (parts.Length > 3) return false;
            double total = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                bool last = i == parts.Length - 1;
                string part = parts[i];
                if (part.Length == 0 || part.StartsWith("-") || part.StartsWith("+")) return false;
                if (!last && part.Contains(".")) return false;
                if (!double.TryParse(part, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double value)) return false;
                if (i > 0 && value >= 60) return false;
                total = total * 60 + value;
            }
            if (double.IsInfinity(total) || total > float.MaxValue) return false;
            seconds = (float)total;
            return true;
        }

        public static bool IsValid(string text) => TryParse(text, out _);
    }
}
