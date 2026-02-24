import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../design/design_system.dart';
import '../../../utils/i18n/extensions/context_extensions.dart';
import '../../../utils/i18n/models/translatable_string.dart';
import '../../../layout/layout_manager.dart';
import '../../../layout/layout_presets.dart';
import '../../histConfig/is_navigating_provider.dart';

/// @demo-page navigation-child
/// @description Standalone route for Child A to test breadcrumbs
///
/// DEMO NAV1 CHILD A - Standalone Route
/// =====================================
///
/// This is the same as _DemoChildPageA from demo_nav1_page.dart,
/// but as a standalone route for testing breadcrumb hierarchy.
///
/// Breadcrumb path: Home > DemoNav1 > Child A
class DemoNav1ChildAPage extends ConsumerWidget {
  const DemoNav1ChildAPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return LayoutManager(
      slots: LayoutPresets.defaultPageBrowser(
        context: context,
        body: _buildBody(context, ref),
      ),
    );
  }

  Widget _buildBody(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);

    return SingleChildScrollView(
      child: Container(
        padding: const EdgeInsets.all(Spacing.xl),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Header
            _buildHeader(context, ref, theme),

            const SizedBox(height: Spacing.xl),

            // Info card
            _buildInfoCard(theme, ref),

            const SizedBox(height: Spacing.lg),

            // Back button
            ElevatedButton.icon(
              onPressed: () {
                ref.read(isNavigatingProvider.notifier).set(true);
                context.go('/demo-nav1');
              },
              icon: const Icon(Icons.arrow_back),
              label: Text(
                ref.tr(t(pt: 'Voltar para DemoNav1', en: 'Back to DemoNav1')),
              ),
            ),

            // Generate tall content for scroll testing
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
                                pt: 'Role até o final para testar "Voltar ao Topo"',
                                en: 'Scroll to bottom to test "Back to Top"',
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
                          pt: '🎉 Fim da página! Teste o botão "Voltar ao Topo"',
                          en: '🎉 End of page! Test the "Back to Top" button',
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

  Widget _buildHeader(BuildContext context, WidgetRef ref, ThemeData theme) {
    return Container(
      padding: const EdgeInsets.all(Spacing.xl),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          colors: [
            Colors.blue.withValues(alpha: 0.2),
            Colors.blue.withValues(alpha: 0.05),
          ],
        ),
        borderRadius: BorderRadius.circular(Spacing.lg),
      ),
      child: Row(
        children: [
          const Icon(Icons.height, size: 48, color: Colors.blue),
          const SizedBox(width: Spacing.lg),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Child A - Scroll Test',
                  style: theme.textTheme.headlineSmall?.copyWith(
                    color: Colors.blue,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: Spacing.xs),
                Text(
                  ref.tr(
                    t(
                      pt: 'Rota independente para testar breadcrumbs',
                      en: 'Standalone route to test breadcrumbs',
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

  Widget _buildInfoCard(ThemeData theme, WidgetRef ref) {
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
              Icon(Icons.route, color: theme.colorScheme.primary),
              const SizedBox(width: Spacing.sm),
              Text(
                ref.tr(t(pt: 'Teste de Breadcrumbs', en: 'Breadcrumb Test')),
                style: theme.textTheme.titleMedium?.copyWith(
                  fontWeight: FontWeight.bold,
                ),
              ),
            ],
          ),
          const SizedBox(height: Spacing.md),
          Text(
            ref.tr(
              t(
                pt:
                    'Verifique o caminho de navegação acima:\n'
                    'Início > DemoNav1 > Child A\n\n'
                    'Clique nos breadcrumbs para testar navegação.',
                en:
                    'Check the navigation path above:\n'
                    'Home > DemoNav1 > Child A\n\n'
                    'Click breadcrumbs to test navigation.',
              ),
            ),
            style: theme.textTheme.bodyMedium,
          ),
        ],
      ),
    );
  }
}
