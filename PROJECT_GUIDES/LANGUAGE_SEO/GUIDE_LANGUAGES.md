# Language System Guide for AI Agents

## Translation System Overview

This project uses **inline translations** with the `useTranslation` hook. NO external translation files are used.

## Core Rules - MUST FOLLOW

### 1. **Always Portuguese First, English Second**
```tsx
// ✅ CORRECT
useTranslation('Texto em Português', 'English Text')

// ❌ WRONG
useTranslation('English Text', 'Texto em Português')
```

### 2. **Direct Inline When Possible**
```tsx
// ✅ PREFERRED - Direct inline
<h1>{useTranslation('Encontre Tratamentos', 'Find Treatments')}</h1>
<button>{useTranslation('Pesquisar', 'Search')}</button>

// ❌ AVOID - Unnecessary constants
const title = useTranslation('Título', 'Title');
return <h1>{title}</h1>;
```

### 3. **Constants ONLY for React Hook Rules**
Use constants when hooks can't be called inline (conditionals, loops):

```tsx
// ✅ REQUIRED for conditionals
const humanText = useTranslation('Saúde Humana', 'Human Health');
const petText = useTranslation('Cuidados Animais', 'Pet Care');
return <span>{isHuman ? humanText : petText}</span>;

// ✅ REQUIRED for loops/maps
const labels = [
  useTranslation('Início', 'Home'),
  useTranslation('Pesquisar', 'Search'),
];
return labels.map(label => <div key={label}>{label}</div>);
```

### 4. **Hook Rules - Critical Limitations**
```tsx
// ❌ NEVER - Conditional hook calls
{condition ? useTranslation('A', 'B') : useTranslation('C', 'D')}

// ❌ NEVER - Inside callbacks/loops directly
array.map(() => useTranslation('Text', 'Text'))

// ❌ NEVER - Inside nested functions
function helper() { return useTranslation('Text', 'Text'); }
```


## Common Mistakes to Avoid

1. **Creating constants for simple inline text** - Use direct inline calls
2. **English first** - Always Portuguese first, English second
3. **Hook rule violations** - Cannot call hooks inside conditions/loops
4. **Missing translations** - Every user-facing text must be translated

## Testing
Run `npm run build` to ensure no TypeScript/hook rule errors exist after adding translations.
