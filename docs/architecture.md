# Architecture Overview

## Overview

Todo Project Tracker is a containerised C# ASP.NET Core 8 web application that pulls tasks from Microsoft To-Do (via Microsoft Graph API) and organises them by project and activity using `#hashtag` naming conventions.

```
┌──────────────────────────────────────────────────────────┐
│                     Browser (User)                        │
└────────────────────────┬─────────────────────────────────┘
                         │ HTTPS
┌────────────────────────▼─────────────────────────────────┐
│           Azure App Service (Linux Container)             │
│                                                           │
│  ┌──────────────────────────────────────────────────┐    │
│  │         TodoTracker  (ASP.NET Core 8)            │    │
│  │                                                   │    │
│  │  ┌─────────────┐   ┌───────────────────────────┐ │    │
│  │  │HomeController│   │   TodoController (API)    │ │    │
│  │  └──────┬──────┘   └────────────┬──────────────┘ │    │
│  │         │                       │                 │    │
│  │  ┌──────▼───────────────────────▼──────────────┐ │    │
│  │  │              TodoService                    │ │    │
│  │  └──────────────────────┬──────────────────────┘ │    │
│  └─────────────────────────┼──────────────────────┘ │    │
│                            │                         │    │
│  ┌─────────────────────────▼──────────────────────┐ │    │
│  │         Microsoft Graph SDK (GraphServiceClient) │ │    │
│  └─────────────────────────┬──────────────────────┘ │    │
└────────────────────────────┼─────────────────────────────┘
                             │ HTTPS / OAuth 2.0
         ┌───────────────────▼─────────────────────┐
         │         Microsoft Graph API              │
         │      (Microsoft To-Do tasks)             │
         └─────────────────────────────────────────┘
```

## Task Tag Convention

Tasks in Microsoft To-Do use the following naming pattern:

```
#<Project> #<Activity> <Task description>
```

| Position | Tag | Meaning |
|----------|-----|---------|
| 1st `#` | `#ProjectName` | The project or initiative |
| 2nd `#` | `#ActivityName` | The activity within the project |
| Remainder | free text | The task description |

**Example:** `#WebRedesign #UXResearch Write user interview questions`

Tasks without tags are not surfaced in the project view.

## Components

### web-apps/

- **Program.cs** — Application startup, DI registration, OTEL setup  
- **Controllers/HomeController.cs** — Renders the project/activity dashboard  
- **Controllers/TodoController.cs** — REST API endpoint for creating tasks  
- **Services/TodoService.cs** — Graph API integration, tag parsing, project grouping  
- **Models/** — Domain models (TodoItem, Project, Activity, ProjectViewModel, CreateTaskRequest)  
- **Views/** — Razor views and layout  
- **Dockerfile** — Multi-stage build for containerisation  

### web-app-tests/

xUnit-based unit tests covering tag parsing, model construction, and grouping logic.

### infra/

Terraform templates targeting **Azure Government** (`usgovernment` provider environment):

| Resource | Type | Notes |
|----------|------|-------|
| Container Registry | `azurerm_container_registry` | System-managed identity |
| Storage Account | `azurerm_storage_account` | System-managed identity |
| App Service Plan | `azurerm_service_plan` | Linux |
| Web App | `azurerm_linux_web_app` | System-managed identity; pulls image from ACR |
| RBAC Assignment | `azurerm_role_assignment` | Web App → ACR (AcrPull) |
| RBAC Assignment | `azurerm_role_assignment` | Web App → Storage (Blob Data Contributor) |

## Authentication

Authentication uses Microsoft Entra ID (formerly Azure AD) with work/school accounts via OpenID Connect (`Microsoft.Identity.Web`). The app requests `Tasks.ReadWrite` and `User.Read` Graph scopes.

## Telemetry

OpenTelemetry is used for distributed tracing and metrics:
- `OpenTelemetry.Instrumentation.AspNetCore` — HTTP request spans  
- `OpenTelemetry.Instrumentation.Http` — Outbound HTTP calls (Graph API)  
- Console exporter (development); swap for OTLP/Azure Monitor exporter in production  
