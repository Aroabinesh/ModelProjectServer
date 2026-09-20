using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ModelProject.Entities.Enums;

namespace ModelProject.Data;

// One-time bulk data generator for load-testing pagination at scale. Run with:
//   dotnet run -- seed-bulk [count]
// (count defaults to 100000). Uses SqlBulkCopy instead of EF SaveChanges/AddRange - inserting
// hundreds of thousands of rows through change-tracked EF entities is minutes-slow and memory
// heavy; SqlBulkCopy streams rows straight to SQL Server's bulk-insert path in seconds.
public static class BulkDataSeeder
{
    private static readonly string[] AssetTypes =
        { "Compressor", "Conveyor", "HVAC", "Vehicle", "Pump", "Generator", "Boiler", "Chiller" };

    public static async Task RunAsync(ApplicationDbContext db, int count, CancellationToken cancellationToken = default)
    {
        if (count < 1)
            throw new ArgumentOutOfRangeException(nameof(count), "count must be at least 1.");

        await db.Database.MigrateAsync(cancellationToken);

        var facilityIds = await db.Facilities.AsNoTracking().Select(f => f.Id).ToListAsync(cancellationToken);
        if (facilityIds.Count == 0)
            throw new InvalidOperationException("No facilities found - seed base/reference data before running bulk seed.");

        var technicianIds = await db.Technicians.AsNoTracking().Select(t => t.Id).ToListAsync(cancellationToken);

        var connectionString = db.Database.GetConnectionString()
            ?? throw new InvalidOperationException("No connection string configured.");

        // Unique per run, so the AssetCode unique index never collides with the fixed demo
        // seed data (AC-1001, CV-1002, ...) or with a previous bulk-seed run.
        var runStamp = DateTime.UtcNow.Ticks % 1_000_000;

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await BulkInsertAssetsAsync(connection, facilityIds, runStamp, count, cancellationToken);

        var newAssets = await db.Assets.AsNoTracking()
            .Where(a => a.AssetCode.StartsWith($"BULK-{runStamp}-"))
            .OrderBy(a => a.AssetCode)
            .Select(a => new { a.Id, a.FacilityId })
            .ToListAsync(cancellationToken);

        await BulkInsertWorkOrdersAsync(
            connection,
            newAssets.Select(a => (a.Id, a.FacilityId)).ToList(),
            technicianIds,
            cancellationToken);
    }

    private static async Task BulkInsertAssetsAsync(
        SqlConnection connection, IReadOnlyList<int> facilityIds, long runStamp, int count, CancellationToken cancellationToken)
    {
        var table = new DataTable();
        table.Columns.Add("FacilityId", typeof(int));
        table.Columns.Add("AssetCode", typeof(string));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("AssetType", typeof(string));
        table.Columns.Add("Status", typeof(string));

        var statuses = Enum.GetValues<AssetStatus>();

        for (var i = 1; i <= count; i++)
        {
            table.Rows.Add(
                facilityIds[i % facilityIds.Count],
                $"BULK-{runStamp}-{i:D6}",
                $"Bulk Asset {i}",
                AssetTypes[i % AssetTypes.Length],
                statuses[i % statuses.Length].ToString());
        }

        using var bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = "Assets",
            BatchSize = 5000,
            BulkCopyTimeout = 0
        };
        bulkCopy.ColumnMappings.Add("FacilityId", "FacilityId");
        bulkCopy.ColumnMappings.Add("AssetCode", "AssetCode");
        bulkCopy.ColumnMappings.Add("Name", "Name");
        bulkCopy.ColumnMappings.Add("AssetType", "AssetType");
        bulkCopy.ColumnMappings.Add("Status", "Status");

        await bulkCopy.WriteToServerAsync(table, cancellationToken);
    }

    private static async Task BulkInsertWorkOrdersAsync(
        SqlConnection connection,
        IReadOnlyList<(int AssetId, int FacilityId)> assets,
        IReadOnlyList<int> technicianIds,
        CancellationToken cancellationToken)
    {
        var table = new DataTable();
        table.Columns.Add("AssetId", typeof(int));
        table.Columns.Add("FacilityId", typeof(int));
        table.Columns.Add("Title", typeof(string));
        table.Columns.Add("Description", typeof(string));
        table.Columns.Add("Priority", typeof(string));
        table.Columns.Add("Status", typeof(string));
        table.Columns.Add("AssignedTechnicianId", typeof(int));
        table.Columns.Add("ScheduledStartDate", typeof(DateTime));
        table.Columns.Add("ScheduledEndDate", typeof(DateTime));
        table.Columns.Add("CreatedAt", typeof(DateTime));
        table.Columns.Add("UpdatedAt", typeof(DateTime));

        var priorities = Enum.GetValues<WorkOrderPriority>();
        var statuses = Enum.GetValues<WorkOrderStatus>();
        // Spread CreatedAt over the last two years (one minute apart) so sorting/filtering by
        // date and by facility/status against a realistic, non-degenerate distribution.
        var baseDate = DateTime.UtcNow.AddYears(-2);

        for (var i = 0; i < assets.Count; i++)
        {
            var (assetId, facilityId) = assets[i];
            var status = statuses[i % statuses.Length];
            var createdAt = baseDate.AddMinutes(i);
            var hasTechnician = status != WorkOrderStatus.New && technicianIds.Count > 0;

            table.Rows.Add(
                assetId,
                facilityId,
                $"Bulk Work Order {i + 1}",
                $"Auto-generated work order #{i + 1} for load testing.",
                priorities[i % priorities.Length].ToString(),
                status.ToString(),
                hasTechnician ? technicianIds[i % technicianIds.Count] : DBNull.Value,
                hasTechnician ? createdAt.Date : DBNull.Value,
                hasTechnician ? createdAt.Date.AddDays(3) : DBNull.Value,
                createdAt,
                createdAt);
        }

        using var bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = "WorkOrders",
            BatchSize = 5000,
            BulkCopyTimeout = 0
        };
        bulkCopy.ColumnMappings.Add("AssetId", "AssetId");
        bulkCopy.ColumnMappings.Add("FacilityId", "FacilityId");
        bulkCopy.ColumnMappings.Add("Title", "Title");
        bulkCopy.ColumnMappings.Add("Description", "Description");
        bulkCopy.ColumnMappings.Add("Priority", "Priority");
        bulkCopy.ColumnMappings.Add("Status", "Status");
        bulkCopy.ColumnMappings.Add("AssignedTechnicianId", "AssignedTechnicianId");
        bulkCopy.ColumnMappings.Add("ScheduledStartDate", "ScheduledStartDate");
        bulkCopy.ColumnMappings.Add("ScheduledEndDate", "ScheduledEndDate");
        bulkCopy.ColumnMappings.Add("CreatedAt", "CreatedAt");
        bulkCopy.ColumnMappings.Add("UpdatedAt", "UpdatedAt");
        // RowVersion is a SQL Server ROWVERSION column - the engine stamps it automatically and
        // rejects any attempt to insert it explicitly, so it's deliberately left unmapped.

        await bulkCopy.WriteToServerAsync(table, cancellationToken);
    }
}
