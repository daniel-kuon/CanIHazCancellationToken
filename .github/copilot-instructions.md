# GitHub Copilot Instructions

## Purpose
This repository contains Roslyn analyzers and code fixes for CancellationToken usage in .NET projects. GitHub Copilot can assist with code suggestions, refactoring, and documentation.

## Guidelines for Using Copilot
- Prefer suggestions that follow .NET best practices and Roslyn analyzer conventions.
- Ensure all code changes are covered by unit tests in the `CanIHazCancellationTokenAnalyzer.Tests` project.
- For analyzer and code fix logic, use the appropriate folders (`Analyzers/`, `CodeFixes/`).
- Document public APIs and important internal logic with XML comments.
- When adding new analyzers or code fixes, update the release notes (`AnalyzerReleases.*.md`).
- Run tests after making changes to verify correctness.
- Keep the readme up to date with any new features or changes.

## Commit Message Conventions
- Use descriptive commit messages (e.g., `Add analyzer for missing CancellationToken parameter`).

## Additional Notes
- Follow the existing project structure and naming conventions.
- If unsure about a Copilot suggestion, review and adjust for clarity and maintainability.

---
For more information, see the repository README and contributing guidelines.
