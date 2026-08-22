# PaperRoute Roadmap

PaperRoute is moving toward a simple goal: **a trustworthy, local-first home for the complete route of an academic manuscript.**

The live GitHub milestones and issues are the source of truth for active work. This file is the public high-level route.

## Release train

PaperRoute is targeting a rapid pre-1.0 release train. These are **target dates, not promises**: data integrity, migration safety, and release certification take priority over the calendar.

| Release | Target | Focus |
| --- | --- | --- |
| **v0.2.0** | **August 22, 2026** | Metadata, integrations, reminders, calendar export, and Help |
| **v0.3.0** | **August 25, 2026** | Visual Route View and manuscript version history |
| **v0.4.0** | **August 28, 2026** | Submission Packet Vault and per-journal readiness |
| **v0.5.0** | **August 31, 2026** | Reviewer Response Matrix |
| **v0.6.0** | **September 3, 2026** | Deadline Center |
| **v0.7.0** | **September 6, 2026** | Route statistics and time-to-publication analytics |
| **v0.8.0** | **September 9, 2026** | Optional AI-assisted reviewer action extraction |
| **v0.9.0** | **September 12, 2026** | 1.0 hardening and workflow polish |
| **v1.0.0** | **September 15, 2026** | Stable-release certification |

## COMPLETE — v0.1 Reliable Core

PaperRoute v0.1.0 established the trusted local core: installation and updates, schema validation, recovery, import/export, backup/restore, accessibility, high-DPI support, diagnostics, and release hardening.

## RELEASE CANDIDATE — v0.2 Metadata & Integrations

The v0.2 feature set is frozen. Release-candidate work is limited to certification, packaging, documentation, and release-blocking fixes.

- [x] Storage schema 2 metadata foundation and validated schema-1 migration
- [x] Reusable authors and affiliations
- [x] DOI and Crossref metadata enrichment
- [x] ORCID public-profile import / one-way sync
- [x] BibTeX and RIS import/export
- [x] Journal library, target-journal workflow, and submission portal shortcuts
- [x] Preprint / journal-version linkage and project links
- [x] Publication and CV exports
- [x] Calendar export, reminders, and optional Windows notifications
- [x] User Guide and in-app Help
- [ ] Final v0.1.0 → v0.2.0 upgrade certification
- [ ] Final backup/restore, clean-install, updater, UI, and packaging certification

**Safety rule:** external integrations may suggest metadata, but they must not silently overwrite user-entered metadata or change manuscript lifecycle state. Any import that changes lifecycle placement must be an explicit user choice.

## v0.3.0 — The Route — target August 25

- Visual Route View and manuscript rerouting (#23)
- Manuscript version history linked to submissions and decisions (#24)

## v0.4.0 — Submission Readiness — target August 28

- Submission Packet Vault and file integrity (#25)
- Per-journal readiness checklists (#26)

## v0.5.0 — Reviewer Response Workflow — target August 31

- Reviewer Response Matrix (#27)

## v0.6.0 — Deadline Center — target September 3

- Deadline Center built on the canonical v0.2 reminder engine (#28)

## v0.7.0 — Route Analytics — target September 6

- Route statistics and time-to-publication analytics (#30)

## v0.8.0 — Optional AI Assistance — target September 9

- Optional AI-assisted reviewer action extraction (#29)
- AI remains opt-in, preview-before-apply, and never a dependency for the reviewer workflow.

## v0.9.0 — 1.0 Hardening — target September 12

- Migration, recovery, backup/restore, and updater burn-down
- Keyboard, DPI, resize, theme, and secondary-dialog polish
- File Drawer revival / rerouting workflow polish
- Portable project-sharing format decision and any safe pre-1.0 groundwork
- Installer trust/signing decision and remaining release infrastructure work

## v1.0.0 — Trusted Research Workflow — target September 15

PaperRoute 1.0 is not defined by feature count.

> **I trust this application with my research workflow.**

The 1.0 bar includes:

- Stable versioned data model and tested migrations
- Reliable installer and updater
- Strong automated regression coverage
- Recovery tooling and proven backup/restore
- Accessible keyboard-first UI
- Transparent local route analytics
- Complete rerouting / File Drawer workflow
- Clear privacy boundaries and no silent external transmission of manuscript content
- Release artifacts, checksums, documentation, and upgrade path verified before publication

### Release discipline

A target date may slip when a release candidate exposes a data-loss, migration, recovery, packaging, or updater defect. PaperRoute does not trade trustworthiness for cadence.
