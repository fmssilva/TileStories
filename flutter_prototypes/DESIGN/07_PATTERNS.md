# Patterns & Recipes
> Copy-paste starting points for common UI tasks.
> Every pattern includes its motion wrapper — static code alone is not enough.

---

## New Page (standard)

```dart
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../design/design_system.dart';
import '../../../layout/layout_manager.dart';
import '../../../layout/layout_presets.dart';

class MyNewPage extends ConsumerWidget {
  const MyNewPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return LayoutManager(
      slots: LayoutPresets.defaultPageBrowser(
        context: context,
        body: _buildBody(context),
      ),
    );
  }

  Widget _buildBody(BuildContext context) {
    return ResponsiveContainer(
      // Wrap body in entrance animation — never let content snap in
      child: _PageEntrance(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Page Title', style: context.headlineMedium),
            SizedBox(height: Spacing.lg),
            // content here
          ],
        ),
      ),
    );
  }
}

// Standard page entrance — fade + slight upward translate
class _PageEntrance extends StatefulWidget {
  final Widget child;
  const _PageEntrance({required this.child});

  @override
  State<_PageEntrance> createState() => _PageEntranceState();
}

class _PageEntranceState extends State<_PageEntrance>
    with SingleTickerProviderStateMixin {
  late final AnimationController _ctrl = AnimationController(
    vsync: this, duration: AnimationTokens.slow,
  )..forward();

  late final _opacity = Tween(begin: 0.0, end: 1.0).animate(
    CurvedAnimation(parent: _ctrl, curve: AnimationTokens.easeOut),
  );
  late final _slide = Tween(begin: const Offset(0, 0.05), end: Offset.zero).animate(
    CurvedAnimation(parent: _ctrl, curve: AnimationTokens.easeOut),
  );

  @override
  void dispose() { _ctrl.dispose(); super.dispose(); }

  @override
  Widget build(BuildContext context) => FadeTransition(
    opacity: _opacity,
    child: SlideTransition(position: _slide, child: widget.child),
  );
}
```

---

## Card

```dart
// Stateful — handles hover + press feedback
class AppCard extends StatefulWidget {
  final Widget child;
  final VoidCallback? onTap;
  const AppCard({required this.child, this.onTap, super.key});

  @override
  State<AppCard> createState() => _AppCardState();
}

class _AppCardState extends State<AppCard> {
  bool _hovered = false;
  bool _pressed = false;

  @override
  Widget build(BuildContext context) {
    return MouseRegion(
      onEnter: (_) => setState(() => _hovered = true),
      onExit: (_) => setState(() => _hovered = false),
      child: GestureDetector(
        onTapDown: (_) => setState(() => _pressed = true),
        onTapUp: (_) { setState(() => _pressed = false); widget.onTap?.call(); },
        onTapCancel: () => setState(() => _pressed = false),
        child: AnimatedScale(
          scale: _pressed ? 0.97 : 1.0,
          duration: AnimationTokens.fast,
          curve: AnimationTokens.easeOut,
          child: AnimatedContainer(
            duration: AnimationTokens.medium,
            curve: AnimationTokens.easeOut,
            transform: Matrix4.translationValues(0, _hovered && !_pressed ? -3 : 0, 0),
            decoration: BoxDecoration(
              color: context.surface,
              borderRadius: RadiusTokens.cardRadius,
              border: Border.all(
                color: context.isDarkMode
                    ? context.tertiary.withValues(alpha: _hovered ? 0.18 : 0.08)
                    : context.outline.withValues(alpha: _hovered ? 0.8 : 0.5),
                width: 1,
              ),
              boxShadow: [
                BoxShadow(
                  color: Colors.black.withValues(alpha: _hovered ? 0.10 : 0.06),
                  blurRadius: _hovered ? 16 : 2,
                  offset: Offset(0, _hovered ? 6 : 1),
                ),
                BoxShadow(
                  color: context.primary.withValues(alpha: _hovered ? 0.06 : 0),
                  blurRadius: 20,
                  offset: const Offset(0, 8),
                ),
              ],
            ),
            child: Padding(
              padding: EdgeInsets.all(Spacing.lg),
              child: widget.child,
            ),
          ),
        ),
      ),
    );
  }
}

// Usage:
AppCard(
  onTap: () => _onCardTap(),
  child: Column(
    crossAxisAlignment: CrossAxisAlignment.start,
    children: [
      Text(title, style: context.titleMedium),
      SizedBox(height: Spacing.xs),
      Text(subtitle, style: context.bodySmall.copyWith(
        color: context.onSurfaceVariant,
      )),
    ],
  ),
)
```

