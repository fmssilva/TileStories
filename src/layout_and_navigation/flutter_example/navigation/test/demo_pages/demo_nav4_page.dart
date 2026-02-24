import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../design/design_system.dart';
import '../../../utils/i18n/extensions/context_extensions.dart';
import '../../../utils/i18n/models/translatable_string.dart';
import '../../../layout/layout_manager.dart';
import '../../../layout/layout_presets.dart';
import '../../../layout/layout_slots.dart';
import '../../../layout/pageState/page_state_registry_provider.dart';

/// @demo-page navigation-test scroll-restoration state-restoration
/// @description Comprehensive demo page to test:
/// - Scroll position save/restore
/// - Tab state save/restore
/// - Form state save/restore
/// - Long scrollable content
///
/// ARCHITECTURE:
/// DemoNav4Page (ConsumerWidget) → LayoutManager → _DemoNav4Body (StatefulWidget)
/// The body sits INSIDE LayoutManager so it can access PageStateRegistryProvider
/// via didChangeDependencies, which fires after LayoutManager calls restore().
class DemoNav4Page extends ConsumerWidget {
  const DemoNav4Page({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final baseSlots = LayoutPresets.defaultPageBrowser(
      context: context,
      body: const _DemoNav4Body(),
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

class _DemoNav4Body extends ConsumerStatefulWidget {
  const _DemoNav4Body();

  @override
  ConsumerState<_DemoNav4Body> createState() => _DemoNav4BodyState();
}

class _DemoNav4BodyState extends ConsumerState<_DemoNav4Body>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;

  final _nameController = TextEditingController();
  final _emailController = TextEditingController();
  final _messageController = TextEditingController();

  bool _subscribeNewsletter = false;
  String _selectedCountry = 'portugal';

  // Guard: restore saved state only once per mount.
  bool _stateRestored = false;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 3, vsync: this);
    _tabController.addListener(_onTabChanged);
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (_stateRestored) return;
    final reg = PageStateRegistryProvider.of(context);
    final savedTab = reg.get('tab');
    final savedName = reg.get('name');
    final savedEmail = reg.get('email');
    final savedMessage = reg.get('message');
    final savedCountry = reg.get('country');
    final savedNewsletter = reg.get('newsletter');
    if (savedTab != null ||
        savedName != null ||
        savedEmail != null ||
        savedMessage != null ||
        savedCountry != null ||
        savedNewsletter != null) {
      if (savedTab != null) {
        _tabController.animateTo((savedTab as int).clamp(0, 2));
      }
      if (savedName != null) {
        _nameController.text = savedName as String;
      }
      if (savedEmail != null) {
        _emailController.text = savedEmail as String;
      }
      if (savedMessage != null) {
        _messageController.text = savedMessage as String;
      }
      if (savedCountry != null) {
        setState(() => _selectedCountry = savedCountry as String);
      }
      if (savedNewsletter != null) {
        setState(() => _subscribeNewsletter = savedNewsletter as bool);
      }
      _stateRestored = true;
    }
  }

  void _onTabChanged() {
    if (_tabController.indexIsChanging) return;
    PageStateRegistryProvider.of(context).set('tab', _tabController.index);
  }

  void _saveField(String key, dynamic value) {
    PageStateRegistryProvider.of(context).set(key, value);
  }

  @override
  void dispose() {
    _tabController.removeListener(_onTabChanged);
    _tabController.dispose();
    _nameController.dispose();
    _emailController.dispose();
    _messageController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Column(
      children: [
        // Tab bar
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
                icon: const Icon(Icons.article),
                text: ref.tr(t(pt: 'Conteúdo', en: 'Content')),
              ),
              Tab(
                icon: const Icon(Icons.edit_note),
                text: ref.tr(t(pt: 'Formulário', en: 'Form')),
              ),
              Tab(
                icon: const Icon(Icons.info),
                text: ref.tr(t(pt: 'Info', en: 'Info')),
              ),
            ],
          ),
        ),

