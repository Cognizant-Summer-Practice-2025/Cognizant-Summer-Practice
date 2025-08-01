#!/usr/bin/env node

const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

console.log('🏗️  Building shared components package...\n');

try {
  // Clean previous builds
  console.log('🧹 Cleaning previous builds...');
  execSync('npm run clean', { stdio: 'inherit' });

  // Type check
  console.log('🔍 Type checking...');
  execSync('npm run type-check', { stdio: 'inherit' });

  // Build the package
  console.log('📦 Building package...');
  execSync('npm run build:package', { stdio: 'inherit' });

  // Verify dist folder exists and has content
  const distPath = path.join(__dirname, 'dist');
  if (!fs.existsSync(distPath)) {
    throw new Error('Build failed: dist folder not created');
  }

  const distFiles = fs.readdirSync(distPath);
  if (distFiles.length === 0) {
    throw new Error('Build failed: dist folder is empty');
  }

  console.log('✅ Build completed successfully!');
  console.log(`📁 Generated files: ${distFiles.join(', ')}`);

  // Create npm package
  console.log('\n📦 Creating npm package...');
  const packResult = execSync('npm pack', { encoding: 'utf8' });
  const packageFile = packResult.trim();

  console.log(`✅ Package created: ${packageFile}`);
  console.log('\n🎉 Build and pack completed successfully!');
  console.log('\nTo install this package in other projects:');
  console.log(`npm install ./frontend/shared-components/${packageFile}`);

} catch (error) {
  console.error('❌ Build failed:', error.message);
  process.exit(1);
}