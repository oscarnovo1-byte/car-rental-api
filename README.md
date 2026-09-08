# Car Rental API

A RESTful ASP.NET Core Web API for an airport car rental system, developed as part of a .NET technical challenge.

The application allows customers to register, search for available vehicles, create rentals, modify existing rentals, and cancel reservations while ensuring that a car cannot be assigned to multiple customers during overlapping rental periods.

The project focuses on business rules, clean separation of responsibilities, automated testing, caching, and reproducible execution through Docker.

## Features

- Customer registration.
- Car availability search by rental period and vehicle characteristics.
- Rental creation.
- Rental retrieval and listing.
- Rental modification.
- Rental cancellation.
- Prevention of overlapping rentals.
- Consideration of scheduled car services when checking availability.
- In-memory availability caching.
- Cache invalidation after rental changes.
- Global exception handling.
- SQLite persistence.
- Docker and Docker Compose support.
- Unit and integration tests.

## Architecture

The solution follows a layered architecture inspired by Domain-Driven Design (DDD), with a CQRS-style separation between commands and queries.

```text
                 HTTP
                  |
        ASP.NET Core Controllers
                  |
        +---------+---------+
        |                   |
     Commands            Queries
        |                   |
        +---------+---------+
                  |
          Application Layer
      (Handlers & Abstractions)
                  |
             Domain Model
                  |
         Repository Interfaces
                  |
            Infrastructure
        (EF Core / Caching)
                  |
                SQLite
```

### Project Structure

```text
src/
├── CarRental.Api
├── CarRental.Application
├── CarRental.Domain
└── CarRental.Infrastructure

tests/
├── CarRental.UnitTests
└── CarRental.IntegrationTests
```

### Layers

- **CarRental.Api** — HTTP endpoints, application composition, and global exception handling.
- **CarRental.Application** — Commands, queries, handlers, and persistence/query abstractions.
- **CarRental.Domain** — Entities, enums, domain exceptions, and core business rules.
- **CarRental.Infrastructure** — Entity Framework Core persistence, repositories, query implementations, and caching.

## Design Patterns

The project intentionally applies design patterns to keep responsibilities separated.

| Pattern | Purpose |
|---|---|
| Repository | Abstracts persistence operations from the Application layer. |
| Decorator | Adds caching behavior to car availability queries without modifying the underlying query implementation. |
| Dependency Injection | Composes handlers, repositories, queries, caching, and infrastructure services. |

The availability cache is implemented as a Decorator around the availability query abstraction rather than adding caching responsibilities to controllers or persistence queries.

## Business Rules

The application enforces the main rental rules at the appropriate domain/application boundaries:

- A rental must reference an existing customer.
- A rental must reference an existing car.
- The rental end date must be later than the start date.
- A car cannot have multiple active rentals during overlapping periods.
- Cancelled rentals do not block future availability.
- Cars with scheduled service during the requested period are unavailable.
- Updating a rental excludes the rental being modified when checking for overlaps.

## Technology Stack

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- IMemoryCache
- xUnit
- Moq
- Docker
- Docker Compose

## API Endpoints

### Customers

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/customers` | Register a customer. |

### Cars

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/cars/availability` | Search for available cars for a rental period. |

### Rentals

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/rentals` | Create a rental. |
| `GET` | `/api/rentals` | List rentals. |
| `GET` | `/api/rentals/{id}` | Retrieve a rental by ID. |
| `PUT` | `/api/rentals/{id}` | Update an existing rental. |
| `PATCH` | `/api/rentals/{id}/cancel` | Cancel a rental. |

## Example Workflow

A typical workflow is:

1. Query available cars.
2. Register a customer.
3. Create a rental using the customer and car IDs.
4. Query availability again to verify that the rented car is no longer available for the same period.

### Check Car Availability

```bash
curl "http://localhost:8080/api/cars/availability?startDate=2026-10-01&endDate=2026-10-05"
```

### Register a Customer

```bash
curl -X POST "http://localhost:8080/api/customers" \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "John Doe",
    "address": "Main Street 123",
    "email": "john@example.com"
  }'
