Overview
REST API built with .NET to manage car auctions. Supports four vehicle types (Sedan, Hatchback, SUV, Truck) with full auction lifecycle management — create, start, bid, and close.

How to Run

dotnet run --project src/CarAuction.Api
dotnet test
API available at https://localhost:7051, Swagger at /swagger.

Project Structure

src/
├── CarAuction.Domain/          # Entities, interfaces, domain exceptions
├── CarAuction.Application/     # Services, DTOs, factory
├── CarAuction.Infrastructure/  # In-memory repositories
└── CarAuction.Api/             # Controllers, middleware

tests/
└── CarAuction.Tests/
    ├── Domain/                 # Unit tests — Auction, Vehicle, Bid
    ├── Application/            # Unit tests — Services with Moq
    └── Integration/            # Integration and concurrency tests


Design Decisions
•Clean Architecture — strict dependency rule, domain has no external dependencies. Business logic is framework-agnostic and easy to test in isolation.

•Vehicle hierarchy — each type is a separate class inheriting from Vehicle. They are distinct business concepts with their own attributes and validation.

•Factory pattern — VehicleFactory centralises vehicle creation. Adding a new type only requires a new class and a new factory case — services and controllers don't change.

•Auction states — Scheduled → Active → Closed. The Scheduled state isn't in the requirements but made sense to model the full lifecycle. State transitions are enforced inside the entity.

•Concurrency — critical sections are protected.

Exception handling — domain exceptions are raised by entities (business rule violations), application exceptions by services (orchestration failures). A GlobalExceptionHandler maps everything to HTTP responses, keeping controllers clean.


Requirement coverage:
• 4a: ID duplication prevented with locks on external IDs
• 4b: Vehicle existence + active auction validation before creating new auction
• 4c: Auction active + bid amount > current validated in PlaceBid
• 4d: Race conditions handled via SemaphoreSlim, idempotency in Start/Close

Assumptions
•Auction inherits StartingBid from the vehicle
•One active auction per vehicle at a time — a new one can be created if not exists active auction
•Search by manufacturer/model is case-insensitive partial match
•No ScheduledStartTime — starting an auction is an explicit action
•No authentication — BidderId is trusted from the request
•In-memory storage — repository interfaces allow swapping to a real database without touching domain or application layers