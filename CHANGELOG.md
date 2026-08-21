# Changelog

### v0.2E development
- Added BibTeX and RIS import with preview-before-apply record selection.
- Added DOI/title duplicate protection and reusable structured-author matching during bibliography import.
- Added explicit control over whether imported publication records enter Published or remain Ideas.
- Added warnings for unsupported and ambiguous source fields instead of silently discarding them.
- Added BibTeX and RIS export for user-selected PaperRoute manuscripts.
- Added regression coverage for parsing, export, round-trip metadata, duplicate detection, author reuse/order, and lifecycle-safe imports.

### v0.2D development
- Added standards-aware ORCID iD normalization and checksum validation.
- Added user-initiated, read-only public ORCID profile lookup with preview-before-apply controls for names, reusable affiliations, and candidate works.
- Added one-way ORCID work import with DOI/title deduplication, provenance metadata, and explicit control over whether dated works enter Published or remain Ideas.
- Added Crossref enrichment of manuscript-specific affiliations for authors already attached to a manuscript without duplicating the structured author.
- Improved Manuscript Details author controls so move buttons remain accessible at narrower widths while the author list keeps useful vertical space.
- Added regression coverage for ORCID validation/parsing/application, duplicate work handling, explicit Published import, and Crossref enrichment of already-assigned authors.

### v0.2C development
- Added DOI normalization and Crossref metadata lookup with preview-before-apply controls.
- Added selective enrichment for DOI, title, publication details, abstract/keywords, and structured authors.
- Added Crossref author matching by ORCID/name and reusable affiliation matching without changing lifecycle state.
- Added Crossref provenance in manuscript external identifiers and regression coverage for normalization, parsing, selective apply behavior, and author deduplication.

All notable changes to PaperRoute Tracker will be documented here.

## [Unreleased]

### Added

- Storage schema 2 foundation for v0.2 metadata and integrations.
- Automatic, validated migration from storage schema 1 to schema 2.
- Preservation of the previous schema metadata as `schema.v1.bak` during schema-1 to schema-2 migration.
- Structured manuscript metadata for abstracts, keywords, DOI, publication details, preprint links, and external identifiers.
- Regression coverage for schema migration safety and schema-2 metadata persistence.
- Isolated development storage for Visual Studio debugger launches so experimental builds do not modify the stable PaperRoute library.
- Stage-filter counts showing the number of manuscripts currently in each lifecycle stage.
- Regression coverage for development-profile selection and stage-count summaries.
- Reusable author records with structured names, optional ORCID, notes, and a single-user "Me" designation.
- Reusable affiliation records for institutions, departments, and locations.
- Manuscript-specific structured author order, affiliation assignments, and corresponding-author designation.
- Authors & Affiliations library management from the Data menu.
- Portable backups now carry reusable author/affiliation metadata when present.
- Regression coverage for author-library recovery, structured-author persistence, metadata-safe manuscript cloning, and author-library backup/restore.
- ORCID public-profile lookup and selective one-way import for author identity, affiliations, and works.
- ORCID work provenance and DOI/title duplicate protection.

### Changed

- Development version advances to `0.2.0-alpha.1`.
- Existing schema-1 libraries are validated before schema metadata is upgraded.
- Schema migration no longer rejects every lower positive schema version; supported migrations are applied explicitly and sequentially.
- Developer windows and Diagnostics clearly identify when the isolated development storage profile is active.
- Existing free-text co-author values are retained as legacy author text rather than being silently parsed into authoritative people.
- Manuscript editing now deep-copies schema-2 metadata and structured authors so unrelated edits cannot drop DOI/publication/preprint metadata.
- External integrations remain preview-first and user controlled; dated ORCID works only enter Published when the user explicitly selects that import behavior.

### Compatibility

- Schema-1 manuscript data is not rewritten during the schema-1 to schema-2 migration.
- Invalid schema-1 manuscript JSON prevents the schema upgrade and leaves the original schema/data unchanged.
- Schema versions newer than the current build continue to fail closed.

## [0.1.0] - 2026-08-20

### Added

- Fresh installations now default to the **Stable** update channel while existing users retain their persisted channel selection.
- Regression coverage verifies Stable defaults and preservation of existing Preview settings.
- Release workflows now generate and publish SHA-256 checksums for packaged release artifacts.
- File Drawer metadata is now visible from Manuscript Details, including the filed date and an editable File Drawer reason.
- Updating a File Drawer reason records the change in manuscript history for traceability.

### Changed

- GitHub Actions used by CI and release workflows are pinned to immutable commit SHAs.
- Main-dashboard layout now scales more reliably at high Windows display scaling, including 150%, 175%, and 200%.
- Manuscript cards, shelf heights, toolbar rows, and action buttons now size more responsively from rendered content instead of relying on fixed pixel geometry.
- Manuscript Details and submission controls have improved spacing and DPI-safe sizing.
- Zero-count Needs Attention indicators remain readable in Dark mode without appearing active.
- Header controls have cleaner labels and more restrained menu chevrons.
- The PaperRoute wordmark aligns with its subtitle and opens About PaperRoute on double-click.

### Fixed

- Single-item Published and File Drawer shelves no longer show unnecessary internal scrolling when enough space is available.
- High-DPI layouts no longer clip the Add Manuscript label, Add Submission control, or Manuscript Details footer actions under the tested scaling range.
- README local-storage documentation no longer contains stale rebrand wording or accidental Markdown fencing.

### Testing

- Automated regression suite now contains **51 passing tests**, including Stable/Preview update-channel persistence coverage.
- Keyboard-only navigation and focus behavior passed the RC accessibility smoke test.
- Main UI and affected dialogs passed display-scaling review at **100%, 125%, 150%, 175%, and 200%**.

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