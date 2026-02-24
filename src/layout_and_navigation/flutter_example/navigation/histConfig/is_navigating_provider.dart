import 'package:flutter_riverpod/flutter_riverpod.dart';

// =============================================================================
// IS-NAVIGATING FLAG
// =============================================================================
//
// WHY THIS EXISTS:
// go_router's NavigatorObserver has no access to Riverpod [ref], so it cannot
// distinguish "user clicked a link" from "browser back/forward" on its own.
// The redirect callback does have [ref], but it fires for ALL navigations —
// including browser BACK — with no built-in way to tell them apart.
//
// SOLUTION: Nav widgets call `ref.read(isNavigatingProvider.notifier).set(true)`
// immediately before calling `context.go(path)`.  The router's redirect reads
// this flag (consuming/resetting it to false in the same call) and uses it to
// decide push() vs undoRedo().
//
// Because Riverpod state lives outside go_router, it is NOT carried forward
// into back-navigation events — browser BACK always sees the flag as false.

final isNavigatingProvider = NotifierProvider<IsNavigatingNotifier, bool>(
  IsNavigatingNotifier.new,
);

class IsNavigatingNotifier extends Notifier<bool> {
  @override
  bool build() => false;

  /// Call this BEFORE context.go(path) to mark the navigation as intentional.
  void set(bool value) => state = value;

  /// Read the flag and immediately reset it to false.
  /// Used by the router redirect — consuming the flag prevents it from sticking.
  bool consume() {
    final v = state;
    if (v) {
      state = false; // only mutate when needed — avoids spurious notifications
    }
    return v;
  }
}
