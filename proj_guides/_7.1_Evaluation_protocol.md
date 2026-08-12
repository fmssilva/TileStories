# TileStories — Practical On-Site Evaluation Protocol
### A step-by-step implementation guide (Phase 5, and any earlier formative rounds)

This is the operational document — what you actually do, in what order, with what
materials, on the day. It resolves the open questions you raised (pre/post overlap,
forced vs. free tasks, incentives, in-app vs. separate surveys, photo consent) with a
concrete recommendation for each, plus the reasoning, so you're not deciding this live
on-site.

---

## 0. Before you go — one-time setup (do this in Phase 0, not Phase 5)

- [ ] **Ethics approval submitted and granted.** This covers *both* the research protocol
  (what you measure, consent, minors) *and* the fact that you're intercept-recruiting in
  public space. Ask your supervisor early whether FCT's process also requires you to
  separately notify **Câmara Municipal de Oeiras** — Chafariz Velho and Alto de Santa
  Catarina are both council-maintained public sites, and if you plan to stand there
  repeatedly with a tablet, a small sign, or take photos of visitors, some councils want
  a heads-up even for non-commercial academic work. Worth a 10-minute email early rather
  than finding out on-site.
- [ ] **Printed materials ready:** one-page participant information sheet (what this is,
  how long it takes, what's collected, that it's anonymous, your contact info), and a
  short consent script (see §3).
- [ ] **A visible badge/lanyard** with your name, "NOVA FCT — MSc research," and ideally a
  supervisor contact — people are far more willing to stop for someone who's clearly not
  selling something.
- [ ] **Pilot the knowledge check and the exit survey on 2–3 friends/colleagues first.**
  If everyone gets 100% before even seeing the app, or nobody remembers anything
  afterward either, the questions need adjusting before you burn real participants on
  them.
- [ ] **Decide and freeze the instrument versions** (exact SUS wording, exact UEQ-S
  wording, your exit survey items, your knowledge-check items) — don't tweak instruments
  mid-collection, or your pre/post and cross-site comparisons stop being comparable.

---

## 1. Session log — fill this in for *every* session, before anything else

Keep this as a simple spreadsheet, one row per session. This is what lets you later say
"here's what our sample actually looked like" instead of guessing.

| Field | Example |
|---|---|
| Session ID | `CV-014` (site prefix + number) |
| Site | Chafariz Velho / Alto de Santa Catarina / MNAz |
| Date | |
| Time of day | Morning / Afternoon / Sunset / Evening |
| Weather | Sunny / Overcast / Windy / (avoid rain sessions — tracking + comfort both suffer) |
| App/framework version or phase | e.g. "Phase 2 build, circuits enabled" |
| Device used | Samsung tablet (note if this ever changes) |
| Researcher(s) present | You alone / You + helper |
| Session start–end time | |
| Any technical issues during session | |

Log this **even for sessions that fail or get abandoned** — a failed session is data too
(it tells you about robustness), don't just discard it silently.

---

## 2. Recruitment — the 30-second approach

Standing near (not blocking) the wall, approach individuals or small groups who've
already paused near it, not people mid-walk elsewhere. A simple script:

> *"Hi, sorry to interrupt — I'm a Master's student at NOVA doing research on an app
> that adds stories to this wall using augmented reality. Would you have about 15
> minutes to try it and give some quick feedback? It's anonymous, and there's no
> obligation to finish if you don't want to."*

Practical notes:
- Approach people who've **already shown interest** in the wall (stopped, looked,
  photographed it) — much higher acceptance rate than cold-approaching passers-by.
- If they say no, thank them and move on immediately — no follow-up pressure.
- For groups (families, couples): it's fine to run one session with multiple people
  interacting, but log it as one session with multiple participants, and try to get
  each person's own survey responses separately if age-appropriate.

---

## 3. Consent — two separate asks, not one bundled one

**Adults:**
> *"Before we start — this is anonymous, I won't record your name, only a session
> number. I'll ask you to try the app, answer a couple of very short quizzes and
> questionnaires afterward, and it should take about 15–20 minutes total. You can stop
> at any point. Is that okay?"*

**Photo consent — ask separately, explicitly, opt-in, never bundled:**
> *"Would it also be okay if I take a photo of you using the app, for my thesis report?
> Totally optional, and separate from the rest — happy to continue either way."*
Keep a simple checkbox for this on your session log. If they say no, don't take the
photo — not even "just of your hands."

