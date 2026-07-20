See the attached files of my thesis preparation report that i wrote. 

Read all the lines of all those files so you get a clear image of what we have currently. 

And confirm the current index we have. 
is it something like this?


Abstract                          [not among the uploaded files — not yet written/not in this batch]

1  Introduction                   (chapter-introduction.tex)
   1.1  Context & Motivation
   1.2  Institutional Context & Research Partnership
   1.3  Problem Statement
   1.4  Research Questions
   1.5  Main Contributions
   1.6  Document Structure

2  Related Work                   (chapter-relatedwork.tex)
   2.1  Digital Heritage and Interactive Museum Experiences
        2.1.1  Evolution of Museum Interpretation
        2.1.2  Digital Technologies in Cultural Heritage
        2.1.3  Visitor Behaviour and Museum Learning Models
   2.2  Augmented Reality in Cultural Heritage
        2.2.1  AR Concepts, Taxonomy and Tracking Techniques
        2.2.2  AR Applications in Museums and Heritage Sites
        2.2.3  AR for Large-Scale and Panoramic Heritage Surfaces
        2.2.4  Research Gap: Multi-POI Detection on a Single Panoramic Surface
   2.3  UX, Engagement and Personalisation in Heritage AR
        2.3.1  User-Centred Design Principles for Heritage AR
        2.3.2  Personalisation and Visitor Profiling
        2.3.3  Gamification, Storytelling and Temporal Visualisation
        2.3.4  Evaluation Methods for Heritage AR UX
   2.4  Summary and Research Gap
        2.4.1  Comparative Overview of Related Work
        2.4.2  Positioning of This Thesis

3  Proposed Work                  (chapter-proposed-solution.tex)
   3.1  Problem Framing and Research Questions
   3.2  Domain and Case Study Context
        3.2.1  The Grande Panorama de Lisboa
        3.2.2  Chafariz Velho de Paço de Arcos
        3.2.3  Two Cases, One Surface Class
   3.3  Research Approach
   3.4  App Concept: TileStories
        3.4.1  Core Concept and Key UX Decisions
        3.4.2  Feature Set by Phase
   3.5  Technology Stack: Comparison, Tests, and Decisions
        —  Tracking: From a Hand-Built System to a Visual Positioning Service (unnumbered \subsection*)
        —  Frontend: Unity over Flutter (unnumbered \subsection*)
        —  Data and Backend: A Deliberately Brief Decision (unnumbered \subsection*)
   3.6  System Architecture Overview
   3.7  Evaluation Plan
        3.7.1  Research Design
        3.7.2  Evaluation Instruments
        3.7.3  Ethical Considerations

4  Work Plan                      (chapter-workplan.tex)
   4.1  Work Completed to Date
   4.2  Development Phases Overview
   4.3  Gantt Chart
   4.4  Risk Analysis

Bibliography                      (bibliography.bib)

Appendix A  UI Design Prototypes  (appendix_prototypes.tex)
   A.1  Core User Flow
   A.2  Visual Style Exploration
   A.3  POI Marker Encoding System
Appendix B  Full Risk Analysis





IMPORTANT - READ ALL THE LINES OF THE FILES SO YOU GET A CLEAR PICTURE OF OUR CURRENT REPORT STATE. 

AND SAVE THESE FILES BECAUSE WE ARE GOING TO EDIT THEM IN NEXT TASKS. 







now read all the lines of the attached file with a implementation plan of a framework and template to allow to implement any wall... so basically i was thinking already for my thesis to make some "easy to generalize app" for heritage walls, but now lets take it a step further and actually build a framework / template to allow easy implementation of any kind of wall app. 

so read the file attached. read all the lines so you get a clear vision of my current idea that i am actually going to do in my thesis. 



and then, having in mind the current thesis preparation report we have and the "more framework oriented actual work plan", check, is there something we should change in our preparation thesis report to make it more clear about this framework idea or not really? do a full analysis. 

keep in mind we have maximum of 35 pages, so if we want to add any idea, we also need to remove some previous existing text... the current report is already at maximum size!!! 

so, should we change something about the report to make it more "framework" clear, or is it ok like that? 

do a full analyse, and give me now a plan, A CONCISE PLAN, about the things we should change about the report to align it better with this framework idea now, what to change and where... or if it is ok everything like it is now... 