        // Tab content with scroll restoration
        Expanded(
          child: TabBarView(
            controller: _tabController,
            children: [
              _buildScrollTestTab(theme),
              _buildFormTab(theme),
              _buildInfoTab(theme),
            ],
          ),
        ),
      ],
    );
  }

  /// Tab 1: Scrollable content to test scroll restoration
  Widget _buildScrollTestTab(ThemeData theme) {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(Spacing.xl),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Header
          _buildSectionHeader(
            theme,
            icon: Icons.vertical_align_bottom,
            title: ref.tr(t(pt: 'Teste de Rolagem', en: 'Scroll Test')),
            subtitle: ref.tr(
              t(
                pt: 'Role até o final e navegue para outra página. Depois volte para testar a restauração.',
                en: 'Scroll to the bottom and navigate to another page. Then come back to test restoration.',
              ),
            ),
          ),

          const SizedBox(height: Spacing.xl),

          // Generate lots of content for scrolling
          ...List.generate(50, (index) {
            final sectionNumber = index + 1;
            return Container(
              margin: const EdgeInsets.only(bottom: Spacing.lg),
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
                      CircleAvatar(
                        backgroundColor: _getColorForSection(
                          sectionNumber,
                        ).withValues(alpha: 0.2),
                        child: Text(
                          '$sectionNumber',
                          style: TextStyle(
                            color: _getColorForSection(sectionNumber),
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ),
                      const SizedBox(width: Spacing.md),
                      Expanded(
                        child: Text(
                          ref.tr(
                            t(
                              pt: 'Seção $sectionNumber',
                              en: 'Section $sectionNumber',
                            ),
                          ),
                          style: theme.textTheme.titleMedium?.copyWith(
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: Spacing.sm),
                  Text(
                    ref.tr(
                      t(
                        pt:
                            'Este é o conteúdo da seção $sectionNumber. '
                            'Continue rolando para testar a restauração de posição.',
                        en:
                            'This is the content of section $sectionNumber. '
                            'Keep scrolling to test position restoration.',
                      ),
                    ),
                    style: theme.textTheme.bodyMedium,
                  ),
                ],
              ),
            );
          }),

          // Bottom marker
          Container(
            padding: const EdgeInsets.all(Spacing.xl),
            decoration: BoxDecoration(
              gradient: LinearGradient(
                colors: [
                  theme.colorScheme.primary.withValues(alpha: 0.2),
                  theme.colorScheme.secondary.withValues(alpha: 0.2),
                ],
              ),
              borderRadius: BorderRadius.circular(Spacing.lg),
              border: Border.all(color: theme.colorScheme.primary, width: 2),
            ),
            child: Row(
              children: [
                Icon(Icons.flag, color: theme.colorScheme.primary, size: 32),
                const SizedBox(width: Spacing.md),
                Expanded(
                  child: Text(
                    ref.tr(
                      t(
                        pt:
                            '🎉 Você chegou ao final! Agora:\n'
                            '1. Navegue para outra página\n'
                            '2. Volte usando o botão voltar\n'
                            '3. A posição de rolagem deve ser restaurada',
                        en:
                            '🎉 You reached the end! Now:\n'
                            '1. Navigate to another page\n'
                            '2. Come back using the back button\n'
                            '3. Scroll position should be restored',
                      ),
                    ),
                    style: theme.textTheme.titleMedium?.copyWith(
                      color: theme.colorScheme.primary,
                    ),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  /// Tab 2: Form to test form state restoration
  Widget _buildFormTab(ThemeData theme) {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(Spacing.xl),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          _buildSectionHeader(
            theme,
            icon: Icons.edit_document,
            title: ref.tr(t(pt: 'Teste de Formulário', en: 'Form Test')),
            subtitle: ref.tr(
              t(
                pt: 'Preencha o formulário e navegue para outra página. Depois volte para testar a restauração.',
                en: 'Fill the form and navigate to another page. Then come back to test restoration.',
              ),
            ),
          ),

          const SizedBox(height: Spacing.xl),

          // Name field
          TextField(
            controller: _nameController,
            decoration: InputDecoration(
              labelText: ref.tr(t(pt: 'Nome Completo', en: 'Full Name')),
              hintText: ref.tr(t(pt: 'Digite seu nome', en: 'Enter your name')),
              prefixIcon: const Icon(Icons.person),
              border: const OutlineInputBorder(),
            ),
            onChanged: (v) => _saveField('name', v),
          ),
          TextField(
            controller: _emailController,
            decoration: InputDecoration(
              labelText: ref.tr(t(pt: 'E-mail', en: 'Email')),
              hintText: ref.tr(
                t(pt: 'Digite seu e-mail', en: 'Enter your email'),
              ),
              prefixIcon: const Icon(Icons.email),
              border: const OutlineInputBorder(),
            ),
            keyboardType: TextInputType.emailAddress,
            onChanged: (v) => _saveField('email', v),
          ),

          const SizedBox(height: Spacing.lg),

          // Country dropdown
          DropdownButtonFormField<String>(
            initialValue: _selectedCountry,
            decoration: InputDecoration(
              labelText: ref.tr(t(pt: 'País', en: 'Country')),
              prefixIcon: const Icon(Icons.public),
              border: const OutlineInputBorder(),
            ),
            items: [
              DropdownMenuItem(
                value: 'portugal',
                child: Text(ref.tr(t(pt: 'Portugal', en: 'Portugal'))),
              ),
              DropdownMenuItem(
                value: 'brazil',
                child: Text(ref.tr(t(pt: 'Brasil', en: 'Brazil'))),
              ),
              DropdownMenuItem(
                value: 'spain',
                child: Text(ref.tr(t(pt: 'Espanha', en: 'Spain'))),
              ),
              DropdownMenuItem(
                value: 'other',
                child: Text(ref.tr(t(pt: 'Outro', en: 'Other'))),
              ),
            ],
            onChanged: (value) {
              if (value != null) {
                setState(() => _selectedCountry = value);
                _saveField('country', value);
              }
            },
          ),

          const SizedBox(height: Spacing.lg),

          // Message field
          TextField(
            controller: _messageController,
            decoration: InputDecoration(
              labelText: ref.tr(t(pt: 'Mensagem', en: 'Message')),
              hintText: ref.tr(
                t(pt: 'Digite sua mensagem', en: 'Enter your message'),
              ),
              prefixIcon: const Icon(Icons.message),
              border: const OutlineInputBorder(),
            ),
            maxLines: 5,
            onChanged: (v) => _saveField('message', v),
          ),

          const SizedBox(height: Spacing.lg),

          // Checkbox
          CheckboxListTile(
            title: Text(
              ref.tr(
                t(
                  pt: 'Quero receber newsletter',
                  en: 'I want to receive newsletter',
                ),
              ),
            ),
            value: _subscribeNewsletter,
            onChanged: (value) {
              if (value != null) {
                setState(() => _subscribeNewsletter = value);
                _saveField('newsletter', value);
              }
            },
            controlAffinity: ListTileControlAffinity.leading,
          ),

          const SizedBox(height: Spacing.xl),

          // Test buttons
          Wrap(
            spacing: Spacing.md,
            runSpacing: Spacing.md,
            children: [
              ElevatedButton.icon(
                onPressed: () {
                  ScaffoldMessenger.of(context).showSnackBar(
                    SnackBar(
                      content: Text(
                        ref.tr(
                          t(
                            pt: 'Formulário salvo! Navegue para outra página e volte.',
                            en: 'Form saved! Navigate to another page and come back.',
                          ),
                        ),
                      ),
                    ),
                  );
                },
                icon: const Icon(Icons.save),
                label: Text(ref.tr(t(pt: 'Salvar Estado', en: 'Save State'))),
              ),
              OutlinedButton.icon(
                onPressed: () {
                  setState(() {
                    _nameController.clear();
                    _emailController.clear();
                    _messageController.clear();
                    _subscribeNewsletter = false;
                    _selectedCountry = 'portugal';
                  });
                },
                icon: const Icon(Icons.clear),
                label: Text(ref.tr(t(pt: 'Limpar', en: 'Clear'))),
              ),
            ],
          ),
        ],
      ),
    );
  }

  /// Tab 3: Info about state restoration
  Widget _buildInfoTab(ThemeData theme) {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(Spacing.xl),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          _buildSectionHeader(
            theme,
            icon: Icons.info,
            title: ref.tr(t(pt: 'Sobre esta Página', en: 'About this Page')),
            subtitle: ref.tr(
              t(
                pt: 'Informações sobre os recursos de restauração de estado',
                en: 'Information about state restoration features',
              ),
            ),
          ),

          const SizedBox(height: Spacing.xl),

          _buildInfoCard(
            theme,
            icon: Icons.tab,
            title: ref.tr(t(pt: 'Restauração de Abas', en: 'Tab Restoration')),
            description: ref.tr(
              t(
                pt:
                    'A aba ativa é salva automaticamente. Quando você volta para esta página, '
                    'a mesma aba que estava selecionada será restaurada.',
                en:
                    'The active tab is saved automatically. When you return to this page, '
                    'the same tab that was selected will be restored.',
              ),
            ),
            color: Colors.blue,
          ),

          const SizedBox(height: Spacing.lg),

          _buildInfoCard(
            theme,
            icon: Icons.vertical_align_center,
            title: ref.tr(
              t(pt: 'Restauração de Rolagem', en: 'Scroll Restoration'),
            ),
            description: ref.tr(
              t(
                pt:
                    'A posição de rolagem é salva automaticamente. Quando você volta para esta página, '
                    'a rolagem será restaurada para a mesma posição.',
                en:
                    'The scroll position is saved automatically. When you return to this page, '
                    'scrolling will be restored to the same position.',
              ),
            ),
            color: Colors.green,
          ),

          const SizedBox(height: Spacing.lg),

          _buildInfoCard(
            theme,
            icon: Icons.edit_document,
            title: ref.tr(
              t(pt: 'Restauração de Formulário', en: 'Form Restoration'),
            ),
            description: ref.tr(
              t(
                pt:
                    'Os dados do formulário são salvos quando você preenche os campos. '
                    'Quando você volta, todos os campos são restaurados com os valores anteriores.',
                en:
                    'Form data is saved as you fill in the fields. '
                    'When you return, all fields are restored with the previous values.',
              ),
            ),
            color: Colors.orange,
          ),

          const SizedBox(height: Spacing.xl),

          Container(
            padding: const EdgeInsets.all(Spacing.lg),
            decoration: BoxDecoration(
              color: theme.colorScheme.primaryContainer,
              borderRadius: BorderRadius.circular(Spacing.md),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    Icon(Icons.lightbulb, color: theme.colorScheme.primary),
                    const SizedBox(width: Spacing.sm),
                    Text(
                      ref.tr(t(pt: 'Como Testar', en: 'How to Test')),
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
                          '1. Preencha o formulário na aba "Formulário"\n'
                          '2. Role até o final na aba "Conteúdo"\n'
                          '3. Navegue para outra página (ex: Home)\n'
                          '4. Clique no botão voltar do navegador\n'
                          '5. Verifique que tudo foi restaurado!',
                      en:
                          '1. Fill the form in the "Form" tab\n'
                          '2. Scroll to the bottom in the "Content" tab\n'
                          '3. Navigate to another page (e.g. Home)\n'
                          '4. Click the browser back button\n'
                          '5. Verify that everything was restored!',
                    ),
                  ),
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: theme.colorScheme.onPrimaryContainer,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  // Helper widgets
  Widget _buildSectionHeader(
    ThemeData theme, {
    required IconData icon,
    required String title,
    required String subtitle,
  }) {
    return Container(
      padding: const EdgeInsets.all(Spacing.lg),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          colors: [
            theme.colorScheme.primaryContainer,
            theme.colorScheme.secondaryContainer,
          ],
        ),
        borderRadius: BorderRadius.circular(Spacing.lg),
      ),
      child: Row(
        children: [
          Icon(icon, size: 40, color: theme.colorScheme.primary),
          const SizedBox(width: Spacing.md),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: theme.textTheme.titleLarge?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: Spacing.xs),
                Text(
                  subtitle,
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

  Widget _buildInfoCard(
    ThemeData theme, {
    required IconData icon,
    required String title,
    required String description,
    required Color color,
  }) {
    return Container(
      padding: const EdgeInsets.all(Spacing.lg),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.1),
        borderRadius: BorderRadius.circular(Spacing.md),
        border: Border.all(color: color.withValues(alpha: 0.3)),
      ),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Icon(icon, color: color, size: 32),
          const SizedBox(width: Spacing.md),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: theme.textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                    color: color,
                  ),
                ),
                const SizedBox(height: Spacing.xs),
                Text(description, style: theme.textTheme.bodyMedium),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Color _getColorForSection(int section) {
    final colors = [
      Colors.blue,
      Colors.green,
      Colors.orange,
      Colors.purple,
      Colors.red,
      Colors.teal,
    ];
    return colors[section % colors.length];
  }
}
