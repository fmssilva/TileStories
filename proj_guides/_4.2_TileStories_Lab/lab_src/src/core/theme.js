/* The theme (merge P1b.1, THEME_MAP.md): Unity's ThemeAsset as data, written onto the phone as class rules. The Lab specifies Unity,
   so the table has the asset's shape and names: D.theme = Theme.default.asset (TileStories/Assets/Framework/Runtime/Resources/
   TileStories/Themes; ThemeAsset.cs): key, defaultMode, shared (the values every mode shares), modes (one token set per mode), knobs
   (a wall's closed choices: ThemeKnobOption, the values one option lays over the theme's) and type (the font keys). A token is
   {name, value[, proposed, note]}; a colour is "#RRGGBB" or "#RRGGBB@alpha" (Unity's rgb() / rgba()). Unity's values win
   (_4.2_Lab_Merge_Plan.md §5 call 6): tests/test10g checks every token against Theme.default.uss and the asset's knobs. A token,
   knob or option Unity does not have yet is flagged proposed: true (the additions for the ThemeAsset, THEME_MAP.md §1.2); the test
   fails when Unity adds one, so the flag comes off.
   The wall's picks are D.wall.theme (WallConfigData, guess): its Default Mode and one option per knob. A look picks named options
   of the other knobs (Playful: cobalt, round, solid), read at draw time; its pick wins over the wall's for that knob.
   applyTokens() writes the table into one style element as class rules (ThemeUssGenerator's shape: .ts-app.ts-theme-default.
   ts-mode-<mode>, the font rule, then one rule per knob option: .ts-<knob>-<option>), again on every configChanged. Every root
   that draws the app carries ts-app + its classes (themeMark): a mode or knob change swaps one class on each (themeSwap =
   ThemeController.SetMode). The Lab blends see-through colours in sRGB (CSS cannot do otherwise); Unity renders in linear
   colour, so the contrast helpers below compose in linear light, as ThemeContrastTests does. */