---

## Form

```dart
ResponsiveContainer(
  contentType: ContentType.form,
  child: Form(
    key: _formKey,
    child: Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Text('Form Title', style: context.headlineSmall),
        SizedBox(height: Spacing.xl2),

        // Fields animate in inline validation — theme handles focus animation
        TextFormField(
          decoration: const InputDecoration(labelText: 'Email'),
          validator: (v) => v!.isEmpty ? 'Required' : null,
          // Validation feedback is immediate (onChanged), never wait for submit
          autovalidateMode: AutovalidateMode.onUserInteraction,
        ),
        SizedBox(height: Spacing.md),

        TextFormField(
          decoration: const InputDecoration(labelText: 'Password'),
          obscureText: true,
          autovalidateMode: AutovalidateMode.onUserInteraction,
        ),
        SizedBox(height: Spacing.xl2),

        // Button shows loading state — never goes silent after tap
        AnimatedSwitcher(
          duration: AnimationTokens.medium,
          child: _isSubmitting
              ? FilledButton(
                  key: const ValueKey('loading'),
                  onPressed: null,
                  child: const SizedBox(
                    width: 20, height: 20,
                    child: CircularProgressIndicator(strokeWidth: 2),
                  ),
                )
              : FilledButton(
                  key: const ValueKey('submit'),
                  onPressed: _submit,
                  child: const Text('Submit'),
                ),
        ),
        SizedBox(height: Spacing.sm),
        TextButton(onPressed: _cancel, child: const Text('Cancel')),
      ],
    ),
  ),
)
```

> `autovalidateMode: AutovalidateMode.onUserInteraction` — inline validation always, never wait for submit.

---

## List / Feed (staggered entrance)

```dart
// Items animate in with staggered delay
ListView.separated(
  padding: EdgeInsets.all(Spacing.lg),
  itemCount: items.length,
  separatorBuilder: (_, __) => SizedBox(height: Spacing.sm),
  itemBuilder: (context, index) {
    return _StaggeredItem(
      index: index,
      child: _buildListItem(context, items[index]),
    );
  },
)

Widget _buildListItem(BuildContext context, MyItem item) {
  return AppCard(
    onTap: () => _onItemTap(item),
    child: ListTile(
      contentPadding: EdgeInsets.zero,
      leading: Icon(Icons.place, color: context.primary),
      title: Text(item.name, style: context.titleMedium),
      subtitle: Text(item.description, style: context.bodySmall),
      trailing: Icon(Icons.chevron_right, color: context.onSurfaceVariant),
    ),
  );
}

// Staggered entrance wrapper
class _StaggeredItem extends StatefulWidget {
  final int index;
  final Widget child;
  const _StaggeredItem({required this.index, required this.child});

  @override
  State<_StaggeredItem> createState() => _StaggeredItemState();
}

class _StaggeredItemState extends State<_StaggeredItem>
    with SingleTickerProviderStateMixin {
  late final AnimationController _ctrl = AnimationController(
    vsync: this, duration: AnimationTokens.slow,
  );

  @override
  void initState() {
    super.initState();
    // Cap stagger at 400ms total so long lists don't feel sluggish
    final delay = Duration(milliseconds: (widget.index * 50).clamp(0, 400));
    Future.delayed(delay, () { if (mounted) _ctrl.forward(); });
  }

  @override
  void dispose() { _ctrl.dispose(); super.dispose(); }

  @override
  Widget build(BuildContext context) {
    return FadeTransition(
      opacity: Tween(begin: 0.0, end: 1.0).animate(
        CurvedAnimation(parent: _ctrl, curve: AnimationTokens.easeOut),
      ),
      child: SlideTransition(
        position: Tween(begin: const Offset(0, 0.06), end: Offset.zero).animate(
          CurvedAnimation(parent: _ctrl, curve: AnimationTokens.easeOut),
        ),
        child: widget.child,
      ),
    );
  }
}
```