**Minors (family/child profile):**
1. Ask the accompanying adult first, using the adult script above, and explicitly
   mention the child will be asked too.
2. Then ask the child directly, in simple language:
   > *"Would you like to try a phone game about this wall? You can stop whenever you
   want, it's totally fine."*
3. Photo consent for a child requires the adult's separate explicit yes — never assume
   the general consent covers it.
4. The child should never be separated from the responsible adult during the session.

---

## 4. Context questions (collect right after consent, before anything else)

Keep this to under a minute — 4–5 short questions, not a form:

1. Are you a tourist or a local resident? *(or: how far did you travel to be here today?)*
2. Approximate age band: `<18 / 18–24 / 25–39 / 40–59 / 60+`
3. Is this your first time seeing this wall?
4. Are you here alone / as a couple / with family / with friends?
5. *(Optional, only if relevant to a specific RQ)*: any prior interest in local history?

This is what lets you later report your sample's actual composition honestly (the thing
the jury will ask about) instead of just claiming "a mix of visitors."

---

## 5. Knowledge check — pre and post: the design decision, resolved

**Your instinct about the overlap problem is correct — it's a real, known confound**
(the "testing effect": seeing questions beforehand primes attention during the visit,
inflating the apparent learning gain). Here's the trade-off, and the recommendation:

| Approach | Pros | Cons |
|---|---|---|
| **Identical questions pre/post** | Simple, directly comparable, easy to analyze | Testing effect inflates the result; you're partly measuring "did they remember the quiz" not "did the app teach them" |
| **Split pool (your 1/3 pre, 2/3 post idea)** | Reduces direct priming | With N≈25, splitting further shrinks an already-small sample per item; introduces item-difficulty variance between the two subsets, which you can't control for without piloting at a scale you won't reach |
| **Two parallel forms, counterbalanced** | Textbook-correct | Requires validating the two forms are equally hard — not feasible at this N either |

**Recommendation: use the same short set (5–6 items), and disclose the testing-effect
confound explicitly as a limitation**, rather than engineering a split/parallel design
you can't properly validate anyway. This is consistent with how you've framed the rest
of the evaluation (exploratory, not statistically rigorous) — adding false rigor to just
this one instrument would be inconsistent, not an improvement. One cheap mitigation:
reorder the items and lightly reword the phrasing (not the content) between pre and
post, so it's not a literal copy-paste — reduces rote memorization slightly without the
cost of a full parallel-forms design.

**Example item structure** (adapt per wall — Chafariz and the mural have different
content, so you need a separate short set per site):

1. *What year, roughly, is this wall/structure from?* (multiple choice)
2. *What historical event or purpose is this wall most associated with?* (multiple choice)
3. *True/False: [a specific fact the app surfaces about one featured POI]*
4. *Which of these did NOT survive/is NOT still present today?* (multiple choice, tests
   the status/state-axis content specifically — ties directly to RQ5)
5. *Free-text, one line: name one thing you learned from the wall today.* (pre-session:
   "name one thing you already know about this wall")

Always include a **"not sure / don't know"** option — forcing a guess adds noise you
can't distinguish from real (lack of) knowledge.

---

## 6. Free exploration — forced vs. free, resolved by phase

Your instinct to distinguish development phases from the final evaluation is exactly
right — this is the standard **formative vs. summative** split your report already
sets up in §3.3, just apply it explicitly to this question:

- **Formative testing, throughout development (small groups, 5–8 people, think-aloud):**
  it's completely fine — even better — to give directed tasks ("find a POI from before
  the earthquake," "try starting a circuit"). You're hunting for breakages in specific
  features, not measuring authentic behaviour, so guiding attention is useful here.
- **Final summative evaluation (Phase 5, the 20–30 participant round):** keep it **fully
  free, no forced tasks, no minimum time.** This one directly matters for RQ3, which
  asks about *voluntary, real-world* engagement patterns — forcing a task here would
  contaminate the exact thing you're trying to measure. If circuits/games/profiling
  exist by then, let the participant discover and choose them (or not) on their own;
  whether they engage with those features *unprompted* is itself a finding, not
  something to short-circuit by instructing them to use it.