D.theme={key:"default",defaultMode:"light",
 shared:{
  lengths:[
    {name:"--ts-line-width",value:"1px"},{name:"--ts-size-body",value:"16px"},{name:"--ts-size-body-open",value:"17px"},
    {name:"--ts-size-title",value:"24px"},{name:"--ts-size-caption",value:"14px"},{name:"--ts-size-lede",value:"19px"},
    {name:"--ts-size-heading",value:"20px"},{name:"--ts-size-hero",value:"40px"},{name:"--ts-size-hero-narrow",value:"34px"},
    {name:"--ts-size-dropcap",value:"48px"},{name:"--ts-size-quote",value:"22px"},{name:"--ts-radius-s",value:"6px"},{name:"--ts-radius-m",value:"12px"},
    {name:"--ts-radius-l",value:"20px"},{name:"--ts-radius-xl",value:"30px"},{name:"--ts-radius-round",value:"50%"},{name:"--ts-space-1",value:"4px"},
    {name:"--ts-space-2",value:"8px"},{name:"--ts-space-3",value:"12px"},{name:"--ts-space-4",value:"16px"},{name:"--ts-space-5",value:"24px"},
    {name:"--ts-space-6",value:"32px"},{name:"--ts-space-7",value:"48px",proposed:true,note:"the touch target"},
    {name:"--ts-size-display",value:"28px",proposed:true},{name:"--ts-size-chip",value:"15px",proposed:true},
    {name:"--ts-size-eyebrow",value:"13px",proposed:true,note:"the 13 pt floor"},{name:"--ts-size-title-large",value:"32px",proposed:true},
    {name:"--ts-radius-pill",value:"999px",proposed:true},{name:"--ts-outline-width",value:"0px",proposed:true}],
  numbers:[
    {name:"--ts-icon-stroke",value:1.5,proposed:true}],
  durations:[
    {name:"--ts-duration",value:"0.22s"},{name:"--ts-duration-fast",value:"0.12s",proposed:true},{name:"--ts-duration-slow",value:"0.36s",proposed:true},
    {name:"--ts-duration-slower",value:"0.5s",proposed:true}],
  easings:[
    {name:"--ts-ease",value:"ease-out-cubic"},{name:"--ts-ease-emphasis",value:"ease-out-back",proposed:true}]},
 modes:[{key:"light",tokens:{
    colours:[
      {name:"--ts-bg",value:"#F4F0E8"},{name:"--ts-surface",value:"#FFFFFF"},{name:"--ts-surface-2",value:"#F4F0E8"},{name:"--ts-scrim",value:"#000000@0.7"},
      {name:"--ts-scrim-hero",value:"#0A0806@0.94"},{name:"--ts-on-scrim",value:"#FFFFFF"},{name:"--ts-on-scrim-2",value:"#F1E9DA"},
      {name:"--ts-hero-glow",value:"#0A0806@0.72"},{name:"--ts-text-halo",value:"#000000@0.85"},{name:"--ts-bar",value:"#FFFFFF"},
      {name:"--ts-button-glass",value:"#14110D@0.07"},{name:"--ts-top-shade",value:"#000000@0.5"},{name:"--ts-chrome-glass",value:"#FAF8F4@0.72"},
      {name:"--ts-chrome-band",value:"#FAF8F4@0.94"},{name:"--ts-top-fade",value:"#F4F0E8"},{name:"--ts-navbar-glass",value:"#FAF8F4@0.72"},
      {name:"--ts-chrome-glass-hero",value:"#0A0806@0.92"},{name:"--ts-chrome-line-hero",value:"#FFFFFF@0.32"},
      {name:"--ts-glass-ground",value:"#F4F0E8@0.62"},{name:"--ts-ink",value:"#14110D"},{name:"--ts-ink-2",value:"#3D362D"},
      {name:"--ts-ink-3",value:"#4F473C"},{name:"--ts-on-accent",value:"#FFFFFF"},{name:"--ts-accent",value:"#1E3A8A"},
      {name:"--ts-accent-text",value:"#1E3A8A"},{name:"--ts-accent-shine",value:"#2246AA"},{name:"--ts-accent-2",value:"#C9A227"},
      {name:"--ts-line",value:"#6F665A"},{name:"--ts-hair",value:"#D9D2C6"},{name:"--ts-metal-edge",value:"#433C32"},{name:"--ts-metal-hi",value:"#FFFFFF"},
      {name:"--ts-metal-lo",value:"#8A8276"},{name:"--ts-metal-groove",value:"#B0A89B"},{name:"--ts-elevation-1",value:"#14110D@0.2"},
      {name:"--ts-elevation-2",value:"#14110D@0.32"},{name:"--ts-status-0",value:"#2E7D32"},{name:"--ts-status-1",value:"#7A6200"},
      {name:"--ts-status-2",value:"#A35400"},{name:"--ts-status-3",value:"#A62B1F"},{name:"--ts-status-4",value:"#6B6B6B"},
      {name:"--ts-family-meta",value:"#5E574C"},{name:"--ts-family-about",value:"#7E4206"},{name:"--ts-family-media",value:"#1E4E8C"},
      {name:"--ts-family-guides",value:"#245733"},{name:"--ts-family-stories",value:"#7A2E5C"},{name:"--ts-family-ar",value:"#0F5E5A"},
      {name:"--ts-family-play",value:"#9A2F1F"},{name:"--ts-family-visit",value:"#5A4A8A"},{name:"--ts-family-community",value:"#7A4B00"},
      {name:"--ts-family-navigation",value:"#4A5A12"},{name:"--ts-family-meta-ink",value:"#FFFFFF"},{name:"--ts-family-about-ink",value:"#FFFFFF"},
      {name:"--ts-family-media-ink",value:"#FFFFFF"},{name:"--ts-family-guides-ink",value:"#FFFFFF"},{name:"--ts-family-stories-ink",value:"#FFFFFF"},
      {name:"--ts-family-ar-ink",value:"#FFFFFF"},{name:"--ts-family-play-ink",value:"#FFFFFF"},{name:"--ts-family-visit-ink",value:"#FFFFFF"},
      {name:"--ts-family-community-ink",value:"#FFFFFF"},{name:"--ts-family-navigation-ink",value:"#FFFFFF"},{name:"--ts-family-meta-text",value:"#38332C"},
      {name:"--ts-family-about-text",value:"#532902"},{name:"--ts-family-media-text",value:"#123461"},{name:"--ts-family-guides-text",value:"#163B21"},
      {name:"--ts-family-stories-text",value:"#581F41"},{name:"--ts-family-ar-text",value:"#063A37"},{name:"--ts-family-play-text",value:"#621B10"},
      {name:"--ts-family-visit-text",value:"#382D59"},{name:"--ts-family-community-text",value:"#4C2D00"},
      {name:"--ts-family-navigation-text",value:"#2D3808"},{name:"--ts-sheet-open",value:"#F8F6F2"},{name:"--ts-locate-teal-hi",value:"#7FE3C4"},
      {name:"--ts-locate-teal-lo",value:"#0E9A7C"},{name:"--ts-locate-teal-rim",value:"#086C56"},{name:"--ts-locate-green-hi",value:"#B8EF7A"},
      {name:"--ts-locate-green-lo",value:"#3F9E2C"},{name:"--ts-locate-green-rim",value:"#286E1C"},{name:"--ts-locate-cyan-hi",value:"#8BE4FF"},
      {name:"--ts-locate-cyan-lo",value:"#0A84B8"},{name:"--ts-locate-cyan-rim",value:"#076A94"},{name:"--ts-locate-disc",value:"#FFFFFF"},
      {name:"--ts-locate-disc-edge",value:"#E7ECE9"},{name:"--ts-locate-shadow",value:"#000000@0.28"},{name:"--ts-locate-halo",value:"#000000@0.8"},
      {name:"--ts-locate-chip",value:"#FFFFFF@0.96"},{name:"--ts-locate-chip-ink",value:"#123B32"},{name:"--ts-locate-glass",value:"#FFFFFF@0.78"},
      {name:"--ts-locate-glass-line",value:"#FFFFFF@0.95"},{name:"--ts-section-neutral",value:"#6B6359"},
      {name:"--ts-family-meta-glass",value:"#DFDAD0@0.55"},{name:"--ts-family-about-glass",value:"#F0DAC0@0.55"},
      {name:"--ts-family-media-glass",value:"#CED7E5@0.55"},{name:"--ts-family-guides-glass",value:"#CEE0CB@0.55"},
      {name:"--ts-family-stories-glass",value:"#E4CFDF@0.55"},{name:"--ts-family-ar-glass",value:"#CCE1D8@0.55"},
      {name:"--ts-family-play-glass",value:"#EFD0C4@0.55"},{name:"--ts-family-visit-glass",value:"#DAD2E3@0.55"},
      {name:"--ts-family-community-glass",value:"#EDDEC2@0.55"},{name:"--ts-family-navigation-glass",value:"#DDDEC2@0.55"},
      {name:"--ts-glass-edge",value:"#FFFFFF@0.75"},{name:"--ts-glass-hi",value:"#FFFFFF@0.9"},{name:"--ts-glass-shadow",value:"#14110D@0.16"},
      {name:"--ts-correct",value:"#17541B"},{name:"--ts-wrong",value:"#8E1B12"},{name:"--ts-result-fill",value:"#1E3A8A@0.14"},
      {name:"--ts-video-letterbox",value:"#000000"},{name:"--ts-clear",value:"#000000@0"},{name:"--ts-family-about-light",value:"#E28C32@0.549019635"},
      {name:"--ts-family-about-light-2",value:"#F0C86E@0.4509804"},{name:"--ts-family-about-sweep",value:"#FFFFFF"},
      {name:"--ts-family-stories-light",value:"#AA5ABE@0.549019635"},{name:"--ts-family-stories-light-2",value:"#EB8CAA@0.4509804"},
      {name:"--ts-family-stories-sweep",value:"#FFFFFF"},{name:"--ts-family-media-light",value:"#467DDC@0.549019635"},
      {name:"--ts-family-media-light-2",value:"#6EC8E6@0.4509804"},{name:"--ts-family-media-sweep",value:"#FFFFFF"},
      {name:"--ts-family-guides-light",value:"#46A564@0.549019635"},{name:"--ts-family-guides-light-2",value:"#BED26E@0.4509804"},
      {name:"--ts-family-guides-sweep",value:"#FFFFFF"},{name:"--ts-family-play-light",value:"#DE5F46@0.549019635"},
      {name:"--ts-family-play-light-2",value:"#F0AA5A@0.4509804"},{name:"--ts-family-play-sweep",value:"#FFFFFF"},
      {name:"--ts-family-visit-light",value:"#7D69D2@0.549019635"},{name:"--ts-family-visit-light-2",value:"#B496EB@0.4509804"},
      {name:"--ts-family-visit-sweep",value:"#FFFFFF"},{name:"--ts-family-community-light",value:"#D7A03C@0.549019635"},
      {name:"--ts-family-community-light-2",value:"#F0CD78@0.4509804"},{name:"--ts-family-community-sweep",value:"#FFFFFF"},
      {name:"--ts-family-ar-light",value:"#32AAA0@0.549019635"},{name:"--ts-family-ar-light-2",value:"#78D7C8@0.4509804"},
      {name:"--ts-family-ar-sweep",value:"#FFFFFF"},{name:"--ts-family-meta-light",value:"#A09178@0.549019635"},
      {name:"--ts-family-meta-light-2",value:"#CDC3AA@0.4509804"},{name:"--ts-family-meta-sweep",value:"#FFFFFF"},
      {name:"--ts-family-navigation-light",value:"#96AF3C@0.549019635"},{name:"--ts-family-navigation-light-2",value:"#C8D778@0.4509804"},
      {name:"--ts-family-navigation-sweep",value:"#FFFFFF"},
      {name:"--ts-app-glass",value:"#FBFAF7@0.78",proposed:true,note:"the app glass over the camera (W)"},
      {name:"--ts-app-glass-edge",value:"#14110D@0.12",proposed:true,note:"its rim"},
      {name:"--ts-panel",value:"#F8F6F2",proposed:true,note:"panels and sheets of the app"},
      {name:"--ts-tonal",value:"#E9E3D8",proposed:true,note:"a tonal button"},{name:"--ts-notify",value:"#A1201A",proposed:true,note:"a notification dot"},
      {name:"--ts-on-notify",value:"#FFFFFF",proposed:true},
      {name:"--ts-action-button",value:"#FFFFFF@0.94",proposed:true,note:"icon buttons over the camera; ink --ts-ink"},
      {name:"--ts-tag",value:"#FFFFFF@0.94",proposed:true},{name:"--ts-wall-label",value:"#FAF8F4@0.94",proposed:true,note:"a label on the wall"},
      {name:"--ts-wall-label-ink",value:"#14110D",proposed:true},
      {name:"--ts-elevation-3",value:"#14110D@0.4",proposed:true,note:"an overlay; no shadow on dark panels"},
      {name:"--ts-outline",value:"#14110D",proposed:true,note:"the outline (width 0 in Premium)"},
      {name:"--ts-shadow-hard",value:"#000000@0",proposed:true,note:"a hard shadow (none in Premium)"},
      {name:"--ts-highlight",value:"#FFD98A@0.62",proposed:true,note:"the read-along wash; its line is --ts-accent"},
      {name:"--ts-aurora-light",value:"#000000@0",proposed:true,note:"Aurora, the dark panels living light"},
      {name:"--ts-aurora-light-2",value:"#000000@0",proposed:true}],
    lengths:[
      {name:"--ts-rim-band",value:"3px"}],
    numbers:[
      {name:"--ts-rim-sweep-arc",value:62},{name:"--ts-app-glass-blur",value:28,proposed:true,note:"px; Unity: the camera-blur radius"}]}},
  {key:"dark",tokens:{
    colours:[
      {name:"--ts-bg",value:"#100F0D"},{name:"--ts-surface",value:"#1C1A17"},{name:"--ts-surface-2",value:"#26231F"},{name:"--ts-scrim",value:"#000000@0.7"},
      {name:"--ts-scrim-hero",value:"#0A0806@0.94"},{name:"--ts-on-scrim",value:"#FFFFFF"},{name:"--ts-on-scrim-2",value:"#F1E9DA"},
      {name:"--ts-hero-glow",value:"#0A0806@0.72"},{name:"--ts-text-halo",value:"#000000@0.85"},{name:"--ts-bar",value:"#1C1A17"},
      {name:"--ts-button-glass",value:"#FFFFFF@0.12"},{name:"--ts-top-shade",value:"#000000@0.5"},{name:"--ts-chrome-glass",value:"#1A1B1E@0.72"},
      {name:"--ts-chrome-band",value:"#1A1B1E@0.99"},{name:"--ts-top-fade",value:"#100F0D"},{name:"--ts-navbar-glass",value:"#1A1B1E@0.98"},
      {name:"--ts-chrome-glass-hero",value:"#0A0806@0.92"},{name:"--ts-chrome-line-hero",value:"#FFFFFF@0.32"},
      {name:"--ts-glass-ground",value:"#100F0D@0.8"},{name:"--ts-ink",value:"#F6F1E8"},{name:"--ts-ink-2",value:"#D9D1C3"},
      {name:"--ts-ink-3",value:"#C2B9A8"},{name:"--ts-on-accent",value:"#1A1206"},{name:"--ts-accent",value:"#F2B544"},
      {name:"--ts-accent-text",value:"#F2B544"},{name:"--ts-accent-shine",value:"#FFF1C8"},{name:"--ts-accent-2",value:"#B47828"},
      {name:"--ts-line",value:"#9A9181"},{name:"--ts-hair",value:"#3A362F"},{name:"--ts-metal-edge",value:"#CFC6B5"},{name:"--ts-metal-hi",value:"#4A443B"},
      {name:"--ts-metal-lo",value:"#24211D"},{name:"--ts-metal-groove",value:"#000000@0"},{name:"--ts-elevation-1",value:"#000000@0"},
      {name:"--ts-elevation-2",value:"#000000@0"},{name:"--ts-status-0",value:"#6FBF73"},{name:"--ts-status-1",value:"#C9B458"},
      {name:"--ts-status-2",value:"#E0913A"},{name:"--ts-status-3",value:"#E2735C"},{name:"--ts-status-4",value:"#8E8E8E"},
      {name:"--ts-family-meta",value:"#C4BAA8"},{name:"--ts-family-about",value:"#E8A85A"},{name:"--ts-family-media",value:"#8DB8EA"},
      {name:"--ts-family-guides",value:"#8FD19E"},{name:"--ts-family-stories",value:"#E3A3C8"},{name:"--ts-family-ar",value:"#7FD1C9"},
      {name:"--ts-family-play",value:"#F2A38F"},{name:"--ts-family-visit",value:"#C3B5F0"},{name:"--ts-family-community",value:"#E8C27C"},
      {name:"--ts-family-navigation",value:"#C4D678"},{name:"--ts-family-meta-ink",value:"#14110D"},{name:"--ts-family-about-ink",value:"#14110D"},
      {name:"--ts-family-media-ink",value:"#14110D"},{name:"--ts-family-guides-ink",value:"#14110D"},{name:"--ts-family-stories-ink",value:"#14110D"},
      {name:"--ts-family-ar-ink",value:"#14110D"},{name:"--ts-family-play-ink",value:"#14110D"},{name:"--ts-family-visit-ink",value:"#14110D"},
      {name:"--ts-family-community-ink",value:"#14110D"},{name:"--ts-family-navigation-ink",value:"#14110D"},{name:"--ts-family-meta-text",value:"#DBD6CC"},
      {name:"--ts-family-about-text",value:"#F1D0B3"},{name:"--ts-family-media-text",value:"#C5D8F3"},{name:"--ts-family-guides-text",value:"#B9E0C1"},
      {name:"--ts-family-stories-text",value:"#EECDDF"},{name:"--ts-family-ar-text",value:"#B0DFDA"},{name:"--ts-family-play-text",value:"#F7CDC4"},
      {name:"--ts-family-visit-text",value:"#DAD2F5"},{name:"--ts-family-community-text",value:"#EED2A7"},
      {name:"--ts-family-navigation-text",value:"#CEDD96"},{name:"--ts-sheet-open",value:"#0F1012"},{name:"--ts-locate-teal-hi",value:"#7FE3C4"},
      {name:"--ts-locate-teal-lo",value:"#0E9A7C"},{name:"--ts-locate-teal-rim",value:"#7FE3C4"},{name:"--ts-locate-green-hi",value:"#B8EF7A"},
      {name:"--ts-locate-green-lo",value:"#3F9E2C"},{name:"--ts-locate-green-rim",value:"#B8EF7A"},{name:"--ts-locate-cyan-hi",value:"#8BE4FF"},
      {name:"--ts-locate-cyan-lo",value:"#0A84B8"},{name:"--ts-locate-cyan-rim",value:"#8BE4FF"},{name:"--ts-locate-disc",value:"#FFFFFF"},
      {name:"--ts-locate-disc-edge",value:"#E7ECE9"},{name:"--ts-locate-shadow",value:"#000000@0.5"},{name:"--ts-locate-halo",value:"#000000@0.8"},
      {name:"--ts-locate-chip",value:"#FFFFFF@0.96"},{name:"--ts-locate-chip-ink",value:"#123B32"},{name:"--ts-locate-glass",value:"#FFFFFF@0.78"},
      {name:"--ts-locate-glass-line",value:"#FFFFFF@0.95"},{name:"--ts-section-neutral",value:"#CFC8BC"},{name:"--ts-family-meta-glass",value:"#282621@0.7"},
      {name:"--ts-family-about-glass",value:"#362614@0.7"},{name:"--ts-family-media-glass",value:"#1A2332@0.7"},
      {name:"--ts-family-guides-glass",value:"#1A2A1D@0.7"},{name:"--ts-family-stories-glass",value:"#2C1C2D@0.7"},
      {name:"--ts-family-ar-glass",value:"#182B27@0.7"},{name:"--ts-family-play-glass",value:"#351D17@0.7"},
      {name:"--ts-family-visit-glass",value:"#241F30@0.7"},{name:"--ts-family-community-glass",value:"#332915@0.7"},
      {name:"--ts-family-navigation-glass",value:"#262915@0.7"},{name:"--ts-glass-edge",value:"#FFFFFF@0.14"},{name:"--ts-glass-hi",value:"#FFFFFF@0.18"},
      {name:"--ts-glass-shadow",value:"#000000@0.35"},{name:"--ts-correct",value:"#8FD19E"},{name:"--ts-wrong",value:"#F2A38F"},
      {name:"--ts-result-fill",value:"#F2B544@0.1"},{name:"--ts-video-letterbox",value:"#000000"},{name:"--ts-clear",value:"#000000@0"},
      {name:"--ts-family-about-light",value:"#E28C32@0.419607848"},{name:"--ts-family-about-light-2",value:"#F0C86E@0.298039228"},
      {name:"--ts-family-about-sweep",value:"#F0C86E@0.4509804"},{name:"--ts-family-stories-light",value:"#AA5ABE@0.419607848"},
      {name:"--ts-family-stories-light-2",value:"#EB8CAA@0.298039228"},{name:"--ts-family-stories-sweep",value:"#EB8CAA@0.4509804"},
      {name:"--ts-family-media-light",value:"#467DDC@0.419607848"},{name:"--ts-family-media-light-2",value:"#6EC8E6@0.298039228"},
      {name:"--ts-family-media-sweep",value:"#6EC8E6@0.4509804"},{name:"--ts-family-guides-light",value:"#46A564@0.419607848"},
      {name:"--ts-family-guides-light-2",value:"#BED26E@0.298039228"},{name:"--ts-family-guides-sweep",value:"#BED26E@0.4509804"},
      {name:"--ts-family-play-light",value:"#DE5F46@0.419607848"},{name:"--ts-family-play-light-2",value:"#F0AA5A@0.298039228"},
      {name:"--ts-family-play-sweep",value:"#F0AA5A@0.4509804"},{name:"--ts-family-visit-light",value:"#7D69D2@0.419607848"},
      {name:"--ts-family-visit-light-2",value:"#B496EB@0.298039228"},{name:"--ts-family-visit-sweep",value:"#B496EB@0.4509804"},
      {name:"--ts-family-community-light",value:"#D7A03C@0.419607848"},{name:"--ts-family-community-light-2",value:"#F0CD78@0.298039228"},
      {name:"--ts-family-community-sweep",value:"#F0CD78@0.4509804"},{name:"--ts-family-ar-light",value:"#32AAA0@0.419607848"},
      {name:"--ts-family-ar-light-2",value:"#78D7C8@0.298039228"},{name:"--ts-family-ar-sweep",value:"#78D7C8@0.4509804"},
      {name:"--ts-family-meta-light",value:"#A09178@0.419607848"},{name:"--ts-family-meta-light-2",value:"#CDC3AA@0.298039228"},
      {name:"--ts-family-meta-sweep",value:"#CDC3AA@0.4509804"},{name:"--ts-family-navigation-light",value:"#96AF3C@0.419607848"},
      {name:"--ts-family-navigation-light-2",value:"#C8D778@0.298039228"},{name:"--ts-family-navigation-sweep",value:"#C8D778@0.4509804"},
      {name:"--ts-app-glass",value:"#1A1B1E@0.92",proposed:true,note:"W had 0.80: over a white pixel of the camera it composes, in linear light, to 3.6:1 under the ink; 0.92 keeps ink-2 at 4.9:1"},{name:"--ts-app-glass-edge",value:"#FFFFFF@0.14",proposed:true},
      {name:"--ts-panel",value:"#1C1A17",proposed:true},{name:"--ts-tonal",value:"#302D28",proposed:true},{name:"--ts-notify",value:"#F2B8B5",proposed:true},
      {name:"--ts-on-notify",value:"#3A0A08",proposed:true},{name:"--ts-action-button",value:"#1C1A17@0.94",proposed:true},
      {name:"--ts-tag",value:"#1C1A17@0.96",proposed:true},{name:"--ts-wall-label",value:"#0A0806@0.92",proposed:true},
      {name:"--ts-wall-label-ink",value:"#FFFFFF",proposed:true},{name:"--ts-elevation-3",value:"#000000@0",proposed:true},
      {name:"--ts-outline",value:"#F6F1E8",proposed:true},{name:"--ts-shadow-hard",value:"#000000@0",proposed:true},
      {name:"--ts-highlight",value:"#F2B544@0.24",proposed:true},{name:"--ts-aurora-light",value:"#8B5CF6@0.22",proposed:true},
      {name:"--ts-aurora-light-2",value:"#38BDF8@0.16",proposed:true}],
    lengths:[
      {name:"--ts-rim-band",value:"1px"}],
    numbers:[
      {name:"--ts-rim-sweep-arc",value:40},{name:"--ts-app-glass-blur",value:24,proposed:true}]}}],
 knobs:[
  {knob:"accent",option:"heritage",
   shared:{},
   modes:[{key:"light",tokens:{
     colours:[
       {name:"--ts-accent",value:"#1E3A8A"},{name:"--ts-accent-text",value:"#1E3A8A"},{name:"--ts-on-accent",value:"#FFFFFF"}]}},{key:"dark",tokens:{
     colours:[
       {name:"--ts-accent",value:"#F2B544"},{name:"--ts-accent-text",value:"#F2B544"},{name:"--ts-on-accent",value:"#1A1206"}]}}]},
  {knob:"accent",option:"azulejo",
   shared:{},
   modes:[{key:"light",tokens:{
     colours:[
       {name:"--ts-accent",value:"#17457F"},{name:"--ts-accent-text",value:"#17457F"},{name:"--ts-on-accent",value:"#FFFFFF"}]}},{key:"dark",tokens:{
     colours:[
       {name:"--ts-accent",value:"#8DB8EA"},{name:"--ts-accent-text",value:"#8DB8EA"},{name:"--ts-on-accent",value:"#0B1626"}]}}]},
  {knob:"accent",option:"terracotta",
   shared:{},
   modes:[{key:"light",tokens:{
     colours:[
       {name:"--ts-accent",value:"#8E2F14"},{name:"--ts-accent-text",value:"#8E2F14"},{name:"--ts-on-accent",value:"#FFFFFF"}]}},{key:"dark",tokens:{
     colours:[
       {name:"--ts-accent",value:"#F0A487"},{name:"--ts-accent-text",value:"#F0A487"},{name:"--ts-on-accent",value:"#2A0E05"}]}}]},
  {knob:"accent",option:"garden",
   shared:{},
   modes:[{key:"light",tokens:{
     colours:[
       {name:"--ts-accent",value:"#174F33"},{name:"--ts-accent-text",value:"#174F33"},{name:"--ts-on-accent",value:"#FFFFFF"}]}},{key:"dark",tokens:{
     colours:[
       {name:"--ts-accent",value:"#8FD19E"},{name:"--ts-accent-text",value:"#8FD19E"},{name:"--ts-on-accent",value:"#0B2412"}]}}]},
  {knob:"accent",option:"cobalt",proposed:true,note:"Playful's accent, measured like the four",
   shared:{},
   modes:[{key:"light",tokens:{
     colours:[
       {name:"--ts-accent",value:"#2638B8"},{name:"--ts-accent-text",value:"#2638B8"},{name:"--ts-on-accent",value:"#FFFFFF"}]}},{key:"dark",tokens:{
     colours:[
       {name:"--ts-accent",value:"#FFC83D"},{name:"--ts-accent-text",value:"#FFC83D"},{name:"--ts-on-accent",value:"#1B1446"}]}}]},
  {knob:"look",option:"premium",proposed:true,note:"the base: the theme as Unity has it",
   shared:{},
   modes:[{key:"light",tokens:{}},{key:"dark",tokens:{}}]},
  {knob:"look",option:"playful",proposed:true,note:"W's Playful: text x1.0625 (precomputed), a 2 pt outline, hard shadows, rounded titles",
   shared:{
     lengths:[
       {name:"--ts-size-body",value:"17px"},{name:"--ts-size-body-open",value:"18px"},{name:"--ts-size-title",value:"25.5px"},
       {name:"--ts-size-caption",value:"15px"},{name:"--ts-size-lede",value:"20px"},{name:"--ts-size-heading",value:"21px"},
       {name:"--ts-size-hero",value:"42.5px"},{name:"--ts-size-hero-narrow",value:"36px"},{name:"--ts-size-dropcap",value:"51px"},
       {name:"--ts-size-quote",value:"23.5px"},{name:"--ts-size-display",value:"30px"},{name:"--ts-size-chip",value:"16px"},
       {name:"--ts-size-eyebrow",value:"14px"},{name:"--ts-size-title-large",value:"34px"},{name:"--ts-outline-width",value:"2px"}],
     numbers:[
       {name:"--ts-icon-stroke",value:2.25}]},
   modes:[{key:"light",tokens:{
     colours:[
       {name:"--ts-bg",value:"#F7F2E7"},{name:"--ts-sheet-open",value:"#FFF8EC"},{name:"--ts-panel",value:"#FFF8EC"},{name:"--ts-ink",value:"#1B1446"},
       {name:"--ts-ink-2",value:"#3B3272"},{name:"--ts-app-glass",value:"#FFF8EC@0.96"},{name:"--ts-app-glass-edge",value:"#1B1446"},
       {name:"--ts-tonal",value:"#FFFDF8"},{name:"--ts-outline",value:"#1B1446"},{name:"--ts-shadow-hard",value:"#0E2466"},
       {name:"--ts-wall-label",value:"#FFFFFF@0.96"},{name:"--ts-wall-label-ink",value:"#1B1446"}]}},{key:"dark",tokens:{
     colours:[
       {name:"--ts-bg",value:"#10173A"},{name:"--ts-surface",value:"#2D2760"},{name:"--ts-surface-2",value:"#2D2760"},{name:"--ts-bar",value:"#2D2760"},
       {name:"--ts-sheet-open",value:"#221D45"},{name:"--ts-panel",value:"#221D45"},{name:"--ts-ink",value:"#FFF8EC"},{name:"--ts-ink-2",value:"#DCD6F5"},
       {name:"--ts-app-glass",value:"#221D45@0.97"},{name:"--ts-app-glass-edge",value:"#0B0820"},{name:"--ts-tonal",value:"#263070"},
       {name:"--ts-outline",value:"#0B0820"},{name:"--ts-shadow-hard",value:"#060A22"},{name:"--ts-action-button",value:"#2D2760"},
       {name:"--ts-tag",value:"#2D2760"},{name:"--ts-wall-label",value:"#140F33@0.94"},{name:"--ts-wall-label-ink",value:"#FFFFFF"}]}}],picks:{"accent": "cobalt", "roundness": "round", "glass": "solid"},type:{"displayFont": "fredoka_semibold"}},
  {knob:"roundness",option:"sharp",proposed:true,
   shared:{
     lengths:[
       {name:"--ts-radius-s",value:"4px"},{name:"--ts-radius-m",value:"8px"},{name:"--ts-radius-l",value:"12px"},{name:"--ts-radius-xl",value:"16px"}]},
   modes:[{key:"light",tokens:{}},{key:"dark",tokens:{}}]},
  {knob:"roundness",option:"soft",proposed:true,note:"the theme's own radii",
   shared:{},
   modes:[{key:"light",tokens:{}},{key:"dark",tokens:{}}]},
  {knob:"roundness",option:"round",proposed:true,
   shared:{
     lengths:[
       {name:"--ts-radius-s",value:"8px"},{name:"--ts-radius-m",value:"16px"},{name:"--ts-radius-l",value:"24px"},{name:"--ts-radius-xl",value:"32px"}]},
   modes:[{key:"light",tokens:{}},{key:"dark",tokens:{}}]},
  {knob:"density",option:"comfortable",proposed:true,note:"the theme's own spaces",
   shared:{},
   modes:[{key:"light",tokens:{}},{key:"dark",tokens:{}}]},
  {knob:"density",option:"compact",proposed:true,note:"inner paddings one step down; the touch target (--ts-space-7) stays",
   shared:{
     lengths:[
       {name:"--ts-space-2",value:"6px"},{name:"--ts-space-3",value:"8px"},{name:"--ts-space-4",value:"12px"},{name:"--ts-space-5",value:"16px"},
       {name:"--ts-space-6",value:"24px"}]},
   modes:[{key:"light",tokens:{}},{key:"dark",tokens:{}}]},
  {knob:"glass",option:"blur",proposed:true,note:"the camera blurred under the app glass",
   shared:{},
   modes:[{key:"light",tokens:{}},{key:"dark",tokens:{}}]},
  {knob:"glass",option:"solid",proposed:true,note:"the tint alone, no blur",
   shared:{},
   modes:[{key:"light",tokens:{
     colours:[
       {name:"--ts-app-glass",value:"#FBFAF7@0.84"}],
     numbers:[
       {name:"--ts-app-glass-blur",value:0}]}},{key:"dark",tokens:{
     colours:[
       {name:"--ts-app-glass",value:"#1A1B1E@0.98"}],
     numbers:[
       {name:"--ts-app-glass-blur",value:0}]}}]},
  {knob:"motion",option:"follow",proposed:true,note:"the phone's Reduce Motion decides",
   shared:{},
   modes:[{key:"light",tokens:{}},{key:"dark",tokens:{}}]},
  {knob:"motion",option:"reduced",proposed:true,note:"still for every visitor",
   shared:{
     durations:[
       {name:"--ts-duration",value:"0s"},{name:"--ts-duration-fast",value:"0s"},{name:"--ts-duration-slow",value:"0s"},
       {name:"--ts-duration-slower",value:"0s"}]},
   modes:[{key:"light",tokens:{}},{key:"dark",tokens:{}}]}],
 type:{"displayFont": "fraunces_semibold", "displayWeight": 600, "bodyFont": "atkinson_next_regular", "bodyMediumFont": "atkinson_next_medium", "bodyBoldFont": "atkinson_next_bold"}};
