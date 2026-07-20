								
	0 — Before Anything Else							
		keep in mind the phase and project guides:						
			C:\Users\franc\Desktop\TileStories_App\PROJECT_GUIDES\PHASE_2_PLAN.md					
			.\PROJECT_GUIDES\PROJECT_GUIDE.md					
		Read all project guide files and confirm they are all up to date. read all the lines of th files:						
			.\PROJECT_GUIDES\DESIGN					
			.\PROJECT_GUIDES\FEEDBACK					
			.\PROJECT_GUIDES\LANGUAGE_SEO_ACCESSIBILITY					
			.\PROJECT_GUIDES\NAV_AND_LAYOUT					
								
		Read all relevant existing files in scope — understand structure, types, hooks, state, providers, and components before touching anything						
		Create a  compreensive TODO list of all tasks before starting. Do not skip steps.						
								
	1 — Think Before You Code							
		For every task, think at two levels:						
			Architecture level — where does this fit? Consider 3 options, choose the cleanest					
			Implementation level — how to write it locally? Consider 3 options, choose the cleanest					
		Goal: Clean, simple, maintainable code. No over-engineering. No shortcuts that make future changes harder. No aliases or backwards-compat hacks — just change what needs changing.						
		I want to make this project like if it was a benchmark for a professor to guive to his students and to use as reference for future projects. 						
		And so, before you start implementing, for each problem or feature you will implement, tell me directly in the chat, the best 3 options that we have to implement it, and then proceed to implement the best one, according to the code quality rules i want to follow						
								
	2 — Code Quality Rules							
		No duplication — check if the type, hook, component, or function already exists before creating it						
		Single responsibility — one file, one clear job. If a file grows too large, split it						
			example don't put all providers in 1 file or create some utils file dumping ground... Put one responsbility per file... with clear name... if it grows, split by domain					
		Domain-centric structure — keep all files for a subdomain together in its folder. Don't scatter widgets/pages/providers across layer folders						
		Self-documenting names — folder names, file names, function names should make the structure obvious without needing comments						
		Only implement what's currently needed — no "might be useful later" functions						
		Respect existing central types, hooks, state, and providers — adapt new code to fit them; don't work around them						
			» Unless you actually think we should change our central types, hooks etc to make our app better and cleaner. This is the first version of the app and i give great importance to keep it clean and well organized like if this was a project for a teacher to show to students. So if there are some improvements we could do, tell me what we should do and why and the 3 best options for that and implement the best one.					
		Make clean and simple code. Use the "good libraries" instead of implementing things by hand. I want the best quality code but as simple and as few lines of code as possible. 						
		So, no over-engineering, but also no shortcuts that make future changes harder. No aliases or backwards-compat hacks — just change what needs changing.						
		Lets make code extendable and easy to maintain, but no over engeneer. Lets make it easy to read and understand and with the good amount of layers to make it clean but not too much to avoid hiding how things work						
		I want this project to be a state of the art, with the best architecture and structure and separation of concerns as possible, and as simple code with as few lines of code as possible, but guaranteing all the state of the art functionalities and features and the best UX/UI. 						
		So implement the project as if you were an excelent professor and you were doing a project to give to your student and to serve as banchmark for future projects. 						
		Add so also, add some concise and inline comments to explain all the important details a student should know, but in a concise manner, in as few lines as possible. In terms of language, use the industry terms but explain them in a natural and simple language like 2 coders talking. And DO NOT USE EMOJIS.						
								
	3 — UI/UX Rules							
		Always respect the app's branding, theme, and design system (see PROJECT_GUIDES/DESIGN)						
		Ensure good contrast for both dark and light modes						
		Apply i18n to all user-facing strings						
		Respect accessibility guidelines (see PROJECT_GUIDES/LANGUAGE_SEO_ACCESSIBILITY)						
		Design Philosophy						
			Every screen should have a clear visual hierarchy and a single focal point — the user's eye should always know where to go					
			Aim for considered design, not decorated design. Every visual choice must have a reason					
			Avoid generic "AI site" aesthetics: no purple gradients on white, no Inter/Roboto on everything, no predictable card grids, no flat hero + CTA templates					
			Ask: what would make someone screenshot this? That's the bar					
								
	4 — After Implementing							
		Run flutter analyze and wait for it to finish before checking results (my PC is slow so you need to wait some 10 seconds before the analyser finishes) 						
		Fix all errors and warnings — go to the file directly, no string replacements						
		Run all tests: flutter test lib/ --reporter=compact						
								
					t			
								
								
								
					5 — Testing			
						Rules that apply to ALL layers:		
							Do not skip layers. Do not move to the next layer until the current one is 100% green.	
							Add all test steps to the TODO list upfront — including the iterate-until-green loop for each layer.	
							When fixing failures: understand the big picture first. Ask — is the test wrong, or is the logic wrong? Think 3 options, choose best, re-run. Repeat until 100%.	
							After each layer completes, give a short summary of what was tested so i have a clear picture of the main logic you used	
								
						### LAYER 1 — UNIT TESTS		
						Implement tests to confirm business logic, state management, utilities, etc are all correct		
						Location: .../domains/DOMAIN/test/unit/		
						"For these tests, use ProviderContainer to test providers in isolation - No widgets, no MaterialApp, no routing; 
