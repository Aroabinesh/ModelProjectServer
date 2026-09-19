using Microsoft.EntityFrameworkCore;
using ModelProject.Common.Exceptions;
using ModelProject.Data;
using ModelProject.Dtos.Lookups;
using ModelProject.Dtos.Technicians;
using ModelProject.Entities;

namespace ModelProject.Services.Technicians;

public class TechnicianService : ITechnicianService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<TechnicianService> _logger;

    public TechnicianService(ApplicationDbContext db, ILogger<TechnicianService> logger)
    {
        _db = db;
        _logger = logger;
    }

    private static readonly Func<Technician, TechnicianDto> ToDto = t => new TechnicianDto
    {
        Id = t.Id,
        Name = t.Name,
        Email = t.Email,
        IsActive = t.IsActive,
        StartDate = t.StartDate,
        EndDate = t.EndDate
    };

    public async Task<IReadOnlyList<TechnicianDto>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken)
    {
        var technicians = _db.Technicians.AsNoTracking().AsQueryable();

        if (activeOnly)
            technicians = technicians.Where(t => t.IsActive);

        return await technicians
            .OrderBy(t => t.Name)
            .Select(t => new TechnicianDto
            {
                Id = t.Id,
                Name = t.Name,
                Email = t.Email,
                IsActive = t.IsActive,
                StartDate = t.StartDate,
                EndDate = t.EndDate
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<TechnicianDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var technician = await _db.Technicians.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Technician {id} was not found.");

        return ToDto(technician);
    }

    public async Task<TechnicianDto> CreateAsync(CreateTechnicianDto dto, CancellationToken cancellationToken)
    {
        ValidateDateRange(dto.StartDate, dto.EndDate);

        var email = dto.Email.Trim();
        var emailInUse = await _db.Technicians.AsNoTracking().AnyAsync(t => t.Email == email, cancellationToken);
        if (emailInUse)
            throw new BusinessRuleException($"A technician with email '{email}' already exists.");

        var technician = new Technician
        {
            Name = dto.Name.Trim(),
            Email = email,
            IsActive = dto.IsActive,
            StartDate = dto.StartDate.Date,
            EndDate = dto.EndDate?.Date
        };

        _db.Technicians.Add(technician);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Technician {TechnicianId} created.", technician.Id);

        return ToDto(technician);
    }

    public async Task<TechnicianDto> UpdateAsync(int id, UpdateTechnicianDto dto, CancellationToken cancellationToken)
    {
        var technician = await _db.Technicians.FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Technician {id} was not found.");

        ValidateDateRange(dto.StartDate, dto.EndDate);

        var email = dto.Email.Trim();
        var emailInUse = await _db.Technicians.AsNoTracking()
            .AnyAsync(t => t.Id != id && t.Email == email, cancellationToken);
        if (emailInUse)
            throw new BusinessRuleException($"A technician with email '{email}' already exists.");

        technician.Name = dto.Name.Trim();
        technician.Email = email;
        technician.IsActive = dto.IsActive;
        technician.StartDate = dto.StartDate.Date;
        technician.EndDate = dto.EndDate?.Date;

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Technician {TechnicianId} updated.", id);

        return ToDto(technician);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var technician = await _db.Technicians.FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Technician {id} was not found.");

        var hasWorkOrders = await _db.WorkOrders.AsNoTracking().AnyAsync(w => w.AssignedTechnicianId == id, cancellationToken);
        if (hasWorkOrders)
            throw new BusinessRuleException("Cannot delete a technician with assigned work orders. Deactivate them instead.");

        _db.Technicians.Remove(technician);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Technician {TechnicianId} deleted.", id);
    }

    private static void ValidateDateRange(DateTime startDate, DateTime? endDate)
    {
        if (endDate.HasValue && endDate.Value.Date < startDate.Date)
            throw new BusinessRuleException("End date cannot be earlier than start date.");
    }
}
