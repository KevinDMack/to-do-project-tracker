# Todo Project Tracker

A containerised **C# ASP.NET Core 8** web application that pulls tasks from **Microsoft To-Do** (via Microsoft Graph API) and organises them by project and activity using `#hashtag` naming conventions in task titles.

## Features

- 🔐 **Entra ID authentication** — works with work/school accounts
- 📋 **Project sidebar** — lists all projects derived from the first `#tag` in task titles
- 🏷️ **Activity panels** — groups tasks by the second `#tag` within each project
- ➕ **Add task modal** — creates a new To-Do item pre-populated with `#Project #Activity` tags
- 🐳 **Docker containerised** — builds and runs in a multi-stage Docker container
- 📡 **OpenTelemetry** — built-in distributed tracing and metrics

## Task Naming Convention

Tasks in Microsoft To-Do use the following pattern:

```
#<Project> #<Activity> <Task description>
```

| Position | Tag | Meaning |
|----------|-----|---------|
| 1st `#` | `#ProjectName` | Project or initiative |
| 2nd `#` | `#ActivityName` | Activity within the project |
| Remainder | free text | Task description |

**Example:** `#WebRedesign #UXResearch Write user interview questions`

## Quick Start

See [docs/development.md](docs/development.md) for full setup instructions.

### Dev Container (recommended)

1. Open the repository in VS Code.
2. Click **Reopen in Container** when prompted.
3. All tools (.NET 8, Docker, Azure CLI, Terraform) are pre-installed.

### Local Development

```bash
# Configure Entra ID in appsettings.Development.json (see docs/development.md)
cd web-apps/TodoTracker
dotnet run
```

## Project Structure

```
.
├── web-apps/                      # C# ASP.NET Core web application
│   ├── TodoTracker/               # Application source
│   │   ├── Controllers/           # MVC controllers + API endpoint
│   │   ├── Models/                # Domain models
│   │   ├── Services/              # Graph API integration
│   │   └── Views/                 # Razor views
│   └── Dockerfile                 # Multi-stage Docker build
├── web-app-tests/                 # xUnit unit tests
│   └── TodoTracker.Tests/
├── infra/                         # Terraform (Azure Government)
│   ├── main.tf                    # Resources: ACR, storage, App Service
│   ├── variables.tf
│   ├── outputs.tf
│   └── providers.tf
├── scripts/                       # Utility shell scripts
│   ├── run-tests.sh
│   ├── build-docker-images.sh
│   ├── create-tf-state-rig.sh
│   └── deploy-terraform.sh
├── docs/                          # Documentation
│   ├── architecture.md
│   ├── development.md
│   └── deployment.md
├── .devcontainer/                 # Dev container configuration
├── .vscode/                       # VS Code tasks
└── .github/
    ├── agents/                    # Agent role definitions
    ├── workflows/                 # GitHub Actions CI/CD
    └── ISSUE_TEMPLATE/            # Issue templates
```

## Scripts

| Script | Description |
|--------|-------------|
| `scripts/run-tests.sh` | Run all unit tests |
| `scripts/build-docker-images.sh` | Build (and optionally push) the Docker image |
| `scripts/create-tf-state-rig.sh` | Create Azure storage for Terraform state |
| `scripts/deploy-terraform.sh` | Initialise and apply Terraform |

All scripts are also available as **VS Code tasks** (Ctrl+Shift+P → Tasks: Run Task).

## Infrastructure (Azure Government)

Terraform provisions:
- **Azure Container Registry** (ACR) — with system-managed identity
- **Azure Storage Account** — with system-managed identity
- **Azure App Service** (Linux container) — with system-managed identity, AcrPull access to ACR, and Storage Blob Data Contributor access to the storage account

See [docs/deployment.md](docs/deployment.md) for deployment instructions.

## GitHub Workflows

| Workflow | Trigger | Actions |
|----------|---------|---------|
| `pr-tests.yml` | Pull request to `main` | Run all unit tests |
| `deploy.yml` | Merge to `main` | Run tests → Build Docker image → Push to GitHub Packages |

## Agent Roles

This project uses a multi-agent model. Each agent has a defined role and responsibility:

| Agent | Responsibility |
|-------|---------------|
| [Developer](.github/agents/developer.md) | Application code in `web-apps/` |
| [Doc-Writer](.github/agents/doc-writer.md) | Documentation in `README.md` and `docs/` |
| [Telemetry-Engineer](.github/agents/telemetry-engineer.md) | OpenTelemetry instrumentation |
| [Security-Engineer](.github/agents/security-engineer.md) | Security review of all changes |
| [Automation-Engineer](.github/agents/automation-engineer.md) | Scripts, VS Code tasks, devcontainer, CI/CD |
| [Test-Engineer](.github/agents/test-engineer.md) | Unit tests in `web-app-tests/` |
| [Data-Engineer](.github/agents/data-engineer.md) | Sample and test data |
| [Dev-Manager](.github/agents/dev-manager.md) | Quality review of all agent contributions |

## Contributing

- Use the [Feature Request](.github/ISSUE_TEMPLATE/feature_request.md) or [Bug Report](.github/ISSUE_TEMPLATE/bug_report.md) templates when opening issues.
- All pull requests must pass CI tests before merging.
- Code owner: @KevinDMack (see [CODEOWNERS](CODEOWNERS))

## Documentation

- [Architecture](docs/architecture.md)
- [Development Guide](docs/development.md)
- [Deployment Guide](docs/deployment.md)