Command: flutter test lib/domains/DOMAIN/test/unit/ --reporter=compact"		
						Make thesse tests real as possible. Load real data... Don't mock our own code.		
						"Acceptance criteria:
- All business logic paths covered, state transitions, functions... 
- Edge cases tested (empty, null, max values)"		
								
						### LAYER 2 — WIDGET TESTS		
						tests that actually mount real widgets/components and trigger lifecycles, test user interactions (tap, scroll, drag, text input); Navigation flows (routing with real routes); Provider integration with UI (does tapping update state?); Lifecycle issues (provider updates during build, etc.); UI state (buttons enabled/disabled, text changes, widgets/components visible), test real flow of events in the widgets/components tree, interaction with providers, syncrnous problems, auth problems, etc. 		
						Location: .../domains/DOMAIN/test/widgets/		
						"""to test » Mount real widgets with UncontrolledProviderScope; Use real GoRouter configuration (can stub page contents); - Simulate user actions with WidgetTester:
  * tester.tap(find.text('Button'))
  * tester.enterText(find.byType(TextField), 'text')
  * tester.drag(find.byType(ListView), Offset(0, -500))
  * tester.scrollUntilVisible(find.text('Item'), 500)
- Assert on:
  * Widget visibility: expect(find.text('X'), findsOneWidget)
  * Provider state: expect(container.read(provider), expectedValue)
  * Navigation: expect(GoRouter.of(context).location, '/expected')""
Command: flutter test lib/domains/DOMAIN/test/widgets/ --reporter=compact"		
						"Acceptance criteria:
- Critical user paths covered (navigation, form submission, etc.)
- Lifecycle errors caught (provider updates during build)
- Tests use REAL widgets/components, REAL providers, REAL routes
- Only override providers that need hardware (camera, AR, GPS) — replace those with Mock implementations"		
								
						### LAYER 3 — INTEGRATION TESTS (PC, no device)		
						Tests with the full app mounted to confirm the most important flows user journeys end-to-end, state management along the journey, error paths (network failures, invalid data)		
						"Write 3 types:
1. Smoke tests — confirm all main components render without errors; add concise logs like ""ComponentX rendered correctly""
2. User journey tests — cover the 2–5 main flows for this feature (navigate → interact → assert)
3. Error path tests — what happens if the repository throws? If data is empty? If network fails?


"		
						"Location: .../domains/DOMAIN/test/integration
"		
						"How to test:
- Same as widget tests but with full app mounted
- Use `integration_test` package to run on real browser
- Mock external APIs (use fake HTTP responses)
Command: flutter test lib/domains/DOMAIN/test/integration/ --reporter=compact"		
						"Acceptance criteria:
- tests covering most critical user journeys
- Error paths tested (what if API fails?)
- Tests are deterministic (use mocked HTTP, not real API
- Full app mounted (MaterialApp.router + GoRouter + all real providers)
- Mock only hardware providers (camera, AR session, GPS)"		
								
						### RUN ALL 3 LAYERS TOGETHER:		
							to confirm all tests in all features are still ok.	
							run: flutter test lib/ --reporter=compact	
							» lets make sure that you atually have tests to confirm that all widgets are correctly displayed and in the correct position with the corret dimensions and style... and this for all widgets along the flow of possible actions user cand do with this phase implementation funtionalities. don't want to find any error in manual test later, that could be found in automated integration tests now.	
								
								
								
								
								
								
								
								
								
								
								
								
								
								
								
								
								
						### LAYER 4 — Browser Integration Tests		
						What: same tests as Layer 3 but running inside the real browser		
						Location: integration_test/FEATURE_NAME_test.dart		
						Uses: package:integration_test/integration_test.dart		
						Catches: Chrome rendering bugs (Impeller, Vulkan), real asset loading, real platform channels, screen density issues, spacing, overflows, errors and exceptions... 		
						"Run these tests on the chrome browser:
    flutter drive --driver=test_driver/integration_test.dart --target=integration_test/xxx_test.dart -d chrome > __out.txt 2>&1"		
						When fixing failures: understand the big picture first. Ask — is the test wrong, or is the logic wrong? Think 3 options, choose best, re-run. Repeat until 100%.		
								
								
						### LAYER 5 — Phone Integration Tests		
						What: run the same tests as layer 4 but now running inside the real phone device		
						For that, i already connected my phone in dev mode and connected to my PC through wireless and adb already is able to find it. So just find the phone and do a smoke test to confirm it is connected (try at least 3 times before giving up) 		
						Location: integration_test/FEATURE_NAME_test.dart		
						Uses: package:integration_test/integration_test.dart		
						Catches: Android rendering bugs (Impeller, Vulkan), real asset loading, real platform channels, screen density issues, spacing, overflows, errors and exceptions... 		
						run the app directly in my phone using adb to connct to it and then get the logs in the file __out.txt		
						When fixing failures: understand the big picture first. Ask — is the test wrong, or is the logic wrong? Think 3 options, choose best, re-run. Repeat until 100%.		
								
						» at the end of this layer 5 tests, do a deep analysis and confirm if the test are well automated and they run smooth as is, using my device directly, 		
						or if it would be better to use tools like Patrol or Maestro for device testing... would they bring any value or the test infrastructure we have is already good and well automated? 		
								
								
								
								
								
								
								
								
								
								
								
								
								
								
								
								
								
								
								
								
								
								
								
					You have unlimited time. Be thorough, not fast. Only stop when every layer is ✓			
								
								
	6 — Code writing							
		Do not create files in your memory and then dump them, because you will run out of memory.						
		Instead create the file first and add there the text directly so if you get interrupted we don't lose all the work						
								
	7 — Terminal							
		PowerShell → use ; not &&						
		"Don't apply pipes or filters directly to terminal (ex 2>&1 | findstr /C:""..."" )
Send the logs to a file __out.txt and then you check the logs there in the file (> __out.txt  2>&1)"						
								
	8 — On Finishing							
		Update the phase guide document to mark the things that were done and some update of some change we did because when implementing things we found out those changes were needed or better						
			C:\Users\franc\Desktop\TileStories_App\PROJECT_GUIDES\PHASE_2_PLAN.md					
			.\PROJECT_GUIDES\PROJECT_GUIDE.md					
		And then give a short chat summary: what was implemented, in which file/folder, and what the data flow is. No summary files.						
								
	So create a Compreensive TODO list of tasks to do so you don't skip any step							
	Take your time. You have unlimited time and tokens. So I want quality over speed. Be good, not fast. 							