---

## Responsive Grid (staggered)

```dart
LayoutBuilder(
  builder: (context, constraints) {
    final cols = Breakpoints.getCardColumns(constraints.maxWidth);
    return GridView.builder(
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: cols,
        crossAxisSpacing: Spacing.md,
        mainAxisSpacing: Spacing.md,
        childAspectRatio: 1.2,
      ),
      itemCount: items.length,
      itemBuilder: (_, i) => _StaggeredItem(
        index: i,
        child: MyCard(item: items[i]),
      ),
    );
  },
)
```

---

## Status / Feedback Banner

```dart
// Animates in from top — never snaps in
class _FeedbackBanner extends StatefulWidget {
  final Color bgColor;
  final Color textColor;
  final IconData icon;
  final String message;

  @override
  State<_FeedbackBanner> createState() => _FeedbackBannerState();
}

class _FeedbackBannerState extends State<_FeedbackBanner>
    with SingleTickerProviderStateMixin {
  late final AnimationController _ctrl = AnimationController(
    vsync: this, duration: AnimationTokens.medium,
  )..forward();

  @override
  void dispose() { _ctrl.dispose(); super.dispose(); }

  @override
  Widget build(BuildContext context) {
    return FadeTransition(
      opacity: Tween(begin: 0.0, end: 1.0).animate(
        CurvedAnimation(parent: _ctrl, curve: AnimationTokens.easeOut),
      ),
      child: SlideTransition(
        position: Tween(begin: const Offset(0, -0.3), end: Offset.zero).animate(
          CurvedAnimation(parent: _ctrl, curve: AnimationTokens.easeOut),
        ),
        child: Container(
          padding: EdgeInsets.all(Spacing.md),
          decoration: BoxDecoration(
            color: widget.bgColor,
            borderRadius: RadiusTokens.radiusMd,
          ),
          child: Row(
            spacing: Spacing.sm,
            children: [
              Icon(widget.icon, color: widget.textColor),
              Text(widget.message,
                style: context.bodyMedium.copyWith(color: widget.textColor)),
            ],
          ),
        ),
      ),
    );
  }
}

// Usage:
_FeedbackBanner(
  bgColor: context.successContainer,
  textColor: context.onSuccess,
  icon: Icons.check_circle,
  message: 'Saved!',
)
// Warning: warningContainer / onWarning / Icons.warning_amber
// Error:   colors.errorContainer / context.error / Icons.error_outline
```

---

## Loading / Empty / Error States

