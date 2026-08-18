# ManuscriptPipeline

A local-first, privacy-focused academic manuscript pipeline tracker built with VB.NET and Windows Forms.

> **Status:** Very early development / pre-alpha.  
> The repository currently contains the initial application skeleton while core functionality is being developed.

## About

ManuscriptPipeline is an open-source Windows desktop application for researchers who want a simple way to track manuscripts throughout the academic publication process without storing their research workflow in a cloud service.

The application is intended to provide a visual overview of manuscripts as they progress through stages such as:

**Idea → Draft → Submitted → Under Review → Revision → Published**

Each manuscript will also maintain its own chronological "paper trail" containing submission information, editorial decisions, revision rounds, deadlines, notes, and other publication events.

## Core Principles

- **Local-first:** Manuscript tracking data belongs on the user's computer.
- **Privacy-focused:** No account or cloud service should be required for core functionality.
- **Offline-capable:** Core manuscript management should work without an internet connection.
- **Open source:** The project is developed publicly and released under the GNU GPL v3.
- **Researcher-focused:** Features should reflect the actual academic publication workflow rather than generic project management.
- **Portable data:** Users should be able to inspect, back up, import, and export their own information.

## Planned Features

### Manuscript Pipeline

- Visual publication-stage tracking
- Custom manuscript metadata
- Co-author information
- Target journal information
- Submission dates and manuscript IDs
- Time-in-stage tracking

### Review and Revision Tracking

- Under-review timers
- Editorial decisions
- Major and minor revision rounds
- Revision deadlines
- Reviewer and editor notes
- Response-letter tracking

### Paper Trail

Each manuscript will maintain a chronological history of important events, allowing researchers to see the complete publication journey from initial idea through publication.

### Local Files

Future versions are planned to support links to local manuscript folders and files, making it possible to open drafts, figures, reviewer comments, response letters, and related materials directly from the application.

### Import and Export

Planned formats include:

- JSON
- CSV
- BibTeX

Optional future integrations may include DOI/Crossref and ORCID metadata retrieval. Internet-connected functionality will remain separate from the application's local-first core.

## Development Roadmap

The initial development plan is:

1. Create the core manuscript data model.
2. Implement publication stages and stage transitions.
3. Build the manuscript pipeline interface.
4. Add manuscript history / paper-trail tracking.
5. Add persistent local storage.
6. Add revision deadlines and review timers.
7. Add local file and folder integration.
8. Add import/export functionality.
9. Add optional academic metadata integrations.
10. Package the application for easy Windows installation.

## Technology

ManuscriptPipeline is currently being developed using:

- Visual Basic .NET
- .NET 10
- Windows Forms
- Visual Studio 2026

The initial target platform is Windows.

## Inspiration

The project was inspired by the general concept of academic manuscript pipeline tools, including the workflow presented by [PaperTrek](https://papertrek.app/).

ManuscriptPipeline is an independent open-source project and is not affiliated with or endorsed by PaperTrek.

The goal is not to reproduce PaperTrek's proprietary implementation or visual design, but to explore a local-first, open-source approach to academic manuscript workflow management.

## Contributing

The project is currently in its earliest development stage, but contributions, ideas, bug reports, and feature suggestions will be welcome as the application matures.

## License

ManuscriptPipeline is licensed under the GNU General Public License v3.0.

See `LICENSE.txt` for details.