import { defineConfig } from 'tsup';

export default defineConfig({
  entry: ['src/index.ts'],
  format: ['cjs', 'esm'],
  dts: true,
  splitting: false,
  sourcemap: true,
  clean: true,
  minify: false,
  treeshake: true,
  bundle: true,
  noExternal: [],
  external: [
    'react',
    'react-dom',
    'next',
    'next/image',
    'next/link',
    'next/navigation',
    'next-auth',
    'next-auth/react',
    '@ant-design/icons',
    '@ant-design/nextjs-registry',
    '@microsoft/signalr',
    'antd',
    'lucide-react',
    'tailwindcss',
    'clsx',
    'tailwind-merge',
    'class-variance-authority',
    '@radix-ui/react-alert-dialog',
    '@radix-ui/react-avatar',
    '@radix-ui/react-checkbox',
    '@radix-ui/react-dialog',
    '@radix-ui/react-dropdown-menu',
    '@radix-ui/react-label',
    '@radix-ui/react-progress',
    '@radix-ui/react-select',
    '@radix-ui/react-slot',
    '@radix-ui/react-tabs',
    'crypto-js',
    'recharts',
    'recharts/es6',
    'recharts/lib',
    'recharts/es6/cartesian',
    'recharts/es6/component',
    'recharts/es6/util',
    'recharts/lib/cartesian',
    'recharts/lib/component',
    'recharts/lib/util',
    'stream-chat',
    'stream-chat-react',
    'react-window',
    '@emailjs/browser',
    'react-image-gallery',
    'react-popper',
    'react-markdown',
    'hast-util-sanitize',
    'emoji-mart',
    'textarea-caret'
  ],
  esbuildOptions(options) {
    // Remove global "use client" banner to prevent bundling warnings
    // Components that need "use client" should specify it individually
    
    // Add plugins to handle recharts externalization
    if (!options.plugins) {
      options.plugins = [];
    }
    options.plugins.push({
      name: 'recharts-external',
      setup(build) {
        build.onResolve({ filter: /^recharts/ }, args => {
          return { path: args.path, external: true };
        });
      },
    });
  },
});