# Work Order Management System — Backend

ASP.NET Core 8 Web API + EF Core (code-first) + SQL Server implementation of the Work Order
Management interview task (`ModelProject/Full_Stack_Work_Order_Management_Interview_Task.pdf`).

> This repository currently contains the **backend only**. No React frontend exists in this
> workspace at the time of writing.

## Solution layout

```
ModelProject.sln
├─ ModelProject/              ASP.NET Core 8 Web API
│  ├─ Controllers/            Thin controllers (WorkOrders, Customers, Facilities, Assets, Technicians)
│  ├─ Services/                Business logic (WorkOrderService, LookupService)
│  ├─ Dtos/                   API contracts, separate from persistence entities
│  ├─ Entities/                EF Core (code-first) domain model
│  ├─ Data/                    ApplicationDbContext, Fluent API configurations, seed data, migrations
│  └─ Common/                  Cross-cutting: custom exceptions, global exception middleware
└─ ModelProject.Tests/         xUnit tests (status workflow + WorkOrderService business rules)
```

## Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB, a full instance, or SQL Server in Docker)
- `dotnet-ef` global tool (`dotnet tool install --global dotnet-ef`) if you want to add/apply
  migrations yourself — not required just to run the app, since `dotnet run` does not auto-migrate.

## Setup

1. **Connection string** — `ModelProject/appsettings.json` defaults to LocalDB:

   ```
   Server=(localdb)\MSSQLLocalDB;Database=WorkOrderManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
   ```

   Point `ConnectionStrings:DefaultConnection` at a different SQL Server instance if you're not
   using LocalDB (edit `appsettings.json`, or override via `appsettings.Development.json` /
   the `ConnectionStrings__DefaultConnection` environment variable).

2. **Create/update the database:**

   ```bash
   cd ModelProject
   dotnet ef database update
   ```

   This creates the `WorkOrderManagementDb` database, all tables, indexes, and foreign keys, and
   inserts a small seed dataset (2 customers, 3 facilities, 4 assets, 3 technicians, 3 work
   orders with history) via the `InitialCreate` migration under `Data/Migrations/`.

3. **Run the API:**

   ```bash
   dotnet watch run --project ModelProject
   ```

   Swagger UI is available at `/swagger` in the Development environment and documents every
   endpoint, request/response shape, and status code. `dotnet watch run` also opens it in your
   browser automatically and rebuilds on file changes; use plain `dotnet run` if you don't want
   either behavior.

4. **Run the tests:**

   ```bash
   dotnet test
   ```

## API surface

| Method | Endpoint                          | Purpose                                            |
|--------|------------------------------------|-----------------------------------------------------|
| POST   | `/api/workorders`                  | Create a work order                                 |
| GET    | `/api/workorders`                  | List with search / filter / sort / pagination       |
| GET    | `/api/workorders/{id}`             | Get work order details                              |
| PUT    | `/api/workorders/{id}`             | Update title/description/priority                   |
| DELETE | `/api/workorders/{id}`             | Delete a work order                                  |
| PUT    | `/api/workorders/{id}/assign`      | Assign/reassign a technician                         |
| PUT    | `/api/workorders/{id}/status`      | Change status (workflow-enforced)                    |
| GET    | `/api/workorders/{id}/history`     | Status-change history                                |
| GET    | `/api/customers`                   | Lookup data for dropdowns                            |
| GET    | `/api/facilities?customerId=`      | Lookup data (facility filter, create-form picker)    |
| GET    | `/api/assets?facilityId=`          | Lookup data (create-form asset picker)               |
| GET    | `/api/technicians?activeOnly=`     | Lookup data (technician filter, assign action)       |

`GET /api/workorders` accepts `search`, `status`, `priority`, `facilityId`, `technicianId`,
`sortBy` (`title`|`priority`|`status`|`updatedAt`|`createdAt`, default `createdAt`),
`sortDescending` (default `true`), `page` (default 1), `pageSize` (default 20, capped at 100).

## Data model / assumptions

The task's data model didn't define a `Technician` entity even though `WorkOrder` requires
`AssignedTechnicianId`. A minimal `Technicians` table (`Id`, `Name`, `Email`, `IsActive`) was
added so the relationship is a real, enforceable, queryable foreign key instead of a bare int.

