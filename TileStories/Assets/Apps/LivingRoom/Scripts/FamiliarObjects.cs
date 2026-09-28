using System.Collections.Generic;

namespace TileStories.LivingRoom
{
    // One everyday object whose real size every visitor knows (a card, a coin, a phone): its size in centimetres
    public readonly struct FamiliarObject
    {
        public readonly string Key;
        public readonly float WidthCm;
        public readonly float HeightCm;

        public FamiliarObject(string key, float widthCm, float heightCm)
        {
            Key = key;
            WidthCm = widthCm;
            HeightCm = heightCm;
        }
    }

    // The app's own card service (_3.1 step 11): what the size_comparison block asks of the wall app. A block view gets it
    // through BlockBindContext.Service<IFamiliarObjects>() -- registered by LivingRoomBlocks at startup, never a Framework field.
    public interface IFamiliarObjects
    {
        // The object of this key; false for a key the app does not know
        bool TryGet(string key, out FamiliarObject familiar);
    }

    // The living room's table of familiar objects: the ONE place their real sizes live (the block's Object choice, the drawing and the
    // tests all read it)
    public sealed class FamiliarObjects : IFamiliarObjects
    {
        public const string CreditCard = "credit_card";
        public const string TwoEuroCoin = "two_euro_coin";
        public const string Smartphone = "smartphone";
        public const string SheetA4 = "sheet_a4";

        // The keys the block's Object choice offers, and (same order) the names the Editor shows
        public static readonly IReadOnlyList<string> Keys = new[] { CreditCard, TwoEuroCoin, Smartphone, SheetA4 };
        public static readonly IReadOnlyList<string> EditorLabels = new[] { "Credit card", "2 euro coin", "Smartphone", "A4 sheet" };

        // Real sizes (ISO 7810 card, the coin's diameter, a common phone, ISO 216 A4), width x height with the object upright
        private static readonly FamiliarObject[] Table =
        {
            new(CreditCard, 5.4f, 8.56f),
            new(TwoEuroCoin, 2.575f, 2.575f),
            new(Smartphone, 7.2f, 15f),
            new(SheetA4, 21f, 29.7f),
        };

        public bool TryGet(string key, out FamiliarObject familiar)
        {
            foreach (var entry in Table)
            {
                if (entry.Key != key) continue;
                familiar = entry;
                return true;
            }
            familiar = default;
            return false;
        }
    }
}
