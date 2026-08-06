

## 3. UI, Visual Design, and Content Rules

- **No hardcoded visual values in C# or UXML.** Colors, fonts, spacing, and corner radii
  are defined once as USS variables or a shared design-tokens ScriptableObject, and
  referenced everywhere else. If the visual style needs to change, it should be a change
  in one place, not a find-and-replace across every screen.
- **No hardcoded user-facing strings in code.** Route all visitor-facing text through a
  strings table asset (even before full multi-language localization is wired up), so
  adding a language later is a data change, not a code change.
- **Every screen has one clear focal point.** A visitor's eye should immediately know
  what to look at or do next. Avoid layouts that present many equally-weighted elements
  competing for attention.
- **Every visual element earns its place.** Every line, margin, icon, and decorative
  touch should serve an actual information or usability purpose. Remove anything that is
  decoration for its own sake.
- **Avoid generic template patterns.** Don't default to predictable card grids or
  boilerplate mobile-app layouts. The interface should look considered and specific to
  this project, not assembled from generic UI-kit defaults.
- **Progressive disclosure over information overload.** Show a small amount of
  information first (a marker, a short label); let the visitor open something to go
  deeper (a full content card). Never dump everything about a point of interest onto the
  screen at once.
- **Accessibility is not optional.** Maintain real contrast between text and background
  in both light and dark conditions (this app is used outdoors in variable light). Any
  screen-reader support (TalkBack/VoiceOver-equivalent APIs) that the project uses must
  be tested against actual accessibility settings, not assumed to work because the
  standard UI components were used.
- **Every core feature must work without the camera.** Since some visitors won't or
  can't use the AR camera view, every feature (browsing POIs, reading content, viewing
  the timeline) must remain reachable through a non-AR fallback mode. Do not build a
  feature that is only reachable through the AR camera path.

---