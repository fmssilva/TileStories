# 1. **Kickoff (context + explicit "planning only, no code yet")**

- `Attach rules zip, global work plan, and any existing prototypes/docs for this domain if they exist.`
- **Attach the project zip even if this domain hasn't been formally started yet.**
  Related runtime code has shown up unannounced before (e.g. `MarkerOverlapResolver.cs`
  already existed, fully working, before Displacement was ever discussed) — "no domain
  doc yet" does not mean "no code yet." Check for it regardless.

Read the general guidelines first (clinerules zip).
Including the code structure and organizing
principles (10-structure.md) so you know where things actually live before reading anything else. Read all lines of the file so you have a clear picture. No exceptions.

Read the full project work plan I want to implement: `_0_work_plan.md`. Read all lines of the file so you have a clear picture. No exceptions.


### **start planning**
And now we're starting **`Domain X`**. 
For this whole conversation don't write any implementation code — you can see the project zip to understand the current state or confirm some important implementation described in the guidelines `10-structure`, but don't execute any code implementantion - we're only discussing and planning. Give me your read of what the work plan already says about this domain, and what's actually built vs. just planned, before we design anything.

Read fully. Planning only, no code. Tell me what's already said about this domain and what's actually built vs. planned.

Also tell me:
- Does this domain have a visual/rendered component (UI, AR markers, effects, animations)? If so, flag that now — it changes how we should plan Phase A/B testing later (guidelines §4).
- Does this domain have, or will it need, an Editor-time authoring/preview tool (a custom `EditorWindow`, custom inspector, etc.) that touches this domain's real runtime objects outside Play Mode? If so, flag that too — guidelines §4.4.1 (Edit-Mode tooling parity) applies and needs planning alongside Phase A/B. This is a different axis from Phase A/B, not a third phase after it — say explicitly whether it does or doesn't apply here, don't leave it implicit.

Be good, not fast.

