# Development Guide

## Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | 8.0+ |
| Docker | 24+ |
| Azure CLI | 2.55+ |
| Terraform | 1.5+ |
| VS Code (recommended) | latest |

The easiest way to get started is using the **dev container** which pre-installs all tools.

## Quick Start (Dev Container)

1. Open the repository in VS Code.
2. When prompted, click **Reopen in Container** (or run `Dev Containers: Reopen in Container` from the command palette).
3. All dependencies are installed automatically.

## Local Development (without dev container)

### 1. Register an Entra ID App

1. Go to [Azure Portal](https://portal.azure.com) → Azure Active Directory → App registrations → New registration.
2. Set redirect URI: `https://localhost:5001/signin-oidc`
3. Under **API permissions**, add: `Tasks.ReadWrite`, `User.Read` (Microsoft Graph, Delegated).
4. Create a client secret.

### 2. Configure the Application

Copy `web-apps/TodoTracker/appsettings.Development.json` and fill in:

```json
{
  "AzureAd": {
    "TenantId": "<your-tenant-id>",
    "ClientId": "<your-client-id>",
    "ClientSecret": "<your-client-secret>",
    "Domain": "<yourdomain.onmicrosoft.com>"
  }
}
```

> **Never commit `appsettings.Development.json` with real secrets.** It is in `.gitignore`.

### 3. Run Locally

```bash
cd web-apps/TodoTracker
dotnet run
```

Open `https://localhost:5001`.

### 4. Run Unit Tests

```bash
./scripts/run-tests.sh
```

Or via VS Code task: **Run Tests**.

## Task Naming Convention

```
#<Project> #<Activity> <Task description>
```

Example: `#Marketing #SocialMedia Draft Q3 campaign post`

Tasks without the project tag are not surfaced in the UI.

## Code Structure

```
web-apps/TodoTracker/
├── Controllers/
│   ├── HomeController.cs   ← main dashboard
│   └── TodoController.cs   ← REST API (create task)
├── Models/
│   ├── TodoItem.cs
│   ├── Project.cs
│   ├── Activity.cs
│   ├── ProjectViewModel.cs
│   └── CreateTaskRequest.cs
├── Services/
│   ├── ITodoService.cs
│   └── TodoService.cs      ← Graph API + tag parsing
└── Views/
    └── Home/Index.cshtml   ← dashboard UI
```

## Docker Build

```bash
./scripts/build-docker-images.sh
```

To push to a registry:

```bash
./scripts/build-docker-images.sh --push --registry <acr-login-server> --tag <git-sha>
```

## VS Code Tasks

Open the Command Palette → **Tasks: Run Task** and choose:

| Task | Script |
|------|--------|
| Run Tests | `scripts/run-tests.sh` |
| Build Docker Image | `scripts/build-docker-images.sh` |
| Create TF State Rig | `scripts/create-tf-state-rig.sh` |
| Deploy Terraform | `scripts/deploy-terraform.sh` |