/* the wall's picks (WallConfigData, guess): the Default Mode (light: sunlight first, rule D1; follow = the phone's appearance) and
   one option per knob */
D.wall.theme={mode:"light",look:"premium",accent:"heritage",roundness:"soft",density:"comfortable",glass:"blur",motion:"follow"};
/* the font library (Unity: FontKeyLibrary): a key -> the face the Lab draws it with and Unity's file */
var THEME_FONTS={fraunces_semibold:{css:'"Fraunces", Georgia, serif',file:"Fraunces-SemiBold"},
  atkinson_next_regular:{css:'"Atkinson Hyperlegible Next", "Segoe UI", system-ui, sans-serif',file:"AtkinsonHyperlegibleNext-Regular"},
  atkinson_next_medium:{css:'"Atkinson Hyperlegible Next", "Segoe UI", system-ui, sans-serif',file:"AtkinsonHyperlegibleNext-Medium"},
  atkinson_next_bold:{css:'"Atkinson Hyperlegible Next", "Segoe UI", system-ui, sans-serif',file:"AtkinsonHyperlegibleNext-Bold"},
  fredoka_semibold:{css:'"Fredoka", "Fraunces", sans-serif',file:"Fredoka-SemiBold",proposed:true}};
/* the knobs, in the order their options are laid (base -> look -> the others), and each knob's options in the Editor */
var THEME_KNOBS=["look","accent","roundness","density","glass","motion"];
var THEME_KINDS=["colours","lengths","numbers","durations","easings"];
/* UI Toolkit's easing keywords as CSS curves (USS has a closed list; the Lab draws the same curve) */
var THEME_EASE={"ease-out-cubic":"cubic-bezier(.33,1,.68,1)","ease-out-back":"cubic-bezier(.34,1.56,.64,1)",linear:"linear",ease:"ease","ease-in":"ease-in","ease-out":"ease-out","ease-in-out":"ease-in-out"};

