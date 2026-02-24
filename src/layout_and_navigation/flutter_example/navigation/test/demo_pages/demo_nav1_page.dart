import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../design/design_system.dart';
import '../../../utils/i18n/extensions/context_extensions.dart';
import '../../../utils/i18n/models/translatable_string.dart';
import '../../../layout/layout_manager.dart';
import '../../../layout/layout_presets.dart';
import '../../../layout/layout_slots.dart';
import '../../../layout/pageState/page_state_registry_provider.dart';
import '../../histConfig/is_navigating_provider.dart';

/// @demo-page navigation-test
/// @description Parent demo page to test hierarchical navigation and breadcrumbs
///
/// DEMO NAV1 - PARENT PAGE
/// =======================
///
/// This page tests:
/// - Hierarchical navigation structure (parent → children)
/// - Breadcrumb trail generation
/// - Navigation between child pages
/// - Tab-based navigation within a page
/// - TAB STATE RESTORATION: the active tab index is saved via PageStateRegistry
///   and restored when the user returns via UNDO/REDO.
///
/// STRUCTURE:
/// DemoNav1 (this page)
///   ├─ Child A (tall page for scroll testing)
///   ├─ Child B (medium page)
///   └─ Child C (short page)
///
/// ARCHITECTURE:
/// DemoNav1Page (ConsumerWidget) → LayoutManager → _DemoNav1Body (StatefulWidget)
/// The body sits INSIDE LayoutManager so it can access PageStateRegistryProvider.
class DemoNav1Page extends ConsumerWidget {
  const DemoNav1Page({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final baseSlots = LayoutPresets.defaultPageBrowser(
      context: context,
      body: const _DemoNav1Body(),
    );

    // Tabs handle their own scrolling, so disable outer scroll
    return LayoutManager(
      slots: LayoutSlots(
        body: baseSlots.body,
        header: baseSlots.header,
        footer: baseSlots.footer,
        fab: baseSlots.fab,
        scrollable: false,
        safeArea: baseSlots.safeArea,
        resizeForKeyboard: baseSlots.resizeForKeyboard,
        backgroundColor: baseSlots.backgroundColor,
        isLoading: baseSlots.isLoading,
        lockedOrientation: baseSlots.lockedOrientation,
        systemUiMode: baseSlots.systemUiMode,
        showBackToTop: baseSlots.showBackToTop,
      ),
    );
  }
}

// ─────────────────────────────────────────────────────────────────────────────
// BODY — StatefulWidget that lives INSIDE LayoutManager's subtree.
// Can access PageStateRegistryProvider via didChangeDependencies.
// ─────────────────────────────────────────────────────────────────────────────

class _DemoNav1Body extends ConsumerStatefulWidget {
  const _DemoNav1Body();

  @override
  ConsumerState<_DemoNav1Body> createState() => _DemoNav1BodyState();
}

class _DemoNav1BodyState extends ConsumerState<_DemoNav1Body>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;

  // Guard: restore saved tab only once per mount.
  bool _tabRestored = false;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 3, vsync: this);
    _tabController.addListener(_onTabChanged);
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (_tabRestored) return;
    final saved = PageStateRegistryProvider.of(context).get('tab');
    if (saved != null) {
      final idx = (saved as int).clamp(0, 2);
      _tabController.animateTo(idx);
      _tabRestored = true;
    }
  }

  void _onTabChanged() {
    if (_tabController.indexIsChanging) return;
    final idx = _tabController.index;
    PageStateRegistryProvider.of(context).set('tab', idx);
  }

  @override
  void dispose() {
    _tabController.removeListener(_onTabChanged);
    _tabController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    // Use the manually managed _tabController (instead of DefaultTabController)
    // so we can save/restore the active tab via PageStateRegistry.
    return Column(
      children: [
        // Tab bar for switching between child page views
        Container(
          color: theme.colorScheme.surface,
          child: TabBar(
            controller: _tabController,
            labelColor: theme.colorScheme.primary,
            unselectedLabelColor: theme.colorScheme.onSurface.withValues(
              alpha: 0.6,
            ),
            indicatorColor: theme.colorScheme.primary,
            tabs: [
              Tab(
                icon: const Icon(Icons.looks_one),
                text: ref.tr(t(pt: 'Demo A', en: 'Demo A')),
              ),
              Tab(
                icon: const Icon(Icons.looks_two),
                text: ref.tr(t(pt: 'Demo B', en: 'Demo B')),
              ),
              Tab(
                icon: const Icon(Icons.looks_3),
                text: ref.tr(t(pt: 'Demo C', en: 'Demo C')),
              ),
            ],
          ),
        ),

        // Navigation buttons to test route-based navigation
        Padding(
          padding: const EdgeInsets.all(Spacing.lg),
          child: Wrap(
            spacing: Spacing.md,
            runSpacing: Spacing.md,
            alignment: WrapAlignment.center,
            children: [
              ElevatedButton.icon(
                onPressed: () {
                  ref.read(isNavigatingProvider.notifier).set(true);
                  context.go('/demo-nav1/child-a');
                },
                icon: const Icon(Icons.route),
                label: Text(
                  ref.tr(t(pt: 'Ir para Child A', en: 'Go to Child A')),
                ),
              ),
              ElevatedButton.icon(
                onPressed: () {
                  ref.read(isNavigatingProvider.notifier).set(true);
                  context.go('/demo-nav1/child-b');
                },
                icon: const Icon(Icons.route),
                label: Text(
                  ref.tr(t(pt: 'Ir para Child B', en: 'Go to Child B')),
                ),
              ),
              ElevatedButton.icon(
                onPressed: () {
                  ref.read(isNavigatingProvider.notifier).set(true);
                  context.go('/demo-nav1/child-c');
                },
                icon: const Icon(Icons.route),
                label: Text(
                  ref.tr(t(pt: 'Ir para Child C', en: 'Go to Child C')),
                ),
              ),
            ],
          ),
        ),

        // Tab content
        Expanded(
          child: TabBarView(
            controller: _tabController,
            children: const [
              _DemoChildPageA(),
              _DemoChildPageB(),
              _DemoChildPageC(),
            ],
          ),
        ),
      ],
    );
  }
}

