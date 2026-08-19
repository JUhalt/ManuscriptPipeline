# Changelog

All notable changes to PaperRoute Tracker will be documented here.

## [0.1.0-alpha.3] - 2026-08-19

### Added

- Safe migration from legacy ManuscriptPipeline storage into PaperRoute storage with schema versioning and rollback retention.
- Local diagnostics report with storage paths, runtime information, and privacy-safe troubleshooting details.
- Automated regression coverage for persistence, migration, backup/restore, and spreadsheet import workflows.
- **Copy Version Info** action in About PaperRoute for faster troubleshooting.

### Changed

- Preferences and Diagnostics dialogs now resize and reflow more reliably across display sizes and DPI settings.
- GitHub Actions runs now use concise `CI • <ref>` and `Release • <ref>` names.
- Superseded CI runs on the same ref are automatically canceled.
- CI workflow permissions are explicitly read-only unless a release needs write access.

### Testing focus

- Prove that an installed `0.1.0-alpha.2` Preview build detects, downloads, applies, and restarts into `0.1.0-alpha.3`.
- Verify all manuscript data, settings, and managed-library paths survive the update unchanged.

## [0.1.0-alpha.2] - 2026-08-19

### Added

- Velopack-based Windows installer and update packaging.
- In-app update checks from GitHub Releases.
- Stable and Preview update channels.
- Optional automatic update checks on startup.
- Manual update checks from the Data menu and Settings.
- Release-note preview and download progress before restart.
- Public `ROADMAP.md` summarizing the route to RC and 1.0.

### Changed

- GitHub tag releases now package Velopack installer/update assets rather than only a portable ZIP.
- Installer builds use multi-file self-contained output so future delta updates can avoid repeatedly replacing the bundled .NET runtime.

### Testing focus

- Install alpha.2 with the generated Setup program.
- Use the next prerelease to prove the alpha.2 → alpha.3 automatic-update path before RC.

## [0.1.0-alpha.1] - 2026-08-19

### Added

- Rebrand from the development name ManuscriptPipeline to **PaperRoute Tracker**.
- PaperRoute application icon and refreshed teal/navy visual palette.
- Pipeline, Published, and File Drawer manuscript shelves.
- Manuscript, submission, decision, and correspondence tracking.
- Search, stage filters, sorting, and Needs Attention indicators.
- Light, Dark, and Follow Windows appearance modes.
- Configurable review/revision/File Drawer attention thresholds.
- Standard PaperRoute Excel import template and library export.
- Legacy spreadsheet importer.
- **Mapped spreadsheet importer** for arbitrary Excel column layouts.
- Portable ZIP backups and validated restore workflow.
- GitHub Actions CI for Windows x64 builds.
- Tag-driven GitHub release workflow.

### Compatibility

- Legacy local data and managed-library folder names remain unchanged during the rebrand so existing alpha data continues to load automatically.