// The options of one knob, in the table's order
function themeOptions(knob){return D.theme.knobs.filter(function(o){return o.knob===knob})}
// One option, or undefined
function themeOption(knob,option){return themeOptions(knob).filter(function(o){return o.option===option})[0]}
// The option of each knob the phone shows: the look's own picks first, then the wall's
function themePicks(){var w=D.wall.theme,look=themeOption("look",w.look),p={};
  THEME_KNOBS.forEach(function(k){p[k]=look&&look.picks&&look.picks[k]&&k!=="look"?look.picks[k]:w[k]});return p}
// The mode the phone shows (Unity's ThemeModeRule.Shown): the visitor's pick, then the wall's Default Mode, then the theme's own
function themeMode(){var keys=D.theme.modes.map(function(m){return m.key}),phone=window.matchMedia&&matchMedia("(prefers-color-scheme: dark)").matches?"dark":"light";
  var wall=D.wall.theme.mode==="follow"?phone:D.wall.theme.mode,picks=[P.vis&&P.vis.mode,wall,D.theme.defaultMode];
  for(var i=0;i<picks.length;i++)if(keys.indexOf(picks[i])>=0)return picks[i];return keys[0]}
// The classes of a root that draws the app (ThemeController.Apply): ts-app, the theme, the mode, one per knob
function themeClasses(){var p=themePicks();return "ts-app ts-theme-"+D.theme.key+" ts-mode-"+themeMode()+THEME_KNOBS.map(function(k){return " ts-"+k+"-"+p[k]}).join("")}
var THEME_CLASS=/^ts-(app|theme|mode|look|accent|roundness|density|glass|motion)(-|$)/;
// Mark one root with the theme's classes (its old ones swapped)
function themeMark(el){[].slice.call(el.classList).forEach(function(c){if(THEME_CLASS.test(c))el.classList.remove(c)});themeClasses().split(" ").forEach(function(c){el.classList.add(c)})}
// Swap the classes on every marked root (ThemeController.SetMode): nothing is rebuilt
function themeSwap(){document.querySelectorAll(".ts-app").forEach(themeMark)}