`WorkOrder` also carries a **denormalized `FacilityId`** (copied from `Asset.FacilityId` at
creation time, never accepted from the client — see "Facility-level access" below). The task's
own SQL Server challenge (`WHERE FacilityId = @FacilityId AND Status = @Status ORDER BY
CreatedAt DESC` against the `WorkOrders` table) assumes this column exists directly on
`WorkOrders`; without it, every list/filter request would need to join through `Assets`, which
defeats the point of the covering index described below.

## Business rules and where they live

- **Status workflow** (`New → Assigned → InProgress → Completed`, no skipping, no going
  backwards) is defined once, in `Services/WorkOrders/WorkOrderStatusWorkflow.cs`, and enforced
  in `WorkOrderService.ChangeStatusAsync`. Controllers never see or duplicate this rule.
- **Assigning a technician to a `New` work order** automatically transitions it to `Assigned`
  and writes a `WorkOrderHistory` row (this is what "New → Assigned" means functionally).
  Reassigning a technician later does not change status. Moving a work order to `Assigned` via
  the `/status` endpoint without an assigned technician is rejected (`BusinessRuleException` →
  400).
- **DTO-level validation** (`[Required]`, `[StringLength]`, enum binding) catches malformed
  input before it reaches business logic — handled automatically by `[ApiController]`, which
  returns a `400 ValidationProblemDetails` response.
- **Business-rule validation** (status transitions, technician-required-for-Assigned,
  facility/asset/technician existence) lives in `WorkOrderService`, not the controller, and not
  in EF Core configuration — DTO validation only knows about shape, EF configuration only knows
  about storage constraints, and only the service has the full picture (current entity state +
  the rule).

## Concurrency

`WorkOrder.RowVersion` is a SQL Server `ROWVERSION` column, mapped in EF Core via
`.IsRowVersion()`. Every mutating endpoint (`PUT`, `/assign`, `/status`) requires the caller to
echo back the `rowVersion` (base64) they last read. The service sets that value as the EF
`OriginalValue` for the tracked entity (`WorkOrderService.ApplyRowVersion`) before calling
`SaveChangesAsync`, so the generated `UPDATE` statement includes
`WHERE Id = @id AND RowVersion = @clientRowVersion`. If another user already updated the row,
zero rows match, EF throws `DbUpdateConcurrencyException`, and the service translates that into
a `409 Conflict` (`ConcurrencyConflictException`) with a message telling the caller to reload and
retry — never a silent overwrite.

This was verified two ways:
- `ModelProject.Tests/WorkOrderServiceTests.cs` (`UpdateAsync_WithStaleRowVersion_...`) exercises
  the same `ApplyRowVersion` + `SaveChangesAsync` code path used in production.
- Manually against a real SQL Server LocalDB instance: assign a technician (rowversion advances),
  then replay the same PUT with the original rowversion → confirmed `409 Conflict`.

`WorkOrderHistories.WorkOrderId` cascade-deletes at the SQL Server foreign-key level (`ON DELETE
CASCADE`), confirmed via `sys.foreign_keys`. This isn't unit-tested against the EF Core InMemory
provider because InMemory only performs client-side cascade for navigations already loaded into
the context, which doesn't reflect how the real database enforces it.

## Performance

- **Server-side pagination everywhere** — `GET /api/workorders` never loads more than
  `pageSize` (max 100) rows; the browser never sees the full table, satisfying the "up to 5
  million rows" requirement.
- **Projection, not `Include`** — every read path (`GetListAsync`, `GetByIdAsync`,
  `GetHistoryAsync`, `LookupService`) uses `.Select(...)` directly into a DTO. EF Core translates
  this into one SQL query with the necessary `JOIN`s, so navigation properties like
  `Facility.Name` or `Asset.AssetCode` are fetched without ever materializing full `Facility`/
  `Asset` entities or triggering lazy-load round-trips (the classic N+1 risk with `Include` +
  per-row property access). `Include` would only be justified if the code needed to *mutate* the
  related entity graph, which none of these read paths do.
- **Whitelisted sorting** — `sortBy` is matched against a fixed set of columns rather than
  built as a dynamic `OrderBy(string)`, so every accepted sort is backed by an index and no SQL
  injection surface is introduced.
