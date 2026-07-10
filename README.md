# Order Fulfillment Engine

This project is a high-concurrency order processing engine designed using **.NET**, **Entity Framework Core**, and **Docker (SQL Server)**. The system architecture simulates a high-traffic gaming environment where multiple users attempt to purchase items simultaneously, requiring robust race-condition mitigation and priority-based queue management.

---

## Architecture and Key Components

*   **Optimistic Concurrency Control:** Prevention of race conditions and data corruption during bulk sales through the implementation of concurrency tokens via `RowVersion`.
*   **Priority Queue Management:** Integration of an in-memory asynchronous `PriorityQueue` structure to ensure high-priority orders (`Expedited` / `High`) bypass standard traffic and are processed first.
*   **Data Transfer Objects (DTOs):** API layer decoupling achieved by utilizing the DTO pattern with `[FromBody]` bindings, enforcing validation rules and shielding underlying persistence entities.
*   **Resource Resiliency:** Integration of `CancellationToken` instances across all API controller endpoints to stop execution and free server thread pool resources immediately if a client aborts the HTTP request.
*   **Structured Logging:** Comprehensive system tracing configured through **Serilog**, with persistence handled via file logging utilizing daily rotation intervals (`RollingInterval.Day`)[cite: 1].

---

## API Endpoints (Swagger)

### Orders Management
*   `POST /api/Orders` - Receives and validates an individual purchase request using a clean DTO format.
*   `POST /api/Orders/simulate-burst` - Stress-testing endpoint that generates a massive burst of concurrent orders to test the priority queue under heavy load.

### Fulfillment Processing
*   `POST /api/Fulfillment/process-all` - Dequeues and dispatches all pending orders concurrently by orchestrating asynchronous operations with `Task.WhenAll`.

### Inventory Administration
*   `POST /api/Inventory/create-item` - Provisions a new catalog item along with its corresponding initial stock record.
*   `PUT /api/Inventory/update-stock` - Administrative endpoint to adjust stock levels (`QuantityOnHand`) dynamically.
*   `GET /api/Inventory/verify-no-oversell` - Live audit endpoint that scans the database tables to verify that zero items have entered negative stock territory.

---

## Getting Started

1.  Ensure the SQL Server container is running in **Docker**.
2.  Restore dependencies and apply the latest database migrations:
    ```bash
    dotnet restore
    dotnet ef database update
    ```
3.  Launch the application:
    ```bash
    dotnet run --project LegendaryItemsEngine.Api
    ```
4.  Access the **Swagger UI** interface at: `http://localhost:5236/swagger`