### **continue planning in a new chat**
And so now we will continue planning the implementation of the `domain about the poi marker design`. I send attached the current plan and status of this domain implementation we have currently. read all the lines of this file so you get up to date about the current status. And then also check the current actual code in the zip attached (don't trust in the work plan guide because it might be out of sync with the actuall implemented folders and files and code).

 



# 2. **Idea exploration (repeat as needed, explicitly time-boxed to "discuss, don't build")**

Here's my thinking on `X, Y, Z`...

Also research how other projects/products handle this, and note what's genuinely applicable vs. not given our constraints of a mobile AR app... Give me all the good concrete options with tradeoffs.

And this is a master thesis project so save the good real citations as you go so we can latter use them in the final thesis report. You can save the references alerady in the "domain work plan that I will ask you to write latter on, and so you can already save some references there in the file: `_X.X_DOMAIN_X.md`

And also, confirm if there are some other good ideas that we should think about and evaluate to enrich this app and project in terms of this current domain or related to it?? Some nice UI or UX feature or effect or way to organize things... how can we make this a very interesting and easy to use and inovative app with a good "UAU factor" with a nice design and UX?

And so, to make it clear, I am building a framework to be used to build any app for any big heritage wall of any shape and theme... So when we have some feature or problem to solve, and then we talk about the best solutions, the goal is not to decide for the best solution and implement it. Instead I want to implement ALL good solutions and options to execute and configure each feature. So in my framework I want to have some Editor Window where the developer then can choose and try all the available good options, and also configure the main variables and params for each option. So I don't want to have some hard coded "config values or ranges or options...", instead I want to allow for them to be set up and configured by the developer in the Editor Window. So do a deep analysis and research and think all the good options we might implement, and then lets select all the good options and implemnt them all in order to create a very complete and flexible framework where the developer can choose between all good options or solution for some feature, and also define the main variables or params to configre that options as he wants for his specific app.

**Standing instruction, every domain, not just when I remember to ask:** research the best way to implement these features — should we implement them "by hand" or use some already well-built and tested library, package, or git repo? Example: to implement a zoom feature, use Unity/AR Foundation and other good established libraries instead of implementing it by hand. Keep the general guidelines in mind throughout — simple, easy to understand and maintain, well organized, and "small" to avoid an unnecessarily bloated app. When a library or package does get recommended, it must come with the exact integration path — package name, git URL, or "copy from [specific official samples repo/location]" — not just a name. A vague library recommendation is barely more useful than none to a low-context agent who can't independently go research whether it's actually available or how to add it.

Be good, not fast.



# 3. **Finalize (the actual work order)**

So now write the complete implementation plan guide `_2.6_Select_Filter_Search.md`: 
Make sure to confirm that the guide is clear and with all important details for a separate coding agent who has full project/Unity access but no memory of this conversation and also a low-context memory, for it to know exacly WHAT to do, HOW, WHERE, and WHY... 

Also, to make it clear again, I am building a framework to be used to build any app for any big heritage wall of any shape and theme... So when we have some feature or problem to solve, and then we talk about the best solutions or options, the goal is not to decide and select the best solution and implement it. Instead I want to implement ALL good solutions and options to execute, and configure each feature. So in my framework I want to have some Editor Window where the developer then can choose and try all the available good options, and also configure the main variables and params for each option. So I don't want to have some hard coded "config values or ranges or options...", instead I want to allow for them to be set up and configured by the developer in the Editor Window. So do a deep analysis and research and think all the good options we might implement, and then lets select all the good options and implemnt them all in order to create a very complete and flexible framework where the developer can choose in some Editor Window between all good options or solution for some feature, and also define the main variables or params to configre that options as he wants for his specific app.

And so maybe the file could have content like??: what/how/why/where for every decision, concrete code where it de-risks the work, or concrete libraries or packages or git repos to be used (with the exact integration path, per §2 above — not just a name)... a explicit 'read these real files first' list, a step-order that isolates any risky refactor from new feature work, a verification checklist, the bibliography, plus a mandatory ## Implementation Status section at the very top — a living tracker, not a one-time snapshot. Every entry gets one of: ✅ confirmed correct (verified how), ❌ confirmed missing/broken (what and why), or ⚠️ needs a non-code action. Tell the implementing agent explicitly: after finishing and testing each section of this doc, update this Status block immediately, in this same file, before moving to the next section — never a separate progress file, never batched to the end.

If this domain has any visual/rendered component (UI, AR markers, effects, animations): the doc MUST specify a two-phase build order, and the implementing agent must not skip from Phase A to Phase B. Read the guidelines (40-testing.md) 

If this domain also has, or will get, an Editor-time authoring/preview tool
that touches its real runtime objects outside Play Mode, the doc should also apply guidelines about Edit-Mode tooling parity as its own subsection, not fold it silently into Phase A or B tests... because it's a different axis (which runtime context renders the component). Read the guidelines (40-testing.md)

For every phase/step in this doc's Phase A/B (and §4.4.1??, where it applies) plan, specify exactly which verification tier applies and what the pass criterion is — never just 'test it': name the specific Assert calls (Tier 0), which of the four no-vision UI-quality checks apply (Tier 0.5 — occlusion via raycast, WCAG contrast, tap-target size, text truncation), and, only where Tier 0/0.5 genuinely can't reach, the exact itemized checklist a vision agent should run (Tier 1) — never an open 'confirm it looks right' prompt. State explicitly where human-in-the-loop (Tier 2) is required and where it isn't. Write the handoff summary format this domain's language agent must produce for the next agent: what to check, not what to conclude.

This file needs to contain all important details of what, how, where, why... but keep prose concise — code and structure carry the detail, not paragraphs. Lets write all important details in a concise manner. It is important for the files to be concise to avoiod ovreloading the models context window during reading or editing the file. 

Also, if this changes anything in the global work plan `_0_work_plan.md` or in the `_5.1_Editor_Tab` or in the `.clinerules/10-structure`, patch it directly and tell me what changed and why.

If a lesson from this domain is a
general process rule (not domain-specific), patch the specific guidelines document in the zip `.clinerules` instead and tell me what changed and why.

Be good, not fast.


## 3.1 **Deep re-verification (mandatory, not a light pass — do this before handing the file over)**

Every serious bug caught in this project so far — the `Configure()` call missing from
two of three required wiring sites, a pipeline step running in the wrong order, a
count-cap sorting by the wrong field — was found only because this step was explicitly
requested afterward, as a separate ask. It isn't optional, and it isn't the same thing
as the light "re-check against our conversation" pass — it's two distinct, deliberate
adversarial passes:

**Pass 1 — features/config, against the chat, not the doc.** Re-read the full chat
history for this domain, block by block. For every feature, option, or variable
discussed — including ones mentioned once in passing — confirm the doc actually
exposes it as a real, developer-configurable field in the Editor Window, not just
described in prose. Confirm no idea got silently dropped, simplified away, or left as
a vague "future work" note when it was actually concrete and buildable now (ask
directly: is this genuinely still open, or did I just not think it through enough the
first time?).

**Pass 2 — wiring, against the doc itself, not the chat.** Re-read the finished doc
end to end as if you'd never written it. For every cross-reference (section numbers,
method names, class names), confirm it actually points at the right place. For every
described behavior, confirm the described call order/sequencing actually produces
that behavior — don't assume steps compose correctly just because each one reads fine
in isolation; trace a concrete example through the full pipeline by hand. For every
formula described in prose ("gets shrunk based on density"), confirm an actual formula
exists, not just a description of the effect. For every place two methods are supposed
to share state or agree with each other, confirm that's enforced (a cached field), not
left as a hedge for the implementing agent to resolve ("re-resolve here if these don't
already share state" is not an acceptable final answer).

Fix everything Pass 1 and Pass 2 find, directly in the file, before presenting it —
don't just report the findings and leave the file as drafted.

Then, separately, do one more check: re-read the whole doc once more end to end and
confirm it's still internally consistent after the Pass 1/2 fixes (a fix in one section
can easily orphan a reference elsewhere).

Be good, not fast.



# 4. **Debug / Confirmation**

The agent says it implemented/fixed [X]. Don't trust that report — extract the actual project zip and verify directly against the real files.

If a Phase A test
scene exists for this domain, check whether it still passes via Unity MCP first —
that narrows the search before you re-derive anything from scratch.

If a prior agent's report says something passed, treat that as a claim to verify, not a fact — re-run the Tier 0/0.5 checks yourself before trusting the summary, per guidelines' verification-tiers section (confirm the current section number in `40-testing.md` before citing it — it has moved before and may again).

Update `_N_M_DomainX_Design.md` Implementation Status section to reflect what you actually find, not what was claimed.

If you find a bug, put the fix in this same file, in a numbered corrections section, not a separate document.

Be good, not fast.