# Xprema Management - Procedure Control System

This project implements a comprehensive procedure control system with workflow management capabilities, inspired by the ABP Framework architecture.

## Features

- Complete workflow definition and execution
- Task tracking and management
- Multi-step procedure configuration
- Timeline tracking for audit purposes
- Status transition management
- Task assignment and participant tracking

## Architecture

The solution follows a layered architecture inspired by ABP Framework:

- **Domain Layer**: Contains entities, domain services, and business logic
- **Application Layer**: Implements application services and DTOs
- **EntityFrameworkCore Layer**: Handles data access and migrations
- **HTTP API Layer**: Exposes RESTful APIs

## API Endpoints

### Procedures API

- `GET /api/procedures`: Get all procedures
- `GET /api/procedures/{id}`: Get a procedure by ID
- `GET /api/procedures/{id}/details`: Get detailed procedure with steps
- `GET /api/procedures/paged`: Get paged list of procedures with filtering
- `POST /api/procedures`: Create a new procedure
- `PUT /api/procedures/{id}`: Update an existing procedure
- `DELETE /api/procedures/{id}`: Delete a procedure
- `PATCH /api/procedures/{id}/activation`: Activate or deactivate a procedure

### Tasks API

- `GET /api/tasks`: Get all tasks
- `GET /api/tasks/{id}`: Get a task by ID
- `GET /api/tasks/{id}/details`: Get detailed task with steps and timeline
- `GET /api/tasks/paged`: Get paged list of tasks with filtering
- `GET /api/tasks/by-procedure/{procedureId}`: Get tasks by procedure ID
- `GET /api/tasks/by-assignee/{assignedToId}`: Get tasks by assigned user ID
- `POST /api/tasks`: Create a new task
- `PUT /api/tasks/{id}`: Update an existing task
- `DELETE /api/tasks/{id}`: Delete a task
- `PATCH /api/tasks/{id}/status`: Update task status
- `PATCH /api/tasks/{id}/assign`: Assign task to a user
- `POST /api/tasks/{id}/steps`: Add a step to a task
- `POST /api/tasks/{id}/participants`: Add a participant to a task

### Workflow API

- `POST /api/workflow/tasks/{taskId}/advance`: Advance a task to the next step
- `POST /api/workflow/tasks/steps/{stepId}/complete`: Complete a task step

### Procedure Composes API

- `GET /api/procedure-composes`: Get all procedure composes
- `GET /api/procedure-composes/{id}`: Get a procedure compose by ID
- `GET /api/procedure-composes/{id}/details`: Get detailed procedure compose with steps
- `POST /api/procedure-composes`: Create a new procedure compose
- `PUT /api/procedure-composes/{id}`: Update an existing procedure compose
- `DELETE /api/procedure-composes/{id}`: Delete a procedure compose

### Actions API

- `GET /api/actions`: Get all actions
- `GET /api/actions/{id}`: Get an action by ID
- `POST /api/actions`: Create a new action
- `PUT /api/actions/{id}`: Update an existing action
- `DELETE /api/actions/{id}`: Delete an action

## Getting Started

1. Clone the repository
2. Make sure you have .NET 8.0 SDK installed
3. Configure the database connection string in `appsettings.json`
4. Run the application using `dotnet run`
5. Access the Swagger UI at `https://localhost:7228/swagger`

## Database Migrations

The application automatically applies database migrations when running in development mode. If you need to manually apply migrations, you can use:

```bash
dotnet ef database update
```

## Authentication and Authorization

Basic authentication and authorization are implemented. To extend this:

1. Implement user management
2. Configure JWT authentication
3. Implement role-based permissions

## License

This project is licensed under the MIT License. 