During free exploration:
- Stand back, don't hover. Only intervene if they're stuck on something technical (app
  crash, tracking lost) — not on content or navigation choices.
- Don't answer "what should I look at next" — redirect gently: *"Whatever looks
  interesting to you is exactly what I want to see."*

---

## 7. Observation — what to actually write down

This is separate from SUS/UEQ-S/exit survey (those are self-report, filled in *after*).
Observation is your own real-time notes *during* the free-exploration phase. Keep a
simple structured note, not free prose, so you can compare across sessions later:

| Timestamp | What happened |
|---|---|
| 0:45 | Struggled to get tracking lock, tried 3 angles |
| 2:10 | First POI tapped — read full card, ~20s |
| 3:30 | Noticed and opened timeline unprompted |
| 5:00 | Skipped several small dot markers, went straight to labelled ones |
| 7:15 | Asked "can I go back to that first building?" |

This, plus your passive analytics log (dwell time, POI taps, circuit completion — comes
from the app automatically), together answer RQ3 far better than a survey question
ever could — the survey tells you how they *felt*, this tells you what they *did*.

---

## 8. Post-session instrument order (do this after free exploration ends)

Run in this order — knowledge check first, while the visit is freshest, before fatigue
from filling in questionnaires sets in:

1. **Post knowledge check** (~2 min)
2. **SUS** — 10 items, ~5 min
3. **UEQ-S** — 8 item-pairs, ~3 min
4. **Exit survey** — 5 items, ~2 min
5. *(Time-permitting only)* **Semi-structured interview** — 5–10 min

Total: ~15–20 minutes per participant including the free-exploration phase. Budget
accordingly when planning how many sessions you can realistically run per outing.

### SUS — exact standard wording (do not alter)
1. I think that I would like to use this system frequently.
2. I found the system unnecessarily complex.
3. I thought the system was easy to use.
4. I think that I would need the support of a technical person to use this system.
5. I found the various functions in this system were well integrated.
6. I thought there was too much inconsistency in this system.
7. I would imagine that most people would learn to use this system very quickly.
8. I found the system very cumbersome/awkward to use.
9. I felt very confident using the system.
10. I needed to learn a lot of things before I could get going with this system.
(5-point scale: Strongly disagree → Strongly agree)

### UEQ-S — the 8 official item pairs (7-point scale between each pair)
obstructive–supportive · complicated–easy · inefficient–efficient ·
confusing–clear · boring–exciting · not interesting–interesting ·
conventional–inventive · usual–leading edge
*(First 4 = pragmatic quality, last 4 = hedonic quality — report both subscores, not
just the average, since that split is exactly your usability-vs-engagement story.)*

### Exit survey — example items (yours to write, adapt freely)
1. Overall, how satisfied were you with this experience? (1–5)
2. Did the content feel relevant to what interested you? (1–5) *(ties to RQ4)*
3. What was your favourite part or feature?
4. Was anything confusing or frustrating? (open text)
5. Would you recommend this to a friend visiting the site? (Yes/Maybe/No)

---

## 9. Semi-structured interview (only if time and willingness allow)

Keep it to 3–4 open prompts, not a script to read verbatim:

- "Can you tell me a bit about your experience just now?"
- "Was there a moment that stood out, good or bad?"
- "If you could change one thing about it, what would it be?"
- "Did anything feel like it wasn't 'for' someone like you?"

Paired interviews (e.g. a couple together) can reduce the social pressure of a 1-on-1
interview — fine to do if a group is willing together.

---

## 10. Incentives — practical answer

Cash incentives (your €5 idea) are usually genuinely hard to get approved through a
university without a funded project behind them — petty cash and reimbursement rules at
FCT typically require a budget line that a Master's dissertation plan doesn't have.
Worth a short email to your supervisor/coordination office to ask, but don't count on
it, and **you don't need it ethically** — a short voluntary street intercept doesn't
require payment to be ethical.

**Cheaper, easier-to-approve alternatives that still work well:**
- A small printed TileStories postcard/sticker (costs cents, doubles as a nice memento
  and quiet marketing for your project)
- Offering to email them a photo from the session (if they consented to photos)
- Simply thanking them warmly and telling them their input directly shapes a real
  museum tool — many people genuinely enjoy this on its own

If you *do* get incentive budget approved, keep it modest and non-coercive, and never
offer cash incentives directly to a minor — route anything through the accompanying
adult if it happens at all.

