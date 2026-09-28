using System.Collections.Generic;

namespace TileStories.LivingRoom
{
    // The living room app's own visitor words on the POI Detail Card (_3.1 step 11-fix). The words live in the app's own table asset
    // (Resources/LivingRoom/LivingRoomCardStrings.asset, English and Portuguese, a "where" note per row); this class only names the table
    // and its keys, so no visitor word is written in the app's code. LivingRoomBlocks registers the table with the Framework's
    // CardStringSources, and a wall can reword any row in Detail Card > Card Texts (listed under the app's name there).
    public static class LivingRoomCardTexts
    {
        // What Card Texts groups the app's rows under, and what the registry knows the table by
        public const string AppName = "Living Room";

        // The table asset, as Resources.Load reads it (no extension): found at runtime and in Edit Mode alike
        public const string TableResourcePath = "LivingRoom/LivingRoomCardStrings";

        // Every key the app's code reads. A key is added with the code that first shows it, and starts with the app's word so it can never
        // clash with a Framework key or another app's.
        public static class Keys
        {
            // The names of the familiar objects a size_comparison block draws beside a point (one per FamiliarObjects entry)
            public const string ObjectCreditCard = "living_room_object_credit_card";
            public const string ObjectTwoEuroCoin = "living_room_object_two_euro_coin";
            public const string ObjectSmartphone = "living_room_object_smartphone";
            public const string ObjectSheetA4 = "living_room_object_sheet_a4";

            public static readonly IReadOnlyList<string> All = new[] { ObjectCreditCard, ObjectTwoEuroCoin, ObjectSmartphone, ObjectSheetA4 };
        }
    }
}