```

The response contains the generated customer ID.

### Create a Rental

Replace `<customer-id>` and `<car-id>` with IDs obtained from the previous requests.

```bash
curl -X POST "http://localhost:8080/api/rentals" \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "<customer-id>",
    "carId": "<car-id>",
    "startDate": "2026-10-01",
    "endDate": "2026-10-05"
  }'
```

## Running with Docker

Docker Compose is the simplest way to run the application with its persistent SQLite database.

### Prerequisites

- Docker Desktop
- Docker Compose

### Build and Start

From the repository root:

```bash
docker compose up --build
```

The API is exposed at:

```text
http://localhost:8080
```

### Stop the Application

```bash
docker compose down
```

The SQLite database remains persisted in the Docker named volume.

### Start from a Clean Database

To remove the persistent database and recreate the environment from scratch:

```bash
docker compose down -v
docker compose up --build
```

The application creates the SQLite database and seed data automatically during startup.

## Running Locally

### Prerequisites

- .NET 10 SDK

Restore dependencies:

```bash
dotnet restore
```

Build the solution:

```bash
dotnet build
```

Run the API:

```bash
dotnet run --project src/CarRental.Api
```

The local API URL is determined by the ASP.NET Core launch configuration.

## Database and Seed Data

The application uses SQLite through Entity Framework Core.

For this challenge, the database is automatically created when the application starts.

When the `Cars` table is empty, a small set of vehicles is seeded so that a clean installation can immediately execute availability and rental scenarios.

### Seed Cars

| Type | Model |
|---|---|
| SUV | Toyota RAV4 |
| Sedan | Toyota Corolla |
| Compact | Volkswagen Golf |

The seed operation is idempotent: existing cars are not duplicated when the application restarts.

When running through Docker Compose, the SQLite database is stored in a named Docker volume so data survives container recreation.

## Availability Caching

Car availability queries use `IMemoryCache`.

Caching is implemented using the Decorator pattern:

```text
Controller
    |
ICarAvailabilityQuery
    |
CachedCarAvailabilityQuery
    |
    | cache miss
    v
EfCarAvailabilityQuery
    |
  SQLite
