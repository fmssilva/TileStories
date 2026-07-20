// OPTIMIZED AI Context Summary Generator

// Generates 6 focused summary files from @index-* tags
//
// Run:
// dart run PROJECT_GUIDES/SUMMARY_FILES/Summaries_Maker.dart

import 'dart:io';

void main() async {
  print('🚀 AI Context Summary Generator v2.0\n');

  final projectRoot = Directory.current.path;
  final sep = Platform.pathSeparator;
  final libDir = Directory('$projectRoot${sep}lib');
  final outputDir = Directory(
    '$projectRoot${sep}PROJECT_GUIDES${sep}SUMMARY_FILES',
  );

  if (!libDir.existsSync()) {
    print('❌ lib/ not found at ${libDir.path}');
    exit(1);
  }

  outputDir.createSync(recursive: true);

  final data = await scan(libDir);
  generate(data, outputDir, sep);

  print(
    '\n✅ Generated ${data.totalTags} summaries from ${data.filesScanned} files',
  );
  print('📂 Output: ${outputDir.path}');
}

Future<ScanResult> scan(Directory libDir) async {
  print('📂 Scanning ${libDir.path}...\n');

  final result = ScanResult();

  await for (final entity in libDir.list(recursive: true)) {
    if (entity is! File || !entity.path.endsWith('.dart')) continue;
    if (entity.path.endsWith('.freezed.dart') ||
        entity.path.endsWith('.g.dart')) {
      continue;
    }

    result.filesScanned++;
    final content = await entity.readAsString();
    final relPath =
        entity.path.replaceFirst(libDir.path, 'lib').replaceAll('\\', '/');

    final items = _extractTags(content, relPath);
    result.totalTags += items.length;

    for (final item in items) {
      final domain = item.metadata['layer']?.split('/').first ?? 'unknown';

      result.domains
          .putIfAbsent(domain, () => DomainInfo(domain))
          .addItem(item);

      switch (item.type) {
        case 'model':
          result.models.add(item);
        case 'provider':
          result.providers.add(item);
        case 'widget' || 'page':
          result.ui.add(item);
        case 'repository':
          result.repositories.add(item);
        case 'util' || 'extension':
          result.utils.add(item);
      }
    }
  }

  print(
    '📊 Found ${result.domains.length} domains, ${result.totalTags} tagged items\n',
  );
  return result;
}

List<IndexItem> _extractTags(String content, String filePath) {
  final items = <IndexItem>[];
  final lines = content.split('\n');

  for (var i = 0; i < lines.length; i++) {
    final line = lines[i].trim();
    if (!line.startsWith('/// @index-')) continue;

    final match = RegExp(r'@index-(\w+)\s+(.+)').firstMatch(line);
    if (match == null) continue;

    final type = match.group(1)!;
    final name = match.group(2)!;
    final metadata = <String, String>{};

    var j = i + 1;
    while (j < lines.length && lines[j].trim().startsWith('///')) {
      final metaMatch = RegExp(r'@([\w-]+)\s+(.+)').firstMatch(lines[j].trim());
      if (metaMatch != null) {
        metadata[metaMatch.group(1)!] = metaMatch.group(2)!;
      }
      j++;
    }

    items.add(IndexItem(type, name, filePath, metadata));
  }

  return items;
}

void generate(ScanResult data, Directory outputDir, String sep) {
  print('📝 Writing summaries...\n');

  _writeDomains(data, outputDir, sep);
  _writeModels(data, outputDir, sep);
  _writeProviders(data, outputDir, sep);
  _writeUI(data, outputDir, sep);
  _writeRepositories(data, outputDir, sep);
  _writeUtils(data, outputDir, sep);
  _writeFull(data, outputDir, sep);
}

void _writeDomains(ScanResult data, Directory outputDir, String sep) {
  final b = StringBuffer();
  b.writeln('# 🏛️ Domains Map\n');
  b.writeln('**Purpose:** High-level overview of all domains in the app\n');
  b.writeln('**Total Domains:** ${data.domains.length}\n');
  b.writeln('---\n');

  final sorted = data.domains.values.toList()
    ..sort((a, b) => a.name.compareTo(b.name));

  for (final domain in sorted) {
    b.writeln('## `${domain.name}`\n');
    b.writeln('**Models:** ${domain.models}');
    b.writeln('**Providers:** ${domain.providers}');
    b.writeln('**UI Components:** ${domain.ui}');
    b.writeln('**Repositories:** ${domain.repositories}\n');

    if (domain.items.isNotEmpty) {
      b.writeln('**Components:**');
      for (final item in domain.items.take(5)) {
        b.writeln('- `${item.name}` (${item.type})');
      }
      if (domain.items.length > 5) {
        b.writeln('- ... and ${domain.items.length - 5} more');
      }
      b.writeln('');
    }
  }

  File(
    '${outputDir.path}${sep}SUMMARY_DOMAINS.md',
  ).writeAsStringSync(b.toString());
  print('✓ SUMMARY_DOMAINS.md');
}

