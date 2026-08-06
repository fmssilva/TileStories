# 1. **Kickoff (context + explicit "planning only, no code yet")**

- `Attach rules zip, global work plan, and any existing prototypes/docs for this domain if they exist.`

Read the general guidelines first (clinerules zip).
Including the code structure and organizing
principles (10-structure.md) so you know where things actually live before reading anything else. Read all lines of the file so you have a clear picture. No exceptions.

Read the full project work plan I want to implement: `_0_work_plan.md`. Read all lines of the file so you have a clear picture. No exceptions.


### **start planning**
And now we're starting **`Domain X`**. 
For this whole conversation, don't touch the project zip or write implementation code — we're only discussing and planning. Give me your read of what the work plan already says about this domain, and what's actually built vs. just planned, before we design anything.

Read fully. Planning only, no code. Tell me what's already said about this domain and what's actually built vs. planned.

Also tell me:
- Does this domain have a visual/rendered component (UI, AR markers, effects, animations)? If so, flag that now — it changes how we should plan Phase A/B testing later (guidelines §4).
- Does this domain have, or will it need, an Editor-time authoring/preview tool (a custom `EditorWindow`, custom inspector, etc.) that touches this domain's real runtime objects outside Play Mode? If so, flag that too — guidelines §4.4.1 (Edit-Mode tooling parity) applies and needs planning alongside Phase A/B. This is a different axis from Phase A/B, not a third phase after it — say explicitly whether it does or doesn't apply here, don't leave it implicit.

Be good, not fast.

### **continue planning in a new chat**
And so now we will continue planning the implementation of the `domain about the poi marker design`. I send attached the current plan and status of this domain implementation we have currently. read all the lines of this file so you get up to date about the current status. And then also check the current actual code in the zip attached (don't trust in the work plan guide because it might be out of sync with the actuall implemented folders and files and code).

And so you know, we implemented first the whole marker design system in the AR Living Room direclty... but it is hard and slow to test that way. So after we came up with the new guidelines, where, when we have some UI elements to implement, so first we implement them in a simple test scene and only when everything is well validated, then yes we will implement thigns in the AR LIvingRoom scene. (Th guidelines i sent you already reflect this new methodology yes?)
And so now, ihad a agent to implement and test everything in the test scene. And i also confirmed visually that everything looks good. And everything does actually look good. Except, is still one thing to be done: 
the "sun effect, both in circle or in countour, they are beeing rendered as squares - see the attached image i sent you). so do a full analysis of the code we have and try to find where might be the problem... and so update the section 8 (i think is section 8 about the test scene implemetnation.??) update it with the correct debug procedures and corrections to implement in order to make this sun effects to be circular and not squared. 

and then confirm or update and think where to add this notes... about, having finalised section 8 in the test scnee... how can we now implement everything in the AR Living Room scene and test everythign? Keep in mind that we had basically everything implemented already... but things were not running well and so we implemented this test scene to confirm good rendering of everythign. so now maaybe we just need to confirm that all steps are well done again, and we are using the final, well implemented marker design elements that actually render well? and so update the whole work plan so after the agent finishes the test scene, it knows how to confirm everything in the AR living room. 

be good, not fast. 



# 2. **Idea exploration (repeat as needed, explicitly time-boxed to "discuss, don't build")**

Here's my thinking on `X, Y, Z`...

Also research how other projects/products handle this, and note what's genuinely applicable vs. not given our constraints of a mobile AR app... Give me 2-3 concrete options with tradeoffs, not a single recommendation yet.

And this is a master thesis project so save the good real citations as you go so we can latter use them in the final thesis report. You can save the references alerady in the "domain work plan that I will ask you to write latter on, and so you can already save some references there in the file: `_X.X_DOMAIN_X.md`

And also, confirm if there are some other good ideas that we should think about and evaluate to enrich this app and project in terms of this current domain or related to it?? Some nice UI or UX feature or effect or way to organize things... how can we make this a very interesting and easy to use and inovative app with a good "UAU factor" with a nice design and UX?

Be good, not fast.



# 3. **Finalize (the actual work order)**

"We've decided on [X]. Now write the complete `_N.M_DomainX_Design.md`: 
so confirm again if the guides are clear and with all important details for a for a separate low-context coding agent who has full project/Unity access but no memory of this conversation, to know exacly what to do, how, where and why... 

and so maybe the files could have content like??: what/how/why/where for every decision, concrete code where it de-risks the work, a explicit 'read these real files first' list, a step-order that isolates any risky refactor from new feature work, a verification checklist, the bibliography, plus a mandatory ## Implementation Status section at the very top — a living tracker, not a one-time snapshot. Every entry gets one of: ✅ confirmed correct (verified how), ❌ confirmed missing/broken (what and why), or ⚠️ needs a non-code action. Tell the implementing agent explicitly: after finishing and testing each section of this doc, update this Status block immediately, in this same file, before moving to the next section — never a separate progress file, never batched to the end.

If this domain has any visual/rendered component (UI, AR markers, effects, animations): the doc MUST specify a two-phase build order, and the implementing agent must not skip from Phase A to Phase B. Read the guidelines (40-testing.md)

If this domain also has, or will get, an Editor-time authoring/preview tool
that touches its real runtime objects outside Play Mode, the doc should also apply guidelines about Edit-Mode tooling parity as its own subsection, not fold it silently into Phase A or B tests... because it's a different axis (which runtime context renders the component). Read the guidelines (40-testing.md)

For every phase/step in this doc's Phase A/B (and §6.4.1, where it applies) plan, specify exactly which verification tier applies and what the pass criterion is — never just 'test it': name the specific Assert calls (Tier 0), which of the four no-vision UI-quality checks apply (Tier 0.5 — occlusion via raycast, WCAG contrast, tap-target size, text truncation), and, only where Tier 0/0.5 genuinely can't reach, the exact itemized checklist a vision agent should run (Tier 1) — never an open 'confirm it looks right' prompt. State explicitly where human-in-the-loop (Tier 2) is required and where it isn't. Write the handoff summary format this domain's language agent must produce for the next agent: what to check, not what to conclude.

This file needs to contain all important details of what, how, where, why... but keep prose concise — code and structure carry the detail, not paragraphs. Lets write all important details in a concise manner. It is important for the files to be concise to avoiod ovreloading the models context window during reading or editing the file. 

Then re-check the whole doc against our conversation for anything missed before you hand it to me.

Also, if this changes anything in the global work plan `_0_work_plan.md`, patch it directly and tell me what changed and why

If a lesson from this domain is a
general process rule (not domain-specific), patch `_0_guidelines.md` instead and tell me what changed and why.

Be good, not fast.

# 4. **Debug / Confirmation**

The agent says it implemented/fixed [X]. Don't trust that report — extract the actual project zip and verify directly against the real files.

If a Phase A test
scene exists for this domain, check whether it still passes via Unity MCP first —
that narrows the search before you re-derive anything from scratch.

If a prior agent's report says something passed, treat that as a claim to verify, not a fact — re-run the Tier 0/0.5 checks yourself before trusting the summary, per guidelines §6.5.

Update `_N_M_DomainX_Design.md` Implementation Status section to reflect what you actually find, not what was claimed.

If you find a bug, put the fix in this same file, in a numbered corrections section, not a separate document.

Be good, not fast.