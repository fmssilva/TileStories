import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'layout_slots.dart';
import 'platform_info.dart';
import 'widgets/back_to_top_button.dart';
import '../navigation/histConfig/history_provider.dart';
import 'scrollController/scroll_registry.dart';
import 'scrollController/scroll_registry_provider.dart';
import 'pageState/page_state_registry.dart';
import 'pageState/page_state_registry_provider.dart';

/// Main layout orchestrator that renders a page based on [LayoutSlots] configuration.
///
/// RESPONSIBILITY:
/// 1. Set system UI mode and orientation
/// 2. Build the body with optional scroll, safe area, and loading overlay
/// 3. Handle FAB positioning (corner vs. side in landscape app mode)
/// 4. Wrap everything in a Scaffold with the configured header/footer
/// 5. Manage the [ScrollRegistry] lifecycle for scroll position restoration
/// 6. Manage the [PageStateRegistry] lifecycle for arbitrary page state restoration
///
/// BODY PIPELINE ORDER (innermost → outermost):
///   body → SingleChildScrollView (if scrollable) → SafeArea (if safeArea)
///        → loading Stack (if isLoading) → FAB Stack (if landscape app)
///        → back-to-top Stack (if showBackToTop)
///        → PageStateRegistryProvider → ScrollRegistryProvider → Scaffold
///
/// SCROLL RESTORATION:
/// - [initState]: creates [ScrollRegistry] with saved positions from history
/// - Widgets below use [ScrollRegistryProvider.of(context).controller(id)]
/// - [dispose]: saves all cached positions back to history, then disposes registry
///
/// PAGE STATE RESTORATION:
/// - [initState]: creates [PageStateRegistry] with saved state from history
/// - Widgets below use [PageStateRegistryProvider.of(context)]
///   to read initial values (get) and write updates (set)
/// - [dispose]: saves the snapshot back to history
///
class LayoutManager extends ConsumerStatefulWidget {
  final LayoutSlots slots;

  // Constructor
  const LayoutManager({super.key, required this.slots});

  @override
  ConsumerState<LayoutManager> createState() => _LayoutManagerState();
}

class _LayoutManagerState extends ConsumerState<LayoutManager> {
  // ─────────────────────────────────────────────────────────────────────────
  // Scroll registry — created in initState once ref is available
  // ─────────────────────────────────────────────────────────────────────────
  late final ScrollRegistry _registry;

  // Page state registry — created in initState once ref is available
  late final PageStateRegistry _pageStateRegistry;

  // Captured in initState so dispose() can use them without ref.
  // ref is unsafe after the widget is unmounted / the ProviderContainer is disposed.
  late final NavHistoryNotifier _historyNotifier;

  // The history index at which THIS page was loaded.
  // By the time dispose() runs, currentIndex has already moved to the next page.
  // We must save scroll positions to THIS index (the page being left), not to currentIndex.
  // Starts at -1 and is set in the first postFrameCallback after redirect fires.
  int _myHistoryIndex = -1;

  /// ID used by LayoutManager's own main-page scroll controller
  static const String _pageScrollId = 'page';

  @override
  void initState() {
    super.initState();
    _historyNotifier = ref.read(navHistoryProvider.notifier);

    // Create both registries with empty saved state initially.
    // We cannot read history here: the redirect's push/undoRedo is deferred to
    // addPostFrameCallback, so history is not yet updated when initState runs.
    _registry = ScrollRegistry(savedPositions: const {});
    _pageStateRegistry = PageStateRegistry();

    // After the first frame: the redirect's postFrameCallback has fired,
    // history is updated (push/undoRedo ran, which also flushed any staged
    // saves from the PREVIOUS page's dispose). Now:
    //   1. Capture our own history index.
    //   2. Restore scroll positions and page state for THIS page.
    //
    // WHY postFrameCallback (not inside initState directly)?
    // initState runs BEFORE the router's redirect postFrameCallback, so history
    // isn't updated yet when initState runs. We must wait for the same frame
    // boundary the router uses. Both callbacks fire after the same frame build,
    // but redirect's callback was scheduled first so it runs first.
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted) return;

      // Step 1: Capture our history index (now valid after redirect's push/undoRedo).
      _myHistoryIndex = ref.read(navHistoryProvider).currentIndex;

      // Step 2: Restore scroll positions.
      final savedPositions = _historyNotifier.getScrollPositions() ?? {};
      if (savedPositions.isNotEmpty) {
        _registry.restorePositions(savedPositions);
      }

