# Feedback & States Guide
> Components: `lib/components/feedback/` — Import: `design_system.dart`

---

## Decision Table

| Situation                   | Use                                          |
| --------------------------- | -------------------------------------------- |
| Content loading             | `SkeletonLoader` matching content shape      |
| Button/action loading       | `CircularProgressIndicator` inline in button |
| Known progress              | `LinearProgressIndicator(value: 0.0–1.0)`    |
| Success (non-critical)      | `SnackBar` (3s auto-dismiss)                 |
| Success with undo           | `SnackBar` with `SnackBarAction` (5s)        |
| Delete / destructive action | `AlertDialog` confirmation first             |
| Fatal error                 | `AlertDialog`                                |
| Retryable error             | `ErrorDisplay` with retry button             |
| Empty list/results          | `EmptyState` with recovery action            |
| Form validation             | Inline text under field, immediate           |

---

## State Transitions — Always Animated

Never snap between states. Use `AnimatedSwitcher` for all state changes.

```dart
AnimatedSwitcher(
  duration: AnimationTokens.medium,
  switchInCurve: AnimationTokens.easeOut,
  switchOutCurve: AnimationTokens.easeIn,
  child: switch ((isLoading, items.isEmpty, hasError)) {
    (true, _, _) => SkeletonList(key: const ValueKey('skeleton')),
    (_, _, true) => ErrorDisplay(key: const ValueKey('error'), ...),
    (_, true, _) => EmptyState(key: const ValueKey('empty'), ...),
    _            => ContentWidget(key: const ValueKey('content'), ...),
  },
)
```

---

## Available Components

### `AsyncValueBuilder<T>` — preferred for FutureProvider/StreamProvider
```dart
AsyncValueBuilder<List<Item>>(
  value: ref.watch(itemsProvider),
  data: (items) => ItemList(items: items),
  loadingMessage: ref.tr(t(pt: 'A carregar...', en: 'Loading...')),
  onRetry: () => ref.invalidate(itemsProvider),
  checkEmpty: (items) => items.isEmpty,
  emptyMessage: ref.tr(t(pt: 'Sem itens', en: 'No items')),
)
```

### `ErrorDisplay` — retryable errors
```dart
ErrorDisplay(
  message: ref.tr(t(pt: 'Erro ao carregar', en: 'Failed to load')),
  onRetry: () => ref.invalidate(dataProvider),
)
```

### `EmptyState` — no data
```dart
EmptyState(
  icon: Icons.explore_outlined,
  message: ref.tr(t(pt: 'Nenhum resultado', en: 'No results found')),
  action: EmptyStateAction(
    label: ref.tr(t(pt: 'Limpar filtros', en: 'Clear filters')),
    onPressed: () => ref.read(filtersProvider.notifier).clear(),
  ),
)
```

> Empty states must always have a recovery action. They are designed, staggered moments — see `08_MOTION_AND_FEEL.md`.

---

## CRUD Patterns

### Save / Create
```dart
Future<void> save() async {
  setState(() => _isSaving = true);
  try {
    await ref.read(provider.notifier).save(item);
    if (mounted) context.showSnackBar(
      ref.tr(t(pt: 'Guardado com sucesso', en: 'Saved successfully')),
    );
  } catch (_) {
    if (mounted) context.showSnackBar(
      ref.tr(t(pt: 'Erro ao guardar', en: 'Failed to save')),
    );
  } finally {
    if (mounted) setState(() => _isSaving = false);
  }
}

// Button reflects state
AnimatedSwitcher(
  duration: AnimationTokens.medium,
  child: _isSaving
      ? FilledButton(
          key: const ValueKey('loading'),
          onPressed: null,
          child: const SizedBox(
            width: 18, height: 18,
            child: CircularProgressIndicator(strokeWidth: 2),
          ),
        )
      : FilledButton(
          key: const ValueKey('save'),
          onPressed: save,
          child: Text(ref.tr(t(pt: 'Guardar', en: 'Save'))),
        ),
)
```

### Delete — confirm first
```dart
Future<void> delete() async {
  final confirmed = await showDialog<bool>(
    context: context,
    builder: (context) => AlertDialog(
      title: Text(ref.tr(t(pt: 'Confirmar eliminação', en: 'Confirm deletion'))),
      content: Text(ref.tr(t(pt: 'Esta ação não pode ser desfeita.', en: 'This cannot be undone.'))),
      actions: [
        TextButton(
          onPressed: () => Navigator.pop(context, false),
          child: Text(ref.tr(t(pt: 'Cancelar', en: 'Cancel'))),
        ),
        FilledButton(
          style: FilledButton.styleFrom(backgroundColor: context.error),
          onPressed: () => Navigator.pop(context, true),
          child: Text(ref.tr(t(pt: 'Eliminar', en: 'Delete'))),
        ),
      ],
    ),
  );
  if (confirmed == true) await ref.read(provider.notifier).delete(id);
}
```

### Undo pattern (delete with snackbar)
```dart
context.showSnackBar(
  ref.tr(t(pt: 'Item removido', en: 'Item removed')),
  action: SnackBarAction(
    label: ref.tr(t(pt: 'Desfazer', en: 'Undo')),
    onPressed: () => ref.read(provider.notifier).restore(item),
  ),
);
// SnackBar duration: 5s when undo is available
```

### Optimistic update
```dart
Future<void> toggleFavorite() async {
  final previous = ref.read(favoritesProvider);
  ref.read(favoritesProvider.notifier).toggle(id); // immediate UI update
  try {
    await api.toggleFavorite(id);
  } catch (_) {
    ref.read(favoritesProvider.notifier).state = previous; // revert
    if (mounted) context.showSnackBar(
      ref.tr(t(pt: 'Erro ao atualizar', en: 'Update failed')),
    );
  }
}
```

---

## Form Validation

```dart
TextFormField(
  autovalidateMode: AutovalidateMode.onUserInteraction, // always inline, never wait for submit
  validator: (v) => (v?.isEmpty ?? true)
      ? ref.tr(t(pt: 'Campo obrigatório', en: 'Required field'))
      : null,
)
```

---

## ❌ Never

```dart
// Loading content with a spinner
if (isLoading) return const CircularProgressIndicator(); // use SkeletonLoader

// Raw error text
Text('Error: ${error.toString()}') // use ErrorDisplay

// State change without animation
if (isLoading) return LoadingWidget(); // use AnimatedSwitcher

// Hardcoded text
SnackBar(content: Text('Saved!')) // use ref.tr(t(...))

// Blocking dialog for fast operations — use inline button loading state instead
showDialog(...AlertDialog(content: CircularProgressIndicator()))
```