```dart
// ─── Loading — skeleton, not spinner ───────────────────────────────────────
// Match the skeleton shape to your actual content layout
class _SkeletonList extends StatefulWidget {
  @override
  State<_SkeletonList> createState() => _SkeletonListState();
}

class _SkeletonListState extends State<_SkeletonList>
    with SingleTickerProviderStateMixin {
  late final AnimationController _shimmer = AnimationController(
    vsync: this,
    duration: const Duration(milliseconds: 1200),
  )..repeat(reverse: true);

  @override
  void dispose() { _shimmer.dispose(); super.dispose(); }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: _shimmer,
      builder: (context, _) {
        final alpha = lerpDouble(0.06, 0.14, _shimmer.value)!;
        final color = context.onSurface.withValues(alpha: alpha);
        return Column(
          children: List.generate(4, (i) => Padding(
            padding: EdgeInsets.only(bottom: Spacing.sm),
            child: Container(
              height: 72,
              decoration: BoxDecoration(
                color: color,
                borderRadius: RadiusTokens.cardRadius,
              ),
            ),
          )),
        );
      },
    );
  }
}

// ─── Crossfade between states ───────────────────────────────────────────────
AnimatedSwitcher(
  duration: AnimationTokens.medium,
  switchInCurve: AnimationTokens.easeOut,
  switchOutCurve: AnimationTokens.easeIn,
  child: switch ((isLoading, items.isEmpty, hasError)) {
    (true, _, _)  => const _SkeletonList(key: ValueKey('skeleton')),
    (_, _, true)  => _ErrorState(key: const ValueKey('error'), onRetry: _load),
    (_, true, _)  => const _EmptyState(key: ValueKey('empty')),
    _             => _ContentList(key: const ValueKey('content'), items: items),
  },
)

// ─── Empty state — designed, staggered, with recovery action ────────────────
class _EmptyState extends StatefulWidget {
  @override
  State<_EmptyState> createState() => _EmptyStateState();
}

class _EmptyStateState extends State<_EmptyState>
    with SingleTickerProviderStateMixin {
  late final AnimationController _ctrl = AnimationController(
    vsync: this, duration: AnimationTokens.slow,
  )..forward();

  @override
  void dispose() { _ctrl.dispose(); super.dispose(); }

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        spacing: Spacing.md,
        children: [
          // Icon: scale in first
          FadeTransition(
            opacity: Tween(begin: 0.0, end: 1.0).animate(
              CurvedAnimation(parent: _ctrl,
                curve: const Interval(0.0, 0.6, curve: Curves.easeOut)),
            ),
            child: ScaleTransition(
              scale: Tween(begin: 0.7, end: 1.0).animate(
                CurvedAnimation(parent: _ctrl,
                  curve: const Interval(0.0, 0.6, curve: Curves.easeOut)),
              ),
              child: Icon(Icons.explore_outlined,
                size: SizeTokens.iconXl, color: context.onSurfaceVariant),
            ),
          ),
          // Text: fades in after icon
          FadeTransition(
            opacity: Tween(begin: 0.0, end: 1.0).animate(
              CurvedAnimation(parent: _ctrl,
                curve: const Interval(0.3, 0.8, curve: Curves.easeOut)),
            ),
            child: Text('Nothing discovered yet',
              style: context.bodyMedium.copyWith(color: context.onSurfaceVariant)),
          ),
          // Action: last to appear
          FadeTransition(
            opacity: Tween(begin: 0.0, end: 1.0).animate(
              CurvedAnimation(parent: _ctrl,
                curve: const Interval(0.5, 1.0, curve: Curves.easeOut)),
            ),
            child: FilledButton(
              onPressed: widget.onAction,
              child: const Text('Start exploring'),
            ),
          ),
        ],
      ),
    );
  }
}

// ─── Error state — same stagger, always has retry ───────────────────────────
// Same structure as _EmptyState — swap icon to Icons.error_outline,
// color to context.error, message and button label to match context.
```

---

## Button Variants

```dart
// Primary action
FilledButton(onPressed: onSave, child: const Text('Save'))

// Secondary action
OutlinedButton(onPressed: onCancel, child: const Text('Cancel'))

// Ghost / text
TextButton(onPressed: onSkip, child: const Text('Skip'))

// Floating CTA
ElevatedButton(onPressed: onAction, child: const Text('Explore'))

// Icon button (min 48×48 enforced by theme)
IconButton(onPressed: onClose, icon: const Icon(Icons.close))
```

> All button styles come from the theme. **Do not** pass `style:` unless a one-off exception.
> Buttons have press scale (0.97) built into the theme — verify this is active in `app_theme.dart`.

---

## Snackbar

```dart
// Simple
context.showSnackBar('Settings saved');

// With action
context.showSnackBar(
  'Item deleted',
  action: SnackBarAction(label: 'Undo', onPressed: _undo),
);
```

---

## Checklist Before Submitting

- [ ] No hardcoded colors, sizes, spacing, or fonts
- [ ] Page uses `LayoutManager` with `LayoutPresets` or `LayoutSlots`
- [ ] `context.*` shortcuts used for colors and text styles
- [ ] Page body wrapped in `_PageEntrance` (fade + translate)
- [ ] List items use `_StaggeredItem` for entrance animation
- [ ] Cards use `AppCard` (or equivalent) with hover lift + press scale
- [ ] Loading state uses skeleton loader matching content shape — no `CircularProgressIndicator` for content areas
- [ ] State transitions (loading↔content↔empty↔error) use `AnimatedSwitcher`
- [ ] Forms use `autovalidateMode: AutovalidateMode.onUserInteraction`
- [ ] Buttons show loading state when async action is in progress
- [ ] Responsive: tested `isMobile`/`isTablet`/`isDesktop` breakpoints
- [ ] Tap targets ≥ 48px
- [ ] `semanticLabel` on images; `Semantics(button: true)` on custom tappable widgets
- [ ] Empty and error states are designed, staggered, and have recovery actions