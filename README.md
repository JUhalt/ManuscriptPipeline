<p align="center">
  <img src="docs/paperroute-logo.png" width="160" alt="PaperRoute logo">
</p>

# PaperRoute

[![Build PaperRoute Tracker](https://github.com/JUhalt/PaperRoute-Tracker/actions/workflows/build.yml/badge.svg)](https://github.com/JUhalt/PaperRoute-Tracker/actions/workflows/build.yml)

**A local-first academic manuscript tracker for researchers.**

> **Track • Submit • Publish**

PaperRoute Tracker helps researchers manage manuscripts from idea through submission, peer review, revision, publication—or the File Drawer—without requiring an account or sending the core workflow database to a cloud service.


## Current status

**v0.1.0** — first stable release of the PaperRoute Reliable Core.

The stable 0.1 line focuses on trustworthy local storage, manuscript lifecycle tracking, spreadsheet import/export, backup/restore, installation and updates, accessibility, and recovery. New feature development continues toward v0.2 Metadata & Integrations.

## Highlights

- **Pipeline, Published, and File Drawer shelves** for the complete manuscript lifecycle.
- **Journal submission history** with manuscript numbers, dates, notes, and publisher portal links.
- **Editorial decisions** including rejection, revision, acceptance, and revision deadlines.
- **Correspondence and local-file tracking** for decision letters, reviewer comments, response letters, revised manuscripts, and related material.
- **Needs Attention dashboard** for overdue revisions, long reviews, missing target journals, and recent rejections.
- **Search, stage filtering, and sorting** across the manuscript library.
- **Light, Dark, and Follow Windows themes** using modern .NET 10 WinForms theming.
- **Excel import/export** using the PaperRoute workbook format.
- **Legacy tracker import** for the original development spreadsheet format.
- **Column-mapping import wizard** for arbitrary spreadsheets whose headings do not match PaperRoute.
- **Portable ZIP backup and restore** with an emergency pre-restore backup.
- **Local-first storage** and a managed local document library.

## Privacy and local-first design

PaperRoute is designed so that its core manuscript-tracking workflow works offline. No PaperRoute account is required.

Current PaperRoute application data is stored under `%LocalAppData%\PaperRoute\`.

The manuscript database, automatic backup, settings, and storage-schema metadata are stored there. Managed document copies are stored in the PaperRoute managed library, normally `Documents\PaperRoute Library\`.

When legacy ManuscriptPipeline storage is migrated, PaperRoute retains the legacy source data where possible for rollback and recovery rather than silently deleting it.

## Installing PaperRoute

### Recommended: GitHub Release installer

On the GitHub **Releases** page, download and run the PaperRoute Setup executable from the latest stable release. Installed builds can then check GitHub Releases for future PaperRoute updates.

Fresh installations default to the **Stable** update channel. Users who intentionally want prerelease builds can opt into **Preview** from **Settings → Updates**.

### Portable CI build

The repository's **Build PaperRoute Tracker** workflow still produces a self-contained `PaperRouteTracker-win-x64` artifact for smoke testing. Portable/developer builds intentionally do not perform in-place automatic updates; install PaperRoute using the Setup program to test the updater.

## Importing existing work

Choose **Data → Import Spreadsheet...**.

PaperRoute uses three import paths automatically:

### 1. Standard PaperRoute workbook

Use **Data → Get Import Template...** to generate the supported multi-sheet workbook. It contains:

- `Manuscripts`
- `Submissions`
- `Decisions`
- `Correspondence`

This is the best format for loss-minimized round-trip import/export.

### 2. Legacy tracker

PaperRoute recognizes the original development tracker when it contains the expected legacy columns such as `TITLE`, `JOURNAL`, `SUBMITTED`, `RESPONSE`, and `STATUS`.

### 3. Map your spreadsheet

If PaperRoute does not recognize either known format, it opens a column-mapping wizard. You can map headings such as:

```text
Paper Name       → Title
Authors          → Co-authors
Outlet           → Submission journal
Date Sent        → Submitted date
Current Status   → Current stage
Outcome          → Editorial decision
Decision Date    → Decision date
Comments         → Notes
```

Only **Title** is required. PaperRoute auto-suggests mappings from common academic spreadsheet headings and shows sample values before import.

## Backup and restore

Choose **Data → Backup Library...** to create a portable ZIP containing:

```text
backup-info.txt
manuscripts.json
library.xlsx
files\
```

Managed document copies are included in the backup. Externally linked files remain references to their original paths.

**Restore Backup...** validates the archive, previews record/file counts, asks for explicit confirmation, creates an emergency backup of the current library, and then restores the selected archive.

## File Drawer

PaperRoute treats **Published** and **File Drawer** as terminal shelves, while still allowing a filed manuscript to be restored to the active Pipeline. The configurable File Drawer suggestion threshold is intended as a prompt, not an automatic decision.

## Building from source

Requirements:

- Windows 11 recommended
- .NET 10 SDK
- Visual Studio 2026 or another environment capable of building VB.NET WinForms projects

Clone the repository and build:

```powershell
git clone https://github.com/JUhalt/PaperRoute-Tracker.git
cd PaperRoute-Tracker
dotnet restore ManuscriptPipeline.slnx
dotnet build ManuscriptPipeline.slnx --configuration Release
```

The internal project/folder name remains `ManuscriptPipeline` for compatibility and to avoid unnecessary namespace churn. The built assembly is `PaperRouteTracker.exe`.

## Technology

- Visual Basic .NET
- .NET 10 Windows Forms
- ClosedXML for Excel workbook support
- System.Text.Json for local persistence
- GitHub Actions for Windows CI and release builds
- Velopack for Windows installation and automatic updates

## Inspiration and independence

PaperRoute was inspired by the broader idea of academic manuscript pipeline tools, including the workflow concepts presented by PaperTrek. PaperRoute is an independent open-source project and is not affiliated with or endorsed by PaperTrek.

The implementation, local-first data model, import/export system, backup workflow, and interface are independently developed for PaperRoute.

## Roadmap

See [`ROADMAP.md`](ROADMAP.md) for the public high-level route. GitHub milestones and issues are the live source of truth for active release work.

## Contributing

Bug reports, usability feedback, importer edge cases, and pull requests are welcome. See [`CONTRIBUTING.md`](CONTRIBUTING.md).

## License

PaperRoute Tracker is licensed under the **GNU General Public License v3.0**. See [`LICENSE.txt`](LICENSE.txt).
