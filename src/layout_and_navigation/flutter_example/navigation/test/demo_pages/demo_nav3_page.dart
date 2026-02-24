import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../design/design_system.dart';
import '../../../utils/i18n/extensions/context_extensions.dart';
import '../../../utils/i18n/models/translatable_string.dart';
import '../../../layout/layout_manager.dart';
import '../../../layout/layout_presets.dart';

/// @demo-page navigation-test scroll-restoration-test
/// @description Test page for scroll restoration functionality
/// Tests that scroll position is ONLY restored on UNDO/REDO, not on normal navigation
class DemoNav3Page extends ConsumerWidget {
  const DemoNav3Page({super.key});

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

    return Container(
      constraints: const BoxConstraints(maxWidth: 800),
      padding: const EdgeInsets.all(Spacing.xl),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Header
          _buildHeader(theme, ref),
          const SizedBox(height: Spacing.xl),

          // Instructions card
          _buildInstructionsCard(theme, ref),
          const SizedBox(height: Spacing.xl),

          // Scrollable content sections
          ...List.generate(20, (index) {
            final sectionNumber = index + 1;
            return _buildSection(theme, ref, sectionNumber);
          }),

          // Footer marker
          _buildFooterMarker(theme, ref),
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
            theme.colorScheme.tertiaryContainer,
            theme.colorScheme.secondaryContainer,
          ],
        ),
        borderRadius: BorderRadius.circular(Spacing.lg),
      ),
      child: Row(
        children: [
          Icon(Icons.science, size: 48, color: theme.colorScheme.tertiary),
          const SizedBox(width: Spacing.md),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  ref.tr(
                    t(
                      pt: 'Teste de Restauração de Scroll',
                      en: 'Scroll Restoration Test',
                    ),
                  ),
                  style: theme.textTheme.headlineMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                    color: theme.colorScheme.tertiary,
                  ),
                ),
                const SizedBox(height: Spacing.xs),
                Text(
                  ref.tr(
                    t(
                      pt: 'Demo Nav 3 - Página de teste simples',
                      en: 'Demo Nav 3 - Simple test page',
                    ),
                  ),
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: theme.colorScheme.onTertiaryContainer,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildInstructionsCard(ThemeData theme, WidgetRef ref) {
    return Container(
      padding: const EdgeInsets.all(Spacing.lg),
      decoration: BoxDecoration(
        color: theme.colorScheme.primaryContainer,
        borderRadius: BorderRadius.circular(Spacing.md),
        border: Border.all(color: theme.colorScheme.primary, width: 2),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(
                Icons.lightbulb_outline,
                color: theme.colorScheme.primary,
                size: 32,
              ),
              const SizedBox(width: Spacing.sm),
              Text(
                ref.tr(t(pt: 'Como Testar', en: 'How to Test')),
                style: theme.textTheme.titleLarge?.copyWith(
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
                    '1️⃣ Role até a Seção 40 abaixo\n'
                    '2️⃣ Clique em outra aba (ex: Home)\n'
                    '3️⃣ ✅ TESTE NORMAL: Clique novamente na aba "Demo Nav 3"\n'
                    '   → Deve começar no TOPO (sem restauração)\n\n'
                    '4️⃣ Role até a Seção 40 novamente\n'
                    '5️⃣ Clique em outra aba (ex: Home)\n'
                    '6️⃣ ✅ TESTE UNDO: Clique no botão VOLTAR (⬅️)\n'
                    '   → Deve RESTAURAR a posição na Seção 40',
                en:
                    '1️⃣ Scroll down to Section 40 below\n'
                    '2️⃣ Click another tab (e.g. Home)\n'
                    '3️⃣ ✅ NORMAL TEST: Click "Demo Nav 3" tab again\n'
                    '   → Should start at TOP (no restoration)\n\n'
                    '4️⃣ Scroll down to Section 40 again\n'
                    '5️⃣ Click another tab (e.g. Home)\n'
                    '6️⃣ ✅ UNDO TEST: Click the BACK button (⬅️)\n'
                    '   → Should RESTORE position at Section 40',
              ),
            ),
            style: theme.textTheme.bodyLarge?.copyWith(
              color: theme.colorScheme.onPrimaryContainer,
              height: 1.6,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildSection(ThemeData theme, WidgetRef ref, int sectionNumber) {
    // Color variation for visual distinction
    final Color sectionColor = _getColorForSection(theme, sectionNumber);

    return Container(
      margin: const EdgeInsets.only(bottom: Spacing.lg),
      padding: const EdgeInsets.all(Spacing.lg),
      decoration: BoxDecoration(
        color: sectionColor.withValues(alpha: 0.1),
        borderRadius: BorderRadius.circular(Spacing.md),
        border: Border.all(
          color: sectionColor.withValues(alpha: 0.3),
          width: 2,
        ),
      ),
      child: Row(
        children: [
          // Section number badge
          Container(
            width: 60,
            height: 60,
            decoration: BoxDecoration(
              color: sectionColor,
              shape: BoxShape.circle,
            ),
            child: Center(
              child: Text(
                '$sectionNumber',
                style: theme.textTheme.titleLarge?.copyWith(
                  fontWeight: FontWeight.bold,
                  color: Colors.white,
                ),
              ),
            ),
          ),
          const SizedBox(width: Spacing.md),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  ref.tr(
                    t(pt: 'Seção $sectionNumber', en: 'Section $sectionNumber'),
                  ),
                  style: theme.textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                    color: sectionColor,
                  ),
                ),
                const SizedBox(height: Spacing.xs),
                Text(
                  ref.tr(
                    t(
                      pt: 'Conteúdo de teste para restauração de scroll. Role até encontrar a seção desejada.',
                      en: 'Test content for scroll restoration. Scroll to find your desired section.',
                    ),
                  ),
                  style: theme.textTheme.bodyMedium,
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildFooterMarker(ThemeData theme, WidgetRef ref) {
    return Container(
      margin: const EdgeInsets.only(top: Spacing.xl),
      padding: const EdgeInsets.all(Spacing.xl * 2),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          colors: [theme.colorScheme.primary, theme.colorScheme.tertiary],
        ),
        borderRadius: BorderRadius.circular(Spacing.lg),
      ),
      child: Column(
        children: [
          const Icon(Icons.flag, size: 64, color: Colors.white),
          const SizedBox(height: Spacing.md),
          Text(
            ref.tr(t(pt: '🎉 Fim da Página!', en: '🎉 End of Page!')),
            style: theme.textTheme.headlineMedium?.copyWith(
              fontWeight: FontWeight.bold,
              color: Colors.white,
            ),
          ),
          const SizedBox(height: Spacing.sm),
          Text(
            ref.tr(
              t(
                pt: 'Você rolou 50 seções. Agora teste a restauração!',
                en: 'You scrolled 50 sections. Now test restoration!',
              ),
            ),
            style: theme.textTheme.bodyLarge?.copyWith(color: Colors.white),
            textAlign: TextAlign.center,
          ),
        ],
      ),
    );
  }

  Color _getColorForSection(ThemeData theme, int sectionNumber) {
    final colors = [
      theme.colorScheme.primary,
      theme.colorScheme.secondary,
      theme.colorScheme.tertiary,
      Colors.orange,
      Colors.teal,
      Colors.purple,
    ];
    return colors[sectionNumber % colors.length];
  }
}
