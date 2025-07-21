# CI/CD Setup Guide

This document describes how to set up the automated publishing pipeline for the CanIHazCancellationToken package.

## Overview

The CI/CD pipeline consists of three main workflows:

1. **CI (`ci.yml`)** - Builds and tests on pull requests
2. **GitHub Packages (`publish-github-packages.yml`)** - Publishes pre-release versions on merged PRs
3. **NuGet (`publish-nuget.yml`)** - Publishes stable versions on releases

## Required Secrets

### For NuGet.org Publishing

You need to add the `NUGET_API_KEY` secret to your repository:

1. Go to https://www.nuget.org/account/apikeys
2. Create a new API key with the following settings:
   - **Package owner**: Your account
   - **Scopes**: Push new packages and package versions
   - **Packages**: Select specific packages → `CanIHazCancellationToken`
3. Copy the generated API key
4. In your GitHub repository, go to Settings → Secrets and variables → Actions
5. Click "New repository secret"
6. Name: `NUGET_API_KEY`
7. Value: Paste the API key from step 3

### For GitHub Packages Publishing

GitHub Packages uses the built-in `GITHUB_TOKEN` which is automatically available in workflows. No additional setup is required.

## Workflows

### 1. Continuous Integration (ci.yml)

**Trigger**: Pull requests to main branch
**Purpose**: Validate that code builds and packages correctly

This workflow:
- Sets up .NET 8.0
- Restores dependencies
- Builds the analyzer project
- Creates a test package
- Uploads the package as an artifact

### 2. GitHub Packages Publishing (publish-github-packages.yml)

**Trigger**: Push to main branch (when PRs are merged)
**Purpose**: Publish pre-release versions for testing

This workflow:
- Generates a version based on commit count: `1.0.{commit-count}-alpha.{short-sha}`
- Updates the project file with the new version
- Builds and packs the project
- Publishes to GitHub Packages

**Example versions**: `1.0.15-alpha.a1b2c3d`

### 3. NuGet Publishing (publish-nuget.yml)

**Trigger**: 
- Release published (automatic)
- Manual dispatch (for testing)

**Purpose**: Publish stable versions to NuGet.org

This workflow:
- Uses the release tag version (e.g., `v1.0.0` → `1.0.0`)
- Updates the project file with the release version  
- Builds and packs the project
- Publishes to NuGet.org
- Uploads package as release artifact

### 4. Create Release (create-release.yml)

**Trigger**: Manual dispatch only
**Purpose**: Helper workflow to create releases with consistent formatting

This workflow:
- Creates a GitHub release with the specified version
- Generates a release template with sections for changes
- Attaches the NuGet package to the release
- Creates the release as a draft for review

## Publishing Process

### For Pre-release Versions (GitHub Packages)

1. Merge a PR to main branch
2. The `publish-github-packages.yml` workflow runs automatically
3. A pre-release version is published to GitHub Packages
4. Users can install with: `dotnet add package CanIHazCancellationToken --version 1.0.X-alpha.XXXXXX --source https://nuget.pkg.github.com/daniel-kuon/index.json`

### For Stable Releases (NuGet.org)

#### Option 1: Automatic (Recommended)
1. Run the "Create Release" workflow manually:
   - Go to Actions → Create Release → Run workflow
   - Enter the version (e.g., `1.0.0`)
   - Choose if it's a pre-release
2. Review and edit the draft release
3. Publish the release
4. The `publish-nuget.yml` workflow runs automatically
5. Package is published to NuGet.org

#### Option 2: Manual Release Creation
1. Create a release manually through GitHub UI
2. Use tag format: `v1.0.0` (with 'v' prefix)
3. The `publish-nuget.yml` workflow runs automatically
4. Package is published to NuGet.org

#### Option 3: Manual Dispatch
1. Go to Actions → Publish to NuGet → Run workflow
2. Enter the version manually
3. Workflow publishes to NuGet.org

## Version Management

- **Development builds**: `1.0.{commit-count}-alpha.{short-sha}`
- **Release versions**: Use semantic versioning (e.g., `1.0.0`, `1.1.0`, `2.0.0`)
- **Pre-releases**: Use suffix like `-alpha`, `-beta`, `-rc` (e.g., `1.0.0-beta.1`)

## Package Installation

### From NuGet.org (Stable)
```xml
<PackageReference Include="CanIHazCancellationToken" Version="1.0.0">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

### From GitHub Packages (Pre-release)
First, add the package source:
```xml
<PackageSource>
  <add key="github" value="https://nuget.pkg.github.com/daniel-kuon/index.json" />
</PackageSource>
```

Then install:
```xml
<PackageReference Include="CanIHazCancellationToken" Version="1.0.X-alpha.XXXXXX">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

## Troubleshooting

### Common Issues

1. **NuGet push fails with 403**: Check that your API key is valid and has the correct scopes
2. **GitHub Packages push fails**: Ensure the repository has the correct permissions settings
3. **Version conflicts**: Make sure you're not trying to publish a version that already exists
4. **Package validation errors**: Check that all required metadata is present in the .csproj file

### Monitoring

- Check the Actions tab for workflow status
- View package status at:
  - NuGet.org: https://www.nuget.org/packages/CanIHazCancellationToken/
  - GitHub Packages: Repository → Packages tab