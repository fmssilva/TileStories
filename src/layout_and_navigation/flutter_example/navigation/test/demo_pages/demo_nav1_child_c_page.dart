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
/// @description Standalone route for Child C to test breadcrumbs
class DemoNav1ChildCPage extends ConsumerWidget {
  const DemoNav1ChildCPage({super.key});

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
            Container(
              padding: const EdgeInsets.all(Spacing.xl),
              decoration: BoxDecoration(
                gradient: LinearGradient(
                  colors: [
                    Colors.orange.withValues(alpha: 0.2),
                    Colors.orange.withValues(alpha: 0.05),
                  ],
                ),
                borderRadius: BorderRadius.circular(Spacing.lg),
              ),
              child: Row(
                children: [
                  const Icon(Icons.compress, size: 48, color: Colors.orange),
                  const SizedBox(width: Spacing.lg),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Child C - Minimal Content',
                          style: theme.textTheme.headlineSmall?.copyWith(
                            color: Colors.orange,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        const SizedBox(height: Spacing.xs),
                        Text(
                          ref.tr(
                            t(
                              pt: 'Teste: Início > DemoNav1 > Child C',
                              en: 'Test: Home > DemoNav1 > Child C',
                            ),
                          ),
                          style: theme.textTheme.bodyMedium?.copyWith(
                            color: theme.colorScheme.onSurface.withValues(
                              alpha: 0.7,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),

            const SizedBox(height: Spacing.xl),

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