void _writeModels(ScanResult data, Directory outputDir, String sep) {
  final b = StringBuffer();
  b.writeln('# 📦 Models\n');
  b.writeln('**Total:** ${data.models.length}\n');
  b.writeln('---\n');

  final byLayer = <String, List<IndexItem>>{};
  for (final m in data.models) {
    byLayer.putIfAbsent(m.metadata['layer'] ?? 'unknown', () => []).add(m);
  }

  for (final entry in byLayer.entries) {
    b.writeln('## ${entry.key}\n');
    for (final m in entry.value) {
      b.writeln('### `${m.name}`');
      b.writeln('**File:** `${m.filePath}`');
      if (m.metadata['description'] != null) {
        b.writeln('**Desc:** ${m.metadata['description']}');
      }
      if (m.metadata['fields'] != null) {
        b.writeln('**Fields:** ${m.metadata['fields']}');
      }
      if (m.metadata['depends-on'] != null) {
        b.writeln('**Depends:** ${m.metadata['depends-on']}');
      }
      b.writeln('');
    }
  }

  File(
    '${outputDir.path}${sep}SUMMARY_MODELS.md',
  ).writeAsStringSync(b.toString());
  print('✓ SUMMARY_MODELS.md');
}

void _writeProviders(ScanResult data, Directory outputDir, String sep) {
  final b = StringBuffer();
  b.writeln('# 🔌 Providers\n');
  b.writeln('**Total:** ${data.providers.length}\n');
  b.writeln('---\n');

  final byLayer = <String, List<IndexItem>>{};
  for (final p in data.providers) {
    byLayer.putIfAbsent(p.metadata['layer'] ?? 'unknown', () => []).add(p);
  }

  for (final entry in byLayer.entries) {
    b.writeln('## ${entry.key}\n');
    for (final p in entry.value) {
      b.writeln('### `${p.name}`');
      b.writeln('**File:** `${p.filePath}`');
      if (p.metadata['description'] != null) {
        b.writeln('**Desc:** ${p.metadata['description']}');
      }
      if (p.metadata['type'] != null) {
        b.writeln('**Type:** ${p.metadata['type']}');
      }
      if (p.metadata['state'] != null) {
        b.writeln('**State:** ${p.metadata['state']}');
      }
      if (p.metadata['depends-on'] != null) {
        b.writeln('**Depends:** ${p.metadata['depends-on']}');
      }
      b.writeln('');
    }
  }

  File(
    '${outputDir.path}${sep}SUMMARY_PROVIDERS.md',
  ).writeAsStringSync(b.toString());
  print('✓ SUMMARY_PROVIDERS.md');
}

void _writeUI(ScanResult data, Directory outputDir, String sep) {
  final b = StringBuffer();
  b.writeln('# 🎨 UI Components (Widgets + Pages)\n');
  b.writeln('**Total:** ${data.ui.length}\n');
  b.writeln('---\n');

  final pages = data.ui.where((i) => i.type == 'page').toList();
  final widgets = data.ui.where((i) => i.type == 'widget').toList();

  if (pages.isNotEmpty) {
    b.writeln('## Pages (${pages.length})\n');
    for (final p in pages) {
      b.writeln('### `${p.name}`');
      b.writeln('**File:** `${p.filePath}`');
      if (p.metadata['description'] != null) {
        b.writeln('**Desc:** ${p.metadata['description']}');
      }
      if (p.metadata['depends-on'] != null) {
        b.writeln('**Uses:** ${p.metadata['depends-on']}');
      }
      b.writeln('');
    }
  }

  if (widgets.isNotEmpty) {
    b.writeln('## Widgets (${widgets.length})\n');
    for (final w in widgets) {
      b.writeln('### `${w.name}`');
      b.writeln('**File:** `${w.filePath}`');
      b.writeln('**Layer:** ${w.metadata['layer'] ?? '?'}');
      if (w.metadata['description'] != null) {
        b.writeln('**Desc:** ${w.metadata['description']}');
      }
      if (w.metadata['ui-type'] != null) {
        b.writeln('**Type:** ${w.metadata['ui-type']}');
      }
      b.writeln('');
    }
  }

  File('${outputDir.path}${sep}SUMMARY_UI.md').writeAsStringSync(b.toString());
  print('✓ SUMMARY_UI.md');
}

