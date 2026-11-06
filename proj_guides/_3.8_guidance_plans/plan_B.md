# Plan B: one guidance system for the TileStories guide

Planner B, 2026-11-06. Research, model, presentation, merge, Editor, onboarding, build rounds, risks. Nothing in the project was
edited. Where a source was only seen through search results (not opened), it says **(search result)**.

---

## 0. The short answers to the developer's six questions

1. **When does the guide guide, and are App / Content / Fun a good split?** Yes, with two changes. (a) The guide appears from
   the first page of the app and never leaves, but it **pushes** little and is always there to **pull** (the ✦). (b) Keep the
   three kinds the developer named, and add **Care** (look up from the phone, take a break, glare, battery: event-driven,
   outside the mix). The kinds are mixed by a schedule, but **the best item teaches the app through the content** ("Belém
   Tower is in the middle. Tap it and I'll show you around": content that teaches the tap). The model links such an item to
   the app feature it teaches, so one tap closes both.
2. **What do the best products do?** One message at a time, at a pause in the visitor's task, near the thing it is about,
   and gone for good once the visitor has used the feature (TipKit, NN/g, Material, Appcues, Pendo). Global throttles and
   priorities. Help you can pull is always there (a launcher or menu). Characters bring warmth only if they are polite: Clippy
   and Navi failed by interrupting at the wrong moment and repeating themselves. Sources in section 2.
3. **The schedule.** A two-step choice at each **opening** (a pause): first a kind, by weights that move along the visit
   (App 70 % → 15 %, Content 25 % → 60 %, Fun 5 % → 25 %); then the best item of that kind, by importance × recovery ×
   relevance, with the Editor's order as the tie-break. "Along the visit" means **both** time and what the visitor has
   done, whichever is further along. Repeats: each item has its own caps per visit and ever, its own gap, and a **Stop
   when** event (the feature was used) that retires it for good. Global gates and caps sit above all of it. Section 3.4.
4. **The guidance object.** About 25 fields, set at three levels, **Kind → Group → Item**, each level inheriting from the one
   above, written "As the group (…)" as the Editor already writes "As the block (…)". Section 3.2.
5. **Where in the Editor.** A new Detail Card section, **8 · Guidance**, after 7 · Audiences. Not inside 6 · Guide (already
   the longest section, and lists of items at level 3-4 would be lost there). Not a new top-level tab (the tabs are scopes:
   the wall, one point, the framework, the words; guidance is framework-wide). Not an "Other settings" tab (a junk drawer).
   Per-point items go in Specific Marker > Guide script. Presets: First visit, Returning, Family, Expert, Quiet. Section 6.
6. **The rule.** Every row of 8 · Guidance has an On / Off or options, a demo on the phone when changed (`rowDemos`), and the
   self-test's NO DEMO check covers the new section. Every item has **Show now**; the schedule has a **visit simulator** and
   **Show next now**. Tests run as scenes the developer can press. Sections 6.4, 6.5 and 8.

---

## 1. First principles: what a visitor at a tile wall needs

Before any product, the situation:

- **They stand outdoors, in the sun, in a street or a square, often with other people,** with a phone held up at arm's
  length. Arms tire after 20-40 s of holding a phone up. Glare. Noise. Sound is often unwelcome (people around) until chosen.
- **The wall is the point, not the phone.** Each second on the phone is a second not looking at the tiles. Good guidance
  sends the eyes back to the wall ("look at the left corner, the crack from 1755").
- **Most visitors come once.** A tourist uses TileStories for one walk, maybe two. There is no long learning curve to
  amortise. The first 60 seconds decide whether they stay, and "remembered across visits" matters less than "right in this
  visit". Locals, students and guides return: they need a lighter, different welcome.
- **Their needs, in the order they arise:**
  1. *Is it working? Where do I point?* (the wall found or not; the AR state). If this fails, nothing else matters.
  2. *What is here?* (orientation: what this wall is, what is worth looking at).
  3. *How do I get more?* (tap a marker; the card; the ✦).
  4. *Tell me / show me* (a tour, a question), when they are interested.
  5. *Let me look in peace* (stop, pause, quiet).
  6. *Fun, if I am with children or in the mood* (a game, a joke).
  7. *Take care of me* (look up, step back from the kerb, a break, a sunny-screen tip).
- **What follows:**
  - App guidance exists only to unlock content. So **teach the app through content whenever possible**, and teach each
    feature at the moment it is in reach (pull revelation), never as a lesson up front.
  - **One voice.** Today the visitor can meet a wall pill, card hints, a ✦ hint, Azu's comments, Moments, a first-card
    screen and the introduction, each with its own clock. They can collide. One scheduler, one at a time, one memory.
  - **Silence is a feature.** The guide speaks at pauses (the phone still, a card closed, a tour ended), never over the
    narration, never while the visitor is touching or moving, and less and less as the visitor shows they know.
  - **Always pullable.** Whatever was dismissed is one tap away in the ✦.

---

## 2. Research: what leading products teach us

### 2.1 OS-level tips and design systems

- **Apple TipKit** ([DisplayFrequency doc](https://developer.apple.com/documentation/tipkit/tips/configurationoption/displayfrequency),
  [WWDC23 "Make features discoverable with TipKit"](https://developer.apple.com/videos/play/wwdc2023/10229/),
  [session notes](https://wwdcnotes.com/notes/wwdc23/10229)):
  - A global **display frequency** is "the minimum duration after one tip is shown before another tip will become eligible"
    (immediate / hourly / daily / weekly / monthly). A tip can opt out (`ignoresDisplayFrequency`).
  - **Eligibility rules** on parameters and on **events** the app donates ("visited the detail view at least 3 times").
  - **MaxDisplayCount** per tip; **invalidate(reason:)** with `.actionPerformed` (the visitor used the feature),
    `.tipClosed`, `.displayCountExceeded`; invalidation persists. **TipGroup(.ordered)** shows tips of a group in order,
    each waiting for the one before ([secondary summary](https://openskillindex.com/skills/dpearson2699-swift-ios-skills-tipkit),
    search result). Testing switches: show all tips, hide all, reset the datastore.
  - **Teaches us:** the core of our item (eligibility by events, a per-item cap, retire on use, a global gap, ordered groups,
    test switches). We copy this vocabulary almost one to one.
- **Apple HIG, Onboarding** ([opened](https://developer.apple.com/design/human-interface-guidelines/onboarding)): fast, fun,
  optional; teach through interactivity; "a collection of context-specific tips instead of a single onboarding flow", shown
  near the area they refer to; a skipped tutorial is not shown again but stays findable in help or settings; postpone
  nonessential setup; ask for a permission when its reason is clear.
  - **Teaches us:** the app's first pages are short and skippable; the camera permission is asked with its reason, by the
    mascot; Who's exploring is optional; How to use the card stays findable.
- **ARKit coaching overlay** ([ARCoachingOverlayView](https://developer.apple.com/documentation/arkit/arcoachingoverlayview),
  [Unity ARCoachingGoal](https://docs.unity3d.com/Packages/com.unity.xr.arkit@5.0/api/UnityEngine.XR.ARKit.ARCoachingGoal.html)):
  appears by itself whenever the session needs coaching, the app hides its own UI while it shows, and it goes when its goal
  is met. **Teaches us:** "Wall not found" is a **system state that preempts** all guidance; the scheduler is silent while it
  shows (Loading & states already owns it).
- **Material Design, feature discovery** ([guideline](https://m1.material.io/growth-communications/feature-discovery.html),
  search result): a single-tap action gets a **tap target** with hint text; a multi-step one a guided flow of up to three
  parts; prompts work "to the right users at contextually relevant moments", otherwise they are "intrusive and annoying";
  motion shows where the target comes from. **Teaches us:** the spotlight + one line for a one-tap feature; the mascot's walk
  to the target is our "motion shows where it comes from".

### 2.2 NN/g research

- **Onboarding tutorials** ([opened](https://www.nngroup.com/articles/onboarding-tutorials/)): push revelations ("pushy,
  devoid of context, and intrusive") versus pull revelations (triggered by a signal that the user would benefit now);
  "Users want to start using the product right away" (the paradox of the active user); tutorials do not reliably improve
  performance; walkthroughs are justified for novel interactions such as AR; "Make it easy to dismiss (and recall)"; "No
  memorization!". **Teaches us:** prefer pull; push only what every visitor needs (the opening line, pointing at the wall);
  AR is the one place a short first-run walkthrough is justified.
- **Instructional overlays and coach marks** ([opened](https://www.nngroup.com/articles/mobile-instructional-overlay/)):
  "Bombarding users with frequent hint screens causes them to dismiss hints more quickly"; one interaction per tip, one at a
  time, when the user reaches the section; make the annotation visually distinct from the UI. **Teaches us:** a global gap;
  one item, one feature; the mascot's bubble is our distinct annotation layer (it never looks like a button of the app).
- **Help and documentation** ([article](https://www.nngroup.com/articles/help-and-documentation/)) and **banner blindness**
  ([old and new findings](https://www.nngroup.com/articles/banner-blindness-old-and-new-findings/), search result): users
  ignore what looks like an ad or a promotion, wherever it sits, and animation is one of its signals. **Teaches us:** no
  standing pill or banner; no looping motion; the message comes from a character, briefly, about the thing in view.

### 2.3 Product-tour tools (their objects and their throttles)

| Tool | Objects | Targeting, priority, frequency | What we take |
|---|---|---|---|
| **Appcues** ([priorities](https://docs.appcues.com/flows/flow-priorities), [frequency limit](https://docs.appcues.com/user-experiences-studio/experiences-frequency-limit)) | Flows (modals, tooltips, hotspots), checklists, launchpad | One flow per page view; priority by **weight**, then trigger chain, then "Show once" over "Every time", then newest; a **global frequency limit** (e.g. 2 an hour, off by default) with a per-flow override; test mode and manual launch ignore the limit | Weights only where items compete; a global cap with an override for Essential items; pulled items ignore caps |
| **Pendo** ([throttling and ordering](https://www.pendo.io/pendo-blog/introducing-guide-throttling-ordering/)) | Guides, resource center | **Throttling:** a minimum interval between automatic guides; extra ones are **delayed, not dropped**; **ordering**; a guide can override the throttle | The global gap delays rather than drops; an ordered queue |
| **Userpilot** ([order and throttle](https://docs.userpilot.com/in-app-engagement/flows/use-cases/order-throttle.md), [mobile ordering](https://docs.userpilot.com/in-app-engagement/mobile-content/mobile-content-ordering.md)) | Flows, checklists, resource center, tooltips, banners | Ordering when several are eligible; throttling across all, "Ignore throttling" per item; separate throttles per channel | Separate budgets per kind are possible; per-item ignore |
| **Amplitude Guides** ([throttling](https://amplitude.com/docs/en/guides-and-surveys/throttling)) | Guides, surveys | Limits per period, also **per tag**; the most restrictive wins | Caps per group (tag) as well as global |
| **WalkMe** ([ShoutOuts](https://learning.sap.com/courses/getting-started-with-building-walkme-solutions/introducing-shoutouts_ba867a00-cc5e-4cb7-8e85-e3aec2b21133), [Menu](https://learning.sap.com/courses/getting-started-with-building-walkme-solutions/exploring-entry-points-menu_fabd9ff1-656a-4948-9f5b-f7b11108e60d)) | Smart Walk-Thrus, ShoutOuts, SmartTips, Launchers, the Menu | Auto play **Once / Once a day / Always**, cancelled by Close or the action button; segmentation rules; launchers and a menu as entry points | "Stop when" the action is taken; the ✦ is our Menu and launcher |
| **Intercom** ([tour frequency, community](https://community.intercom.com/proactive-support-11/how-to-ensure-that-account-users-see-tour-once-after-feature-launch-then-never-again-892)) | Product tours, checklists, banners, the messenger | "Send once, the first time the person matches the rules" / every time (with a cap and a gap); one tour a session | The visit as the unit; once-ever vs once-a-visit |

**Together they teach:** the object set (tour = walkthrough, tooltip = tip, hotspot = cue, checklist, launcher = ✦); and
the four throttles every one of them converges on: **a global gap, a cap per period, a priority order with weights, and a
per-item frequency with "stop on action"**. Override flags exist for the few essential items. None of them mixes kinds by a
curve over the session: that is our addition, and it must be explainable in the Editor (the simulator, section 6.5).

### 2.4 Mascots and game tutorials

- **Duolingo.**
  - Its **notification engine** is a "sleeping, recovering bandit" ([Yancey & Settles, KDD 2020](https://research.duolingo.com/papers/yancey.kdd20.pdf)):
    templates that are ineligible this round "sleep"; a template's value **recovers** with the time since it was last used
    (a novelty boost). **Teaches us:** the recovery factor in our score; repeated items lose force and regain it with time.
  - **Delayed sign-up**: a lesson before any account, credited with about +20 % daily users in an interview
    ([Taplytics](https://taplytics.com/blog/duolingo-ab-test-onboarding), secondary; [Good UX](https://goodux.appcues.com/blog/duolingo-user-onboarding)).
    **Teaches us:** no account; the profile question can wait; value first.
  - **Its characters** are Rive state machines with visemes and lesson poses ([Duolingo blog](https://blog.duolingo.com/world-character-visemes),
    [Rive on Lily](https://rive.app/blog/duolingo-s-ai-powered-video-call-brings-lily-to-life)): they **react** to what the
    learner does. **Teaches us:** the mascot's best moments are reactions to the visitor, not unprompted speeches.
- **Headspace** ([Good UX](https://goodux.appcues.com/blog/headspaces-mindful-onboarding-sequence), search result): two or
  three questions (why you came, when) shape the first session; critics found the flow long when the answers changed
  little. **Teaches us:** ask only what changes the experience (Who's exploring changes tone and length; sound changes the
  channel), and show the change at once.
- **Pokémon GO's Professor Willow** ([quotes, Bulbapedia](https://bulbapedia.bulbagarden.net/wiki/Professor_Willow/Quotes)):
  the professor hands you Poké Balls and you catch your first Pokémon in AR **before** you are asked a name; then he sets
  tasks. **Teaches us:** the first act is the core act (here: point at the wall, see a marker, tap it), the character frames
  it, the profile comes after.
- **Valve's instructor hints** ([env_instructor_hint](https://developer.valvesoftware.com/wiki/Env_instructor_hint)): each hint
  has a **display limit** (times it can be seen ever), a timeout, a target entity with an on-screen icon and a 100-character
  cap. **Teaches us:** a per-item lifetime cap, a short text cap, the hint pinned to the thing in the world.
- **Hints can hurt** ([O'Rourke et al. 2014](https://grail.cs.washington.edu/wp-content/uploads/2015/08/orourke2014hsm.pdf)):
  in a large-scale study of an educational game, hint systems lowered players' performance and engagement. **Teaches us:** a push hint
  must earn its place; the default is fewer.
- **Navi** ([Wikipedia](https://en.wikipedia.org/wiki/Navi_(The_Legend_of_Zelda)),
  [GamesRadar on Miyamoto](https://www.gamesradar.com/games/the-legend-of-zelda/zelda-creator-shigeru-miyamoto-always-knew-that-navi-was-the-biggest-weakpoint-of-ocarina-of-time-and-once-said-i-wanted-to-remove-the-entire-system)):
  repeated advice at random moments made "Hey! Listen!" a meme; later companions speak when the player asks or in scenes.
  **Teaches us:** never repeat a line word for word; the companion waits to be asked once the player knows.

### 2.5 The Clippy lesson, and the science of interrupting

- **Swartz, "Why People Hate the Paperclip"** ([summary](https://jerz.setonhill.edu/?p=1399),
  [Harvard Crimson](https://www.thecrimson.com/article/2005/4/28/what-not-to-do-with-a/)): agents must leave the user "in
  control" and follow human etiquette; anthropomorphic styles are "cute the first time, silly the second time, and an
  annoying distraction the third time". **Teaches us:** variants (never the same line twice in a row), politeness rules,
  a one-tap "not now".
- **Horvitz, "Principles of mixed-initiative user interfaces"** ([CHI 99](https://erichorvitz.com/chi99horvitz.pdf)): weigh
  the cost of bothering against the value of help; when unsure, offer, do not act; time out; easy ways to reject; remember
  recent interactions. **Teaches us:** our score is an expected-value guess; an ignored item lowers the next one's chance
  (the guide becomes quieter by itself); invitations ("Shall I…?") rather than actions.
- **Adamczyk & Bailey, "If not now, when?"** ([CHI 2004](https://hci.rwth-aachen.de/materials/conferences/CHI2004/1p271.pdf)):
  mental workload drops at subtask boundaries, most after a large chunk; interruptions there cost less. **Teaches us:**
  guidance starts only at **openings** (a card closed, a tour ended, the phone settled), never mid-action.
- **Proactive AI assistants**: in a CHI 2025 study, 53 % of proactive prompts led to engagement, 12 % disrupted, 35 % were
  ignored, and timing at sub-task boundaries mattered ([Pu et al.](https://arxiv.org/pdf/2502.18658)); suggestions given too
  often were used less ([Need Help?](https://arxiv.org/pdf/2410.04596)). **Teaches us:** even good proactive help is ignored
  a third of the time; frequency is the first lever.
- **Habituation** ([Anderson et al., CHI 2015](https://scholarsarchive.byu.edu/facpub/1955); [Vance et al., MISQ 2018](https://misq.org/tuning-out-security-warnings-a-longitudinal-examination-of-habituation-through-fmri-eye-tracking-and-field-experiments.html)):
  a warning whose look changes ("polymorphic") resists habituation in the brain's attention areas. **Teaches us:** rotate
  text variants and the mascot's gesture for any item that repeats.

### 2.6 Museum and city guides

- **Smartify** ([Dezeen](https://www.dezeen.com/2017/10/08/new-app-smartify-hailed-shazam-art-world-technology/)): scan a work,
  get its name, text and audio; save it. **Pull only.** **Teaches us:** the core loop is "point, recognise, tell"; the guide
  should make that loop obvious, not add another.
- **Bloomberg Connects** ([App Store](https://apps.apple.com/us/app/-/id1476456847), [a museum's page](https://mmfa.org/visit/free-digital-guide/)):
  numbered lookups beside works, offline guides, captions, transcripts, font size. **Teaches us:** the visitor chooses; the
  app is calm; accessibility is baseline.
- **Beacon push in museums** ([Australian Museum](https://australian.museum/learn/news/blog/the-year-of-the-beacon-and-staged-strategy/),
  [Cuseum](https://cuseum.com/blog/6-tips-for-leveraging-beacons-in-museums)): proximity pings were made small and easy to
  dismiss so visitors could "keep listening and looking"; frequent pings wear visitors down; most objects need none.
  **Teaches us:** content pushes are for the point **in view and settled**, small, dismissable, few.
- **Streetmuseum, Museum of London** ([PetaPixel](https://petapixel.com/2010/05/24/museum-of-london-releases-augmented-reality-app-for-historical-photos)):
  AR "then and now" photos found by location, also browsable on a map. **Teaches us:** content prompts can say "there is
  something here you can see"; and a fallback without standing there.
- **Google Maps Live View** ([Google help](https://support.google.com/maps/answer/9332056)): use AR only when needed; the app
  reminds walkers to lower the phone and switches to the map when it is lowered. **Teaches us:** the **Care** kind (look up,
  step back, lower the phone) exists in the best AR product, and it is event-driven.

---

## 3. The model

### 3.1 The kinds

| Kind | What it is for | Examples | In the mix? |
|---|---|---|---|
| **App** | How to use TileStories | Point at the tiles; tap a marker; the ✦; the grabber; the Navigation Bar; pause; ask by voice; Settings | Yes, weight falls along the visit |
| **Content** | What is here, what next | The point in the middle; "a tour of 3 min?"; "try Ask about the view"; "another card nearby"; "look at the left corner" | Yes, weight rises |
| **Fun** | Delight, play | Moments (water, cookie); a joke; the treasure hunt invitation; seasonal (a sardine in June) | Yes, small, rises a little; off on Solemn points |
| **Care** (new) | The visitor's comfort and safety | "Arms tired? Lower the phone, I'll keep talking"; "Look up, mind the street"; "The sun is strong: try dark mode"; battery low | **No**: event-driven, but obeys the gates |
| *System* (not in this list) | The app's state | Wall not found, offline, loading (10.13 Loading & states) | **No**: preempts and silences guidance |

- **Merge rule (teaching through content):** any item has a **Teaches** field naming the app features it teaches. A Content
  item "Tap Belém Tower and I'll show you around" teaches *Open a card*. When the visitor opens a card, every item whose
  Stop when or Teaches is *Open a card* is satisfied: the App tip "Tap a marker" is never needed again. When a Content
  item that teaches is eligible, the scheduler prefers it over the bare App item (a 1.3 × relevance bonus, section 3.4).
- **Forms** (how an item shows; separate from its kind): **Tip** (a bubble), **Invitation** (a bubble with one or two chips:
  "Start the tour · Later"), **Cue** (motion on the element itself, no mascot: today's card Hints), **Walkthrough** (ordered
  steps: How to use the card, the first pages), **Remark** (a tappable bubble about the point in view). One item, one form.

### 3.2 The guidance object (every field, type, default, why)

Settings exist at **Kind**, **Group** and **Item** level. A field left as inherited reads "As the group (…)" / "As the kind
(…)" with the inherited value in brackets, as the Editor already does for blocks. Fields marked ◆ are item-only.

**Identity and words**

| Field | Type | Default | Why |
|---|---|---|---|
| Label ◆ | text (Editor only) | "New tip" | The Editor names things, never ids |
| Kind ◆ | App / Content / Fun / Care | the group's | The mix |
| Group ◆ | a group of its kind | the first | Shared settings, caps, order |
| Form | Tip / Invitation / Cue / Walkthrough / Remark | Tip (App), Remark (Content), Tip (Fun) | How it shows |
| Text ◆ | one text per wall language, **1-4 variants**, slots `{point}`, `{year}`, `{count}`, `{button}` | one variant | Habituation: variants rotate, never the same twice in a row |
| Text per audience ◆ | Same for all / a variant set per Family, Explorer, Expert | Same for all | Tone follows the audience |
| Returning text ◆ | text, optional | none | "Welcome back" instead of a full welcome |
| Chips ◆ (Invitation) | up to 2: label + action | "Yes" → the action, "Later" → snooze | One decision, never three |
| Spoken | Never / **When sound is on** / Always | When sound is on | Outdoors, no sound until chosen (Guide study rule 10) |

**What it is about and where it shows**

| Field | Type | Default | Why |
|---|---|---|---|
| Target ◆ | None / a wall point (any, the middle one, a named one) / a marker / an app control (from the registry: ✦, grabber, Navigation Bar, Locate, talk button, Settings mark, Ask about the view pill…) / a card section / a block (by kind or a named one) / a card page | None | Help next to the thing (HIG, NN/g) |
| Target must be on screen | Yes / No | Yes (when there is a target) | No talk about what cannot be seen |
| Where | Any / On the wall / On a card / On a page / In a tour (between steps) | from the target, else Any | The context |
| Mascot place | **Near the target** / At home (beside the ✦) / Walks to the target (on the wall) | Near the target | Pointing has evidence (Li et al. 2019 in `_3.4`) |
| Pointing | On / Off | On | The same |
| Focus | None / Ring / Spotlight (Dim 0.2-0.6) | Ring (App), Spotlight (Walkthrough), None (Content, Fun) | One focal point (D5); dim only to teach a control |
| Bubble action ◆ | None / Open its card / Start its tour / Open Ask about the view / Open the ✦ menu / Open Settings at… / Do the chip | from the target | A tip you can act on at once |

**When it may show**

| Field | Type | Default | Why |
|---|---|---|---|
| Trigger | At an opening (default) / On event X / After idle N s (5-60) / When the target settles in the middle (1.2 s) / Visit start | At an opening | Breakpoints (Adamczyk & Bailey) |
| Needs done | a list of events (each with "at least N times") | none | TipKit's event rules ("after opening 2 cards") |
| Needs not done | a list of events | the item's Stop when | Never teach what the visitor already does |
| Visitor | Everyone / First visit / Returning | Everyone | The returning welcome |
| Audience | All / any of Family, Explorer, Expert | All | Fun mostly for Family |
| Not on Solemn points | Yes / No | Yes (Fun), No (others) | Memorials, churches, 1755 |
| Earliest / latest in the visit | minutes (0-60) | 0 / none | "Expires": app tips stop mattering after 15 min |

**How important, how often**

| Field | Type | Default | Why |
|---|---|---|---|
| Importance | Essential / 4 / **3** / 2 / 1 | 3 | Essential skips the kind lottery and the caps (Appcues, Pendo override); only the opening and Care |
| Importance along the visit | **Flat** / Early / Middle / Late / Custom (3 points) | Flat (App items: Early) | The developer's "importance that changes" |
| Shows per visit | 1-10, or Unlimited | 1 | Most items once |
| Shows ever | 1-20, or Unlimited | App 3, Content Unlimited, Fun Unlimited | TipKit MaxDisplayCount, Valve display limit |
| Gap before it shows again | 30 s-30 min | 5 min | Its own cooldown |
| Stop when ◆ | events: the feature used, the bubble tapped, the chip chosen | the feature it teaches | TipKit `.actionPerformed`; retired for good |
| Teaches ◆ | events | none | The merge rule (3.1) |
| Ideal rate (Content, generated) | per minute, 0.2-6 | from the group | The developer's "ideal frequency"; read as a target, the gates still win |
| On screen | Auto (17 characters a second, 2.5-8 s) / Until tapped | Auto (Tip), Until tapped (Invitation) | Readable; never a time limit that cannot be stopped (WCAG 2.2.1) |

**Group-only fields:** order of its items (the tie-break), **Group weight** (0.5-2, 1), **Group cap per visit** (e.g. 12 for
the remarks on the wall), **Gap between items of this group** (e.g. 7 s for the remarks), **Ordered** (Yes: each item waits
for the one before to be retired, TipKit's TipGroup.ordered; the onboarding and How to use the card).

**Kind-only fields:** On / Off; the kind's line in the mix curve (3.4); defaults for every field above.

**Generated items (templates).** Some items exist once per thing: "the point in the middle" (Azu's remarks), "a card
nearby", each block kind's cue. A template is one item whose Target is "each wall point" or "each block of kind K"; memory
is kept per instance (said about Belém Tower ≠ said about the monastery). This keeps the list short.

### 3.3 The groups (the default library)

```
App
  Opening (Ordered)          · Welcome (Essential, visit start) · Tap a marker (teaches Open a card)
  On the wall                · Ask about the view exists · Move closer / zoom · The ✦ (today's "Need anything? Tap ✦")
  On the card (Cues)         · Grabber nudge · Locate label · Navigation Bar · Before / After · Swipe strips · Underlined words
  The guide                  · Pause anytime · Talk or type · Stop and resume · Captions
  How to use the card (Walkthrough, pulled only) · Grabber · Settings mark · Navigation Bar · Locate · A block and its page
Content
  What's in view (template)  · In the middle: {point} · {point} has a card: tap and I'll show you around
  Suggestions                · A tour of {point}, {min} min? · Ask me about the view · A route of the wall · The oldest here
  After a card               · Another card nearby: {point} · Look for it on the wall (Locate)
  This point's own (per point, Specific Marker) · "Find the crack from 1755, bottom left" …
Fun
  Moments (In a tour, between steps) · Water break · Cookie
  Play                       · A joke · Treasure hunt? (Family) · Seasonal
Care
  Comfort and safety         · Lower the phone (held up 40 s) · Look up near the street (walking with the camera) · Strong sun: dark mode · Battery low
```

### 3.4 The scheduler

**Openings.** The scheduler only starts an item at an opening: the camera settled for 1.2 s on the wall; a card closed; a
card opened (after 1.5 s, the Hints' rule); a page opened; a tour ended; between two tour steps (only items with Where = In
a tour); idle for the item's N s. Between openings it re-checks once a second while idle.

**The gates (fixed rules, no option can break them):**
1. **One at a time.** Nothing starts while a bubble, a cue or a walkthrough step shows.
2. **Never over the narration** (any line being said, from any voice source) or over the visitor speaking.
3. **Quiet during a tour,** except items allowed "In a tour (between steps)" (the Moments), and never over a sentence.
4. **Quiet while the visitor acts:** a touch in the last 2 s, a drag or scroll in the last 0.8 s, the phone (in the Lab: the
   wall) moving in the last 0.7 s, a panel open (Settings, Ask, the ✦ menu, the dock at Full, the first-card screen).
5. **Quiet while a System state shows** (wall not found, offline).
6. **Never over what it is about**, and never over the block being told.
7. A **pulled** item (from the ✦ or a Try) ignores the caps and gaps (Appcues: manual launch ignores the limit), not gates 1-2.

**The global limits (Editor rows, defaults):** Gap between any two **20 s** (from the end of one to the start of the next;
delays, never drops, as Pendo); At most **4 every 5 minutes**; At most **30 a visit**; Care and Essential items skip the
gap but not the gates.

**Choosing at an opening (two steps).**
1. **The stage of the visit, s (0 to 1).** `s_time = minutes in the visit / Visit length (15 min)`, `s_done = the share of
   the milestones done` (opened a card 0.3, heard a tour step 0.2, asked a question 0.2, used Ask about the view 0.15,
   opened a block's page 0.15). **Shift by:** Time / What the visitor did / **Both** (default: `s = max(s_time, s_done)`,
   because a visitor who has done things needs no lessons, and one who has ignored the lessons for ten minutes should
   hear something else).
2. **Pick a kind** by its weight at s, among the kinds that have at least one eligible item (weights renormalised). Default
   curve, three points (s = 0, 0.3, 1):

   | Kind | s = 0 | s = 0.3 | s = 1 |
   |---|---|---|---|
   | App | 70 | 40 | 15 |
   | Content | 25 | 50 | 60 |
   | Fun | 5 | 10 | 25 |

   The draw is random with a seed (the Lab: fixed per run, so tests repeat). Essential items skip this step.
3. **Pick the item** of that kind with the highest score:
   `score = importance(s) × group weight × recovery × relevance`
   - `importance(s)`: 1-4 times its curve (Early: 1.5 at s = 0 to 0.5 at s = 1; Late the reverse; Middle peaks at 0.5).
   - `recovery = 1 − e^(−t/τ)`, t = seconds since this item (or this instance) last showed, τ = its Gap / 2; never shown = 1
     (Duolingo's recovering arms; a fresh item wins over a repeated one).
   - `relevance`: 1.5 when its target is in the middle of the screen, 1.2 on the screen, 1.3 more when it Teaches an
     eligible App item's feature, 0 when Target must be on screen and it is not.
   - Ties: the Editor's order (the group's order, then the item's).
4. **Ideal rate** (generated Content, e.g. the remarks): when the group has a rate, its items are eligible only when the
   group is behind its rate (`shown in the last 5 min < rate × 5`); the group's own gap and cap still hold. This keeps
   "remarks every 7 s at most, 12 a visit" exactly as today.

**Learning from the visitor (On by default).** Ignored (no tap, no action until it goes) **3 in a row** → the global gap
doubles (up to 4 ×) for the visit; an item the visitor taps or acts on → the gap returns to normal. A dismiss (a swipe on
the bubble, or ×) on an App item retires it; on Content or Fun it snoozes its group for 5 min. (Horvitz: lower the cost of
poor guesses; remember recent interactions.)

**What the visitor controls**
- A tap on the bubble does its action; a tap elsewhere or a swipe dismisses it (no extra ×: a 44 pt target would weigh the
  bubble down; the swipe is the system's).
- **Settings > The guide > Tips:** Often (gap × 0.7) / **Sometimes** / Only when I ask (only Essential, Care and pulled).
  **Show tips again** resets the memory.
- **The ✦ menu > Tips and help** (new line under Guide settings): the App items not yet retired, then the retired ones, each
  with **Show me** (pull); and **Show me around** (How to use the card, extended to the wall).
- Under a screen reader: no spoken tips (the guide never talks over VoiceOver / TalkBack); bubbles are announced politely
  and stay until dismissed.

**What is remembered**
- **A visit** ends after 4 h away from the app (an Editor row, 1-12 h). Per visit: counts, the stage, the ignore streak,
  the snoozes.
- **Across visits** (`DeviceStateStore`, on the phone only, never sent): per item and instance, shows ever, last shown,
  retired and why (used / dismissed / cap); the events done ever (opened a card ever → never teach it again); returning
  visitor = a previous visit exists.

---

## 4. Presentation

**The rule of restraint.** One bubble, one ring or spotlight, one mascot gesture, one line. No standing pills, no looping
pulses, no confetti. The mascot moving to the thing is the one motion, and it explains (D6). Every look below has its
Reduce Motion form and its Unity path.

| Part | Look and timing (defaults) | Reduce Motion | Unity 6.3 path |
|---|---|---|---|
| **Bubble** | Opaque paper, 7:1 text (D1), at most 2 lines and 80 characters (the Guide study rule), tail towards the mascot; in 220 ms (scale 0.94 → 1 and fade, ease out), out 160 ms; the spoken word lit (the read-along light); a chip row for Invitations (44 pt) | Fade only | A 9-slice `VisualElement` with a tail sprite; USS transitions on `scale` and `opacity`; the glyph tint by `PostProcessTextVertices` (checked) |
| **Placement** | Beside the target on the side with room (above, below, left, right in that order), inside the safe area, never over the target, the block being told or text; 12 pt from the target | same | Target's `worldBound` → a small layout solver in C# |
| **Mascot near the target** | From its home (beside the ✦) it hops along an arc (0.45 s) to the bubble's side, points (the point clip), and returns after the bubble goes; on the wall it **walks to the point** (the existing walk) | A cut (it appears there), no hop | The mascot's element `translate` tween (`experimental.animation`); 3D: the render-texture element and the Animator's point clip with an aim constraint; on the wall the existing hand-off to the world-space mascot |
| **Ring** | A warm ring around the target, one soft pulse on arrival (600 ms), then still | Still ring | `generateVisualContent` + `Painter2D` stroke, or a 9-slice glow sprite |
| **Spotlight** | The rest dims (0.35; 0.2-0.6), the target's rounded rect cut out; glides 0.55 s when the target moves; on the wall, the camera image dims except the point's pin | No glide | `Painter2D` `Fill(FillRule.OddEven)` (checked), placed each frame (`schedule.Execute`) |
| **Cue** (card Hints) | Unchanged from 10.12 (the nudge, the sweep, the slide, the glow, the label) | As 10.12 | As 10.12 (`CardHintService` becomes the scheduler) |
| **Sound** | **Off** by default. Optional "pip" under 300 ms, 6-10 dB under the voice, only when sound is on and the camera is not the view | none | AudioSource on a mixer group |
| **Haptics** | **None** for anything the guide starts (Guide study 4.5: haptics only on the visitor's own actions); a light tick when a chip is chosen | same | The native haptics plugin (planned) |
| **Variants** | 1-4 text variants rotate; the mascot's gesture varies (point, lean, wave) by item | gesture: none | Animator state per gesture |
| **History** | Every tip also goes to the dock's conversation under "Tips", so a bubble is never the only channel | same | The conversation's `ListView` |

**Premium, modern, minimal, live.**
- *Premium:* the bubble is the same material as the dock and the card; the type is the UI face; the mascot's motion is
  physical (anticipation, a small overshoot, settle).
- *Modern:* no modal overlays except the first pages and an asked-for walkthrough; tips sit in the world next to their thing.
- *Minimal:* by default, about 1 push item a minute early on and 1 every 2-3 minutes later (from the gaps and caps).
- *Live:* texts are templates on live data ("In the middle: {point}, {year}"); a Content item exists only for what the
  camera sees now; the mascot looks at the target before it speaks.

**Accessibility.** 7:1 text; Dynamic Type / `fontScale` reflows the bubble (it grows in lines, never truncates); chips 44 pt;
the bubble announced to the screen reader (Unity 6's `AssistiveSupport` announcement API: to check by reflection, as the
project does); no auto-dismiss under a screen reader; no spoken tip over it; captions log in the dock.

---

## 5. Merge with what exists

| Feature today | Decision | Reason |
|---|---|---|
| **Global Scene > Wall hint** (the dark pill "Tap a marker to open its card") | **Drop the pill. Its words become two App items, said by the mascot:** *Welcome* (Essential, the visit's first item: "Hi! Point me at the tiles. When a marker glows, tap it and I'll tell you its story.", returning: "Welcome back! Tap a marker to begin.") and *Tap a marker* (if no card is opened 20 s after the welcome, the mascot walks to the nearest marker and points: "Tap here."). Both stop when a card opens. The Global Scene row is replaced by one line: "Said by the guide: Detail Card > 8 · Guidance > Opening". | The developer's request; a standing pill is banner-blind and uncoordinated; the mascot can point at the actual marker; one opening message |
| **Card Controls > Hints** (grabber nudge, Locate label, Navigation Bar, Before / After, swipe strips, underlined words) | **Merge** as App > On the card, form **Cue**. Their moments and looks are unchanged; their rules (one at a time, not while scrolling, not in the first 1.5 s, not in a tour) become the scheduler's gates; "Once a visit / Every card / Off" maps to Shows per visit. The rows stay visible in 5 · Card Controls > Hints as a short summary with "Edited in 8 · Guidance" (the element-foldout rule), or move entirely: open question 4. | One memory, one at a time across the wall and the card; `CardHintService` was already the planned one-at-a-time service |
| **How to use the card** | **Merge** as App > How to use the card, form **Walkthrough**, **pulled only** (Settings and the ✦ > Tips and help). Keep its five steps. | Pull help, findable again (HIG, NN/g) |
| **Invite to try / the invitation ladder** | **Keep in the tour** (it reacts to the tour's own step and is never scheduled). Share the presentation (bubble, point, spotlight) and the memory: a gesture done in the ladder retires the matching App cue. | It is not push guidance; it is the tour's dialogue |
| **Moments** (water break, cookie) | **Merge** as Fun > Moments, Where = In a tour (between steps), Lively only. Today's rules (one a tour, one every 5 min, never over a sentence, Water break after 60 s) become its fields. | Same caps, now visible next to the other fun |
| **Azu comments** (remarks on the wall) | **Merge** as Content > What's in view, a **template** per wall point, trigger "settles in the middle 1.2 s", group gap 7 s (4-30), group cap 12 a visit, action open the card / Ask about the view. The 6 · Guide row stays as On / Off with "Edited in 8 · Guidance". | It is content guidance; now it can yield to a more important item |
| **The ✦ hint** ("Need anything? Tap ✦") | **Merge** as App > On the wall > *The ✦*, trigger after 4 pokes or idle 25 s, Lively only, once a visit, Stop when the ✦ menu is opened. "Tap on Azu = Points to the ✦" stays as the pulled form. | Same behaviour, now coordinated |
| **The guide's introduction** | **Keep in Narration** (it belongs to the tour), but it **donates** "met the guide", so the Welcome's returning variant is used and nothing re-introduces Azu. | One memory |
| **Who's exploring** (first-card screen) | **Keep the screen**; add a **Where to ask** row: At the app's start (the first pages) / **At the first card** (today) / Never. | The framework merges options |
| **Loading & states** (wall not found, offline…) | **Keep** as System; the scheduler is silent while a state shows; their Mascot looks stay. | Preemption (ARKit coaching) |
| **Locate label** | Merge as a Cue (above); `seenLabel` stays the rule. | — |

---

## 6. The Editor

### 6.1 Where: Detail Card > **8 · Guidance** (decided)

- **Rejected: inside 6 · Guide.** 6 · Guide already holds Narration, the voice test, the dock, Captions, Focus, Invite to
  try, the Mascot with its sources and styles, Ask about the view, the guide button, Azu comments and Ask Azu. Item lists
  inside it would sit at level 3-4 and be hard to find; the self-test and the row demos already treat it as one block.
- **Rejected: a new top-level tab "Help".** The four tabs are scopes (the wall, one point, the framework, the words), not
  features; a fifth for one feature breaks the pattern and separates guidance from the mascot and audience options it reads.
- **Rejected: an "Other settings" tab with Languages & Terms.** A junk drawer; words already live on their element.
- **Chosen:** a Detail Card section next to 6 · Guide and 7 · Audiences (which it depends on), with the same foldouts, `(?)`
  helps, Aa words per item, row demos and JSON preview. Per-point items go in **Specific Marker > Guide script > This
  point's guidance**. The words live on each item's Aa (the item is their owner, `guid_*` keys), so Languages & Terms'
  Texts check lists them like any other.

### 6.2 The tree

```
Detail Card
└─ 8 · Guidance
   ├─ Guidance ........................ On / Off · Preset [First visit ▾] (reads Custom once edited)
   │                                   Tips for the visitor (their Settings default): Often / Sometimes / Only when I ask
   │                                   Spoken: Never / When sound is on / Always
   ├─ Schedule
   │  ├─ Openings ...................... Camera settled [1.2 s] · Card closed ✓ · Card opened (after 1.5 s) ✓ · Tour ended ✓ · Page opened ✓
   │  ├─ Limits ........................ Gap between any two [20 s] · At most [4] every 5 min · At most [30] a visit
   │  ├─ Along the visit ............... Shift by: Time / What the visitor did / Both · Visit length [15 min]
   │  │                                  Milestones (one row each, its share): Opened a card 0.3 · Heard a tour step 0.2 · …
   │  ├─ The mix ....................... the curve: three lines (App, Content, Fun) over the visit, 3 points each, dragged or typed
   │  ├─ Learns from the visitor ....... On / Off · Ignored in a row before quieter [3] · Quieter by up to [4 ×]
   │  └─ Memory ........................ A new visit after [4 h] away · Remember across visits: On / Off
   ├─ Look
   │  ├─ Bubble ........................ Max characters [80] · Reading speed [17 /s] · Shortest [2.5 s] · Longest [8 s] · Entrance: Pop / Fade
   │  ├─ Mascot ........................ Goes near the target: Hop / Walk / Stays home · Points: On / Off · Gestures vary: On / Off
   │  ├─ Focus ......................... Ring look · Spotlight dim [0.35] · Glide [0.55 s]
   │  └─ Sound and touch ............... Pip: Off / On · Haptic on a chip: On / Off
   ├─ App ............................. On / Off · defaults for its items (Form, Focus, Mascot place, Importance curve, Shows ever…)
   │  ├─ Opening (Ordered) ............. group settings · [items: Welcome ▸ · Tap a marker ▸] · + Add item
   │  │  └─ Welcome ..................... (the item's rows, 3.2) · Aa words · Show now · Why not now?
   │  ├─ On the wall · On the card (Cues) · The guide · How to use the card (Walkthrough)
   │  └─ + Add group
   ├─ Content ......................... On / Off · defaults · What's in view (template) · Suggestions · After a card · + Add group
   ├─ Fun ............................. On / Off · defaults · Moments · Play · + Add group
   ├─ Care ............................ On / Off · Comfort and safety
   └─ Simulator (Editor tools, never in the app)
      ├─ Visitor ....................... First visit / Returning · Audience · Solemn point · Sound on / off
      ├─ Visit script .................. Idle visitor / Busy visitor / Tour lover / Custom (a list: 0:00 open app, 0:40 open a card …)
      ├─ Timeline ...................... Play ×1 / ×10 / ×60 · a scrubber 0-30 min · the strip of what showed, coloured by kind
      ├─ Queue now ..................... a table: item · eligible ✓/✗ and why not · score (its four factors) · next time
      ├─ Show next now · Reset memory · Copy the visit as JSON
      └─ The share per kind per 5 minutes (a small table, the curve's target beside it)
```

### 6.3 Lists: add, edit, reorder, weigh

- A group's items are a list like Specific Marker's blocks: each row shows the label, the form icon, the first variant's
  first words, the importance, a status chip ("would show now" / "waiting 1:20" / "retired") and ^ / v / duplicate /
  delete. Order = the tie-break. **+ Add item** asks the form first (five cards with a picture each), then opens the item.
- Groups have ^ / v (their order within the kind) and a **Group weight** slider.
- Relative weights between kinds are the mix curve; within a kind, importance × group weight; the Essential level is
  limited to the Opening and Care (the Editor says so if another item asks for it).
- **Presets** (one popup that asks first: "Replace the schedule's 11 settings with Quiet? **Replace the schedule** · **Keep my
  settings**", Ctrl+Z undoes; the popup names the count, per the popup rule):

  | Preset | Gap | Per 5 min | Mix at start → end (App / Content / Fun) | Learns | Fun |
  |---|---|---|---|---|---|
  | **First visit** (default) | 20 s | 4 | 70/25/5 → 15/60/25 | On | Lively only |
  | Returning | 30 s | 3 | 30/60/10 → 10/65/25 | On | Lively only |
  | Family | 15 s | 5 | 55/25/20 → 10/50/40 | On | On |
  | Expert | 45 s | 2 | 40/60/0 → 5/95/0 | On | Off |
  | Quiet | 90 s | 1 | Essential and Care only; the rest when asked | — | Off |

  **7 · Audiences > Looks per audience** gets a row **Guidance preset** (Family → Family, Explorer → First visit, Expert →
  Expert), as Character and Liveliness are set there today. The returning visitor shifts to Returning by itself (an Editor
  row: On).

### 6.4 Each row's demo (the developer's rule)

Every row of 8 · Guidance declares its demo with `rowDemos({key: spec})`; the runner and the ring are today's.

| Row(s) | Demo on the phone |
|---|---|
| Guidance On / Off, Preset, Tips for the visitor | The wall, a fresh visit: the first item the schedule picks shows (Off: the phone stays silent and the Editor says "nothing would show") |
| Spoken | The Welcome is shown and said (or shown without voice) |
| Openings | The phone does the opening (settles the wall, closes a card…) and the item that it lets in shows |
| Limits, Learns | The simulator plays the 30 s around the change at ×10 on the phone; the preview bar shows a small countdown to the next allowed item |
| Along the visit, The mix, Milestones | The simulator jumps to the stage where the row matters (s = 0.3) and the item the mix picks there shows; the queue table highlights its kind |
| Memory | Returning visitor: the Welcome's returning words show |
| Bubble, Mascot, Focus, Sound and touch | The Welcome (or the item open in the Editor) shows with the new look |
| Kind On / Off and a kind's defaults | That kind's most important eligible item shows (Off: the next kind's) |
| A group's settings | Its first eligible item shows |
| Every item field | **Show now** with that field's effect, the phone first put where the item belongs (the wall with its point in the middle, the card scrolled to its block, a tour between steps) |
| A word on an item's Aa | That item shows with the word |

The self-test's NO DEMO check is extended from `s7`, `s8` to the new section (`s9`); a row without a demo fails.

### 6.5 How a developer tests the schedule

- **Show now** on any item (ignores caps and gaps, not gates 1-2); **Show next now** (what the schedule would pick now,
  ignoring the gap); **Why not now?** on each item, in words: "Retired: the visitor opened a card", "Its gap: 1:20 left",
  "Its target is not on the screen", "Quiet: a tour is playing", "Its group's cap: 12 of 12".
- **The simulator** plays a visit on the phone itself, with a scripted visitor, at ×1, ×10 or ×60, seeded, and draws the
  strip of what showed. Its scripts are the same buttons the tests press (the developer's rule that tests go through
  controls the developer can press).
- **Guidance scenes** in the guide's test console (`guide/guide-scenes.js` style): Fresh visit on Ribeira; Returning
  visit; During a tour (nothing but a Moment between steps); The busy visitor (opens a card at once: no app tip about
  markers ever shows); The idle visitor (the mix moves to content after 15 min); Solemn point (no fun).

---

## 7. Onboarding at the app's start: one system

**One system, with one extra form.** The first pages are an App group **First run** (Ordered, form **Page**, trigger "first
launch"), the only items allowed to fill the screen. They use the same words, memory, mascot and Editor as every other
item; the scheduler takes over the moment the camera opens.

**The pages (each skippable, at most four, under 30 s):**
1. **Hello.** The mascot walks in on the app's paper: "Hi! I'm your guide to Lisbon's tiles." One button: **Start**. (No
   account, ever, for this.)
2. **The camera, with its reason** (HIG: ask with the reason): "I need the camera to see the tiles. Nothing is recorded."
   → the system's permission prompt. Denied → a page that says how to allow it, and the app still opens the map / list.
3. **Who's exploring?** (when Where to ask = At the app's start): Family / Explorer / Expert, **Skip** (= Explorer), "You can
   change this in Settings". The choice changes the mascot's line at once (Headspace: show that the answer mattered).
4. **Sound?** "Will you hear me? Headphones / Sound on / Captions only". Sets Spoken and the voice; outdoors with no answer,
   captions only.
Then the camera opens and the **Welcome** (the old wall hint) is the first scheduled item. If the wall is not found, the
System state (Wall not found, Mascot look) shows instead and the Welcome waits.

**Other app pages** (a map of walls, a list, a wall's home page, if the app has them) get their own App groups ("On the
map": "Walls near you glow; tap one"), same model, same scheduler. The Lab has no such pages yet; it gets a **First launch**
button on the preview bar to play the First run group on the phone (round 8).

---

## 8. Build plan for the Lab, in rounds

Each round: its module(s) per `PROTOTYPE_STRUCTURE.md` (one home per feature, named extension points, no wrappers), its
tests in `tests/`, `tests_baseline.json` updated, the self-test `bad=0`, and `_3.3` updated the same round.

| Round | Adds | Proves (tests) |
|---|---|---|
| **R1 · The wall hint becomes the mascot's opening** | New module `guide/guidance.js` (state `S.guidance`: kinds, groups, items; a minimal scheduler with gates 1-5 and the global gap); the **Opening** group with *Welcome* and *Tap a marker*; the bubble from Azu beside the ✦ (`azBubble`), pointing at the nearest marker; the `.walltip` pill and the Global Scene row removed (the row becomes a link line); 8 · Guidance with On / Off, the two items' Text (Aa, en + pt), Spoken, Show now; their `rowDemos`; the `wall_tip` key retired and two `guid_*` keys added (owner: 8 · Guidance) | `test9e`: a fresh visit on Ribeira shows the Welcome within 1.5 s of the wall settling, no `.walltip` exists; after a card opens, *Tap a marker* never shows that visit; 20 s with no card → the mascot points at a marker; a returning visit uses the returning words; under Reduce Motion no hop; the Welcome never starts while the tour talks; the self-test passes with the new rows (NO DEMO covers `s9`); `test8s`/`test9d` still pass |
| **R2 · The scheduler core and its tools** | Openings, all gates, limits (gap, per 5 min, per visit), per-item caps, gap, Stop when, retire, the visit and cross-visit memory (`DeviceStateStore` in the Lab: `localStorage` in try/catch); **Show next now**, **Why not now?**, the Queue table; the ✦ hint and Azu comments moved in as items (behaviour identical) | `test9e` grows: a retired item never returns; the global gap delays (never drops) a second item; the cap per 5 min holds; Azu comments keep 7 s / 12 a visit; the ✦ hint once a visit; `test8s`, `test9d` unchanged |
| **R3 · The mix along the visit, presets** | The stage (Time / Done / Both), the mix curve row (three lines, 3 points), the score (importance curve, recovery, relevance), seeded draws, the simulator (scripts, timeline, ×1/×10/×60, the strip, the share table), presets with the ask-first popup, the Guidance preset per audience | With seed 1 and the Idle visitor: App ≥ 60 % of push items in the first 3 min, ≤ 25 % after 12 min; the Busy visitor: no marker tip; each preset applies its values and reads Custom after an edit; each popup button tested on its own (the popup rule) |
| **R4 · The card Hints join** | The six card cues and How to use the card as App items (Cue, Walkthrough); the Hints' rules become gates; 5 · Card Controls > Hints shows a summary with a link (or moves: open question 4) | `test8g` passes unchanged (same moments, same looks); a cue and a wall tip never overlap; the global gap holds across wall → card |
| **R5 · Content guidance** | Suggestions (tour offer, Ask about the view, route, oldest), After a card (a nearby card, Locate), the Teaches link, templates per point, Specific Marker > Guide script > This point's guidance | A Content item that teaches Open a card is chosen over the bare App tip; opening a card retires both; a per-point item shows only on its point; no Content item names a point not on the screen |
| **R6 · Fun and Care** | Moments moved in; Play (joke, treasure hunt invite, seasonal); Solemn; Care (lower the phone after the wall is held 40 s, strong sun → dark mode, battery) | Moments keep their rules (`test8k`); nothing fun on a Solemn point; Care skips the gap but not the narration |
| **R7 · The visitor's controls** | Settings > The guide > Tips (Often / Sometimes / Only when I ask) and Show tips again; the ✦ > Tips and help list with Show me; Show me around; Learns from the visitor; dismiss by swipe; the Tips history in the dock | Only when I ask: only Essential, Care and pulled items show; three ignored → the gap doubles; a pulled tip shows even with the caps reached; Show tips again resets |
| **R8 · The first pages** | A First launch button on the preview bar; the First run group (Page form): Hello, camera with its reason, Who's exploring (Where to ask row), Sound | Each page skippable; Skip = Explorer; the camera denied path; the Welcome follows the first pages; Who's exploring at the first card does not show again when answered at the start |
| **R9 · Polish and the Unity path** | Gesture variants, text variants rotation, the pip, the chip haptic, screen reader announcement wording, `_3.3` section on guidance with each Unity path, the Lab's record copy and publish | Never the same variant twice in a row; every bubble 7:1 light and dark (measured); Reduce Motion: nothing moves |

**Unity (after the Lab):** a `GuidanceLibrary` ScriptableObject (kinds, groups, items) edited in the POI Editor's 8 ·
Guidance; a `GuidanceService` (plain C#, one per scene) with the scheduler; an event bus where features **donate** events
(`Donate("card.opened")`, TipKit's verb); a **target registry** (UI Toolkit elements by `name` / a `guide-target` class,
wall points by their anchors through `WorldToScreenPoint` → `RuntimePanelUtils.ScreenToPanel`); the memory in
`DeviceStateStore`; EditMode tests for the scheduler's math (seeded), PlayMode tests for the gates; PlayMode Live Config for
Show now from the Editor.

---

## 9. Risks and open questions (each with a recommendation)

1. **Too talkative (Clippy, Navi).** *Recommend:* the First visit defaults above (about one push item a minute at first,
   fewer later), Learns from the visitor On, every App item retired on use, and a field test with 5 real visitors at a wall
   before raising any number.
2. **Time or progress for the shift?** *Recommend:* Both (`max`), so the busy visitor skips lessons and the idle one is not
   lectured forever.
3. **Randomness vs testability.** *Recommend:* seeded draws in the Lab and in Unity's tests; the simulator shows the seed;
   Show next now is deterministic for a given seed.
4. **Card Hints: keep their rows in 5 · Card Controls, or move them?** The element-foldout rule says a word lives on its
   element; the system says one list. *Recommend:* the items live in 8 · Guidance (one list, one memory), and each element's
   foldout (Navigation Bar, Locate Button…) shows a one-line "Its tip: Navigation Bar · Once a visit · Edit" link. The
   developer decides.
5. **Speaking tips outdoors.** *Recommend:* When sound is on (the visitor chose headphones or sound, or started a tour with
   sound); never by default on the first page.
6. **Live AI writing push remarks.** Tempting ("Azu comments" from the model). *Recommend:* push items are authored
   templates only (no invention, no cost per remark, no latency); the agent answers only when asked. The Editor may offer
   **Draft with AI** for the texts, as Audiences does.
7. **Authoring load of per-point content.** *Recommend:* templates on the wall's data (the middle point, a card nearby, the
   oldest) cover every wall at zero cost; per-point items are optional extras.
8. **The 70 / 30 numbers are guesses.** *Recommend:* treat them as Lab defaults marked (Lab), judged in the real scene
   (`00-process.md`), and log in the Lab's simulator what a visit would get.
9. **Care items could feel nannying.** *Recommend:* only Lower the phone and Look up on by default, each once a visit;
   battery and sun On only after a field test.
10. **Who's exploring at the start vs the first card.** *Recommend:* the default stays At the first card (value first:
    Pokémon GO, Duolingo); At the app's start is the option for audiences that need it (schools, families).
11. **Privacy of the memory.** *Recommend:* on the phone only (`DeviceStateStore`), never sent; Show tips again clears it.
12. **The ✦ menu grows a fifth line (Tips and help).** *Recommend:* yes, last, under Guide settings; it is the pull half of
    the whole system and the answer to "where did that tip go?".