/* ----- writing the table ----- */
// A value as CSS: "#RRGGBB@a" -> rgba(), an easing keyword -> its curve
function themeCss(v,kind){if(kind==="colours"){var c=themeRgba(v);return c[3]>=1?"rgb("+c[0]+", "+c[1]+", "+c[2]+")":"rgba("+c[0]+", "+c[1]+", "+c[2]+", "+c[3]+")"}
  if(kind==="easings")return THEME_EASE[v]||v;return String(v)}
// One rule of a token set ("" when the set is empty)
function themeRule(sel,set){var out=[];THEME_KINDS.forEach(function(k){(set&&set[k]||[]).forEach(function(t){out.push(t.name+":"+themeCss(t.value,k))})});return out.length?sel+"{"+out.join(";")+"}\n":""}
// The font rule: the four font tokens from the type's keys (the halo twins are Unity's outlined atlases; the Lab draws the same face)
function themeFontRule(sel,type){var d=THEME_FONTS[type.displayFont],b=THEME_FONTS[type.bodyFont],out=[];
  if(b)out.push("--ts-font-body:"+b.css,"--ts-font-body-halo:"+b.css);if(d)out.push("--ts-font-display:"+d.css,"--ts-font-display-halo:"+d.css);return out.length?sel+"{"+out.join(";")+"}\n":""}
