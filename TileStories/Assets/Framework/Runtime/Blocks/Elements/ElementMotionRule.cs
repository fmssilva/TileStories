namespace TileStories
{
    // How a row moves (_3.3 R17.3), pure. A row stores ONE motion value (effect + strength); Auto plays the element's own default
    // (ElementKindDefinition.AutoMotion: pictures a subtle parallax, text a reveal), and an effect the element does not offer (a hand-edited
    // file, an element whose choices changed) plays that default too. Reduce Motion is not a choice per row but this rule: every effect has
    // ONE still fallback -- a reveal becomes a short cross-fade, everything that moves becomes still -- which the Editor's Motion table shows
    // read-only, so accessibility cannot be configured away.
    public static class ElementMotionRule
    {
        // What an effect becomes under Reduce Motion
        public enum Reduced
        {
            // Nothing moves: the row is simply there (a still picture for parallax / slow zoom / pan)
            Still,
            // The row appears with a short cross-fade, no travel
            CrossFade,
        }

        // The effect a row plays: its stored one when its element offers it and it is not Auto, else the element's Auto
        public static string EffectOf(ElementRow row, ElementKindDefinition element)
        {
            if (element == null) return ElementOptions.MotionNone;
            string effect = row.Motion;
            return effect != ElementOptions.MotionAuto && element.OffersMotion(effect) ? effect : element.AutoMotion;
        }

        // What `effect` becomes when the visitor, the OS or the wall reduces motion
        public static Reduced ReducedOf(string effect) =>
            effect == ElementOptions.MotionReveal ? Reduced.CrossFade : Reduced.Still;
    }
}
