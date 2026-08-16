# Publishing on GitHub

## First push

Create an empty repository named `Toastify-Reloaded`, then from the project folder:

```powershell
git init
git add .
git commit -m "Initial release"
git branch -M main
git remote add origin https://github.com/Marlius92/Toastify-Reloaded.git
git push -u origin main
```

## CI

`.github/workflows/build.yml` runs on `windows-latest` and:

1. restores .NET packages;
2. builds Release;
3. creates a self-contained x64 ZIP;
4. uploads it as a workflow artifact.

## Release

Create and push a version tag:

```powershell
git tag v1.0.0
git push origin v1.0.0
```

`.github/workflows/release.yml` creates x64 and ARM64 self-contained ZIP packages and publishes them in a GitHub Release.

## Recommended repository settings

- Default branch: `main`
- Enable Issues
- Enable Discussions only if you want user support there
- Protect `main` once other contributors are added
- Require the `build` check before merge
- Do not commit `dist/`, `bin/`, `obj/` or personal diagnostic logs
