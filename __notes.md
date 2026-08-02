

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



now in the editor tab, can we have some undo option in case for example we delete some category or some other "hard" change, so we are able to undo it with some button or with Ctl+Z? 

now about the symbols, we currently have them right inside the runtime folder:
C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Runtime\UI\Markers\Icons
and
C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Runtime\UI\Markers\SymbolCircle.png

but these are png images. this is "assets" or something like that. AND i want it to be possible for the developer to add new symbols, new png images as he wants to use in a new wall. 
so what is the best way to have this well organized and clean? first of all, does the runtime needs these png images here inside this folder? or could it read these images from anywhere? example from the respective wall folder?? and so do a deep analysis and think what is the best way for us to "offeer these default symbols imges", but then allow for new images to be added? maybe the developer can add images to the wall folder, example to a folder like C:\Users\franc\Desktop\TileStories\TileStories\Assets\Apps\LivingRoom\MediaAssets\Images\marker_symbols
and then our editor is able to get them from there? and then if needed copy them to the runtime folder??? or even just leave them there in the wall folder? and if needed copy the defaults from the runtime folder to the wall folder...?? or what to do? how can we have default symbols and also allow to add any kind of new symbol imge in an easy way so a user can add them and see them right away in the editor mode? 
or maybe the developer can just drag the imges he wants from the project asets tab directly to the editor tab and that action directly copies those images to the final destination... example the runtime folder if needed or other place??
and this choosing of image, for the developer should be done by actually draging the image or then eearching for it by name, and not by "key", so it is intuitive and easy to use... and so in the editor tab, instead of having those 3 colums for the category name and symbol key... maybe we can have just the ategory name and then the symbol name of the png file, and then also we show directly the symbol imge on the third colum so we have a preview of the symbol. can we do that? and so when we add some new category we should be able to have some + button that we can click and go and find the image we want to use for the symbol category... and be able to drag and drop the image from the project tab in unity directly to that spot... ??? see the best options and ways to do this. 

and for the badge we can use exacly the same thing. the same "editor component" we can use for category symbols and for badge symbols, because they are exacly the same... only the size really changes... so keep things simple... create a good editor component maybe and then we can use it for both the category symbol "enum definition" and also for the badge "enum definition"?? 



---
and then, we need to reorganize the whole POI Authoring tab into a good and well organized set of components. because currently everything is in the first level, and always visible. we need to organize things by groups or by components and so we can have some "expland" and minimize button in each component and so we can hide or expand that component information. 

and so, for starters, we should put all those buttons of "big actions" right on top of the POI Authoring.
so we have the config path, the marker prefab, the correction anchor, the wall mesh reference... 
and then we have the buttons:
load config (and here... should we allow to add the path or to click in some "+" button and so we add the specific config we want? currently maybe it is a bit hard coded?? and we should be able to select the config we want from our wall or from any new wall??)
then the buttons populate and clear rig and capture positions to json and save config and copy streamingassets buttons all of them here together on top of the POI Authoting.
and about these button... lets review how many buttons and for what functions we have them?? 
example, as soon as we click in populate the rig button, can we right away in automatic paint the "clear the rig" button in red or something or orange with some warning symbol, so the developer knows that he should clear the rig before running the app to avoid duplciates... and even we can add this message bellow to be clear to the developer why he should clear th rig before runing...?? 

And then we have capture positions to json and save config... lets maybe just have a button "Save configs to json" or something like that? and so when we click it we save evrything to json (positions, symbols, styles etc.. basically evrything...) (and this button should also become orange or yellow with some warning symbol and some message, automatic, as soon as we do some change in the POI Authoring... if we change ome position or style or something... so this button become yellow so the developer knows that he should save things...??)
and then the button copy to streamingassets is ok. 
and then we can remove the button select all rig objects (we dont need it)

and so these global button are always visible on top of the POI authoring tab...

then we have a big componnt bellow, with a scrooll bar on the side, and we put everything else inside it (and with a scrool because we'll have many things and we won't b able to see them all without a scrooll bar)


and so inside this big scrollable container we should have 2 big collapsible components:
one for 
"Global Scene Options" or something like that, 
and then another for "specific Markers options"...


and inside the global walls component we should have the respective configurable options... and all organized by groups or components like:
a component "Marker". and then we expand it and we see the line to select the "marker shape", and then the "Category Symbols" component with the whole symbols enum definition...

then a component "Badge Symbols" which is by itself a line with a toggle button, and so in a easy and clear way we can select or deselect to ue bdges. and then we can click in that line to expand that componetn and IF we select the badge option ON, so then yes we can see the whole badge enum options there also...
then the "Outline" which is by itself a line with a toggle button, and so in a easy and clear way we can select or deselect to use the outlines. and then we can click in that line to expand that component and IF we select to use a outline, so then right bellow we have a line saying "outline color" and it is a drop down and we can choose gold or hue. (so currently we have a simple drop down with gold and hue and none... lets break that up in a toggle that says use a outline or not, and then a drop down with the outline options we have - gold and hue)... and then bellow that drop down of the line color, we have the section of the "outline styles" and there we can define the number of levels we want, the label for each level (some input text free), and the kind of line (the drop down with the available options) (we only need these 3 simple fields - we don't need more). AND we should cap the "add more levels" options to the available number of line types + 1, so we can use no line and each line type... and when we have no more so we can't add new line types... and we should also allow to remove line types levels... 


and then in the "specific marker options" we have a inner component for "position" and another for "marker style" or something like that. 


so do a deep analysis and see the best options and best way to implement all this in a clean and simple and good looking and good editor UI well organized tab way, easy to work and understand and navigate. 














