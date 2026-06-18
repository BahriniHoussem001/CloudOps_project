# CloudOps Service Platform

CloudOps Service Platform is a backend application designed to simulate a service request management platform with modular architecture, asynchronous messaging, database persistence, Docker-based infrastructure, and DevOps-ready foundations.

## Project Goal

The goal of this project is to practice and demonstrate DevOps, Cloud Engineering, backend architecture, containerization, messaging, and deployment workflows using a real-world inspired backend system.

## Current Architecture

The project is currently designed as a **Modular Monolith**.

This means the backend is one ASP.NET Core API application, but it is organized into separate business modules.

Current modules:

* Users
* Services
* Requests
* Notifications

Each module contains its own endpoints, DTOs, models, and business logic.

## Tech Stack

* ASP.NET Core 8 Web API
* Minimal APIs
* PostgreSQL
* Entity Framework Core
* RabbitMQ
* Docker
* Docker Compose
* Swagger / OpenAPI

## Infrastructure

The application runs with Docker Compose and includes:

* `cloudops-api`: ASP.NET Core backend API
* `cloudops-postgres`: PostgreSQL database
* `cloudops-rabbitmq`: RabbitMQ message broker with management UI

## Main Flow

When a user creates a service request:

1. The client sends a request to `POST /api/requests`
2. The API validates the user and service
3. The request is saved in PostgreSQL
4. The API publishes a `RequestCreatedEvent` to RabbitMQ
5. A RabbitMQ consumer listens to the queue
6. The consumer creates a notification automatically
7. The notification can be retrieved through the Notifications API

Flow:

```text
POST /api/requests
        ↓
Save request in PostgreSQL
        ↓
Publish RequestCreatedEvent to RabbitMQ
        ↓
Consumer receives the message
        ↓
Create notification in PostgreSQL
```

## RabbitMQ

RabbitMQ is used to decouple the Requests module from the Notifications module.

Instead of creating a notification directly inside the request endpoint, the system publishes an event:

```text
RequestCreatedEvent
```

Then the notification consumer reacts to this event.

This makes the system easier to extend later with other consumers such as:

* Email notifications
* SMS notifications
* Audit logs
* Admin alerts

## Idempotency

The notification consumer includes idempotency protection.

A `RelatedRequestId` field is stored in the `Notifications` table.

Before creating a notification, the consumer checks whether a notification already exists for the same request.

This prevents duplicate notifications if RabbitMQ redelivers the same message.

## API Endpoints

### Health

```http
GET /api/health
```

Checks:

* API status
* PostgreSQL connection
* RabbitMQ connection

### Users

```http
GET /api/users
GET /api/users/{id}
POST /api/users
PUT /api/users/{id}
DELETE /api/users/{id}
```

### Services

```http
GET /api/services
GET /api/services/{id}
POST /api/services
PUT /api/services/{id}
DELETE /api/services/{id}
```

### Requests

```http
GET /api/requests
GET /api/requests/{id}
POST /api/requests
PUT /api/requests/{id}
DELETE /api/requests/{id}
```

### Notifications

```http
GET /api/notifications
GET /api/notifications/{id}
POST /api/notifications
PUT /api/notifications/{id}
DELETE /api/notifications/{id}
PUT /api/notifications/{id}/mark-as-read
GET /api/users/{userId}/notifications
GET /api/users/{userId}/notifications/unread-count
PUT /api/users/{userId}/notifications/mark-all-as-read
```

## Docker Compose Services

### PostgreSQL

PostgreSQL stores the application data:

* Users
* Services
* ServiceRequests
* Notifications

It uses a Docker volume for persistence.

### RabbitMQ

RabbitMQ handles asynchronous messaging.

Management UI:

```text
http://localhost:15672
```

Default credentials:

```text
Username: cloudops
Password: cloudops
```

### API

Swagger UI:

```text
http://localhost:8080/swagger
```

Health check:

```text
http://localhost:8080/api/health
```

## Run the Project

From the project root:

```powershell
docker compose up -d --build
```

Check running containers:

```powershell
docker compose ps
```

View API logs:

```powershell
docker compose logs api
```

Stop containers:

```powershell
docker compose down
```

Stop containers and remove volumes:

```powershell
docker compose down -v
```

## Database Migrations

The API applies EF Core migrations automatically at startup.

Manual migration commands can also be executed from the API project:

```powershell
Add-Migration MigrationName
Update-Database
```

## Logging

The API includes application logs for important actions:

* Service request created
* RabbitMQ message published
* RabbitMQ consumer started
* Message consumed
* Notification created
* Consumer processing errors

These logs can be checked using:

```powershell
docker compose logs api
```

## Error Handling

The API includes a global exception handling middleware.

Unexpected errors are returned as clean JSON responses instead of raw technical errors.

Example:

```json
{
  "success": false,
  "message": "An unexpected error occurred"
}
```

## Current Status

Completed:

* Modular backend structure
* CRUD endpoints for core modules
* PostgreSQL integration
* EF Core migrations
* RabbitMQ publisher
* RabbitMQ consumer
* Automatic notification creation
* Idempotency protection
* User notification endpoints
* Health check for API, database, and RabbitMQ
* Global exception middleware
* Application logging
* Docker Compose setup

Next steps:

* Add frontend application
* Dockerize the frontend
* Add frontend service to Docker Compose
* Add tests
* Add CI/CD pipeline
* Add cloud deployment
* Add monitoring and security scanning