void _writeRepositories(ScanResult data, Directory outputDir, String sep) {
  final b = StringBuffer();
  b.writeln('# 🗄️ Repositories\n');
  b.writeln('**Total:** ${data.repositories.length}\n');
  b.writeln('---\n');

  for (final r in data.repositories) {
    b.writeln('### `${r.name}`');
    b.writeln('**File:** `${r.filePath}`');
    if (r.metadata['description'] != null) {
      b.writeln('**Desc:** ${r.metadata['description']}');
    }
    if (r.metadata['caching'] != null) {
      b.writeln('**Caching:** ${r.metadata['caching']}');
    }
    if (r.metadata['data-source'] != null) {
      b.writeln('**Source:** ${r.metadata['data-source']}');
    }
    b.writeln('');
  }

  File(
    '${outputDir.path}${sep}SUMMARY_REPOSITORIES.md',
  ).writeAsStringSync(b.toString());
  print('✓ SUMMARY_REPOSITORIES.md');
}

void _writeUtils(ScanResult data, Directory outputDir, String sep) {
  final b = StringBuffer();
  b.writeln('# 🔧 Utils & Extensions\n');
  b.writeln('**Total:** ${data.utils.length}\n');
  b.writeln('---\n');

  for (final u in data.utils) {
    b.writeln('### `${u.name}`');
    b.writeln('**File:** `${u.filePath}`');
    b.writeln('**Type:** ${u.type}');
    if (u.metadata['description'] != null) {
      b.writeln('**Desc:** ${u.metadata['description']}');
    }
    b.writeln('');
  }

  File(
    '${outputDir.path}${sep}SUMMARY_UTILS.md',
  ).writeAsStringSync(b.toString());
  print('✓ SUMMARY_UTILS.md');
}

void _writeFull(ScanResult data, Directory outputDir, String sep) {
  final b = StringBuffer();
  b.writeln('# 📚 Complete Summary\n');
  b.writeln('**Generated:** ${DateTime.now()}\n');
  b.writeln('**Domains:** ${data.domains.length}');
  b.writeln('**Models:** ${data.models.length}');
  b.writeln('**Providers:** ${data.providers.length}');
  b.writeln('**UI:** ${data.ui.length}');
  b.writeln('**Repositories:** ${data.repositories.length}');
  b.writeln('**Utils:** ${data.utils.length}');
  b.writeln('**Total:** ${data.totalTags}\n');
  b.writeln('---\n');

  b.writeln('## Quick Index\n');

  b.writeln('### Domains');
  for (final d in data.domains.values) {
    b.writeln('- `${d.name}` (${d.items.length} components)');
  }
  b.writeln('');

  b.writeln('### Models');
  for (final m in data.models) {
    b.writeln('- `${m.name}` → ${m.filePath}');
  }
  b.writeln('');

  b.writeln('### Providers');
  for (final p in data.providers) {
    b.writeln('- `${p.name}` → ${p.filePath}');
  }
  b.writeln('');

  File(
    '${outputDir.path}${sep}SUMMARY_FULL.md',
  ).writeAsStringSync(b.toString());
  print('✓ SUMMARY_FULL.md\n');
}

class ScanResult {
  final domains = <String, DomainInfo>{};
  final models = <IndexItem>[];
  final providers = <IndexItem>[];
  final ui = <IndexItem>[];
  final repositories = <IndexItem>[];
  final utils = <IndexItem>[];
  int filesScanned = 0;
  int totalTags = 0;
}

class DomainInfo {
  final String name;
  final items = <IndexItem>[];
  int models = 0, providers = 0, ui = 0, repositories = 0;

  DomainInfo(this.name);

  void addItem(IndexItem item) {
    items.add(item);
    switch (item.type) {
      case 'model':
        models++;
      case 'provider':
        providers++;
      case 'widget' || 'page':
        ui++;
      case 'repository':
        repositories++;
    }
  }
}

class IndexItem {
  final String type, name, filePath;
  final Map<String, String> metadata;

  IndexItem(this.type, this.name, this.filePath, this.metadata);
}