so do a deep analyis and give me now only the topics of the things we should change for now and where. so some sort of bullet points about changes to do in a vry concise way. 
only latter we will actually implement those changes in detail ok? 

so now give me just the "update report plan" in a concise but concrete way for us to confirm what we should change or not and we define a good plan before we do it. 














having the "related work" we already wrote above, and having in mind now our detailed work plan that we'll implement, lets now continue writing the report and write the next sub sections:
4  Work Plan    ~3-4 pages
   4.1  Work Completed to Date
        — absorbs old 3.8 "Prior Work Already Completed"; shows what's done
          before showing what's left
   4.2  Development Phases Overview
   4.3  Gantt Chart
   4.4  Risk Analysis



Now one think that maybe we should change/update. 
My thesis initially was specific for the Grande Panorama de Lisboa of museu do azulejo. But the museu is under resconstruction currently and so, probably i might even be able to actually build the app for the grande panorama. So the panorama stays as my central motivation for this app, but lets make it clear in the whole spirit of the thesis that the grande panorama is just a motivation, and that our research and developemnt and findings and challenges and so on, are extendable for any other "heritage wall", either a big panorama wall of some 23m with 150 POIs like the grande panorama and made of tiles, as well as any other big wall, or smaller walls with heritage info to be "enhaned for better UX". 

