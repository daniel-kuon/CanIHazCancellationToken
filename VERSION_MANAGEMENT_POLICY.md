# Version Management Policy

This document outlines the version management policy for the CanIHazCancellationToken project to prevent unintended downgrades that could introduce compatibility issues, security vulnerabilities, or break functionality.

## Core Principles

### 1. No Automatic Downgrades
- Package versions should never be automatically downgraded
- .NET framework/SDK versions should never be automatically downgraded
- Any downgrade must be explicit and well-justified

### 2. Version Consistency
- Maintain consistent package versions across the solution
- Use `Directory.Build.props` to enforce minimum versions
- Lock file support (`packages.lock.json`) to ensure reproducible builds

### 3. Upgrade Safety
- Prefer stable package versions over pre-release versions
- Test thoroughly when upgrading major versions
- Document breaking changes in release notes

## Current Version Requirements

### .NET Framework Targets
- `CanIHazCancellationTokenAnalyzer`: `netstandard2.0` (minimum)
- `CanIHazCancellationTokenAnalyzer.Tests`: `net8.0` (minimum) 
- `CanIHazCancellationTokenSample`: `net8.0` (minimum)

### Key Package Versions
- `Microsoft.CodeAnalysis`: `4.14.0` (minimum)
- `Microsoft.CodeAnalysis.CSharp`: `4.13.0` (minimum)
- `Microsoft.CodeAnalysis.Analyzers`: `3.11.0` (minimum)
- `Microsoft.CodeAnalysis.CSharp.Workspaces`: `4.13.0` (minimum)

### .NET SDK
- Minimum SDK version specified in `global.json`
- Use `rollForward: "latestMajor"` for flexibility while maintaining baseline

## Implementation

### 1. Copilot Instructions
Enhanced `.github/copilot-instructions.md` with explicit version management guidelines.

### 2. MSBuild Properties
`Directory.Build.props` enforces minimum package versions using version ranges:
```xml
<PackageReference Update="Microsoft.CodeAnalysis" Version="[4.14.0,)" />
```

### 3. GitHub Actions
`version-validation.yml` workflow validates:
- .NET SDK version consistency
- Target framework versions are not downgraded
- Package versions meet minimum requirements
- Alerts on changes to version-sensitive files

### 4. Package Lock Files
Enable `RestorePackagesWithLockFile` to ensure reproducible builds and detect unexpected version changes.

## Guidelines for Contributors

### When Adding New Dependencies
1. Research the latest stable version
2. Check compatibility with existing dependencies
3. Add minimum version constraints to `Directory.Build.props`
4. Update this document with new requirements

### When Upgrading Existing Dependencies
1. Update minimum versions in `Directory.Build.props`
2. Run full test suite
3. Update release notes if breaking changes are involved
4. Consider impact on analyzer compatibility

### When Downgrades Are Necessary
1. Document the reason in PR description
2. Update minimum version requirements
3. Verify no security vulnerabilities are introduced
4. Consider alternative solutions

## Monitoring and Enforcement

### Automated Checks
- GitHub Actions validate version consistency on every PR
- MSBuild warnings/errors for version constraint violations
- Dependabot security alerts for vulnerable packages

### Manual Reviews
- PR reviews should verify version changes are intentional
- Release notes should document version requirement changes
- Regular dependency audits for security and performance

## Emergency Procedures

### Security Vulnerabilities
1. Assess if upgrade is possible
2. If downgrade is required, document security implications
3. Create tracking issue for future upgrade path
4. Notify maintainers of the decision

### Compatibility Issues
1. Investigate root cause of compatibility problem
2. Consider alternative packages or approaches
3. If downgrade is unavoidable, minimize scope
4. Plan for resolution in next major version

---

**Note**: This policy is enforced through tooling and should be followed by all contributors including Copilot-generated suggestions.