/// CHILD PAGE A - Tall page for scroll testing
class _DemoChildPageA extends ConsumerWidget {
  const _DemoChildPageA();

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);

    return SingleChildScrollView(
      child: Container(
        padding: const EdgeInsets.all(Spacing.xl),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Header
            _buildHeader(
              context,
              ref,
              theme,
              title: 'Child A - Scroll Test',
              icon: Icons.height,
              color: Colors.blue,
            ),

            const SizedBox(height: Spacing.xl),

            // Info card
            _buildInfoCard(
              theme,
              title: ref.tr(t(pt: 'Teste de Rolagem', en: 'Scroll Testing')),
              description: ref.tr(
                t(
                  pt:
                      'Esta página é propositalmente alta para testar:\n'
                      '• Restauração de posição de rolagem\n'
                      '• Botão "Voltar ao Topo"\n'
                      '• Animação do anel de progresso',
                  en:
                      'This page is intentionally tall to test:\n'
                      '• Scroll position restoration\n'
                      '• Back to top button\n'
                      '• Progress ring animation',
                ),
              ),
            ),

            // Generate tall content
            ...List.generate(
              30,
              (index) => Container(
                margin: const EdgeInsets.only(top: Spacing.lg),
                padding: const EdgeInsets.all(Spacing.lg),
                decoration: BoxDecoration(
                  color: theme.colorScheme.surfaceContainerHighest,
                  borderRadius: BorderRadius.circular(Spacing.md),
                ),
                child: Row(
                  children: [
                    CircleAvatar(
                      backgroundColor: Colors.blue.withValues(alpha: 0.2),
                      child: Text('${index + 1}'),
                    ),
                    const SizedBox(width: Spacing.md),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            ref.tr(
                              t(
                                pt: 'Item $index - Conteúdo de Teste',
                                en: 'Item $index - Test Content',
                              ),
                            ),
                            style: theme.textTheme.titleMedium,
                          ),
                          const SizedBox(height: Spacing.xs),
                          Text(
                            ref.tr(
                              t(
                                pt: 'Role até o final e teste o botão "Voltar ao Topo"',
                                en: 'Scroll to the bottom and test the "Back to Top" button',
                              ),
                            ),
                            style: theme.textTheme.bodySmall?.copyWith(
                              color: theme.colorScheme.onSurface.withValues(
                                alpha: 0.6,
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
            ),

            const SizedBox(height: Spacing.xl),

            // Bottom marker
            Container(
              padding: const EdgeInsets.all(Spacing.xl),
              decoration: BoxDecoration(
                color: Colors.blue.withValues(alpha: 0.1),
                borderRadius: BorderRadius.circular(Spacing.md),
                border: Border.all(color: Colors.blue),
              ),
              child: Row(
                children: [
                  const Icon(Icons.flag, color: Colors.blue),
                  const SizedBox(width: Spacing.md),
                  Expanded(
                    child: Text(
                      ref.tr(
                        t(
                          pt: '🎉 Você chegou ao final! Agora volte para cima.',
                          en: '🎉 You reached the end! Now go back to the top.',
                        ),
                      ),
                      style: theme.textTheme.titleMedium?.copyWith(
                        color: Colors.blue,
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

/// CHILD PAGE B - Medium page
class _DemoChildPageB extends ConsumerWidget {
  const _DemoChildPageB();

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);

    return SingleChildScrollView(
      child: Container(
        padding: const EdgeInsets.all(Spacing.xl),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Header
            _buildHeader(
              context,
              ref,
              theme,
              title: 'Child B - Medium Content',
              icon: Icons.vertical_align_center,
              color: Colors.green,
            ),

            const SizedBox(height: Spacing.xl),

            // Info card
            _buildInfoCard(
              theme,
              title: ref.tr(
                t(pt: 'Teste de Navegação', en: 'Navigation Testing'),
              ),
              description: ref.tr(
                t(
                  pt:
                      'Esta página testa navegação entre páginas filho.\n'
                      'Use as abas ou botões para navegar.',
                  en:
                      'This page tests navigation between child pages.\n'
                      'Use tabs or buttons to navigate.',
                ),
              ),
            ),

            // Medium content
            ...List.generate(
              10,
              (index) => Container(
                margin: const EdgeInsets.only(top: Spacing.lg),
                padding: const EdgeInsets.all(Spacing.lg),
                decoration: BoxDecoration(
                  color: theme.colorScheme.surfaceContainerHighest,
                  borderRadius: BorderRadius.circular(Spacing.md),
                ),
                child: Row(
                  children: [
                    CircleAvatar(
                      backgroundColor: Colors.green.withValues(alpha: 0.2),
                      child: const Icon(Icons.check, color: Colors.green),
                    ),
                    const SizedBox(width: Spacing.md),
                    Expanded(
                      child: Text(
                        ref.tr(
                          t(
                            pt: 'Item $index - Conteúdo Médio',
                            en: 'Item $index - Medium Content',
                          ),
                        ),
                        style: theme.textTheme.titleMedium,
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

/// CHILD PAGE C - Short page
class _DemoChildPageC extends ConsumerWidget {
  const _DemoChildPageC();

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);

    return SingleChildScrollView(
      child: Container(
        padding: const EdgeInsets.all(Spacing.xl),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Header
            _buildHeader(
              context,
              ref,
              theme,
              title: 'Child C - Minimal Content',
              icon: Icons.compress,
              color: Colors.orange,
            ),

            const SizedBox(height: Spacing.xl),

            // Info card
            _buildInfoCard(
              theme,
              title: ref.tr(
                t(pt: 'Teste de Breadcrumbs', en: 'Breadcrumb Testing'),
              ),
              description: ref.tr(
                t(
                  pt:
                      'Verifique o caminho de navegação:\n'
                      'Início > DemoNav1 > Child C\n\n'
                      'Clique nos breadcrumbs para testar navegação.',
                  en:
                      'Check the navigation path:\n'
                      'Home > DemoNav1 > Child C\n\n'
                      'Click breadcrumbs to test navigation.',
                ),
              ),
            ),

            // Short content
            ...List.generate(
              3,
              (index) => Container(
                margin: const EdgeInsets.only(top: Spacing.lg),
                padding: const EdgeInsets.all(Spacing.lg),
                decoration: BoxDecoration(
                  color: theme.colorScheme.surfaceContainerHighest,
                  borderRadius: BorderRadius.circular(Spacing.md),
                ),
                child: Row(
                  children: [
                    CircleAvatar(
                      backgroundColor: Colors.orange.withValues(alpha: 0.2),
                      child: const Icon(Icons.star, color: Colors.orange),
                    ),
                    const SizedBox(width: Spacing.md),
                    Expanded(
                      child: Text(
                        ref.tr(
                          t(
                            pt: 'Item $index - Conteúdo Curto',
                            en: 'Item $index - Short Content',
                          ),
                        ),
                        style: theme.textTheme.titleMedium,
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

// ============================================================================
// HELPER WIDGETS
// ============================================================================

/// Build header section with icon and title
Widget _buildHeader(
  BuildContext context,
  WidgetRef ref,
  ThemeData theme, {
  required String title,
  required IconData icon,
  required Color color,
}) {
  return Container(
    padding: const EdgeInsets.all(Spacing.xl),
    decoration: BoxDecoration(
      gradient: LinearGradient(
        colors: [color.withValues(alpha: 0.2), color.withValues(alpha: 0.05)],
      ),
      borderRadius: BorderRadius.circular(Spacing.lg),
    ),
    child: Row(
      children: [
        Icon(icon, size: 48, color: color),
        const SizedBox(width: Spacing.lg),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                title,
                style: theme.textTheme.headlineSmall?.copyWith(
                  color: color,
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: Spacing.xs),
              Text(
                ref.tr(
                  t(
                    pt: 'Página de demonstração para teste de navegação',
                    en: 'Demo page for navigation testing',
                  ),
                ),
                style: theme.textTheme.bodyMedium?.copyWith(
                  color: theme.colorScheme.onSurface.withValues(alpha: 0.7),
                ),
              ),
            ],
          ),
        ),
      ],
    ),
  );
}

/// Build info card
Widget _buildInfoCard(
  ThemeData theme, {
  required String title,
  required String description,
}) {
  return Container(
    padding: const EdgeInsets.all(Spacing.lg),
    decoration: BoxDecoration(
      color: theme.colorScheme.surfaceContainerHighest,
      borderRadius: BorderRadius.circular(Spacing.md),
      border: Border.all(
        color: theme.colorScheme.outline.withValues(alpha: 0.2),
      ),
    ),
    child: Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            Icon(Icons.info_outline, color: theme.colorScheme.primary),
            const SizedBox(width: Spacing.sm),
            Text(
              title,
              style: theme.textTheme.titleMedium?.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
          ],
        ),
        const SizedBox(height: Spacing.md),
        Text(description, style: theme.textTheme.bodyMedium),
      ],
    ),
  );
}