in fact, because the museum is under construction, i implemented most of the app and tests and development using the walls of Xafariz Velho from Paço de Arcos, which is a big wall (some maybe 30 or 40 meters long), also made of tiles (with that "portuguese blue painting over white tiles"), and is also very reach in terms of heritage content and is dated also from 1755... and it is different from the grande panorama because it is a outdor wall and so sun light and shadows from surrounding trees change along the day... and also is even a bit "harder problem" then the grande panorama wall because it is a "circular" wall... so, in many walls we only need to consider x,y coordinates, in some walls like grande panorama which in practice is a sort of U wall, we also need a z coodinate, and in other irregular walls like xafariz we also need to consider yaw rotation values... (https://www.oeiras.pt/chafariz-velho-de-pa%C3%A7o-de-arcos). (Possibly even some walls are not vertical and so we would need to consider x/z rotation angles... but i don't consider that for this application/thesis study)

and so, in the introduction of the thesis, and maybe also in the sub section (2.2.3  AR for Large-Scale and Panoramic Heritage Surfaces) and in the section (3. Proposed Work) i will explain this main idea, that the grande panorama was the initial motivation, but in fact any "heritage wall" can be enhanceed with this app ideas... and xafariz velho is another example that was widely used during development... and so we give practicle examples of the challenges and problems and solutions and findings and so on about the concrete grande panorama and/or xafariz velho, but it is just to make things practical because any heritage wall can benefit from our "thesis and app". 

and so during these writings of these sub sections, have this in mind. So use grande panorama, and if valuable also xafaris velho as practical examples, but always keep the spirit of the thesis that this is a generalizable app and not a grande panroama specific. 


and so now lets write the sub section
4  Work Plan    ~3-4 pages
   4.1  Work Completed to Date
        — absorbs old 3.8 "Prior Work Already Completed"; shows what's done
          before showing what's left
   4.2  Development Phases Overview
   4.3  Gantt Chart
   4.4  Risk Analysis



so do a full research of papers and sites, and apps and aplications... check what is the best info that i can use to write the first sub section of the related work? example currently i have this text that i give bellow... in what way can we improve? 

what other good references could i use that are good, and current and well reviewed? 
focus on these 3 criterea because all 3 are important: 
- good paper in terms of content;
- CURRENT - WE ARE TALKING ABOUT AR IMPLEMENTATIONS AND SO PAPERS ABOUT IT SHOULD BE FROM 2021-2026. AND IDEALY FROM 2023-2026. LETS NOT USE OLDER PAPERS TALKING ABOUT AR IN MUSEUMS BECAUSE THEY WILL BE OUTDATED!!!! Only when we are talking about "definitions and principles and other things that might have been writen and are still valid, then yes we can use older references, but in terms of practical implementation and application details, lets use as much current references as possible. 
- WELL REVIEWD, MEANING FROM GOOD PLATFORMS THAT CONDUCT GOOD REVIEWD OF THEIR PAPERS. EXAMPLE: 

## Tier 1 — The absolute gold standard for your topics:

**ACM Digital Library** is the single most important platform for HCI, interactive systems, and cultural computing. The flagship venues you want — CHI, ISMAR, JOCCH, ACM TOCHI, ACM Computing Surveys — are all here. ACM Digital Library is the clear number one when it comes to academic databases for computer science, with 540,000+ articles in the full-text collection and over 2.8 million bibliographic entries. Paperpile Crucially, as of January 1, 2026, all ACM-published journals are Open Access ACM — meaning most of what you find there is now freely downloadable.

**IEEE Xplore** is your second essential platform, covering ISMAR (the world's leading AR conference), IEEE TVCG (the top AR/VR journal), and IEEE Access. IEEE Xplore holds more than 4.7 million research articles from electrical engineering, computer science, and electronics — covering journal articles, conference papers, and technical standards. Paperpile

**Elsevier ScienceDirect** houses journals like Computers in Human Behavior, Computers & Education, Digital Applications in Archaeology and Cultural Heritage, and Journal of Cultural Heritage — all directly relevant.

**Springer Nature / SpringerLink** covers Virtual Reality (Springer), Personal and Ubiquitous Computing, and the Lecture Notes in Computer Science series (which includes many IEEE/ACM co-published conference proceedings from ISMAR, HCI International, etc.).

**Wiley** and **Taylor & Francis** are where you'll find International Journal of Human-Computer Interaction (IJHCI), Museum Management and Curatorship, and Current Issues in Tourism.

## Tier 2 — Solid, indexed, peer-reviewed:

**Nature portfolio journals** (npj Heritage Science, Nature Scientific Reports) are rigorous open-access journals — the papers from there in your existing list are actually very solid.

**ACM JOCCH** (Journal on Computing and Cultural Heritage) is the single most targeted journal for your thesis topic. It sits inside the ACM Digital Library and is effectively the field's home journal.

## The specific key venues for your thesis:

**ACM CHI** (Conference on Human Factors in Computing Systems)

**IEEE ISMAR** (International Symposium on Mixed and Augmented Reality) — published in IEEE TVCG

**ACM JOCCH** (Journal on Computing and Cultural Heritage)

**ACM Trans. on Computer-Human Interaction** (TOCHI)

**ACM Computing Surveys**

**IEEE TVCG** (Transactions on Visualization and Computer Graphics)

**Springer Virtual Reality**

**Springer Personal and Ubiquitous Computing**

**Elsevier Digital Applications in Archaeology and Cultural Heritage**

**Taylor & Francis International Journal of Human-Computer Interaction**









even if for this state of the art things we can search in other things like normal websites and apps and blogs that do some sort of demos of good apps for heritage... ?? so we don't need to focus only on papers... we can focus also in commercial and existing apps and products... and AR frameworks and practical museum apps... ?? 







so give me a good coontent for this sub section, improving what what i currently have, with better organization and bettere content, and selecting only good references... (add a link to a good platform for each reference you use)... 




here are some notes that i wrote before... check if they might be usefull, but be critical about them, and make a deep research to confirm and validate eveerything, and keep in mind the "related work" analyse we did and also the detailed "work plan" that we did, so things start fiting well alltogether.






...








and so give me this subsection well improved and with good flow and with good references and with good examples. 


and then also check if there are good elements that we could use or create that are beyond simple text. example some image or diagram or schema or something... would it be valuable to have some "visual element" in this subsection or better to have them in other sections where they might be more important? 
Check if it would be valuable to include during the text:
- Elements besides text, like tables or graphs or images or schemas (and when you don't have them, just put a mock to mark that place with instructions of the kind of image or plot or diagram you want etc)
- Original analysis and insights
- Specific examples or case studies
- Personal perspective or unique angle
- Critical thinking and synthesis
- Conversational elements
»» BUT IMPORTANT: lets think if this element is really valuable and has more value than simple text. lets not just have elements which normally ocupy more space and that might not give full value for the space they ocupy. and also think if these visual elements have good value to be used in this sub section or should we use them in other subsection whre visual elements might be more apropriate? 
»» And also think if some element should be put in anexs or appendixs, if some element is actually important to have and keep and show, but not so important to be in the body of the report
»» for the visual elements that should be some image or infophrafic or schema etc. write the main idea that element should show, example some base command for me to give to an AI agent to create that schema or infographics, example something like:
"Create a strategy map for 'Circuit and Game Paths' in the TileStories Framework. Show the transition from fixed sequences to adaptive paths. Include: 
The Ashwell Taxonomy (Gauntlet, Hub-and-Spoke, Branch-and-Bottleneck, Parallel Tracks).
Entry-Point Resolution (starting at the nearest POI to the visitor's physical position instead of index 0).
Ambient Pivot logic (10-15s dwell-time threshold to update targets).
The Butterfly Prompt (offering a circuit switch based on observed interest). Use a clean, professional flow-chart or diagram style that emphasizes visitor agency and 'headless CMS' architecture."






and so do a full research, and give me this sub section well writen with good flow, with good structure with paragraphs and even sub sections if really needed (in my current example i have a lot of sub secctions and bullet points, but lets organize the sub section better, and considering the space is limited think well what sub structures should we have? sub sub sections?? or better only simple paragraphs...??)
and so give me this sub section well written, and well formated in the correct latex format for me to past in my curent latex document. 




and in terms of the writen style: 
**Critical Requirement in terms of type of style of text to write:**				
	**Purpose & Audience**			
		The goal is to (inform / persuade / describe)		
		Who will read this: (experts, teachers, students)		
				
				
				
	**Tone & Style**			
		Academic text / Coding community / Heritage Community / UX community / AR community		
		A bit formal and using the correct terms, but keep it simple and natural language explanations and intuitions. Not like a novel book.		
		First person plural 85%, mixed with third person 15%		
		preference of Active voice 85%, instead of passive voice		
				
				
				
				
	**AI detection** Avoid -7 point penalty in grade for AI-generated text			
	Write text that:			
		1. Uses varied sentence structures (mix 15-25 word sentences with occasional 8-12 word ones)		
		2. Includes natural academic hedging language ("suggests", "indicates", "appears", "tends to")		
		3. Varies transition phrases (avoid repeating "this demonstrates", "multiple studies", etc.)		
		4. Integrates citations naturally within sentences, not just at the end		
		5. Uses discipline-specific terminology appropriately but not artificially		
		6. Includes occasional academic qualifiers ("largely", "primarily", "generally")		
		7. Maintains scholarly rigor while avoiding overly predictable phrasing patterns		
		8. Break away from AI-generated text patterns		
				
# **size**			
		for this sub section of proposed work lets keep it under 10-12 pages including possible visual elements.		
		so lets be selective of information. lets focus on our goal of our thesis and select the info that is really important and interesting for the things we will do in practice. 		





at the end give me also the full bibliographic references data for each reference you used, including links for me to get those papers from the good academic and reviewed platforms, or commercial/practical aplications examples etc... 


so this is a big task. leets do it in a complete and deep way at the first time. lets not do something fast that latter we need to correct and improve on it.
take your time. do a good and deep research of papers and sites etc... 
and give me a good sub section well writen and formated for latex for me to copy for my latex file.
and also give me the references used
so be good. not fast.
















about this:

One real gap worth flagging: the four-epoch temporal layering (pre-1755 / earthquake / Pombaline / today) is a headline contribution (#2 in §1.5) but has zero visual representation anywhere — it's prose-only. A compact timeline diagram in §3.4 would be genuinely more useful there than another paragraph, and could replace rather than add to existing epoch-description text (net-neutral page cost). Recommend adding this, paid for by trimming the equivalent prose it replaces.

well initiali this "time lapse was though about actual time lapse because of the 1755 earthquackes and so we could show before, during after... but now that we move into a framework, we must move forward that and we should think in terms of a "general aplicable lapse" which can be of different dimensions like time, or for example like styles or... i don't know... any kind of "lapse" or some sense of tabs options...?? confirm again the work plan what we talk about this there... and lets be clear in the report, or even, lets not go to much deep and detail for now and just pass the idea and then when we implement it then yes we'll confirm everything..??? 





so confirm also this detail, and then lets proceed to implement the changes in the respective documents to make the report better and representing a bit better this framework idea and to keep it under 35 pages in the body 







