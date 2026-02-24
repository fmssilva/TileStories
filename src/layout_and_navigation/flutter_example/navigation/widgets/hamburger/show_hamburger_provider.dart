import 'package:flutter_riverpod/flutter_riverpod.dart';

// =============================================================================
// HAMBURGER MENU VISIBILITY PROVIDER
// =============================================================================

// -----------------------------------------------------------------------------
// PROVIDER PATTERN CHOICE: NotifierProvider vs StateProvider
// -----------------------------------------------------------------------------
//
// This project uses **NotifierProvider** for this provider. Here's why:
//
// ## NotifierProvider (USED HERE)
// **Best for:** State with business logic, validation, or multiple update methods
// **Characteristics:**
//   - Explicit methods (e.g., setVisible(), toggle())
//   - Encapsulates business logic within the Notifier class
//   - Better for complex state that may grow over time
//   - More testable (can test Notifier methods independently)
//   - Self-documenting (method names explain what they do)
//
// **Example:**
//   ```dart
//   final provider = NotifierProvider<MyNotifier, int>(MyNotifier.new);
//   ref.read(provider.notifier).increment();  // Explicit method
//   ```
//
// ## StateProvider (NOT USED)
// **Best for:** Simple values that get directly set from UI
// **Characteristics:**
//   - Direct state access: `ref.read(provider.notifier).state = newValue`
//   - No encapsulation - any code can mutate state
//   - Good for quick prototypes or trivial state
//   - Less type-safe (easy to set invalid values)
//
// **Example:**
//   ```dart
//   final provider = StateProvider<int>((ref) => 0);
//   ref.read(provider.notifier).state = 42;  // Direct assignment
//   ```
//
// ## Decision Rationale
// We chose **NotifierProvider** because:
// 1. **Consistency**: Matches nav_history_provider.dart pattern
// 2. **Clarity**: `setVisible(bool)` is clearer than `state = bool`
// 3. **Extensibility**: Can easily add `toggle()`, `reset()` methods later
// 4. **Validation**: Could add checks in setVisible() if needed
// 5. **Best Practice**: Riverpod team recommends NotifierProvider over StateProvider
//
// ## When to Use Each
// - Use **NotifierProvider** when:
//   - State has business logic or validation
//   - Multiple ways to update state (increment/decrement, add/remove, etc.)
//   - State management will grow in complexity
//   - You want explicit, named methods
//
// - Use **StateProvider** when:
//   - Prototyping or MVP
//   - Truly simple state (like a slider value)
//   - No business logic whatsoever
//   - Temporary state that will be replaced later
//
// -----------------------------------------------------------------------------

/// Main provider for hamburger menu visibility
/// Manages whether hamburger button should be shown based on available width
final showHamburgerProvider = NotifierProvider<ShowHamburgerNotifier, bool>(
  ShowHamburgerNotifier.new,
);

///
/// Show Hamburger Notifier
///
/// Controls whether the hamburger menu should be displayed in the UI.
/// This is automatically managed by NavTabsRow based on available width.
///
/// BEHAVIOR:
/// - Set to `true` when nav tabs don't fit in available width (< 2 tabs)
/// - Set to `false` when nav tabs can be displayed in header
///
/// USAGE:
/// ```dart
/// // Reading the state
/// final showHamburger = ref.watch(showHamburgerProvider);
/// if (showHamburger) {
///   return const Hamburger();
/// }
///
/// // Setting the state (in NavTabsRow)
/// ref.read(showHamburgerProvider.notifier).setVisible(true);
/// ```
///
/// WHO USES THIS:
/// - NavTabsRow: Sets this based on responsive width calculation
/// - Header/Layout: Reads this to conditionally show hamburger button
/// - Mobile views: May override to always show hamburger
///
class ShowHamburgerNotifier extends Notifier<bool> {
  @override
  bool build() {
    // Default: don't show hamburger (tabs will be shown)
    // This will be updated by NavTabsRow based on actual available width
    return false;
  }

  /// Update hamburger visibility
  void setVisible(bool visible) {
    if (state != visible) {
      state = visible;
    }
  }
}
