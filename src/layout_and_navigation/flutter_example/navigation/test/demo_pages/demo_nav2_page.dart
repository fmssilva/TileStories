import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../design/design_system.dart';
import '../../../utils/i18n/extensions/context_extensions.dart';
import '../../../utils/i18n/models/translatable_string.dart';
import '../../../layout/layout_manager.dart';
import '../../../layout/layout_presets.dart';
import '../../../layout/scrollController/scroll_registry_provider.dart';

/// @demo-page navigation-test nested-scroll-test
/// @description Demo page to test nested scrolling (outer + inner scrolls)
///
/// DEMO NAV2 PAGE
/// ==============
///
/// This page tests:
/// - Outer scroll (managed by LayoutManager using 'page' key)
/// - Inner scrollable containers (ListView inside outer scroll)
/// - No scroll conflicts or infinite loops
/// - Proper scroll restoration for both outer and inner scrolls
///
/// STUDENT NOTE: This demonstrates the SIMPLIFIED scroll registry pattern.
/// No initState, no dispose, no manual registration - just request controllers
/// from context.scrollRegistry and the system handles everything automatically.
///
/// ARCHITECTURE:
/// - LayoutManager creates ScrollRegistry on page navigation
/// - Provides it via InheritedWidget (ScrollRegistryProvider) to all children
/// - Pages request controllers via ScrollRegistryProvider.of(context).controller('id')
/// - Controllers are auto-created on first request, with position restored from history
/// - Positions auto-restored on UNDO/REDO via initialScrollOffset
/// - LayoutManager saves all cached positions on dispose
class DemoNav2Page extends ConsumerWidget {
  const DemoNav2Page({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    // Use LayoutManager with preset (outer scroll enabled)
    // IMPORTANT: Wrap body in Builder to access ScrollRegistryProvider
    // which is provided by LayoutManager AROUND the body
    return LayoutManager(
      slots: LayoutPresets.defaultPageBrowser(
        context: context,
        body: Builder(builder: (context) => _buildBody(context, ref)),
      ),
    );
  }

  /// Build the page body content with nested scrollable areas
  Widget _buildBody(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);

    // Get scroll registry from context (provided by LayoutManager via InheritedWidget)
    final registry = ScrollRegistryProvider.of(context);

    // Request scroll controllers by ID
    final innerScroll1 = registry.controller('demo_nav2_inner_1');
    final innerScroll2 = registry.controller('demo_nav2_inner_2');
    final innerScroll3 = registry.controller('demo_nav2_inner_3');

    return Container(
      padding: const EdgeInsets.all(Spacing.xl),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Header
          _buildHeader(theme, ref),
          const SizedBox(height: Spacing.xl),

          // Instructions
          _buildInstructions(theme, ref),
          const SizedBox(height: Spacing.xl),

          // Inner scrollable area 1
          _buildInnerScrollArea(
            theme,
            ref,
            title: ref.tr(t(pt: 'Área Rolável 1', en: 'Scrollable Area 1')),
            controller: innerScroll1,
            color: Colors.blue,
            itemCount: 20,
          ),
          const SizedBox(height: Spacing.xl),

          // Inner scrollable area 2
          _buildInnerScrollArea(
            theme,
            ref,
            title: ref.tr(t(pt: 'Área Rolável 2', en: 'Scrollable Area 2')),
            controller: innerScroll2,
            color: Colors.orange,
            itemCount: 20,
          ),
          const SizedBox(height: Spacing.xl),

          // Inner scrollable area 3
          _buildInnerScrollArea(
            theme,
            ref,
            title: ref.tr(t(pt: 'Área Rolável 3', en: 'Scrollable Area 3')),
            controller: innerScroll3,
            color: Colors.green,
            itemCount: 20,
          ),
          const SizedBox(height: Spacing.xl),

          // Bottom section
          _buildBottomSection(theme, ref),
        ],
      ),
    );
  }

  Widget _buildHeader(ThemeData theme, WidgetRef ref) {
    return Container(
      padding: const EdgeInsets.all(Spacing.xl),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          colors: [
            theme.colorScheme.secondaryContainer,
            theme.colorScheme.tertiaryContainer,
          ],
        ),
        borderRadius: BorderRadius.circular(Spacing.lg),
      ),
      child: Row(
        children: [
          Icon(Icons.view_stream, size: 48, color: theme.colorScheme.secondary),
          const SizedBox(width: Spacing.md),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  ref.tr(
                    t(pt: 'Teste de Scroll Aninhado', en: 'Nested Scroll Test'),
                  ),
                  style: theme.textTheme.headlineMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                    color: theme.colorScheme.secondary,
                  ),
                ),
                const SizedBox(height: Spacing.xs),
                Text(
                  ref.tr(
                    t(
                      pt: 'Demo Nav 2 - Múltiplos scrolls',
                      en: 'Demo Nav 2 - Multiple scrolls',
                    ),
                  ),
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: theme.colorScheme.onSecondaryContainer,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildInstructions(ThemeData theme, WidgetRef ref) {
    return Container(
      padding: const EdgeInsets.all(Spacing.xl),
      decoration: BoxDecoration(
        color: theme.colorScheme.surfaceContainerHighest,
        borderRadius: BorderRadius.circular(Spacing.lg),
        border: Border.all(color: theme.colorScheme.outline),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(Icons.info_outline, color: theme.colorScheme.primary),
              const SizedBox(width: Spacing.sm),
              Text(
                ref.tr(t(pt: 'Como testar:', en: 'How to test:')),
                style: theme.textTheme.titleMedium?.copyWith(
                  fontWeight: FontWeight.bold,
                  color: theme.colorScheme.primary,
                ),
              ),
            ],
          ),
          const SizedBox(height: Spacing.md),
          Text(
            ref.tr(
              t(
                pt:
                    '1. Role a página toda para baixo (scroll externo)\n'
                    '2. Role cada área colorida (scrolls internos)\n'
                    '3. Navegue para outra página\n'
                    '4. Clique UNDO (← botão) para voltar\n'
                    '5. TODOS os scrolls (externo + 3 internos) serão restaurados!\n\n'
                    '✅ Sistema de scroll aninhado implementado!\n'
                    'O sistema agora rastreia e restaura múltiplas\n'
                    'posições de scroll por página usando um registro\n'
                    'de controllers.',
                en:
                    '1. Scroll the entire page down (outer scroll)\n'
                    '2. Scroll each colored area (inner scrolls)\n'
                    '3. Navigate to another page\n'
                    '4. Click UNDO (← button) to go back\n'
                    '5. ALL scrolls (outer + 3 inner) will be restored!\n\n'
                    '✅ Nested scroll system implemented!\n'
                    'The system now tracks and restores multiple\n'
                    'scroll positions per page using a controller\n'
                    'registry.',
              ),
            ),
            style: theme.textTheme.bodyMedium,
          ),
        ],
      ),
    );
  }

  Widget _buildInnerScrollArea(
    ThemeData theme,
    WidgetRef ref, {
    required String title,
    required ScrollController controller,
    required Color color,
    required int itemCount,
  }) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        // Title
        Container(
          padding: const EdgeInsets.symmetric(
            horizontal: Spacing.md,
            vertical: Spacing.sm,
          ),
          decoration: BoxDecoration(
            color: color.withValues(alpha: 0.2),
            borderRadius: const BorderRadius.only(
              topLeft: Radius.circular(Spacing.md),
              topRight: Radius.circular(Spacing.md),
            ),
          ),
          child: Row(
            children: [
              Icon(Icons.view_list, color: color, size: 20),
              const SizedBox(width: Spacing.sm),
              Text(
                title,
                style: theme.textTheme.titleMedium?.copyWith(
                  fontWeight: FontWeight.bold,
                  color: color,
                ),
              ),
            ],
          ),
        ),

        // Scrollable list
        Container(
          height: 300, // Fixed height for inner scroll
          decoration: BoxDecoration(
            color: color.withValues(alpha: 0.05),
            border: Border.all(color: color.withValues(alpha: 0.3)),
            borderRadius: const BorderRadius.only(
              bottomLeft: Radius.circular(Spacing.md),
              bottomRight: Radius.circular(Spacing.md),
            ),
          ),
          child: ListView.builder(
            controller: controller,
            itemCount: itemCount,
            itemBuilder: (context, index) {
              return Container(
                padding: const EdgeInsets.all(Spacing.md),
                margin: const EdgeInsets.symmetric(
                  horizontal: Spacing.sm,
                  vertical: Spacing.xs,
                ),
                decoration: BoxDecoration(
                  color: theme.colorScheme.surface,
                  borderRadius: BorderRadius.circular(Spacing.sm),
                ),
                child: Row(
                  children: [
                    CircleAvatar(
                      backgroundColor: color.withValues(alpha: 0.3),
                      radius: 16,
                      child: Text(
                        '${index + 1}',
                        style: TextStyle(
                          color: color,
                          fontWeight: FontWeight.bold,
                          fontSize: 12,
                        ),
                      ),
                    ),
                    const SizedBox(width: Spacing.md),
                    Expanded(
                      child: Text(
                        ref.tr(
                          t(
                            pt: 'Item ${index + 1} - Role esta área',
                            en: 'Item ${index + 1} - Scroll this area',
                          ),
                        ),
                        style: theme.textTheme.bodyMedium,
                      ),
                    ),
                  ],
                ),
              );
            },
          ),
        ),
      ],
    );
  }

  Widget _buildBottomSection(ThemeData theme, WidgetRef ref) {
    return Container(
      padding: const EdgeInsets.all(Spacing.xl),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          colors: [
            theme.colorScheme.primaryContainer,
            theme.colorScheme.secondaryContainer,
          ],
        ),
        borderRadius: BorderRadius.circular(Spacing.lg),
      ),
      child: Column(
        children: [
          Icon(Icons.check_circle, size: 64, color: theme.colorScheme.primary),
          const SizedBox(height: Spacing.md),
          Text(
            ref.tr(t(pt: 'Fim da Página', en: 'End of Page')),
            style: theme.textTheme.headlineSmall?.copyWith(
              fontWeight: FontWeight.bold,
              color: theme.colorScheme.primary,
            ),
          ),
          const SizedBox(height: Spacing.sm),
          Text(
            ref.tr(
              t(
                pt: 'Se você chegou aqui, o scroll externo está funcionando!',
                en: 'If you reached here, the outer scroll is working!',
              ),
            ),
            style: theme.textTheme.bodyMedium?.copyWith(
              color: theme.colorScheme.onPrimaryContainer,
            ),
            textAlign: TextAlign.center,
          ),
        ],
      ),
    );
  }
}
