

about this:
__Section 4.2 Decision:__ Should manually-typed positions be tagged as "manual" in captured_position_source (recommended default)?
I think yes.
but then, what are we going to do with this "flag"? should we for example just add some "?" to the POI marker labels, or add some "circle" around the POI marker or some other visual marker so the developer in the edit mode can clearly see the markers that were already well positioned in the glb vs the ones that are still just interpolated from the x,ynorm or just hand writen positions...?? or what are the best options to make a good "positioning whole system"? 

Question 1 — What do we do with the "manual" tag once it exists?
Agreed it should be tracked. The question is what surfaces it — and I'd draw a hard line here: this stays entirely in Editor-only tooling, never in the runtime/visitor-facing marker. Putting a "?" or a ring on a marker a real museum visitor sees would leak internal QA metadata into the shipped experience — exactly the Framework/Apps, Editor/Runtime boundary this project has protected everywhere else (EditorOnly tags, "never shown to a visitor," etc.). A visitor doesn't care whether a position was field-captured or interpolated; a developer deciding what's left to do very much does. Two different audiences, two different places.

Concretely, three additions to the Authoring Tool window (all Editor-only, none touching MarkerView.cs or anything that ships):

Color-code the OnSceneGUI sphere handles the tool already draws — green for field-captured (Workflow A/B), orange for manual, grey/blue for still-interpolated-not-captured. This is the single highest-value addition, since it's visible at exactly the moment someone is deciding what to place next.
Annotate the POI list in the tool window with the same status, as a short prefix or colored label per row — this is where someone reviews "what's left."
A one-line summary at the top of the window: "3/3 POIs captured (0 manual) - 2/2 calibration anchors captured" — cheap, and gives an at-a-glance completeness check before considering a wall done.
Worth noting for later, not building now: this same signal is exactly what a Stage 4 baker validation check should refuse to bake on — "don't let a wall ship with manual or uncaptured positions" is a natural extension of the NaN/Infinity checks already planned for B3, just not needed yet.











about the whole process to positioning markers: Use TileStories > POI Authoring Tool:
Load config → Populate Rig from JSON → Position markers using sphere handles → Capture Positions to JSON → Save config → Copy to StreamingAssets → Build & Run...
One thing that the "normal unity developer will do" is to just populate the rig, and then just go to the hierarchy and click each POI and position it, and then click "Ctrl+S" and he will think everything is well done and saved to the config json?? is this probable to happen? so what should we do? maybe we can save to config json automatic when the user saves normally... so when the unity saves the prject at some moment we always save things to the config json automatic? is this good?


Question 3 — The Ctrl+S trap. Real risk. Auto-capture-on-save is the wrong fix, though.
This is a sharper catch than the rig-cleanup issue, honestly — someone can place markers, hit Ctrl+S out of habit, close Unity, and not discover config.json is stale until much later, with no visual signal anything's wrong in the meantime.

I'd argue against silently auto-running "Capture Positions to JSON" on every scene save, for a specific reason: it trades one silent failure for a different one. If someone saves mid-experiment — dragging a marker around, hasn't found the right spot yet, saves for an unrelated reason — an auto-capture would bake that half-placed position into config.json without them ever deciding to finalize it. "Capture" is currently a deliberate act; making it an invisible side effect of an unrelated action (saving the scene) removes that deliberateness in both directions, not just the one you're trying to fix.

Better version of the same instinct: make the mistake loud instead of invisible, using the exact same EditorSceneManager.sceneSaving hook already planned for the rig-cleanup check (Task 2.3), upgraded to actually compare state instead of just checking presence:

On scene save, for every marker currently under POIAuthoringRig, compare its current scene position against what's currently saved in config.json for the matching POI id (using a distance threshold like Vector3.Distance(a, b) > 0.001f, not exact equality — float precision noise would otherwise cause constant false alarms).
If anything differs, show an actual EditorUtility.DisplayDialog — a blocking popup, not a Console line easy to scroll past — with a "Capture Now" button right in the dialog that runs the capture immediately, one click, the moment the problem is caught.
This keeps the person in control (they click, nothing happens invisibly) while making the failure mode almost impossible to walk away from unnoticed. A cheap complementary touch: an asterisk in the tool window's title when there are uncaptured changes, matching the "unsaved document" convention from basically every text editor — an ambient reminder, not just a save-time interrupt.













then the new marker configured in the config json and well flexible and editable... 		

	size of things: dev defines in json the size of the markers, and then in the editor panel we allow to adjust that size ... 	

		and, the size of markers, even if they are ugui elements in the word, can we make their size in relation to the screen? because if we set the size like in real cm in the world, then, if the user is very near they will be very big, if the user is faraway they will be very small. so lets have the size to be set up in pixeis in screen or something? can we do that? 

		

	hierarchy of levels of categories ... and so, in practice we can have it clear in the guide that in practice we have 3 kinds of categories of markers: we have the first category the dev wants (ex type of buildings - religious, military... ), other second category (example level of destruction, or price or whatever... ), and then the "relevance category" which determines the markers that are bigger and the ones that are smaller... 	

		you said 2 levels, but maybe lets have more levels. lets have level 1 with big markers and labels, level 2 with still big markers but a bit smaller and without labels, level 3 with even smaller markers, and level 4 only little markers really 

		i think to simplify, we can have the same kind of marker logic at all levels. instead of having symbols in big markers and only circles in small markers... no, lets have symbols in all levels... so we have the same logic for all kinds of markers and  we just change the size... 

		and maybe even, we can, instead of hard code some possible number of levels, we can leave it flexible and allow for the dev to add as many levels as he wants, considering the marker is always the same and we just adjust the size, we can allow this flexibility easy yes? 

		and maybe as default for sizes we can take the bigger size and then divide by 2 or other ratio to calculate the smaller levels markers... ?? and we can also allow in the editor panel to adjust this ratio...??? 

















