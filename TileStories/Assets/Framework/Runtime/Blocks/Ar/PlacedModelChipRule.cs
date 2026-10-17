namespace TileStories
{
    // What the card's placed-model chip says (_3.2 section 5.4, step 9; the two AR TODOs _3.1 left): while a model placed with place_in_ar
    // stands in the room (the ONE ArPlacementService), a small chip floats above the sheet at every stop, so the visitor knows it is there
    // even at peek. Pure.
    //   - Hidden: the wall's Placed Model Control is none, no card is open, or nothing stands
    //   - ThisCard: the open card is the point the model was placed for -- "Placed by the wall" and a Remove
    //   - OtherCard: another point's card is open (Keep Model On Switch kept the model) -- the chip says which point's model stands and offers
    //     no Remove: only the card that placed it does (a tap on the chip opens that card)
    public static class PlacedModelChipRule
    {
        public enum State { Hidden, ThisCard, OtherCard }

        // `control`: card_settings.navigation.placed_model_control (an unknown value reads as the default, the chip); `placedPoiId`: the point
        // whose model stands (null: none); `cardPoiId`: the point of the open card
        public static State Of(string control, bool cardOpen, string placedPoiId, string cardPoiId)
        {
            if (control == CardOptions.PlacedModelNone || !cardOpen || string.IsNullOrEmpty(placedPoiId)) return State.Hidden;
            return placedPoiId == cardPoiId ? State.ThisCard : State.OtherCard;
        }
    }
}
