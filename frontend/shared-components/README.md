# Shared Components Package

A shared UI components and utilities package for Cognizant Summer Practice services, built with Next.js, React, TypeScript, and Tailwind CSS.

## Features

- 🎨 **shadcn/ui** components with Tailwind CSS
- 🔧 **Utility functions** for common operations
- 🎣 **Custom hooks** for React components
- 📦 **TypeScript** support with full type definitions
- 🌟 **Tree-shakable** exports for optimal bundle size
- 🎯 **Portfolio templates** and components
- 💬 **Messaging components**
- 👤 **User management** components
- 🔐 **Authentication** utilities

## Installation

This package is designed to be consumed by other services in the monorepo:

```bash
npm install @cognizant-summer-practice/shared-components
```

## Usage

### UI Components

```tsx
import { Button, Input, Card } from '@cognizant-summer-practice/shared-components';

function MyComponent() {
  return (
    <Card>
      <Input placeholder="Enter text..." />
      <Button>Click me</Button>
    </Card>
  );
}
```

### Layout Components

```tsx
import { Header, Providers } from '@cognizant-summer-practice/shared-components';

function Layout({ children }) {
  return (
    <Providers>
      <Header />
      <main>{children}</main>
    </Providers>
  );
}
```

### Hooks

```tsx
import { useModalAnimation } from '@cognizant-summer-practice/shared-components';

function Modal() {
  const { isVisible, show, hide } = useModalAnimation();
  // ... component logic
}
```

### Utilities

```tsx
import { cn, encrypt, decrypt } from '@cognizant-summer-practice/shared-components';

// Tailwind class merging
const classes = cn('bg-blue-500', 'hover:bg-blue-600');

// Encryption utilities
const encrypted = encrypt('sensitive data');
const decrypted = decrypt(encrypted);
```

## Development

### Building and Packing the Package

```bash
npm run build-and-pack
```

This will:
1. Clean previous builds
2. Type check the code
3. Build the package using tsup
4. Create an npm package (.tgz file)

### Individual Commands

```bash
# Just build the package
npm run build

# Create npm package (after building)
npm run pack

# Type checking only
npm run type-check

# Clean build artifacts
npm run clean
```

## Package Structure

```
src/
├── components/        # React components
│   ├── ui/           # shadcn/ui components
│   ├── home-page/    # Home page components
│   ├── portfolio-*   # Portfolio-related components
│   └── ...
├── hooks/            # Custom React hooks
├── lib/              # Utility functions and contexts
├── utils/            # General utilities
└── styles/           # Global styles
```

## Exports

The package provides the following exports:

- **UI Components**: Button, Input, Select, Dialog, etc.
- **Layout Components**: Header, Providers
- **Specialized Components**: Portfolio templates, messaging components
- **Hooks**: Custom React hooks
- **Utilities**: Helper functions, encryption, templates
- **Contexts**: React contexts for state management
- **API Functions**: Data fetching utilities

## Contributing

1. Make changes to the source code
2. Test your changes in a consuming service
3. Update version in package.json
4. Build and publish the package

## Dependencies

The package has peer dependencies on:

- React 19+
- Next.js 15+
- TypeScript 5+

All other dependencies are bundled with the package.