- **Composite index for the SQL Server challenge query:**

  ```sql
  SELECT * FROM WorkOrders
  WHERE FacilityId = @FacilityId AND Status = @Status
  ORDER BY CreatedAt DESC;
  ```

  `WorkOrderConfiguration` defines
  `IX_WorkOrders_FacilityId_Status_CreatedAt (FacilityId, Status, CreatedAt DESC)`. `FacilityId`
  and `Status` as leading equality columns let SQL Server seek directly to the matching rows
  instead of scanning the table; `CreatedAt DESC` as the third key column means the index is
  already in the exact order the query asks for, so SQL Server can satisfy `ORDER BY CreatedAt
  DESC` by reading the index in order instead of adding a `Sort` operator. Confirmed applied via:

  ```sql
  SELECT i.name, c.name, ic.key_ordinal, ic.is_descending_key
  FROM sys.indexes i
  JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
  JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
  WHERE i.name = 'IX_WorkOrders_FacilityId_Status_CreatedAt'
  ORDER BY ic.key_ordinal;
  ```

  To validate the improvement in a real deployment: capture the actual execution plan
  (`SET STATISTICS IO, TIME ON` + `SSMS "Include Actual Execution Plan"`, or
  `sys.dm_exec_query_stats`) before and after the index exists, and confirm the plan changes from
  a `Clustered Index Scan` (or Key Lookup–heavy plan) to an `Index Seek` on
  `IX_WorkOrders_FacilityId_Status_CreatedAt` with a `Sort` operator no longer present, plus a
  drop in logical reads.

- **How to tell React vs. API vs. network vs. SQL Server is the bottleneck**, in order of what
  to check first:
  1. Browser DevTools → Network tab: total request time vs. time-to-first-byte. If TTFB is small
     and the transfer/parse time is large, the payload is too big (fix: smaller DTOs, more
     pagination) — a React problem, not a server one.
  2. If TTFB is large, check the API's own logs (Serilog request logging is enabled — every
     request logs its elapsed time) and compare it to the time spent inside the EF Core call
     specifically (`Microsoft.EntityFrameworkCore.Database.Command` is logged at `Information`
     in Development and includes each SQL statement + duration).
  3. If the SQL statement itself is slow, run it directly against SQL Server with
     `SET STATISTICS TIME, IO ON` and inspect the execution plan — that isolates "slow query
     plan / missing index" from "slow network" or "slow API code around the query".
  4. If the SQL statement is fast but the overall request is slow, the API layer itself (e.g.
     synchronous blocking, N+1 queries, oversized serialization) is the culprit — verified above
     via structured logs rather than guessing.
  - On the frontend, React-specific rendering cost (not data-fetch cost) shows up as long
    "Scripting" time in the Performance tab with normal network timing — that's the signal to
    virtualize the table (e.g. `react-window`) or reduce re-renders (memoized rows,
    `useMemo`/`useCallback` at the table-row level) rather than looking at the API at all.

## Optimistic concurrency, facility security, and known limitations

- **Facility-level access if a client tampers with `facilityId`:** `FacilityId` is never
  accepted as client input — `CreateAsync` derives it server-side from the `Asset` being
  referenced, and `UpdateAsync`/`AssignTechnicianAsync`/`ChangeStatusAsync` never touch it at
  all. A malicious payload with a spoofed `facilityId` simply has no effect. The one remaining
  gap (out of scope here, listed as an "optional advanced extension" in the task) is
  *authorization*: there's currently no concept of "which facilities can this caller see" —
  adding that would mean attaching the caller's authorized facility IDs to their identity (JWT
  claim or a `UserFacilities` table) and filtering every query in `WorkOrderService` and
  `LookupService` by it, in addition to keeping the existing "never trust client-supplied
  FacilityId" rule.
- **What would change before production:** add authentication/authorization (see above), rate
  limiting, a caching layer for the lookup endpoints (customers/facilities/assets/technicians
  change rarely and are read on every page load), response compression, health check endpoints,
  and CI-driven database migrations instead of an interactively-run `dotnet ef database update`.
- **N+1 risk:** the only place it could realistically appear is if a future feature iterated a
  list of `WorkOrder` entities and accessed `.Asset.Name` (or similar) per item without a
  projection — e.g. inside a `foreach` after `ToListAsync()` instead of inside `.Select(...)`.
  Every current read path avoids this by projecting directly in the LINQ query (see
  "Performance" above), which is also why `Include` is not used anywhere in this codebase.
