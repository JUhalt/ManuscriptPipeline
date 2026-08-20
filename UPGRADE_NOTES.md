# PaperRoute upgrade notes

PaperRoute is designed to preserve the local research library across application upgrades.

## Installed builds

For normal upgrades, use an installed PaperRoute build rather than replacing application files manually.

- **Stable** receives stable releases.
- **Preview** receives prereleases such as alpha and release-candidate builds.
- Automatic update checks can be enabled or disabled in PaperRoute settings.
- Manual update checks are available from **Settings → Check for Updates...**.

Portable and developer builds intentionally do not replace themselves in place.

## Before upgrading

PaperRoute upgrades are designed not to overwrite the manuscript database, settings, schema metadata, managed document library, or externally linked files. Even so, creating a current PaperRoute backup before an important upgrade is a sensible precaution.

Use **Data → Backup Library...** to create a portable backup.

## Legacy ManuscriptPipeline data

PaperRoute can migrate compatible legacy ManuscriptPipeline storage into the current PaperRoute storage layout.

The migration is intentionally conservative: legacy source data is retained where practical for rollback and recovery rather than silently deleted.

The internal Visual Basic project/folder name remains `ManuscriptPipeline` for compatibility. This does not change the user-facing application name or current PaperRoute storage location.

## After upgrading

Confirm the following:

1. PaperRoute opens normally.
2. Your manuscript count and shelves look correct.
3. A few manuscript, submission, decision, and correspondence records open as expected.
4. **Diagnostics** reports the expected application version and storage schema.
5. Your managed document links still open.
6. A fresh backup can be created successfully.

If PaperRoute reports that local storage cannot be validated safely, do not delete or replace the data files. Preserve the reported files and use the diagnostics/recovery information to investigate the problem.