// The whole table as class rules in one style element (ThemeUssGenerator's shape, plus the knob options)
function applyTokens(){var root=".ts-app.ts-theme-"+D.theme.key,css="";
  D.theme.modes.forEach(function(m){css+=themeRule(root+".ts-mode-"+m.key,D.theme.shared)+themeRule(root+".ts-mode-"+m.key,m.tokens)});
  css+=themeFontRule(root,D.theme.type);
  THEME_KNOBS.forEach(function(k){themeOptions(k).forEach(function(o){var s=root+".ts-"+k+"-"+o.option;css+=themeRule(s,o.shared);
    (o.modes||[]).forEach(function(m){css+=themeRule(s+".ts-mode-"+m.key,m.tokens)});if(o.type)css+=themeFontRule(s,o.type)})});
  var st=document.getElementById("ts-theme");if(!st){st=document.createElement("style");st.id="ts-theme";document.head.appendChild(st)}
  if(st.textContent!==css)st.textContent=css}
on("configChanged",function(){applyTokens();themeSwap()});
on("profileChanged",function(){themeSwap()});

/* ----- the resolved table and contrast (the token test, the Editor's badge) ----- */
// Every token's value for a mode and the knobs' picks ({look, accent, ...}; missing = the wall's), laid in Unity's order
function themeResolve(mode,picks){var out={},lay=function(set){THEME_KINDS.forEach(function(k){(set&&set[k]||[]).forEach(function(t){out[t.name]=t.value})})};
  lay(D.theme.shared);lay(themeModeSet(D.theme,mode));
  var look=themeOption("look",picks&&picks.look||D.wall.theme.look);
  THEME_KNOBS.forEach(function(k){var name=k==="look"?(look&&look.option):(picks&&picks[k])||(look&&look.picks&&look.picks[k])||D.wall.theme[k],o=themeOption(k,name);if(!o)return;
    lay(o.shared);lay(themeModeSet(o,mode))});
  return out}