      // Step 3: Restore page state.
      final savedPageState = _historyNotifier.getPageState() ?? {};
      if (savedPageState.isNotEmpty) {
        _pageStateRegistry.restore(savedPageState);
        if (mounted) setState(() {});
      }
    });
  }

  @override
  void dispose() {
    // Save cached scroll positions to THIS page's history entry (not currentIndex,
    // which has already moved to the new page by the time dispose() runs).
    //
    // TIMING: dispose() fires during Flutter's finalizeTree phase (inside buildScope).
    // Riverpod (debug mode) throws AssertionError if `state =` is called here.
    // Using addPostFrameCallback from dispose() leaves a pending callback at test
    // teardown, causing "A Timer is still pending" failures.
    //
    // SOLUTION: Call _historyNotifier.stageSave() — writes only to a plain Dart Map
    // on the notifier object, not to Riverpod `state`. The staged data is then
    // written into history state at the start of the NEXT push()/undoRedo() call,
    // which always runs from a safe redirect postFrameCallback.
    final positions = _registry.cachedPositions;
    final pageStateSnapshot = _pageStateRegistry.snapshot;
    final indexToSave = _myHistoryIndex;

    _historyNotifier.stageSave(indexToSave, positions, pageStateSnapshot);

    // Dispose all ScrollControllers in the registry (safe to do immediately)
    _registry.dispose();

    super.dispose();
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Back-to-top helper
  // ─────────────────────────────────────────────────────────────────────────

  void _scrollToTop() {
    _registry
        .controller(_pageScrollId)
        .animateTo(
          0,
          duration: const Duration(milliseconds: 500),
          curve: Curves.easeOutCubic,
        );
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Build
  // ─────────────────────────────────────────────────────────────────────────

  @override
  Widget build(BuildContext context) {
    final slots = widget.slots;

    // STEP 1: Configure system UI mode (normal, immersive, etc.)
    if (slots.systemUiMode != null) {
      SystemChrome.setEnabledSystemUIMode(slots.systemUiMode!);
    }

    // STEP 2: Lock screen orientation if requested
    if (slots.lockedOrientation != null) {
      SystemChrome.setPreferredOrientations([slots.lockedOrientation!]);
    } else {
      SystemChrome.setPreferredOrientations([]);
    }

    // STEP 3: Build the body with progressive wrapping
    Widget bodyWidget = slots.body;

    // Add the main-page scroll controller so scroll position is tracked.
    // Other scrollable widgets (inner lists, panels) register their own
    // controllers via ScrollRegistryProvider.of(context).controller('my-id').
    if (slots.scrollable) {
      bodyWidget = SingleChildScrollView(
        controller: _registry.controller(_pageScrollId),
        child: bodyWidget,
      );
    }

    // Respect device safe areas (notch, system bars)
    if (slots.safeArea) {
      bodyWidget = SafeArea(child: bodyWidget);
    }

    // Show loading overlay on top of content
    if (slots.isLoading) {
      bodyWidget = Stack(
        children: [
          bodyWidget,
          const Positioned.fill(
            child: ColoredBox(
              color: Color(0x88000000),
              child: Center(child: CircularProgressIndicator()),
            ),
          ),
        ],
      );
    }

    // STEP 4: Handle FAB — in landscape app mode move it to the side
    // (corner FABs get hidden behind content in landscape native apps)
    Widget? scaffoldFab = slots.fab;
    final bool isLandscapeApp =
        PlatformInfo.isLandscape(context) && PlatformInfo.isApp(context);

    if (isLandscapeApp && slots.fab != null) {
      bodyWidget = Stack(
        children: [
          Positioned.fill(child: bodyWidget),
          Positioned(
            right: 16,
            top: 0,
            bottom: 0,
            child: Center(child: slots.fab!),
          ),
        ],
      );
      scaffoldFab = null; // FAB is now inside the body Stack
    }

    // STEP 5: Add back-to-top button overlay
    if (slots.showBackToTop && slots.scrollable) {
      bodyWidget = Stack(
        children: [
          Positioned.fill(child: bodyWidget),
          Positioned(
            bottom: 80,
            right: 16,
            child: BackToTopButton(onPressed: _scrollToTop),
          ),
        ],
      );
    }

    // STEP 6: Provide the PageStateRegistry and ScrollRegistry to all descendant widgets
    bodyWidget = PageStateRegistryProvider(
      registry: _pageStateRegistry,
      child: bodyWidget,
    );
    bodyWidget = ScrollRegistryProvider(registry: _registry, child: bodyWidget);

    // STEP 7: Assemble the Scaffold
    return Scaffold(
      appBar: slots.header,
      body: bodyWidget,
      floatingActionButton: scaffoldFab,
      floatingActionButtonLocation: FloatingActionButtonLocation.endFloat,
      backgroundColor: slots.backgroundColor,
      resizeToAvoidBottomInset: slots.resizeForKeyboard,
      bottomNavigationBar: slots.footer,
    );
  }
}
