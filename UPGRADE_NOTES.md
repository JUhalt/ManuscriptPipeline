# PaperRoute alpha upgrade notes

This source bundle keeps the internal VB root namespace and legacy local-storage folder names from ManuscriptPipeline so existing alpha data continues to load after the PaperRoute rebrand.

## Recommended replacement workflow

1. Close Visual Studio and PaperRoute/ManuscriptPipeline.
2. Make a copy of your current repository folder.
3. Extract this bundle over the repository root and allow source files to be replaced.
4. Do **not** delete your existing `.git` directory.
5. Reopen `ManuscriptPipeline.slnx` in Visual Studio.
6. Run **Build → Rebuild Solution**.
7. Launch the application and confirm your existing manuscript library appears.

## Smoke test

- Open PaperRoute Tracker.
- Confirm existing manuscripts load.
- Test Light/Dark/System themes.
- Open manuscript and submission details.
- Use **Data → Import Spreadsheet...** with an unfamiliar workbook to test the mapping wizard.
- Export the library to Excel.
- Create a portable backup.
- Close and reopen the application.

## First tagged release

After the source builds cleanly and CI is green:

```powershell
git tag v0.1.0-alpha.1
git push origin v0.1.0-alpha.1
```

The release workflow will publish a Windows x64 release asset automatically.