// The token set of `mode` in the theme or in a knob option
function themeModeSet(o,mode){var m=(o.modes||[]).filter(function(x){return x.key===mode})[0];return m&&m.tokens}
// "#RRGGBB" / "#RRGGBB@a" -> [r, g, b, a]
function themeRgba(v){var m=/^#([0-9A-Fa-f]{6})(?:@([0-9.]+))?$/.exec(String(v).trim());if(!m)return null;var n=parseInt(m[1],16);return[n>>16&255,n>>8&255,n&255,m[2]===undefined?1:+m[2]]}
// A channel 0-255 to linear light, and back
function themeLin(c){c/=255;return c<=0.04045?c/12.92:Math.pow((c+0.055)/1.055,2.4)}
function themeGamma(l){var c=l<=0.0031308?l*12.92:1.055*Math.pow(l,1/2.4)-0.055;return Math.round(Math.max(0,Math.min(1,c))*255)}
// WCAG relative luminance of an opaque colour
function themeLum(c){return 0.2126*themeLin(c[0])+0.7152*themeLin(c[1])+0.0722*themeLin(c[2])}
// `top` (with its alpha) over an opaque `under`, blended in LINEAR light as the project renders (ThemeContrastTests.LinearOver)
function themeOver(top,under){var a=top[3];return[0,1,2].map(function(i){return themeGamma(themeLin(under[i])*(1-a)+themeLin(top[i])*a)}).concat(1)}
// The contrast ratio of a text colour on its ground, both composed in linear light over `base` (white when absent) first
function themeContrast(fg,bg,base){var b=bg[3]<1?themeOver(bg,base||[255,255,255,1]):bg,f=fg[3]<1?themeOver(fg,b):fg,l1=themeLum(f),l2=themeLum(b);
  return(Math.max(l1,l2)+0.05)/(Math.min(l1,l2)+0.05)}
