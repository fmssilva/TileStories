# Feedback System Guide

> **For AI Agents**: Concise reference for implementing user feedback in TileStories

## 📁 Location
`src/components/feedback/` - All loading, error, and success feedback components

## 🎯 When to Use Generic vs Inline

### ✅ USE Generic Components From `/feedback`
- **Spinner**: Generic loading states (API calls, lazy loading)
- **ProgressBar**: File uploads, multi-step processes with known progress
- **Alert**: Inline notifications (form validation, page-level messages)
- **Toast**: Non-blocking success/error notifications (save confirmations, background operations)
- **ErrorBoundary**: Wrap route components to catch crashes
- **FeedbackModal**: Success/error dialogs after critical actions (form submissions)
- **NotFound**: 404 error page

### ❌ IMPLEMENT Inline (Don't Use Generic)
- **Skeleton Screens**: Too context-specific (card skeletons, table skeletons) - build directly in component
- **Empty States**: Content-specific messaging (no search results, empty gallery) - build directly in component
- **Loading Buttons**: Use `<Button disabled>` with spinner inside - build directly in component

## 📦 Components Reference

### Loading States
```tsx
import { Spinner, ProgressBar } from '@/components/feedback';

// Simple loading
<Spinner size="md" />

// Progress tracking
<ProgressBar progress={75} variant="success" />
```

### Notifications
```tsx
import { Alert, useToast } from '@/components/feedback';

// Inline alert
<Alert variant="error" onClose={() => {}}>Error message</Alert>

// Toast notification
const toast = useToast();
toast.success('Saved!', { duration: 3000 });
```

### Error Handling
```tsx
import { ErrorBoundary, NotFound } from '@/components/feedback';

// Wrap routes
<ErrorBoundary fallback={<CustomError />}>
  <YourComponent />
</ErrorBoundary>

// 404 page - already in App.tsx routes
```

### Modal Feedback
```tsx
import { FeedbackModal } from '@/components/feedback';

<FeedbackModal
  isOpen={showSuccess}
  onClose={() => setShowSuccess(false)}
  variant="success"
  title="Success!"
  message="Changes saved"
/>
```

## 🔄 Data Flow Patterns

### Toast System
1. Wrap app in `<ToastProvider>` (in `main.tsx`)
2. Use `useToast()` hook anywhere in app
3. Call `toast.success()` / `toast.error()` - toast appears in portal, auto-dismisses

### Async Operations
```tsx
import { useAsync } from '@/components/feedback';

const { execute, loading, error, data } = useAsync(fetchData);

// In component:
{loading && <Spinner />}
{error && <Alert variant="error">{error}</Alert>}
{data && <YourContent />}
```

## 🎨 Variants
All components support: `'info' | 'success' | 'warning' | 'error'`
- Consistent color coding (blue/green/yellow/red)
- Automatic icon selection
- Dark mode compatible

## 🚫 Common Mistakes
- ❌ Don't create custom spinners - use `<Spinner>`
- ❌ Don't build generic alerts - use `<Alert>` or `<Toast>`
- ❌ Don't skip ErrorBoundary on route components - always wrap
- ❌ Don't use modals for every notification - prefer toasts for non-critical feedback
- ❌ Don't abstract skeleton screens - too content-specific

## 📝 Implementation Checklist
- [ ] Loading: Use `<Spinner>` or `useAsync` hook
- [ ] Success: Use `toast.success()` or `<FeedbackModal variant="success">`
- [ ] Error: Use `<Alert>`, `toast.error()`, or `<ErrorBoundary>`
- [ ] Progress: Use `<ProgressBar>` for trackable operations
- [ ] 404s: Already handled by `<NotFound>` in routes

## 🏗️ Folder Structure
```
feedback/
  ├── Alert.tsx              // Inline notifications
  ├── ErrorBoundary.tsx      // Crash recovery
  ├── FeedbackModal.tsx      // Success/error dialogs
  ├── NotFound.tsx           // 404 page
  ├── ProgressBar.tsx        // Progress indicator
  ├── Spinner.tsx            // Loading spinner
  ├── Toast.tsx              // Toast notification
  ├── ToastProvider.tsx      // Toast context
  ├── types.ts               // Shared types
  ├── index.ts               // Exports
  └── hooks/
      ├── useAsync.ts        // Async state management
      ├── useToast.ts        // Toast notifications
      └── index.ts           // Hook exports
```

## 🎓 Best Practices
1. **Loading**: Show spinner immediately on user action
2. **Success**: Use toasts for confirmations, modals for critical actions
3. **Errors**: Display inline for forms, toasts for background operations
4. **Progress**: Only show if operation takes >2s and progress is trackable
5. **Recovery**: Always provide clear next steps (retry, cancel, navigate)