```

This keeps caching concerns separate from both the API controller and the underlying EF Core query.

### Cache Invalidation

Availability information can become stale whenever a rental changes.

The cache is therefore invalidated after:

- rental creation;
- rental update;
- rental cancellation.

Versioned cache keys are used to invalidate existing availability results without coupling rental handlers to individual cached query keys.

## Error Handling

The API uses a global exception handler to translate application/domain failures into consistent HTTP responses.

Examples include:

- `400 Bad Request` for invalid input or domain rules.
- `404 Not Found` when a requested resource does not exist.
- `409 Conflict` when attempting to create or update an overlapping rental.
- `500 Internal Server Error` for unexpected failures.

This keeps exception-to-HTTP mapping outside controllers and avoids duplicating error-handling logic across endpoints.

## Testing

The solution contains both unit and integration tests.

Run the complete test suite from the repository root:

```bash
dotnet test
```

A Release build can also be validated with:

```bash
dotnet build -c Release
dotnet test -c Release --no-build
```

### Unit Tests

Unit tests cover areas such as:

- domain business rules;
- command handlers;
- query handlers;
- rental creation, update, and cancellation;
- customer registration;
- availability caching behavior;
- cache invalidation.

### Integration Tests

Integration tests exercise the application through its HTTP API and infrastructure, including:

- API endpoints;
- SQLite persistence;
- rental workflows;
- error responses;
- availability behavior;
- cache invalidation after rental changes.

Before delivery, the application was also manually validated from a clean Docker environment by:

1. removing the existing container and volume;
2. rebuilding the Docker image;
3. starting with a new SQLite database;
4. querying the seeded cars;
5. registering a customer;
6. creating a rental;
7. querying the same rental period again and verifying that the rented car was no longer available.

## Challenge Requirements and Scope

### Mandatory Technical Requirements

| Requirement | Status |
|---|---|
| `.editorconfig` | Implemented |
| Global exception handler | Implemented |
| Software architecture | Implemented |
| CQRS-style command/query separation | Implemented |
| Design pattern excluding DI | Repository and Decorator |
| Dependency Injection | Implemented |
| Descriptive atomic commits | Implemented |
| At least 10 commits | Implemented |
| Runnable application | Implemented and validated |

### Nice-to-Have Features

| Feature | Status |
|---|---|
| In-memory cache | Implemented |
| Cache invalidation | Implemented |
| Dockerized application | Implemented |
| Authentication | Not implemented |
| Authorization | Not implemented |
| Soft delete | Not implemented |

Authentication and authorization were considered but intentionally left outside the scope of the solution. They are optional challenge features and would introduce additional identity and security infrastructure unrelated to the core rental workflow.

Soft delete was also intentionally omitted. Rental cancellation is explicitly represented as a business operation and status in the domain model; deletion and cancellation represent different lifecycle concerns.

The implementation therefore prioritizes the core rental rules, automated testing, cache consistency, maintainability, and reproducible execution.

## AI-Assisted Development

AI-assisted development was used during this challenge as a collaborative engineering tool.

### Tool Used

- ChatGPT (OpenAI)

No custom agents, MCP servers, or project-specific AI automation were configured for the challenge.

### How AI Was Used

AI was used to:

- discuss architectural alternatives and trade-offs;
- review implementation approaches;
- identify missing scenarios and edge cases;
- assist with unit and integration test design;
- review caching and cache invalidation strategies;
- troubleshoot Docker and development issues;
- review project completeness against the challenge requirements;
- improve project documentation.

### Development Workflow

Development was performed incrementally.

The general AI-assisted workflow was:

1. Define a small implementation goal.
2. Discuss possible approaches and their trade-offs.
3. Select an approach consistent with the existing architecture and challenge scope.
4. Implement the change incrementally.
5. Review the resulting code and behavior.
6. Build the solution and run automated tests.
7. Manually validate relevant API or Docker scenarios.
8. Commit the working increment using a descriptive commit.

AI suggestions were treated as proposals rather than authoritative solutions.

### Examples of Technical Decisions

Some suggestions were accepted, others were adapted, and some were intentionally rejected to keep the solution focused.

Examples include:

- **Caching:** A Decorator around the availability query abstraction was selected instead of putting caching logic in controllers or directly inside the EF Core query.
- **Cache invalidation:** Rental creation, update, and cancellation invalidate availability results because each operation can change whether a car is available.
- **Customer registration:** A minimal customer registration use case was added because rentals require an existing customer and the challenge domain includes customer registration.
- **Seed data:** A small idempotent car seed was added so a clean environment can immediately exercise the availability and rental workflows.
- **Authentication and authorization:** These were discussed but intentionally not implemented because they were optional and would add significant infrastructure outside the core domain being evaluated.
- **Soft delete:** This was not added simply to satisfy an optional checkbox because rental cancellation is already explicitly represented in the domain.

### Review and Validation

AI-assisted code and design suggestions were reviewed against:

- the original challenge requirements;
- the existing project architecture;
- the rental business rules;
- maintainability and scope;
- automated test results.

Accepted changes were compiled and tested before being considered complete.

The final Docker environment was also validated manually from a clean database through an end-to-end rental scenario.

The candidate remains responsible for the final implementation and should be able to explain and justify all submitted code and technical decisions.

## Possible Future Improvements

If the application were evolved beyond the scope of the challenge, possible improvements would include:

- JWT authentication.
- Role- or policy-based authorization.
- Auditing and soft delete where deletion semantics are required.
- EF Core migrations for production database evolution.
- Health checks and additional observability.
- Automated CI pipeline.