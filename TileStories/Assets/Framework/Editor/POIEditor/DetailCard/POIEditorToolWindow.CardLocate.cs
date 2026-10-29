// POIEditorToolWindow.CardLocate.cs
//
// Partial: Detail Card > Locate Button (_3.3 P3d review b) -- how the card's "Show me where it is" pin looks (card_settings.locate,
// LocateFabRule): Style, Colour, Size, Float (CardLocateRows). Each row SHOWS what the card reads (an unknown or out-of-range value as the
// rule reads it) and writes only a real pick, so drawing an old config never rewrites it. Whether the button shows at all is the Navigation
// Bar's Locate Button switch: a note says so while it is off. The (i) texts and Test guides live in POIEditorToolWindow.CardLocateHelp.cs.

using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // The component's rows, in the brief's order: what the tests read the window against
        internal static readonly string[] CardLocateRows = { "Style", "Colour", "Size", "Float" };
        private readonly TestGuideState _cardLocateTest = new TestGuideState();

        private void DrawCardLocateSection()
        {
            var s = _config.card_settings;
            var nav = s.navigation;
            if (!NavBarRule.Enabled(nav) || !NavBarRule.LocateButton(nav?.bar))
                UnityEditor.EditorGUILayout.HelpBox(CardLocateOffNote, UnityEditor.MessageType.Info);
            var locate = s.locate ??= new CardLocateSettings();

            string style = LocateFabRule.Style(locate);
            string pickedStyle = DrawPopupField("Style", style, CardOptions.LocateStyles, CardOptions.LocateStyleLabels, CardLocateStyleHelp);
            if (pickedStyle != style) locate.style = pickedStyle;
            string colour = LocateFabRule.Colour(locate);
            string pickedColour = DrawPopupField("Colour", colour, CardOptions.LocateColours, CardOptions.LocateColourLabels, CardLocateColourHelp);
            if (pickedColour != colour) locate.colour = pickedColour;
            float size = LocateFabRule.Size(locate);
            float slid = DrawSliderField("Size", size, CardLocateSettings.SizeMin, CardLocateSettings.SizeMax, CardLocateSizeHelp);
            if (!Mathf.Approximately(slid, size)) locate.size = slid;
            bool floats = locate.float_motion;
            if (DrawToggleField("Float", floats, CardLocateFloatHelp) != floats) locate.float_motion = !floats;

            DrawDomainTestSubSection(_cardLocateTest, CardLocateSceneTestGuide, CardLocatePlaymodeTestGuide, CardLocateDeviceTestGuide, null);
        }
    }
}
