
## 1. Code Quality Rules

- **MonoBehaviours stay thin.** A MonoBehaviour's job is to exist in the scene and
  respond to Unity's lifecycle (`Awake`, `Update`, `OnTriggerEnter`, UI event callbacks).
  Actual logic — filtering, scoring, parsing, deciding what content to show — belongs in
  plain C# classes with no `MonoBehaviour` inheritance, so that logic can be created and
  tested with a plain `new SomeClass()` and no scene running at all. If a MonoBehaviour
  grows past roughly 150–200 lines, that is a signal logic needs extracting out of it.
- **Configuration is data, not code.** Anything specific to one wall (POI positions,
  category names and colors, localization map identifiers, content text) lives in a
  ScriptableObject asset or a structured config file, never hardcoded inside a script.
  A new wall should be addable by authoring new data assets, not by writing new code.
- **No single class owns everything.** Do not build one `GameManager`-style class that
  owns tracking state, POI state, UI state, and analytics all at once. Split by
  responsibility into separate classes (e.g. a session/state owner, a POI registry, an
  analytics logger) and connect them through serialized references or small C#
  events/`UnityEvent`s, not by having one object know about and control everything else.
- **Session/orchestrator classes are the most common place this rule quietly gets
  broken — watch them specifically.** Any class named `*Session`, `*Manager`, or
  `*Controller` that's responsible for sequencing a startup flow (load config ->
  wait for tracking -> spawn content -> wire UI) has a natural gravity toward
  absorbing "just one more piece of logic" at each step, since it's already touching
  everything. Its job is to call other objects that make decisions, not to make
  decisions itself. If you find yourself writing an `if`/`switch` inside one of these
  classes that decides *what* should happen rather than just *when* to call the thing
  that decides, that logic belongs in a separate class instead. A useful check: could
  you delete every line of this class's own decision logic and replace it with calls
  to smaller classes without changing what the app does? If yes, do that now, before
  the class grows further — this is far cheaper to fix at 50 lines than at 300.
- **Prefer simple duplication over a fragile generic abstraction.** If two pieces of code
  look similar but serve genuinely different purposes, it is often better to keep them as
  two small, separately readable pieces of code than to merge them into one function
  controlled by several boolean flags and `if` branches. Only extract a shared function
  or base class once the exact same logic is genuinely needed unchanged in more than one
  place — and even then, prefer the simplest possible shared shape. A wrong abstraction
  that has to be unwound later costs more than a little repetition did.
- **Use what Unity and the SDKs already give you.** Use Addressables for wall content
  packaging and loading, Unity Test Framework for tests, the Input System package for
  input, UI Toolkit for interface layout and styling, the Localization package if and
  when text localization is implemented, and the Immersal SDK's own APIs for tracking —
  do not hand-roll a custom content loader, input handler, or localization system that
  duplicates what an installed package already does correctly.
- **No speculative code.** Implement only what the current task actually needs. Do not
  add a parameter, a flag, an interface method, or a config field because it "might be
  useful for a future wall" unless a concrete, currently-known requirement needs it.
- **Self-documenting names.** Name every class, method, field, and folder so a reader
  understands its purpose from the name alone, without needing to open the file.
- **Standard C# naming:** `PascalCase` for classes, methods, and public properties;
  `camelCase` for local variables and parameters; a leading underscore for private
  serialized fields (`_wallConfig`) is acceptable and common in Unity codebases — pick one
  convention and use it consistently throughout the project, do not mix styles between
  files.

---

## 2. Comments and Logging

- **One short comment above every method**, written in plain, direct, infinitive-form
  language (e.g. `// Localize the camera pose against the current wall's map`,
  `// Load and validate this wall's POI config`). Where first-person-plural reads more
  naturally for something the code is actively doing, that's fine too (e.g.
  `// we cache the resolved position here so we don't recompute it every frame`).
- **Inline comments explain the why, not the obvious what.** Write them as short, casual
  bullet-style notes, like one developer explaining a tricky bit of code to another
  developer standing next to them — not formal prose, not restating what the line already
  says. Example: `// Immersal returns pose in map space, not world space - convert
  before spawning POIs` is useful. `// loop over the list of POIs` is not.
- **Zero emojis, strict ASCII only — everywhere.** This applies to code, comments, log
  messages, editor window labels, and any text written to a file or the console. Unity's
  console and device logs (Logcat, Player.log) can mangle non-ASCII characters on some
  platforms, so avoid them entirely. Use plain equivalents:
  - Replace `✓` with `[ok]`
  - Replace `→` with `->`
  - Replace `»` with `>>`
- **Logging is minimal and single-line.** A log line exists to help pinpoint a failure,
  not to narrate everything happening. Prefix log lines by system so they're easy to find
  in a long device log: `[Tracking] localized in 2.3s`, `[POI] spawned 12/12`,
  `[Config] loaded 45 POIs for wall Panorama`. Strip temporary trace logs once a task is
  verified working — don't leave debug noise permanently wired into shipped code.
- **Never trust a bare print/log line to mean something is actually correct — assert it.**
  If a log message says something like `[ok] wall localized`, back that claim with an
  actual assertion right above or below it, so that if the condition were ever false the
  assertion would fail loudly instead of a log quietly claiming success. Use
  `Debug.Assert` in runtime code and `Assert.IsTrue`/`Assert.AreEqual` (from
  `NUnit.Framework` or `UnityEngine.Assertions`) in tests, placed at the real invariant
  boundaries: config isn't null before use, a POI list isn't empty before spawning, a
  returned pose isn't the zero/identity default before trusting it, IDs are unique before
  building a lookup table from them.

---