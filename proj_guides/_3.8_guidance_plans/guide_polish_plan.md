# The AI guide: the polish plan (2026-11-11)

The developer's direction (2026-11-11): work only on the AI guide domain (Azu, the tour, the guidance and its games, Ask
Azu, the voices, the panel) until it is finished and polished: great features, great design, great Editor organization.
The rest of the app only where the guide meets it (where it appears, the app's structure, the Editor tab's place).
Parked, not in this plan: the card's seven missing phone effects, the system events, Global Scene's rows (kept as
headings), the card blocks' stand-ins.

## 1. Where it stands (the inventory, build 14121983)

The Guide tab has **649 rows** in six sections:

| Section | Rows | What is in it |
|---|---|---|
| 0 · Preview & Test | 12 | Show next now, Why not now?, Today is, Start the app, The visitor (a paragraph of counts and a 30-row table), the visitor's switches (Tips, Tips stay, Games on the tour, Screen reader), Log, Simulate a visit |
| 1 · The tour | 47 | Narration (11), Voice test, Guide panel (14), Guide button, Captions, Focus, Invite to try, Tour controls (0 rows), Resume button, Hero button, Guide block |
| 2 · Mascot | 19 | one foldout "Mascot" inside "Mascot": the character, its moves, the 3D model's tools, and two link rows (Moments, Water break after) that only point to Fun › On the tour |
| 3 · Ask | 31 | Ask panel (1 row), Ask about the view (6), Ask Azu (24: the visitor's side, the model, the key, the cost and the knowledge mixed) |
| 4 · Guidance | about 540 | Overall, App, Content, Fun (Play and On the tour), Sequences; each item opens a full form of 30-39 rows |

## 2. The Editor's organization: what is wrong

1. **Each item is a 30-39 row form, every field at once.** A joke shows Identity, Words, About, Show, When, Only if,
   Limits, all open. The author's few real choices (what it says, when, for whom) drown.
2. **Words twice.** An item's Words table (EN | PT, editable) and its Says list (the same lines, with ▸ and Remove)
   show the same lines one above the other.
3. **Names that no longer say what is inside.** Guidance: "when the guide speaks up: a tip at a pause" (it now holds
   games, walkthroughs and the first pages). Fun: "a bubble only, never spoken" (On the tour's games are spoken, on a
   beat card). Mascot › Mascot. Ask › Ask panel with one row.
4. **The games are four levels deep.** Guide › 4 · Guidance › Fun › On the tour › the game: the guide's biggest new
   system sits under a section about tips.
5. **Ask Azu mixes three audiences:** what the visitor sees (Spoken answer, Link cards, Trace), the engine (Model,
   Gemini model, Claude tier, key, Timeout, Model steps, Cost) and the knowledge (App context, Photo, Targets, Library).
6. **Preview & Test** opens with a wall of counts; the visitor's switches sit among the tools.
7. **Editor tools inside the app's sections:** the Voice test (a blind test) is in The tour; the 3D model's Import and
   Generate are in Mascot.
8. **A point's own guide text is in another tab** (Specific Marker › Guide script: the steps, the quiz questions, the
   guess, the word, the find, the jokes) with no way there from the item that uses it.
9. **Empty or pointer-only foldouts:** Tour controls (0 rows), Moments and Water break after (links only), Help and Mix
   (tables without rows).

## 3. The proposed Guide tab

```
Guide
├─ 0 · Preview & Test            (Editor tools, never in the app)
│    Start the app · Today is · Show next now · Why not now? · Log
│    The visitor: one line (visit, stage, heard the guide) and a ▸ Details foldout (the table)
│    The visitor's settings (stand-ins): Tips · Tips stay · Games on the tour · Screen reader
│    Simulate a visit · Voice test (moved here)
├─ 1 · Azu                        (the character)
│    Look (Character, Mascot source, Model) · Size · Liveliness · Mouth · How it moves · Moods · Tap on Azu
│    Introduction · Walks to the wall
│    Model tools ▸ (Play a clip, Import…, Generate from a picture)
├─ 2 · The tour                   (the story told)
│    Narration (voice, speed, music) · Captions · Focus · Invite to try
│    Guide panel (Look, heights, its controls: Tour controls' words move in here)
│    Ways in: Guide button · Hero button · Guide block · Resume button
├─ 3 · Games on the tour          (promoted from Guidance › Fun › On the tour)
│    Rhythm and the plan line (one beat per 75 s, the cap, Short / Deep)
│    The quiz (the promise and the finale as one item) · Guess the year · Don't say it! · Find it on the wall
│    The breather (with the Cookie as its variant)
├─ 4 · Azu speaks up              (today's Guidance: tips, remarks, play, walkthroughs)
│    Overall (presets, Talkativeness, limits, memory) · App · Content · Play · Sequences
└─ 5 · Ask Azu
     What the visitor sees: Answers from · Ask about the view · Suggestions · Spoken answer · Voice · Link cards · Trace
     Knowledge: App context · Photo of the view · Targets · Library · General knowledge · Actions in the app
     The engine ▸ (Guide mode, models, tier, key, timeout, steps, cost, questions a session)
```

**Each item's form:** the header line (on, name, when, Show now) and three groups open: **Says** (one list: each line
with EN | PT side by side, ▸ and Remove; the Words table and the Says list merged), **When** (the moment and its
numbers), **Who** (audiences, Lively only, Calm too). The rest (About, Look on the phone, Only if, Limits, Done when)
under one **More** foldout with a count of the changed fields ("More · 2 changed"). A per-point field shows the point's
value and a link: "This point's word: palace (Reviewed) · Specific Marker › Guide script ›".

## 4. The design pass

Every guide surface captured at 390 px, light and dark, in Ink glass and one other Look, at every dock height where it
shows, then judged (one focal point; 0 px over words being told; 44 px targets; 4.5:1 text):
the ✦ and its menu; Azu beside the ✦, on the wall, on the card, at the beat card's edge; the panel's four Looks at
Mini, Compact and Open; captions; the focus (Glint, Ring, Spotlight); every bubble form (Tip, Invitation, Remark,
Moment); every beat card (promise, quiz, guess, word game, find, breather, cookie); Ask (typed, voice, the answer with
its link cards); the first pages. The open look questions from rounds 1-3 belong here: round 1's PT opening line,
what "Mascot Off" means, Azu's size beside the marker, The ✦ on the card, a ring for The ✦ with the 3D model, "The"
mid-sentence in the remark lines, a group's own shorter gap, the remarks counting in Content's cap.

## 5. The features to finish

From the research (`research_tour_entertainment.md`, `_3.8_Guidance_Study.md`) and the rounds' open ends, to be
listed and ordered with the developer after the Editor and the design passes: what the guide still cannot do that a
real guide does. **Accepted by the developer (2026-11-11), all four:** a tour resumes on the next visit where the visitor
stopped; each step in a short and a deep version ("tell me more" / "skip ahead", the visitor's own pace); Ask Azu during
a game (the game pauses, the answer, the game goes on); a point with no written tour (Azu's short spoken summary from
the card). More may come from the design pass.

## 6. The order

1. **Round 14: the Guide tab's organization** (section 3): one home per row, nothing lost (every row's key, demo and
   test kept; the self-test and test8u / test9* find each row by its key, not its place).
2. **Round 15: the design pass** (section 4), fixing as it goes.
3. **Round 16 onwards: the features**, one or two a round, in the developer's order.
