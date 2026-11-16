# Audit B: navigation and card controls (saved from the auditor's hand-back)

82 rows: SAME 31 · ADD 17 · MERGE 9 · DROP 18 · RENAMED 7.

## ADD
- s5bar Shown from: 2..30 blocks, 8 (fewer -> no bar, scroll; Tabs/Pages too). draws. [navigation.min_blocks]
- s5bar Shown at: Half and Full * / Full only (Position = Top only). draws. [navigation.show_at]
- s5sticky Slides away while reading: On * / Off. draws. [navigation.hide_on_scroll]
- s5expand Open one at a time: Off * / On (Accordion layout and Tap = Expand here). behaviour. [accordion_one_open]
- s1a Phone's Back button: Steps the card down * / Left to the app (Android). behaviour. [back_button]
- s5bar Shape: Floating * / Full width. draws.
- s5bar Fill: Solid * / Glass / Clear (only Solid has a shadow; over hero Glass/Clear white on dark glass). draws.
- s5bar Chosen icon: Filled * / Plain. draws.
- s5bar Chosen colour: Section colour * / Accent / Ink. draws.
- s5bar Shown from the start: Always * / After the hero. draws. [bar.show]
- s5bar Editor warning: a card showing more sections than Items before More: "<point> shows N sections: each gets under 60 pt".
- s5pager draws the next-page peek (edge with icon and name) as fixed part (CardPagerView).
- s5pager (Family Pager): no rows; always under Pages ("2 of 6"), under Tabs while bar off (icon, name, count). words nav_pager_*.
- s2next Up Next: Shown On * / Off; next heading, its place ("5 of 13", left out under Pages), Next; at the end "You have reached the end", no button. Keep in the flow.
- s5contents: Shown On * / Off, opened from the bar's More; section rows with counts fixed; list blocks under sections (not kinds).
- s5contents Filter chips: Shown * / Hidden; time (Up to 2 / 5 min) + No headphones / camera / network + Clear; drop format chips.
- s5contents Rows: Sections first * / Every block; Read ticks: Shown * / Hidden. words nav_contents_*.

## MERGE
- s2cfg Block order: By section * / As placed / As placed, Sources last. behaviour.
- s5bar Indicator: Raised * / Pill / Underline / None.
- s5bar Labels: + Text on the chosen.
- s5bar Items before More: 4 / 5 * / 6 (More opens the Contents sheet). [max_slots]
- s5bar After a tap, the card is at: Full * / Where it is (Unity Full; Lab had Half * / Full).
- s5settings Appearance: Light / Dark / Follow the phone.
- s1b Default mode: Light * / Dark / Follow the phone (Unity to add third).
- Reduce Motion: drop wall-wide container.reduce_motion; s5settings Motion (visitor): Follow the phone * / Reduced / Full. draws.
- (bottom bar + More list -> Items before More)

## DROP (reason)
- families At The End (only Sources; Block order's third choice does it); Group By (Sections replace); Follow Scroll (make a fixed rule: the bar follows the scroll by runs); Bold Chosen Icon (fixed: always bold); Placed Model Control (keep the chip always, owner bd:Place In AR); slot keys; navigation.labels; show_counts (Contents always shows counts); family/group warnings (impossible in the Lab); Locate-off note; show_on_wall_nearby word (retire it); Menu button; Bar-With-Kinds second tap; Kind Chips; Icon Tabs / Tabs; Progress Map (maybe later as a bar option); Navigation Presets (no code).

## (?) help additions
- Layout: "A card with fewer blocks than the bar's Shown from, or with one section, always scrolls."
- Jump motion: "Reduce Motion makes it instant." Enabled: "Off: under Tabs or Pages the Section pager is the way between sections." Position: "At Top the bump and disc hang down." Labels: "An icon alone is still named for screen readers." bar: "needs two sections". Locate Size: "the target never drops under 44 pt". Prev/Next: "Off: a sideways swipe still turns the page." Locate Style row lacks a (?).

## Lab bugs / fixes
- Settings mark drawn as ☰; must be the sliders mark.
- Expand Row: draw the full-view button on an open row (full_view_open).
- Glossary: Unity shows a panel under the paragraph; Lab a popup under the word + Read more. Keep the Lab's.
- Locate keys: glass -> glass_ping, teal -> ar_teal (Unity's keys).
- Rule: the Locate pin does not depend on the bar.
- s5top Settings button Hidden removes the visitor's Light/Dark switch (a fixed rule): drop it or limit it.
- s4h's help promises a visitor haptics switch in the card's settings: add it (Gap 4).

## Gaps (proposals)
1. Text size in the Settings panel (Default / Large / Larger), from the phone's font scale.
2. Follow the phone's Reduce Motion (native read).
3. Open in the phone's language when the wall offers it.
4. Haptics and sounds switch in the Settings panel.
5. Second tap on the chosen bar item goes to that section's start.
6. Reopen at: Top * / Where the visitor left (within one visit).

## Open questions (auditor's)
1. After a bar tap: Full (Unity) or Half (Lab)? 2. Next Up in flow vs foot bar; counter counts blocks or sections? 3. Visitor Motion replaces wall-wide Reduce Motion? 4. Items before More = 5? 5. Settings button Hidden?
