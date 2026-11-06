# Guidance: one system for the guide's own initiative (plan A)

Planner A, 2026-11-06. A plan to compare with plan B and merge. No project file was changed. Written for the designer and the
engineer: each part ends with what to build and how to test it. Facts marked (unverified) come from one secondary source.

---

## 0. The answer in ten lines

1. **Yes, a guidance system is a good idea, with one change of frame.** The mascot needs a single brain that decides when it
   speaks up. Without one, each feature (Hints, Azu comments, the ✦ hint, Moments, the wall pill) runs its own timer, and
   together they talk too much (the Clippy and Navi lesson).
2. **The three kinds are right: App, Content and Fun.** They are kinds of *push* (the guide takes the initiative). The visitor's
   own *pull* (the ✦, Ask, "How does this work?") is not scheduled. The system serves it and never limits it.
3. **The schedule is driven by what the visitor already knows, more than by the clock.** An app tip stops for good once its
   feature is used (Apple TipKit's rule). So app guidance fades by itself as the visitor learns. The 70 % → 30 % mix along the
   visit is a soft weight on top, not a quota.
4. **A guidance item is a small record:** what it says, what it is about, when it may show, how important it is (early or
   late), how often it may come back, and what ends it. It belongs to a group, and the group to a kind. Each level has defaults
   that the level below inherits, shown as words ("As the kind (Ring)").
5. **One scheduler for the whole app.** It shows one item at a time, never over the narration, the phone moving or a tour's
   sentence. It keeps a breathing gap between items, a cap per minute and a cap per visit per kind.
6. **It lives in a new top-level Editor tab, Guidance.** It has a visit simulator (a timeline, "Show next now", "Why not
   now?"), presets (First visit, Returning, Family, Expert, Quiet, Chatty) and one list per kind.
7. **The wall pill goes.** Its words become the item "Tap a marker", said by the mascot beside a marker. It comes after the
   **Opening message**, the one message every visit starts with.
8. **The app's first pages and the in-visit tips are one system, with two presenters:** a Page (onboarding) and a Bubble.
   Ordered steps are a Sequence (TipKit's ordered TipGroup).
9. **Everything has a Unity path:** a ScriptableObject catalogue, a pure C# scheduler that can be unit-tested, the UI Toolkit
   bubble (9-slice), the Painter2D OddEven spotlight the guide already uses, and the mascot's render texture.
10. **The build has nine small rounds.** The first replaces the wall pill with the mascot's message and adds the opening
    message, with their tests.

---

## 1. Research: how others guide, and what each teaches

### 1.1 The rules of the platforms

- **Apple TipKit** ([WWDC23 "Make features discoverable with TipKit"](https://developer.apple.com/videos/play/wwdc2023/10229/);
  [TipGroup](https://developer.apple.com/documentation/tipkit/tipgroup.md);
  [WWDC24 "Customize feature discovery with TipKit"](https://developer.apple.com/videos/play/wwdc2024/10070/)).
  - **What tips are for:** "teach someone about a brand-new feature, help with discovery of a hidden feature, or show a faster
    way to accomplish a task". Promotional or error messages are not tips. A tip that is "too complicated to read and remember
    in the moment" is not one either.
  - **When a tip may show:** rules decide eligibility. A parameter rule is a state (logged in). An event rule is something done
    (opened a view 3 times in 5 days).
  - **How often:** a global **display frequency** (`.immediate`, `.hourly`, `.daily`, or a custom interval) is the minimum time
    between any two tips. One tip may skip it (`IgnoresDisplayFrequency`).
  - **When it stops:** `MaxDisplayCount(n)` stops a tip after n showings. `invalidate(reason: .actionPerformed)` stops it for
    good once the person does what it teaches. The state persists, and can sync over iCloud (iOS 18).
  - **Several tips:** a **TipGroup** shows them one at a time, `.ordered` (progressively, each after the one before is done) or
    `.firstAvailable`. `resetEligibility()` on iOS 26 is from a secondary source (unverified).
  - **Teaches us:**
    - eligibility rules plus one global gap plus a per-item cap;
    - **stop once used** is the strongest rule of all;
    - ordered groups give sequences;
    - the tip's words are an action phrase and a benefit.
- **Material Design feature discovery and onboarding** ([Feature discovery](https://m1.material.io/growth-communications/feature-discovery.html);
  [Onboarding](https://m1.material.io/growth-communications/onboarding.html)).
  - **The tap target:** a circle around the one element, on a large disc of colour, with a single action. It pulses 10 % over
    1 s, appears from the element itself, and a tap outside dismisses it.
  - **When:** "Don't show prompts on app open". Limit them, "for example to one per session". After a dismissal, wait "a
    substantial period" before showing it or a similar one. After an accept, a similar one may come sooner.
  - **Guided flows:** a guided flow has up to three parts: the target, then ripples, then a toast that leads back.
  - **The three onboarding models:**
    - **Self-select** (a few meaningful choices, fewer than ten a screen);
    - **Quickstart** (straight into the app; "Don't force education upfront");
    - **Top user benefits** (an autoplay carousel of up to three benefits). Self-select and top user benefits are never
      combined.
  - **Teaches us:**
    - the spotlight's look and motion;
    - the accept / dismiss asymmetry in the cooldowns;
    - Quickstart plus a light Self-select for our first pages.
- **Apple HIG onboarding** ([HIG: Onboarding](https://developer.apple.com/design/human-interface-guidelines/onboarding); the
  page did not render for the fetcher, so this is from memory).
  - Teach by doing, keep it short, defer sign-in, and ask for permissions in context.
  - **Teaches us:** the camera permission is asked by the mascot at the moment the camera is needed, never on a pre-screen.
- **Apple Visual Look Up** ([Macworld](https://www.macworld.com/article/351513/ios-15-how-to-search-inside-your-photos-using-visual-look-up.html)).
  - When Photos recognises something, the Info button gets a small sparkle, and nothing else changes. A plain button means
    nothing was found.
  - **Teaches us:** a quiet, ambient form of content guidance. A mark that says "there is something here", with no bubble. It
    becomes our lowest-intensity presenter, the **Glint**: the marker or the ✦ shines once.

### 1.2 The product-tour tools (their objects and their throttles)

- **Appcues** ([Experiences frequency limit](https://docs.appcues.com/user-experiences-targeting/experiences-frequency-limit)).
  - **The objects:** Flows (tours and modals), Pins (hotspots and tooltips), Checklists, Banners, Launchpads (a menu of help).
  - **One global cap:** n experiences a minute, hour, day or week. Once it is hit, nothing new shows until the window resets.
    - Checklists, Pins, Banners and Launchpads are outside the cap.
    - "Override frequency limits" exempts a single experience.
    - There is **no minimum gap**: two may run back to back.
  - **Teaches us:**
    - the help menu and the checklist (pull) are never capped;
    - a cap alone is not enough without a gap.
- **Pendo** ([Guide throttling and ordering](https://www.pendo.io/pendo-blog/introducing-guide-throttling-ordering/)).
  - **The throttle:** a **minimum interval** between automatic guides. Guides that conflict are queued, not lost ("no messages
    are lost, they are simply spaced out").
  - **The order:** **Ordering** by priority, and an **urgent** override that always goes first.
  - **Teaches us:** priority plus a gap plus an "essential" flag. But content guidance must not queue forever: a remark about
    the point in view is worthless a minute later. Ours needs an expiry.
- **Amplitude Guides** ([Throttling](https://amplitude.com/docs/guides-and-surveys/throttling)).
  - **The limits:**
    - global caps per session, day, week or month;
    - a **time between** guides;
    - **tag-based throttles**, e.g. "onboarding: 5 a session";
    - "The most restrictive limit applies."
  - **One at a time:** pins, popovers, modals, checklists and banners are one at a time per form factor.
  - **Mutual exclusivity:** a user sees only one item of a group.
  - **Teaches us:** the exact layering for us. A global cap, then a cap per kind (our "tags"), then the item's own rule; the
    tightest wins. And "one of these, not all" for variants.
- **Chameleon** ([Rate limiting](https://help.chameleon.io/en/articles/3513345-rate-limiting-experiences)).
  - Rate limits cover automatic tours. Tooltips and walkthroughs **the user starts** are never limited.
  - A dismissed or snoozed item still counts towards the limit.
  - Rate limiting "takes priority over all other settings", and critical items can be exempted.
  - **Teaches us:** what the visitor asks for is never throttled, and a dismissal counts as a showing.
- **Userpilot** ([When to use what](https://docs.userpilot.com/reference/when-to-use);
  [Hotspots](https://docs.userpilot.com/in-app-engagement/spotlights/hotspots)).
  - **The objects:** Flows, Spotlights (hotspot, native tooltip), Checklists, a Resource Center.
  - **The triggers:** element click, custom event, scroll depth, after a delay, an API call.
  - **The frequency:** Once / Every time / Once a session. "Once" for onboarding, "Every time" for a contextual tip that stays
    relevant.
  - **Teaches us:** a short closed list of repeat modes, named in words, and triggers named as the visitor's actions.
- **Intercom Product Tours** ([Show your tour automatically](https://www.intercom.com/help/en/articles/2900893-automatically-show-your-product-tour-to-the-right-customers);
  [community: send once / every time](https://community.intercom.com/proactive-support-11/if-a-user-engaged-with-completed-a-tour-does-it-stop-showing-for-them-5048)).
  - The modes are "Send once, the first time the person matches", or "every time" with a cadence ("not more than once every
    10 days, stop after 3").
  - When a person matches several tours, they get the most recently updated one.
  - **Teaches us:** a cooldown plus a maximum count is the pair a developer understands. "Most recently updated wins" is
    arbitrary; we use importance.
- **WalkMe** ([ShoutOuts, SmartTips, Launchers, via SAP's WalkMe course](https://learning.sap.com/courses/getting-started-with-building-walkme-solutions/introducing-shoutouts_ba867a00-cc5e-4cb7-8e85-e3aec2b21133);
  [Launchers](https://learning.sap.com/courses/getting-started-with-building-walkme-solutions-ko/introducing-launchers)).
  - **The objects:**
    - Smart Walk-Thrus (step by step);
    - ShoutOuts (announcements whose button starts a walk-thru);
    - SmartTips (on confusing fields);
    - Launchers (a button that starts help);
    - Segments (who sees what).
  - **Teaches us:** a bubble may carry **one action button** ("Show me", "Start the tour"). The ✦ is our Launcher.

### 1.3 Mascots and characters

- **Duolingo** ([World character visemes, Duolingo blog](https://blog.duolingo.com/world-character-visemes/);
  [delayed sign-up, Taplytics](https://taplytics.com/blog/duolingo-ab-test-onboarding);
  [Sleeping, recovering bandit for notifications, KDD 2020](https://research.duolingo.com/papers/yancey.kdd20.pdf)).
  - **The animation:** Rive state machines, with the mouth states (20+ shapes) running in parallel with the body states. Idle
    nods, blinks and brows; a reaction to a right or wrong answer.
  - **Onboarding:** a lesson before the sign-up (about +20 % DAU, a secondary source).
  - **Notifications:** chosen by a "recovering" bandit. A message's effect wears off with repetition and **recovers with
    time** since it was last sent, and ineligible messages "sleep".
  - **Teaches us:**
    - a character feels alive from idle behaviour, not from talking more;
    - value before set-up;
    - novelty decays and recovers. That is our per-item **cooldown** and the rule "never the same line twice in a row".
- **Headspace** ([Appcues GoodUX on its onboarding](https://goodux.appcues.com/blog/headspaces-mindful-onboarding-sequence);
  [Figma: building Ebb](https://www.figma.com/blog/headspace-ebb-ai-companion/)).
  - **Onboarding:** three questions (experience, goal, time), then straight into a first session.
  - **Ebb, the AI companion (2024):** a soft, abstract form "gentle, hopeful, compassionate". Testing made sure members knew
    it was an AI.
  - **Teaches us:**
    - at most three set-up choices;
    - a calm presence is premium, a loud one is not;
    - say plainly that Ask Azu is an AI.
- **Pokémon GO's Professor Willow** ([Bulbapedia: Intro to GO](https://bulbapedia.bulbagarden.net/wiki/Walkthrough:Pok%C3%A9mon_GO/Intro_to_GO);
  [launch review](https://gameluster.com/pokemon-go-come-out-and-play/)).
  - **The start:** Willow greets, makes the avatar, then puts the first Pokémon in front of the player in AR. The player
    catches it themselves, and Willow says almost nothing more.
  - **What critics said:** the bare tutorial left the throw to be learned alone (fan sources).
  - **The buddy:** the buddy's moods and reactions are the "fun" layer, and they are started by the player.
  - **Teaches us:** the very first AR action is done, not shown. Our "Tap a marker" should come with the first marker in view,
    and the guide should stay quiet once it has been done.
- **Game companions: Navi and Alyx** ([GamesRadar on Miyamoto and Navi](https://www.gamesradar.com/games/the-legend-of-zelda/zelda-creator-shigeru-miyamoto-always-knew-that-navi-was-the-biggest-weakpoint-of-ocarina-of-time-and-once-said-i-wanted-to-remove-the-entire-system);
  [Half-Life 2 commentary notes](https://combineoverwiki.net/wiki/Commentary_notes)).
  - Miyamoto called Navi the game's biggest weak point. She repeated the same advice at random times, and later companions
    speak only when asked or in cut scenes.
  - Valve's playtests found that players "disliked virtually any hint of bossiness from Alyx" and preferred to set the pace.
  - **Teaches us:** repetition and bad timing are the failure, not the character. Ours varies its lines and never repeats a
    learned tip. It never sounds like an order: an invitation, not "You must".
- **Game hint ladders** ([Bugnet: designing a hint system](https://bugnet.io/blog/how-to-design-a-hint-system-for-puzzles);
  [escape-room hint systems](https://roomescapeartist.com/2019/10/24/escape-room-hint-systems/)).
  - Hints escalate: first a nudge after a delay or a few failures, then a stronger hint on request.
  - Each hint has a release time and comes only if the puzzle is still unsolved.
  - **Teaches us:** this is our existing invitation ladder. It applies to app items too: a first nudge as a glint, then the
    mascot's words, then "Show me".

### 1.4 Research on help, interruptions and agents

- **NN/g on onboarding tutorials, push and pull** ([Onboarding tutorials](https://www.nngroup.com/articles/onboarding-tutorials/);
  [Help and documentation](https://www.nngroup.com/articles/help-and-documentation/)).
  - "Push revelations are … typically pushy, devoid of context, and intrusive."
  - Pull revelations come with a signal that the user needs them now, and "are less likely to be ignored because they provide
    timely information".
  - "Make help easy to dismiss, and just as easy to find again."
  - "Tutorials are dramatically overused."
  - A tutorial fits a truly novel interaction, such as AR, when the user does the task.
  - **Teaches us:**
    - our push must look like pull: tied to what is on the screen now;
    - every tip can be found again in the ✦ ("Things to try");
    - AR's first gesture is the one place a short guided start is justified.
- **NN/g on instructional overlays** ([Mobile instructional overlays](https://www.nngroup.com/articles/mobile-instructional-overlay/)).
  - One hint at a time, at the moment it is relevant, never a chain of them.
  - Ultra-short text.
  - Make the hint look unlike the interface, so nobody taps it by mistake.
  - **Teaches us:** the mascot's bubble is that distinct look (a character speaking, not a control). The full-screen overlay of
    coach marks is out.
- **Banner blindness** ([NN/g](https://www.nngroup.com/articles/banner-blindness-old-and-new-findings/)).
  - People learn to ignore whatever looks like an ad, wherever it sits.
  - **Teaches us:** a pill that is always there (the wall hint) is soon not seen. A message that comes, is said once and goes
    is seen.
- **Interruption at breakpoints** ([Adamczyk & Bailey, CHI 2004](https://hci.rwth-aachen.de/materials/conferences/CHI2004/1p271.pdf);
  [Bailey's talk](https://hci.stanford.edu/seminar/abstracts/06-07/070406-bailey.html)).
  - An interruption at a coarse breakpoint, between two subtasks, costs almost nothing. One at a fine breakpoint raises the
    workload and the time it takes to resume.
  - **Teaches us:** the scheduler's free moments are breakpoints. The card has just settled, a step has ended, the phone has
    come to rest on a point, a page has closed. Never mid-gesture, mid-sentence or mid-scroll.
- **Clippy** ([Horvitz: the Lumière project](https://www.erichorvitz.com/lum.htm);
  [Sinofsky in Notion's "Clippy walked so Notion AI could run"](https://www.notion.com/blog/clippy-walked-so-notion-ai-could-run)).
  - The Bayesian model that decided *when* to appear was replaced by a cruder rule, so it popped up more often (a secondary
    account).
  - Horvitz "had been concerned that this system would be distracting".
  - **Teaches us:** the timing model *is* the product. Ship conservative defaults and let the developer raise them, never the
    reverse.
- **Mixed-initiative principles** ([Horvitz, CHI 1999](https://www.microsoft.com/en-us/research/publication/principles-mixed-initiative-user-interfaces/);
  [Amershi et al., Guidelines for Human-AI Interaction, CHI 2019](https://www.microsoft.com/en-us/research/project/guidelines-for-human-ai-interaction/)).
  - **Horvitz:**
    - consider the user's attention in the timing;
    - weigh the cost of a wrong guess;
    - allow efficient invocation and dismissal;
    - keep a memory of recent interactions;
    - keep learning.
  - **Amershi et al.:** "time services based on context", "support efficient dismissal", "provide global controls", and
    "update and adapt cautiously".
  - **Teaches us:** the visitor's three global controls (All / Essentials only / Off), a one-tap dismiss, and the memory of
    what was shown and done.

### 1.5 Museum and city guides

- **Smartify** ([Dezeen](https://www.dezeen.com/2017/10/08/new-app-smartify-hailed-shazam-art-world-technology/amp/);
  [Hamburger Kunsthalle](https://www.hamburger-kunsthalle.de/en/hamburger-kunsthalle-smartify)).
  - Point the camera at a work and it is recognised ("visual fingerprints"). Then audio, text, a children's version and audio
    description.
  - The prompt is the camera itself, and the app pushes little.
  - **Teaches us:** in an AR guide the best content guidance is "what you are pointing at": our point in the middle.
- **Bloomberg Connects** ([I amsterdam](https://www.iamsterdam.com/en/see-and-do/museums-and-galleries/bloomberg-connects-a-curator-in-your-pocket);
  [Courtauld](https://courtauld.ac.uk/gallery/plan-your-visit/bloomberg-connects)).
  - Curated tours at the visitor's pace, maps, and the accessibility basics (captions, transcripts, font size).
  - It does not take the initiative.
  - **Teaches us:** a dignified, content-first tone. Initiative is our addition, so it must earn its place.
- **AI museum guides** ([Ask Mona at Autun](https://www.askmona.ai/blog/ai-in-museums-lessons-from-the-autun-museum-chatbot);
  [Centre Pompidou](https://www.askmona.fr/en/project/petit-palais-2/)).
  - Chatbots opened from a QR code, no app needed. The visitors' questions are logged to find gaps in what the museum tells.
  - **Teaches us:** the questions asked of Ask Azu are the best source for new guidance items (an open question below).
- **Proximity and AR city guides** ([Guide Plus Go](https://apps.apple.com/us/app/-/id1472817586);
  [Proximity: Explore Nearby](https://apps.apple.com/us/app/-/id6749175807);
  [FORTH: AR with geolocation anchors](https://ami.ics.forth.gr/en/project/ar-geolocation/)).
  - A notice when a landmark is within about 100 m, rough location by beacon and fine location by image.
  - **Teaches us:** "another wall nearby has a story" is content guidance for later. It needs location, which TileStories
    does not ask for yet (an open question).

### 1.6 What this means for TileStories (the best features to have)

1. **One voice, one at a time:** the mascot is the only one that takes the initiative. Every other cue (a glint, a nudge) goes
   through the same scheduler.
2. **Pull first:** the ✦ menu gets "Things to try", a checklist of the app's features with ticks for those already used.
   "How does this work?" starts the sequence, and Ask Azu answers questions about the app.
3. **Push only in context:** about something on the screen now, at a breakpoint, and short (12 words or fewer).
4. **Stop once learned:** done-events end app items for good.
5. **Escalation, never a block:** glint → words → "Show me" (the existing ladder, applied everywhere).
6. **Premium means rare, calm and precise:** a soft spring, a ring that follows the element exactly, a voice that is never cut
   off, and silence most of the time.

---

## 2. The model

### 2.1 The kinds

| Kind | What it does | Examples | Early → late weight (default) |
|---|---|---|---|
| **App** ("How to") | teaches a control or a gesture of the app | Opening message, Tap a marker, the ✦, hold to talk, the Navigation Bar, Before / After, See it as | 0.70 → 0.40 (5 min) → 0.15 (15 min) |
| **Content** ("What's here") | points at the story in front of the visitor | the point in the middle, "this one has a tour", "three points in view: a route?", "the oldest here", "resume your tour" | 0.25 → 0.50 → 0.65 |
| **Fun** ("Play") | play that the guide starts | the water break, the cookie, a joke about the point, "find the ship" (treasure hunt), seasonal (June sardine) | 0.05 → 0.10 → 0.20 |

- **Not a kind: pull.** The ✦ menu, Ask, "Things to try", "How does this work?" and the pokes started by the visitor are not
  scheduled and never limited (Chameleon, Appcues).
- **Not a kind: states.** Loading, Wall not found and Offline (10.13) are the system's states. While one shows, all push
  guidance waits. The mascot variant of a state is drawn by the state.
- **A group, not a kind: Care.** Inside App: "Look up from the phone now and then" (after 4 min of continuous camera use on a
  street wall), "Sound off? Captions are on", and later "Battery low". It is App by its scheduling, but its own group, so it
  can be switched off as one.
- **Kinds are data:** a fourth kind can be added in the Editor. The default is three, because three is what a developer
  balances by eye.

### 2.2 The hierarchy and inheritance

`Overall → Kind → Group → Item`, and an optional `Sequence` (an ordered group).

- Each level has the same presentation and timing fields. A level below inherits them and shows the inherited value as words,
  the Editor's rule: "As the kind (Ring)", "As the group (once a visit)". It overrides only what it changes.
- **Overall** holds the budget: the gap, the cap per minute, the quiet times, the visit clock and the mix along the visit.
- **Kind** holds:
  - its weight curve along the visit;
  - its cap per visit;
  - its gap since the last item of the same kind;
  - its default look.
- **Group** holds a theme switch (e.g. "The card" off for a demo). It also holds one of **All** (each item on its own) /
  **One of these** (Amplitude's mutual exclusivity: the variants of a remark) / **In order** (a Sequence, TipKit `.ordered`).
- **Item** holds the words, the target, the trigger, the rules and the overrides.

**Default groups:**

- **App:**
  - The start: Opening message, Tap a marker, Point at the tiles.
  - The wall: Ask about the view, Zoom by moving closer.
  - The card: the six Hints and How to use the card.
  - The guide: the ✦, Hold to talk, Pause any time, Hide the dock.
  - Settings: Who's exploring, Length, Voice.
  - Care.
- **Content:**
  - What's in view: the point in the middle, with and without a card.
  - Tours: "has a tour", "resume".
  - Routes and finds: route, oldest, linked cards.
- **Fun:**
  - Moments: water, cookie.
  - Jokes: the point's own lines.
  - Games: the treasure hunt invite.
  - Seasonal.

### 2.3 The guidance item (every field)

Types: `bool`, `enum`, `int`, `float`, `text` (one per wall language), `ref` (a named thing from a list: never typed ids).

| Field | Type | Default | Why |
|---|---|---|---|
| **Label** | text (Editor only) | "New guidance" | the name in lists, warnings and the log ("names things, not ids") |
| **On** | bool | On | each item can be switched off without deleting it |
| **Kind**, **Group** | ref | the list it was added to | moving an item moves its inherited defaults |
| **Says** | text × 1-4 variants | one line | **never the same line twice in a row** (Duolingo novelty); 12 words or fewer, a warning past 16 |
| **Placeholders** | in Says | `{point}`, `{button}`, `{count}` | e.g. "That's {point}. Tap and I'll show you around." |
| **Spoken** | enum: As the kind / Spoken / Bubble only | App and Fun: Bubble only; Content: Spoken when the guide's voice is on | a street is noisy and private; tips are read, stories are told |
| **About** (the target) | ref: Nothing / An app control (the ✦, the Navigation Bar, the grabber, the Locate Button, the talk button, the settings mark, Ask about the view, the Wall button…) / A card block (a kind, or "the first in view of kind X") / A card page / The point in the middle / A point (by name) / A marker (the nearest, or by name) / The mascot itself | Nothing | lets the mascot go beside it, point and focus |
| **Mascot goes** | enum: Beside it / Stays where it is / To the middle | Beside it (Stays if About = Nothing) | the developer's "position near the element" |
| **Points at it** | bool | On | the existing "Azu points" |
| **Focus** | enum: As the kind / None / Glint (the element shines once) / Ring / Spotlight | App: Ring; Content: Glint; Fun: None; Spotlight only on Essentials | the developer's "light it, darken the rest". The dim is reserved, or it loses its force |
| **Dim the rest** | float 0.2-0.6 | 0.35 | under the guide's own spotlight value (0.4), so a tip is lighter than a tour |
| **Shows the gesture** | enum: None / Tap / Double tap / Hold / Swipe left-right / Drag / Pinch | None | a ghost finger, for gestures the words cannot carry (Before / After) |
| **Button** | enum: None / Show me / Start the tour / Open the card / Ask about the view / Try it / custom (an action from the action list) | None | WalkMe's ShoutOut button: one action at most |
| **When** (the trigger) | ref: an event from the event list (below), or "when its conditions become true" | "when its conditions become true" | the visitor's actions, named in words |
| **Only if** (conditions) | list of chips: screen is (Wall / Card at Peek, Half, Full / A page / A tour / Ask open); target in view; phone still ≥ n s; visitor idle ≥ n s; visit number ≥ / =; audience is; has / has not used (a feature); item (not) done; liveliness is; point is not Solemn | the target in view, phone still 1.2 s | TipKit's parameter and event rules |
| **Importance** | int 1-5 | 3 | the scheduler's main score |
| **Along the visit** | enum: As the kind / Steady / Early (×1.5 → ×0.5) / Late (×0.5 → ×1.5) / Custom (three points: start, 5 min, 15 min) | As the kind | "importance that changes along the visit" |
| **Essential** | bool | Off | Pendo's "urgent": skips the breathing gap and the kind's caps, but **never** the narration, a moving phone or the visitor's Off |
| **Shows** | enum: Once ever / Once a visit / Up to n a visit / Every time (cooldown only) / Until done | App: Until done; Content: Up to 3 a visit; Fun: Once a visit | Userpilot's and Intercom's modes, merged |
| **n** | int 1-20 | 3 | with "Up to n a visit" and "Until done" (the most before it gives up: TipKit `MaxDisplayCount`) |
| **Again after** (cooldown) | seconds 10-1800 | App 45, Content 20, Fun 300 | the developer's "ideal frequency": a minimum time is easier to judge than "per minute"; the Editor reads it back as "at most about once every 45 s" |
| **Done when** | ref: a feature event (opened a card, opened the ✦ menu, started a tour, used the Navigation Bar, dragged a Before / After, held the talk button, changed Who's exploring…) | per item, e.g. Tap a marker: "opened a card" | **TipKit's invalidation**: stops for good once used |
| **After a dismissal** | enum: Wait (×3 the cooldown) / Not again this visit / Never again | Wait | Material's asymmetry: a dismissal waits longer |
| **Expires** | enum: When its target leaves the screen / After n s in the queue / Never | content: target leaves; others: 10 s | a remark about a point is worthless once the point is gone |
| **On screen for** | Auto (1.6 s + 0.32 s a word, 3-9 s) / n s | Auto | long enough to read; the visitor's "Tips stay until tapped" overrides it (2.2.1) |
| **During a tour** | enum: Never / Between steps | Never (Fun Moments: Between steps) | quiet hours |
| **Audiences** | chips: Family / Explorer / Expert | all | e.g. Jokes for Family and Explorer only |
| **Mood, clip** | enum: As the kind / smile / thinking / surprised…; wave / point / think | point when it points, else talk | the mascot's body says it too |
| **Sound** | enum: As the kind / Soft chime / None | Soft chime for Content, None for App | an earcon draws the eye without words |
| **Haptic** | enum: As the kind / Light / None | None (Essentials: Light) | a touch for the one that matters |

**The read-back line** under each item, written by the Editor in words:

> Tap a marker · App › The start · Essential · after the Opening message, when a marker is in view and the phone is still ·
> at most about every 25 s, up to 3 times a visit, until the visitor opens a card.

**Validation, as named warnings:**

- "Tap a marker: it can never show: About = A marker, but Only if requires a card open";
- "Joke about Ribeira Palace: 19 words; aim for 12";
- "Three items share the line 'Tap ✦'".

### 2.4 Events (the triggers and done-events)

The app raises named events; the list is the same in the Lab and in Unity (the names in brackets are their keys):

- `app_opened`, `visit_started`, `wall_found`, `wall_lost`, `marker_in_view`, `point_settled` (a point in the middle for
  1.2 s), `phone_moving`, `phone_still`;
- `card_opened`, `card_stop` (Peek / Half / Full), `page_opened`, `block_in_view` (kind), `tour_started`, `tour_step_ended`,
  `tour_ended`, `tour_stopped`;
- `gb_menu_opened`, `ask_opened`, `talk_held`, `cam_ask_opened`, `navbar_used`, `locate_used`, `ba_dragged`, `strip_swiped`,
  `term_opened`, `lens_chosen`, `settings_opened`, `audience_set`;
- `mascot_poked`, `idle` (n s with no touch);
- `guidance_shown`, `guidance_dismissed`, `guidance_acted` (for chains: "after item X").

In the Editor they show by their names ("Opened a card", "Held the talk button"), never by their keys.

### 2.5 The scheduler

**One scheduler for the whole app, ticking 4 times a second.** It is a pure function so it can be tested with a fake clock:
`pick(items, visitor, context, now) → {item, why} | {none, why}`.

1. **Is the guide free?** No push while any of these holds (the "why" names the first that holds):
   - the visitor's Tips = Off (Essentials only lets through only `Essential` items);
   - a sentence is being said (tour, answer, an earlier item);
   - a tour is on and the item is not "Between steps", or it is, but this is not the gap between two steps;
   - the phone moved in the last 0.7 s, a touch in the last 0.8 s, the card's first 1.5 s;
   - Ask, Ask about the view, the settings or the ✦ menu is open;
   - a state shows (loading, wall not found, offline);
   - an item is on screen;
   - **breathing room:** less than **Overall gap** (15 s) since the last pushed item ends (skipped by `Essential`);
   - the **cap per minute** (2) is reached in the last 60 s.
2. **Which are eligible?** Each item that is On, its kind and group On, and its trigger fired (or its conditions true), with
   every Only if true. Also:
   - its Shows count not exhausted, its cooldown passed, not done, not expired;
   - its kind's cap per visit not reached, and its kind's own gap passed;
   - in a One of these group, the one the visitor has seen least (ties by order).
3. **Score:** `importance × along_the_visit(t) × kind_weight(kind, t) × freshness × fit`.
   - `t` is the **visit clock**: active minutes (time on the wall or the card with the app in front; a pause in the background
     stops it). A returning visitor starts at **Returning visitors start at** (5 min). So they get the "later" mix: less app,
     more content.
   - `kind_weight` is linear between the three anchors (start, 5 min, 15 min), from the Overall table.
   - `freshness` = 1 for an item never shown; else `min(1, since_last / (2 × cooldown))`. This is the recovering novelty.
   - `fit` = 1.25 when its target is the thing in the middle of the screen or the element just touched, else 1.
   - **The tie:** the order in the list. The developer's drag order is the tiebreak, never the main rule.
4. **Show it:** the presenter places it. The scheduler records `shown` (count, time, the line used) and sets the busy time.
   - A tap on the bubble's text or its button is **acted** (and, if the button does the item's feature, also **done**).
   - A tap on its close, a swipe, or ignoring it until it goes is **dismissed** (counts as shown: Chameleon).
5. **What the 70 / 30 means:** the weights split the *choice*, not the time. Early in a visit, when an App item and a Content
   item are both eligible with equal importance, App wins 0.70 to 0.25. But an App item that is done is not eligible at all.
   So a visitor who learns fast hears content sooner. That is the behaviour the developer described, without a quota that
   would push tips nobody needs.

**The defaults, so a visit sounds like this** (First visit, Balanced):

- 0:00 Opening message.
- 0:08 Tap a marker (if no card yet).
- The visitor opens a card. Nothing for 1.5 s, then the grabber nudge.
- …the tour is quiet…
- 3:10 back on the wall: "That's Lisbon Cathedral. Tap and I'll show you around."
- 4:40 "Need anything? Tap ✦."
- 9:00 a cookie, between two steps.

In all, about 8-12 pushed items in 15 minutes, at most 2 a minute, and none over a sentence.

**Talkativeness** (Overall): **Quiet** (gaps ×2, caps ×0.5, Fun off) / **Balanced** / **Chatty** (gaps ×0.6, caps ×1.5,
for demos). One knob for the developer, with the detailed numbers below it for the expert.

### 2.6 The visitor's controls

- **On the bubble:**
  - a tap on its words does its action, or closes it;
  - a small ✕ (44 px target) or a swipe dismisses it;
  - a long press (0.5 s) asks "Fewer tips like this?" with **Fewer** / **Keep**. Fewer sets the group to "Not again this
    visit".
- **Settings > The guide > Tips:** **All** / **Essentials only** / **Off**. A line under it: "You can always tap ✦ for help."
  Plus **Tips stay until I tap them** (accessibility, WCAG 2.2.1) and **Show the tips again** (resets the memory, with an
  Undo toast).
- **The ✦ menu gets two items:**
  - **Things to try:** the App items as a checklist, a tick on those done, each with **Show me**, which shows the item now
    and ignores the caps (pull is never capped);
  - **How does this work?:** the start of the How to use sequence.
- **Ask Azu:** "how do I…?" is answered from the catalogue (the items' words are part of the library). The tool **Show me
  (a guidance item)**, with policy **Suggest**, puts a button under the answer.

### 2.7 What is remembered

- **Per item:**
  - shown (this visit, ever), when it was last shown and the line used;
  - done (ever), with the event that did it;
  - dismissed (this visit, ever);
  - acted.
- **Per visitor:**
  - the number of visits (a visit ends after 30 min in the background);
  - the active minutes this visit;
  - Tips All / Essentials / Off, and Stay until tapped;
  - introduced (the mascot said who it is);
  - the features used (the done-events).
- **Where:**
  - on the phone only, never sent (it is how the app is used, so it stays local: GDPR);
  - Unity: one JSON in `Application.persistentDataPath`;
  - the Lab: the page's state, plus `localStorage` in try / catch as a per-viewer convenience. **Reset the visitor** is in
    the Editor.
- **"Remember across visits":** On / Off (Off: every visit is a first visit, for kiosks and demos).

---

## 3. Presentation (each with its Unity path)

**Premium means:** one character, one bubble style, few words, exact placement, soft physics and long silences. No banners,
no stacked tooltips, no full-screen overlays of arrows, no red dots.

| Element | Design | Reduce Motion | Unity 6.3 path |
|---|---|---|---|
| **Bubble** | the existing Azu bubble: the dock's dark glass look (solid, no blur), 15 px type, at most 2 lines, a tail towards the mascot; the word being said lit (the read-along) when it is spoken; a ✕ at the corner; the button as a pill inside | appears by fade only | a 9-slice `VisualElement` with a tail sprite, `Label` with `PostProcessTextVertices` for the lit word (checked) |
| **Placement** | the bubble goes on the side of the target with most room, never over the target, the Navigation Bar, the talk button or the narration; it stays 12 px inside the safe area | same | `worldBound` of the target and the panel's `layout` |
| **The mascot goes beside it** | a short glide (0.45 s, a spring); to a far target it turns into light (the dot cube) and forms again there; on the wall, near a marker, it is the world-space mascot at the marker's depth (the existing walk) | a cut, no travel | the Azu render-texture element moved by `style.translate` (USS transition); the wall: the hand-off of `_3.4` 4.11 |
| **Pointing** | the mascot's point clip aimed at the target | the pose only | the Animator's point state; the arm's angle as a parameter (GLB) or a Rive input |
| **Glint** | the target shines once: a light band sweeps over it (0.6 s) | a 0.4 s brightness rise, no sweep | a gradient sprite under the element, `translate` tween |
| **Ring** | the warm ring of the guide's Focus around the target, following it frame by frame | no pulse | `generateVisualContent` + `Painter2D` stroke (checked) |
| **Spotlight** | the rest dimmed to 0.35 with the target cut out (rounded), the ring on it; Essentials and How to use only | same, no glide | `Painter2D` `Fill(FillRule.OddEven)` (checked), placed every frame by `schedule.Execute` |
| **Ghost finger** | a soft white finger does the gesture twice (a tap, a swipe, a drag along the Before / After line) | a still finger and an arrow | a sprite `VisualElement` tweened along a path; on the wall, a quad |
| **Sound** | a soft two-note chime for Content (the guide's earcon family), none for App by default; the music bed ducks | same | an `AudioSource` one-shot through the guide's mixer group |
| **Haptic** | a light tap for Essentials only | same | the Feedback service (9.5) |
| **Accessibility** | each item is announced once to the screen reader as a status; nothing is conveyed by the light alone; every word at 4.5:1 on its bubble; 44 px targets; under a screen reader no auto-travel and the spotlight's dim is off | n/a | Unity 6's screen-reader support (`AccessibilityHierarchy` / `AssistiveSupport`: to verify on 6.3 before relying on it); `aria`-like roles in the Lab |
| **Mascot Off** | the same items, from a small bubble anchored to the target with no character (the old pill's look, but said once, not always there) | same | the same bubble, with no Azu element |

**Where the mascot is drawn, by screen:**

- **On the wall:** beside the ✦ (corner). It walks to a marker for "Tap a marker".
- **On the card:** the guide's Azu, or beside the ✦ above the Navigation Bar.
- **In a tour:** above the dock. Only Between steps items show there.

---

## 4. Merge with what exists

| Today | Decision | How | Why |
|---|---|---|---|
| **Global Scene > Wall hint** (the pill "Tap a marker to open its card") | **Drop the pill; keep the message** | becomes the App item **Tap a marker** (The start, Essential, Until done = opened a card, up to 3 a visit, Again after 25 s, the mascot beside the nearest marker, pointing, a Ring on the marker). The word keeps its key `wall_tip`. The Global Scene row becomes a link: "Now said by the guide: Guidance › App › The start › Tap a marker" | the developer's request; a pill always there goes unseen (banner blindness); the mascot says it once, at the moment a marker is in view |
| (new) **Opening message** | **Add** | the App item **Opening message**: Essential, Once a visit, `visit_started` + wall found. First visit: "Hi, I'm {name}. Point your phone at the tiles and tap a glowing marker." Later visits: "Welcome back! Tap a marker, or tap ✦ for me." On the first visit it holds the marker instruction, so Tap a marker waits 20 s before it repeats it | "one message at the beginning"; one voice; no tip on top of the welcome (Material: no prompts on open other than the welcome) |
| **Card Controls > Hints** (nudge, Locate label, Navigation Bar, Before / After, strips, underlined words) | **Merge** | each becomes an App item in group **The card** with Presenter = **On the element** (no mascot, the element moves itself, as today). Their rules (one at a time, never while scrolling, not in the first 1.5 s, never in a tour) are the scheduler's own. `CardHintService` (10.12) is superseded. Card Controls > Hints stays as a **view** of the same items (the same rows, filtered), so the options stay on their element | one at a time across the app, not only on the card; one home for the data, the rows where the element is |
| **How to use the card** (5 steps, only when asked) | **Merge, as a Sequence** | the Sequence **How to use the card** (In order); started from the settings panel, the ✦ "How does this work?" and Ask; never by itself | it is pull; TipKit's ordered group |
| **Guide > Azu comments** (a remark when a point settles, every 7 s, at most 12) | **Merge** | Content › What's in view: **Point in the middle** (One of these: three lines) and **Point with a card** (the bubble's button: Open the card and start its tour). Again after 7 s and At most 12 become the item's cooldown and the kind's cap. The words keep `fab_*`. The old row becomes a link | the same behaviour, now sharing the gap with every other item |
| **The ✦ hint** ("Need anything? Tap ✦", once a visit, after 4 pokes or 25 s idle) | **Merge** | App › The guide: **The guide button**: Until done = opened the ✦ menu, Once a visit, When = idle 25 s or poked 4 times. It points at the ✦. Liveliness Calm: words only, no point | the same rule, and it stops for good once the visitor has used the ✦ |
| **Mascot > Moments** (water after 60 s of narration, a cookie; one a tour, one every 5 min) | **Merge** | Fun › Moments: **Water break** and **Cookie**, During a tour = Between steps, Again after 300 s, Once a visit, Lively only, not on a Solemn point. Water break after (s) becomes the item's condition "narration ≥ n s". Mascot > Moments stays as the master On / Off (the group's switch) | Fun is a kind; its caps are shared |
| **Invite to try** (the invitation ladder in a tour) | **Keep** in 6 · Guide | it is a step of the tour, not a scheduled item. The scheduler is quiet during it. Its look (the nudge lines, the wait, "No problem") is the same ladder the App items use (glint → words → Show me) | it belongs to the tour's flow; sharing the presenter keeps one look |
| **The guide's introduction** (first tour) | **Keep, coordinated** | if the Opening message already introduced the mascot this visit (`introduced`), the tour's intro drops "I'm Azu…" and says only "Let's meet {point}!" | no double self-introduction |
| **Who's exploring** (the first card's screen) | **Keep, with a place option** | Audiences › Ask who's exploring: **At the app's start** / **On the first card** (today) / Never. The default is At the app's start once the first pages exist (section 6); until then, On the first card. The mascot presents it | the profile belongs to the start (Headspace, Material Self-select), but the first-card screen stays valid without first pages |
| **Loading & states** | **Keep** separate | the scheduler waits while a state shows | a state is not guidance |
| **Ask about the view** pill, **suggested questions** | **Keep** | they are controls; a Content item "Ask about what you see" may point at the pill once | pull controls |
| **Pokes, tickles, the fall** | **Keep** in Mascot | started by the visitor, never scheduled | pull |

---

## 5. The Editor

### 5.1 Where it lives: a new top-level tab, **Guidance**

The tab order: Global Scene · Specific Marker · Detail Card · **Guidance** · Languages & Terms.

- **Why a tab:**
  - Guidance spans the wall (Global Scene), the card (Card Controls), the tour (6 · Guide), the first pages (outside every
    tab) and the ✦.
  - It needs room for three lists, a simulator and a log.
  - In Unity it is one asset (`GuidanceCatalog`) with its own inspector. A tab matches that.
- **Rejected: inside 6 · Guide.** It already has about 13 foldouts. Guidance is not about the card, and the developer would
  scroll past Narration and Ask Azu to edit a wall tip. The Mascot keeps a link row: "What the mascot says by itself:
  Guidance".
- **Rejected: an "Other settings" tab** shared with Languages & Terms. It would be a junk drawer. Languages & Terms has a clear
  identity (words and terms) that it should keep.
- **Kept where they are, as views or links:**
  - Card Controls > Hints (a filtered view);
  - Global Scene > Onboarding hints (a link);
  - 6 · Guide > Azu comments (a link) and Moments (the master switch).
- **The words:** each item's words are on the item (its Aa), because the bubble is the element they are drawn on. Languages
  & Terms lists them under the owner "Guidance".

### 5.2 The tree

```
Guidance
├─ 0 · Preview & Test
│   ├─ Simulate a visit          timeline 0-20 min (lanes: App / Content / Fun / Quiet), Play ×1 ×10 ×30, scrub,
│   │                            each dot = an item shown (tap: show it on the phone); grey ticks = eligible but skipped
│   ├─ Show next now             what the scheduler would pick now, and why; ▸ shows it
│   ├─ Why not now?              per item: the first rule that holds it back, in words
│   ├─ The visitor               Visit: First / 2nd / 5th · Has used: [Opened a card] [The ✦] [Talk] … · Tips: All / Essentials / Off
│   ├─ Reset the visitor         (Undo toast)
│   └─ Log                       the last 30 decisions: time, item, shown / skipped, why
├─ 1 · Overall
│   ├─ Guidance                  On / Off
│   ├─ Preset                    First visit / Returning / Family / Expert / Quiet / Chatty / Custom ("Custom: 3 changes from First visit")
│   ├─ Talkativeness             Quiet / Balanced / Chatty
│   ├─ Breathing room            5-60 s (15)
│   ├─ At most a minute          1-4 (2)
│   ├─ Quiet during a tour       Between steps only / Silent
│   ├─ Visit clock               Returning visitors start at 0 / 3 / 5 / 10 min (5) · A visit ends after 30 min away
│   ├─ Mix along the visit       table: rows App / Content / Fun × columns Start / 5 min / 15 min, and its little curve
│   ├─ Opening message           (the item, opened here too)
│   ├─ The visitor's controls    Tips in Settings: Shown · "Fewer like this": On · Things to try in the ✦: Shown
│   ├─ Remember across visits    On / Off
│   └─ Mascot Off                Plain bubble at the target / Nothing
├─ 2 · App guidance          (kind: On · weight curve · at most a visit 12 · own gap 30 s · default look)
│   ├─ The start             [≡] [On] Opening message      ●●●●● Essential · once a visit         ▸ ✎
│   │                        [≡] [On] Tap a marker         ●●●●● Essential · until done (3)       ▸ ✎
│   ├─ The wall              …
│   ├─ The card              (the six Hints)  …
│   ├─ The guide             [≡] [On] The guide button     ●●●○○ until done · once a visit         ▸ ✎
│   ├─ Settings              …
│   ├─ Care                  …
│   └─ + Add guidance   + Add a group
├─ 3 · Content guidance      (kind row as above; at most a visit 20; own gap 10 s)
│   ├─ What's in view · Tours · Routes and finds
├─ 4 · Fun guidance          (at most a visit 3; own gap 300 s; Solemn points: off)
│   ├─ Moments · Jokes · Games · Seasonal
├─ 5 · Sequences             How to use the card (5 steps) · First run (the app's first pages, section 6)
├─ 6 · Look                  defaults for the bubble, the mascot's move, pointing, focus, dim, ghost finger, sound, haptic,
│                            Reduce Motion, screen reader
└─ config.json               the live JSON of the catalogue
```

**An item, opened (✎):**

```
Tap a marker                                         [On]  ▸ Show now   Why not now?
  Kind / group      App › The start
  Says              EN "Tap a glowing marker to open its story."   PT "Toque num marcador para abrir a história."
                    + Add a line (variant)            [Draft with AI]
  Spoken            As the kind (Bubble only)
  About             A marker: the nearest in view
  Mascot goes       Beside it        Points at it  On
  Focus             Ring             Shows the gesture  Tap
  Button            None
  When              its conditions become true
  Only if           [screen is Wall] [a marker in view] [phone still ≥ 1.2 s] [after: Opening message + 20 s]
  Importance        ●●●●● 5          Along the visit  Early      Essential  On
  Shows             Until done, at most 3 a visit     Again after 25 s     After a dismissal  Wait
  Done when         Opened a card
  Expires           when its target leaves the screen
  Audiences         Family Explorer Expert
  Mood / clip / sound / haptic   As the kind
  ── "At most about every 25 s, up to 3 times a visit, until the visitor opens a card. Early in the visit."
```

### 5.3 Adding, editing, ordering

- **+ Add guidance** opens a small picker of templates. Each fills the fields:
  - **Tip about a control** (App, Until done, About = a control);
  - **Remark about the point in view** (Content);
  - **Offer a tour** (Content, Button = Start the tour);
  - **Moment** (Fun, Between steps);
  - **Step of a sequence**;
  - **Blank**.
- **Duplicate** and **Delete** sit in the row's ✎ menu. Delete removes the item at once and shows an Undo toast for 5 s, with
  no confirmation popup (the popup rule: good UX, and a test per button).
- **Reorder:** drag the handle (≡) within a group, or drag a group within its kind. The order is the tiebreak, and the step
  order in a Sequence.
- **Order across kinds** is set by the weights (the Mix table), never by dragging. The Mix table's little curve and the
  simulator make it visible.
- **Moving an item** to another kind or group: the Kind / group row. The inherited values change, and the Editor says which
  ("Again after: was 45 s (App), now 20 s (Content)").
- **Bulk:** a group's switch, and the kind's switch.
- **Per point:** Specific Marker › Guide script gets **Remarks** (the point's own lines for Content and Fun items:
  "{point}'s own line, else the item's"). Jokes and facts are content, so they live with the point.

### 5.4 Presets

A preset writes the Overall and kind values; the items are untouched. **Custom** lists the changes.

| Preset | Talkativeness | Mix at Start (App / Content / Fun) | Fun | Caps per visit (A / C / F) | Note |
|---|---|---|---|---|---|
| **First visit** (default) | Balanced | 0.70 / 0.25 / 0.05 | On | 12 / 20 / 3 | the developer's schedule |
| **Returning** | Balanced | 0.35 / 0.55 / 0.10 | On | 6 / 20 / 3 | chosen automatically from visit 2 (an Overall option) |
| **Family** | Chatty-lite (gaps ×0.8) | 0.55 / 0.25 / 0.20 | On, games first | 12 / 20 / 6 | Audiences: Family uses it |
| **Expert** | Quiet | 0.30 / 0.70 / 0 | Off | Essentials only / 20 / 0 | Audiences: Expert uses it |
| **Quiet** | Quiet | 0.70 / 0.30 / 0 | Off | Essentials only | for a crowded site, a kiosk |
| **Chatty** | Chatty | 0.50 / 0.35 / 0.15 | On | 30 / 40 / 10 | demos and testing |

**Audiences:** Looks per audience gets the row **Guidance preset**: Family → Family, Explorer → First visit (Returning from
visit 2), Expert → Expert. It is written into the guidance when the audience changes, as Liveliness is.

### 5.5 The demo each row shows (the developer's rule)

Every row joins `rowDemos`, so a change, or the label's ▸, shows it on the phone at once:

- **An item's rows:**
  - they put the phone where the item lives (`view`, `wall`, `stop`, `scroll` to its target);
  - they show the item now, ignoring the gap and the caps (a demo is pull);
  - they ring the bubble and the target.
- **Spoken, Focus, Mascot goes, gesture, button:** the same item, with the new value.
- **Shows, Again after, Done when, Expires:** a 10× accelerated mini-timeline on the phone's preview bar. E.g. Again after
  25 s: the item, a countdown chip "again in 25 s" at 10×, and the item again. Done when: a simulated "opened a card", then
  the chip "done: will not show again".
- **Overall and kind rows** (gap, cap, mix, talkativeness, preset): they run the simulator for 2 visit-minutes at ×30 on the
  phone, with the timeline's dots drawn in the Editor beside the row. The phone plays each item for 1 s.
- **The visitor's controls:** the phone's Settings opened at The guide > Tips.
- The self-test fails a row of the Guidance tab without a demo (`NO DEMO`), as for 6 · Guide.

### 5.6 How the developer tests the schedule

- **Simulate a visit.** A deterministic run, with a fake clock and a script of the visitor's actions:
  - the default: a first visit of 20 min with a card at 1:00, a tour 1:30-6:00, the wall again, a pan every 20 s;
  - **Scripts:** First visit, Fast learner (uses everything in 2 min), Idle (does nothing), Tour lover, Expert.
  - The lanes show what was said, when, and every skip with its reason.
  - The same script runs in the test (`test9e`), so the timeline the developer sees is the one the test checks.
- **Show next now** and **Why not now?** on the live phone: the scheduler on the real state.
- **The scenes in Ask Azu's console** (the Lab's rule: tests press the controls as a finger does), each with a ✓ or a ✗ and its
  reason:
  1. First visit: the opening, then Tap a marker by the nearest marker.
  2. Ignore it: again after 25 s, at most 3.
  3. Open a card: Tap a marker never again, even on the next visit.
  4. During a tour: nothing pushed; a cookie between steps only.
  5. The phone moving: nothing.
  6. 15 min in: content outnumbers app.
  7. The Quiet preset: only Essentials.
  8. Tips Off in Settings: nothing pushed, and the ✦ "Things to try" still works.
  9. Mascot Off: a plain bubble at the marker.
  10. Reduce Motion: no travel, no glint sweep, the words.

---

## 6. Onboarding at the app's start

**One system, two presenters.** The first pages are a **Sequence** in the Guidance catalogue (5 · Sequences › First run). Its
steps use the **Page** presenter: a full screen with the mascot and one choice. The in-visit items use the **Bubble**. The same
words, memory, events and Editor serve both. The scheduler does not run the pages: a Sequence runs in order when it starts.
While it runs, the scheduler is quiet.

**The first pages (Quickstart plus a light Self-select; under 30 s; every page skippable):**

1. **Hello:**
   - the mascot forms from a tile (1 s, a chime) and waves: "Olá! I'm your guide to Lisbon's tiles.";
   - the language as a chip, from the phone's language ("English · change");
   - the buttons **Let's go** and **Show me how it works** (the How to use sequence, after the camera).
2. **Who's exploring** (moved here; Skip = Explorer). Three large choices with a line each, presented by the mascot. It
   writes the audience, and with it the guidance preset.
3. **The camera**, asked at the moment it is needed (HIG): the mascot says "I need the camera to see the tiles. Nothing is
   recorded." Then the system's prompt.
   - Refused: the page says how to allow it in Settings, with a button.
   - Refused: the app also offers the "Explore without the camera" list (open question).
4. **Straight to the camera.** The wall's **Opening message** (short, since the mascot has already said hello: "Point at the
   tiles and tap a glowing marker"), then **Tap a marker**.

**What is not there, and why:**

- no carousel of benefits (NN/g: skipped; Material: never with Self-select);
- no account (nothing to save off the phone);
- no tutorial of the card before a card exists (the Hints teach it in context);
- voice, length and captions are not asked: they are App items later ("You can make me slower in Settings", after the first
  tour), or in Settings.

**On later launches:** no pages. A **Welcome back** opening message (Returning preset). After an app update, a "What's new"
item (App, Once ever, per version) is possible; it is a normal item.

**In the Lab:** the preview bar's **Start the app** button replays the first run (the visitor reset). Each page is a row group
in 5 · Sequences › First run (On, the mascot's line, the choice). Its demo is the page.

---

## 7. The build plan for the Lab (rounds)

Each round follows `skills/Feature_Prototyping.md`:

- a module of its own (`src/guidance/*.js`, `.css`), joining named points, never wrappers;
- `PROTOTYPE_STRUCTURE.md` updated in the same round;
- `python build.py` and the structure check;
- `node run_tests.js` with a new baseline;
- the self-test (`runtest8.ps1`) at `bad=0`;
- the record copy and the publish.

| Round | Adds | Tests |
|---|---|---|
| **R1 · The wall hint becomes the mascot's** | `guidance/catalog.js` (the data: kinds, groups, items; only The start group), `guidance/scheduler.js` (a first `pick` with: the guide free, Essential, Until done, Again after, at most n), `guidance/present.js` (the bubble beside the mascot, the mascot beside the nearest marker, pointing, a Ring on the marker; Mascot Off → a plain bubble at the marker). Items: **Opening message** and **Tap a marker** (`wall_tip` kept). The pill removed (`world/wall.js`); Global Scene > Wall hint becomes a link row. The Guidance tab with 1 · Overall (Guidance On / Off) and 2 · App › The start (both items: On, Says, Shows, Again after, Done when). Their `rowDemos`. The events `visit_started`, `marker_in_view`, `phone_still`, `card_opened` | `test9e`: the opening once a visit; Tap a marker after 1.2 s still and ≥ 20 s after the opening; never while the wall pans; again after 25 s, at most 3; never after a card opens (this visit and the next); Mascot Off: a plain bubble; under Reduce Motion no travel. Scenes 1-3 and 9 in the console. The self-test: the new rows `ok`, Wall hint's row gone from the counts |
| **R2 · The scheduler** | the full `pick`: breathing room, a cap a minute, quiet during (tour, a sentence, Ask, settings, states, moving), kinds and their caps and gaps, importance, Along the visit, the mix with the visit clock, freshness, fit, expiry, One of these. The memory (the visitor, per item), Reset the visitor. 0 · Preview & Test: Show next now, Why not now?, the Log | `test9f` (pure, fake clock): each rule alone, then together; the "why" of each skip; a 15-min script gives App > Content in the first 5 min and Content > App after 10 |
| **R3 · Azu comments and the ✦ hint join** | Content › What's in view (Point in the middle, Point with a card) and App › The guide (The guide button) as items; their old timers removed; the old rows become links | `test8s` and `test9d` unchanged and green (the same behaviour); new: a comment never within 15 s of a tip |
| **R4 · Hints and How to use join** | App › The card: the six Hints with the On the element presenter; Card Controls > Hints as a filtered view of the same rows; the Sequence How to use the card; the ✦ "How does this work?" | `test8g` unchanged and green; new: a hint and a mascot tip never at once |
| **R5 · The simulator and the presets** | Simulate a visit (lanes, scrub, ×10 ×30, scripts); Presets; Talkativeness; the Mix table and curve; Audiences' Guidance preset; the accelerated demos for Shows / Again after / Done when | `test9g`: each script's timeline matches its expected one (counts per kind per 5 min, never two within the gap); each preset changes the timeline as its table says |
| **R6 · The look** | the Glint, the Spotlight for Essentials, the mascot's travel as light, the ghost finger, the chime and haptic, Reduce Motion, the screen reader's status, "Tips stay until I tap them"; the item's full form (all fields), + Add guidance with templates, Duplicate, Delete with Undo, drag to reorder; validation warnings | `test9h`: the bubble never overlaps its target, the bar or the talk button (rects); the ring within 1.5 px of the target while it scrolls; every word ≥ 4.5:1; each button of the item form pressed once (the popup rule) |
| **R7 · Fun** | Fun › Moments (water, cookie: moved from Mascot), Jokes (Specific Marker › Remarks), Games (the treasure hunt invite), Seasonal; the Solemn flag | `test8k` green; new: never on a Solemn point; never over a sentence; Calm: no Fun push |
| **R8 · The first pages** | the Page presenter; 5 · Sequences › First run (Hello, Who's exploring, the camera, to the wall); the preview bar's Start the app; Audiences › Ask who's exploring: At the app's start / On the first card / Never | `test9i`: the first run in order, each Skip, the camera refused; the opening message's short form after it; Who's exploring not asked again on the first card |
| **R9 · Pull and Ask** | the ✦ "Things to try" checklist; Settings › Tips (All / Essentials / Off), Fewer like this; the catalogue in Ask Azu's library; the tool Show me (Suggest) | `test8t` green; new: "how do I see the old palace?" offers Show me on the Before / After item; Tips Off still lets Things to try show |

**The Unity path (for `_3.x` domain planning, not built here):**

- **The data:**
  - `GuidanceCatalog : ScriptableObject` (kinds, groups, items, sequences);
  - `GuidanceItem` is `[Serializable]`, with its words as `CardStrings` keys.
- **The scheduler:**
  - `GuidanceScheduler`, plain C#, `Pick(state, now)`, with EditMode tests on the same scripts as `test9f` / `test9g`;
  - `GuidanceService` (a MonoBehaviour, 4 Hz), with `GuidanceEvents.Raise("card_opened")` from the existing buses
    (`SelectionEventBus`…);
  - `IGuidanceTarget` providers resolve a target to a `VisualElement.worldBound` or a POI's `WorldToScreenPoint`;
  - the memory as JSON in `persistentDataPath`.
- **The presenter:** the UI Toolkit bubble, the spotlight with Painter2D OddEven, and the Azu render-texture element. It
  replaces `CardHintService`.
- **The POI Editor:** a Guidance tab drawing the same rows, and a simulator window on the same `GuidanceScheduler`.

---

## 8. Risks and open questions (each with my recommendation)

1. **Too much talk** (Clippy, Navi). *Recommendation:* ship First visit as defined (≈ 10 pushes in 15 min, 15 s gap) and
   test it with five visitors on a real wall. Keep the Quiet preset one tap away. Lower before raising.
2. **Should the tips be spoken aloud?** In the street, a phone that talks unasked is awkward. *Recommendation:*
   - App and Fun: bubble only, by default.
   - Content: spoken only once the visitor has heard the guide this visit (a tour, an answer) and the sound is on. Else a
     bubble and a soft chime.
   - Ask the developer to listen to both on a real street.
3. **The 70 % as a quota or a weight?** *Recommendation:* a weight. A quota would push tips nobody needs. The done-events make
   app guidance fade on their own, and the simulator shows the real mix.
4. **Visit progress by time, or by experience?** *Recommendation:* active minutes, with returning visitors starting at 5 min.
   Cards opened could be added as a second clock later, if the simulator shows that time alone misjudges a fast visitor.
5. **Removing the pill hurts visitors with the mascot Off.** *Recommendation:* the plain-bubble presenter for Mascot Off (said
   once, at the marker, no character). Test it in R1.
6. **Translation load** (1-4 variants × the languages). *Recommendation:* 1 line by default, 3 for repeated Content remarks
   only, drafted by the existing Draft with AI and approved by a native speaker. A warning when a language lacks a line: it
   is never shown in English instead.
7. **Hints in two places in the Editor** (Card Controls and Guidance). *Recommendation:* one data, two views. The Card Controls
   view says "Edited in Guidance too". A test checks that both views show the same values.
8. **The self-test grows** (about 40 min today). *Recommendation:* the item rows share one demo spec by field, so a new item
   adds few new classes. Accelerate the demos at ×10-×30.
9. **Does 6 · Guide move into a top-level Guide tab one day?** The guide already lives on the wall too. *Recommendation:* not
   now. Revisit when the Guide domain is planned (`_3.4`); then the Guidance tab could become Guide > Guidance.
10. **Location** (for "another wall nearby has a story"). *Recommendation:* out of scope. It needs a location permission and a
    privacy note; propose it with the map feature.
11. **Calm vs Lively and guidance.** *Recommendation:* Liveliness governs only Fun's push and the mascot's gestures. App and
    Content follow Talkativeness. Calm adults still get "Tap a marker".
12. **Who's exploring at the start or on the first card?** *Recommendation:* at the start once the first pages exist. Keep "On
    the first card" as an option (it works with no first pages, e.g. a deep link into a card).
13. **Ask Azu's questions as a source of new tips** (the Ask Mona lesson). *Recommendation:* later, a developer-side report of
    the "how do I…" questions asked, with no personal data. It needs the developer's privacy decision.
14. **Accessibility API in Unity 6.3.** *Recommendation:* verify `AccessibilityHierarchy` on iOS VoiceOver and Android
    TalkBack in a spike before R6's Unity twin is promised.
15. **The tab's name.** *Recommendation:* **Guidance**: it names what it holds (the guide's own initiative, the tips, the first
    pages). "Help" sounds like a help centre, which this is not.
