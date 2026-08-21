# PaperRoute Roadmap

PaperRoute is moving toward a simple goal: **a trustworthy, local-first home for the complete route of an academic manuscript.**

The live GitHub milestones and issues are the source of truth for active work. This file is the public high-level route.

## COMPLETE — v0.1 Reliable Core

PaperRoute v0.1.0 established the trusted local core: installation and updates, schema validation, recovery, import/export, backup/restore, accessibility, high-DPI support, diagnostics, and release hardening.

## NOW — v0.2 Metadata & Integrations

Development begins with the **schema 2 metadata foundation**. Reusable authors and affiliations are the first metadata feature built on that foundation, followed by:

- [x] DOI and Crossref metadata enrichment
- [x] ORCID public-profile import / one-way sync
- BibTeX and RIS import/export
- Rich journal cards, target-journal shortlist, and submission portal shortcuts
- Preprint / journal-version linkage
- Calendar export, reminders, and Windows notifications
- CV/publication-list exports

**Safety rule:** external integrations may suggest metadata, but they must not silently overwrite user-entered metadata or change manuscript lifecycle state. Any import that changes lifecycle placement must be an explicit user choice.

## THEN — v0.3 The Route

- Visual Route View for every manuscript
- Version history linked to submissions and decisions
- Submission Packet Vault
- Per-journal readiness/checklists
- Reviewer Response Matrix
- Deadline Center
- Optional local/opt-in AI extraction of reviewer action items
- Route statistics: days, journals, reroutes, and time-to-publication

## TOWARD v1.0 — Trusted Research Workflow

- Stable versioned data model and tested migrations
- Signed installer and reliable updater
- Strong automated test coverage
- Recovery tooling and proven backup/restore
- Accessible keyboard-first UI
- Personal/local submission analytics
- File Drawer Revival / rerouting workflow
- Portable project-sharing format exploration

### The 1.0 bar

PaperRoute 1.0 is not defined by feature count.

> **I trust this application with my research workflow.**

- [x] DOI normalization and Crossref preview-before-apply metadata enrichment
- [x] ORCID public-profile lookup with selective import and explicit lifecycle placement
