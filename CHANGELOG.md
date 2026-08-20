# Changelog

All notable changes to PaperRoute Tracker will be documented here.

## [0.1.0-rc.1] - 2026-08-19

### Added

- Automatic recovery from a valid `manuscripts.bak` when the primary manuscript data file is missing, blank, or corrupt.
- Preservation of damaged primary manuscript files for recovery and diagnostics when automatic backup recovery succeeds.
- Fail-closed startup behavior when neither the primary manuscript database nor its safety backup can be loaded safely.
- Dedicated, testable manuscript attention service for overdue revisions, upcoming revision deadlines, long reviews, missing target journals, and recent rejections.
- Regression tests for complex manuscript/submission/decision import relationships.
- Regression tests for missing linked files and missing managed-copy import sources.
- Regression coverage proving managed-copy batch failures roll back newly created copies without deleting source files.
- Strict storage-schema validation for malformed, missing, nonnumeric, zero, and future schema versions.
- Graceful startup error reporting when PaperRoute cannot safely validate or migrate its local storage.
- Release workflow verification that Git tags, compiled ProductVersion values, and Velopack package versions agree.

### Changed

- Manuscript recovery now restores a valid safety backup rather than treating unreadable primary storage as an empty library.
- PaperRoute now closes instead of continuing with an artificial empty manuscript library after an unrecoverable load failure.
- Invalid `schema.json` files are preserved and rejected rather than silently being rewritten as the current schema.
- Missing schema metadata can still be adopted safely for valid pre-schema PaperRoute data.
- Needs Attention rules now use deterministic business logic separated from the main WinForms interface.
- Preview and Stable release builds now receive their compiled application version directly from the release tag.
- Release builds verify the published binary version before Velopack packaging.
- Current PaperRoute storage documentation now reflects the post-rebrand `PaperRoute` data and managed-library locations.

### Compatibility

- Storage schema remains **1**. RC.1 does not introduce a new data schema.
- Existing schema-1 libraries remain compatible.
- Legacy ManuscriptPipeline data is still preserved after migration.
- The legacy manuscript-level `RevisionDeadline` property remains available for compatibility; active route-aware deadline logic uses the deadline attached to the relevant editorial decision.

### Testing

- Automated regression suite expanded to **48 tests**.
- Release builds must pass the full automated test suite before packaging.
- Developer/portable builds correctly decline in-place automatic update checks.
- Installed Preview builds correctly identify themselves as installed and can query the GitHub release feed.
- Installed `0.1.0-alpha.3` has been confirmed to report itself as current on the Preview channel before RC.1 publication.

### RC.1 validation focus

- Prove that an installed `0.1.0-alpha.3` Preview build detects `0.1.0-rc.1`.
- Download and apply the RC.1 update through PaperRoute itself.
- Confirm PaperRoute restarts into `0.1.0-rc.1`.
- Confirm manuscript data, settings, schema metadata, managed files, and linked external files remain intact across the update.
- Compare pre-update and post-update SHA-256 manifests.
- Complete updater/recovery, diagnostics/encoding, accessibility/UI, documentation, and clean-machine validation before declaring `0.1.0` stable.

## [0.1.0-alpha.3] - 2026-08-19

### Added

- Safe migration from legacy ManuscriptPipeline storage into PaperRoute storage with schema versioning and rollback retention.
- Local diagnostics report with storage paths, runtime information, and privacy-safe troubleshooting details.
- Automated regression coverage for persistence, migration, backup/restore, and spreadsheet import workflows.
- **Copy Version Info** action in About PaperRoute for faster troubleshooting.

### Changed

- Preferences and Diagnostics dialogs now resize and reflow more reliably across display sizes and DPI settings.
- GitHub Actions runs now use concise CI and Release names.
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
- Manual update checks from the application.
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

- Legacy local data and managed-library folders are preserved during migration so existing alpha data remains recoverable.