---

## 11. In-app surveys vs. separate tool — recommendation

**Don't build SUS/UEQ-S/exit-survey UI into the production app.** Three reasons:
1. It's scope creep against your already-tight six-month plan — building good survey UI
   is its own small project.
2. Standardized instruments (SUS, UEQ-S) need to be presented in their exact validated
   wording and format — building a custom in-app version risks accidentally changing
   what's being measured.
3. If the survey UI itself is clunky, you contaminate your usability score with the
   survey's usability, not just the app's.

**Instead:** administer all four instruments on a **separate device or paper form** you
control directly (a simple Google Form on your own phone/tablet works fine, or paper if
you prefer no digital step at that moment). This keeps "the thing being evaluated"
cleanly separate from "the tool doing the evaluating."

A **lightweight, optional in-app "quick feedback" button** (a single tap, maybe a 1–5
star rating) is a fine *stretch* feature for later, general use beyond the formal study
— but treat it as separate from your Phase 5 evaluation instruments entirely.

---

## 12. Data handling

- Session log, survey responses, and analytics are keyed only by session ID — never
  name.
- Store consent confirmation as a simple checkbox/log entry, not a signature with
  personal details, unless your ethics approval specifically requires signed paper
  consent (confirm this when you get the approval back).
- Photos (where consented) stored separately from survey data, referenced only by
  session ID, never captioned with a name.
- Keep raw data only as long as needed for the thesis; state your retention plan in the
  ethics submission.

---

## 13. Contingency plan

- **Tracking fails on-site:** don't force it — log it as a failed session (§1), thank
  the participant, offer to let them see the browse-mode fallback instead if you have
  one, and move on. Don't waste 10 minutes debugging live in front of a volunteer.
- **Bad weather:** reschedule rather than run a rushed, uncomfortable session — data
  quality (and honestly, the wall's own tracking reliability under rain/glare) both
  suffer.
- **Nobody's stopping:** move recruitment to a different time of day before assuming the
  site is a dead end — you already flagged sunset at Alto de Santa Catarina as likely
  better than midday.
- **A session runs long and a queue forms:** it's fine to politely shorten the interview
  step for that session (drop straight to a thank-you) rather than rush the core
  instruments.

---

## 14. Quick "what to bring" checklist for the day

- [ ] Tablet, charged, app installed and tested that morning
- [ ] Badge/lanyard
- [ ] Printed info sheet (a few copies)
- [ ] Session log (spreadsheet on your phone, or paper backup)
- [ ] Survey form ready (Google Form link or paper copies)
- [ ] Backup battery pack
- [ ] Water / sun protection if outdoors midday
- [ ] Small tokens (postcards/stickers) if you're using them

---

## 15. What each instrument ultimately feeds into (for your analysis chapter later)

| Instrument | Feeds |
|---|---|
| Context questions | Sample description, subgroup framing |
| Pre/post knowledge check | RQ5 (indicative learning signal only) |
| Observation notes + analytics | RQ3 (engagement patterns) |
| SUS | RQ1/RQ2 (usability, legibility at scale) |
| UEQ-S | RQ3 (engagement/hedonic quality) |
| Exit survey | RQ4 (personalisation fit), general satisfaction |
| Interview | Qualitative depth across all RQs |

---
---

# One-slide version — "Evaluation Cycle" (backup slide, show only if asked)

A simple 6-step horizontal flow, minimal text per step — this is what goes on the
actual slide, everything above is what you say if someone clicks into it:

```
1. CONSENT          2. PRE-CHECK        3. FREE USE
   + context            (~2 min)            No forced time
   (~1 min)                                 or tasks

4. POST-CHECK        5. SURVEYS          6. (Optional)
   (~2 min)             SUS + UEQ-S +       Interview
                        Exit (~10 min)      (~5-10 min)

Throughout: passive analytics (dwell time, POIs, circuits)
            + researcher observation notes
```

**One-line script if this slide comes up:**
> "Each session runs about 15 to 20 minutes: consent and a couple of context questions,
> a short pre-visit knowledge check, then completely free exploration — no tasks, no
> minimum time — followed by the same knowledge check again, then the three
> questionnaires, and a short interview if there's time and interest. Passive analytics
> and my own observation notes run throughout, so I get both what they did and how they
> felt about it."
