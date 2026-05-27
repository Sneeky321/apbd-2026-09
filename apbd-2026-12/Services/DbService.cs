using apbd_2026_12.Data;
using apbd_2026_12.DTOs;
using apbd_2026_12.Exceptions;
using apbd_2026_12.Models;
using Microsoft.EntityFrameworkCore;

namespace apbd_2026_12.Services;

public class DbService : IDbService
{
    private readonly HospitalContext _context;
    
    public DbService(HospitalContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<GetPatientDto>> GetPatientsAsync(string? search)
    {
        var query = _context.Patients
            .Include(p => p.Admissions)
            .ThenInclude(a => a.Ward)
            .Include(p => p.BedAssignments)
            .ThenInclude(ba => ba.Bed)
            .ThenInclude(b => b.BedType)
            .Include(p => p.BedAssignments)
            .ThenInclude(ba => ba.Bed)
            .ThenInclude(b => b.Room)
            .ThenInclude(r => r.Ward)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                EF.Functions.Like(
                    p.FirstName,
                    $"%{search}%")
                ||
                EF.Functions.Like(
                    p.LastName,
                    $"%{search}%"));
        }

        return await query
            .Select(p => new GetPatientDto
            {
                Pesel = p.Pesel,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Age = p.Age,
                Sex = p.Sex,

                Admissions = p.Admissions
                    .Select(a => new GetAdmissionDto
                    {
                        Id = a.Id,
                        AdmissionDate = a.AdmissionDate,
                        DischargeDate = a.DischargeDate,

                        Ward = new GetWardDto
                        {
                            Id = a.Ward.Id,
                            Name = a.Ward.Name,
                            Description = a.Ward.Description
                        }
                    }).ToList(),

                BedAssignments = p.BedAssignments
                    .Select(ba => new GetBedAssignmentDto
                    {
                        Id = ba.Id,
                        From = ba.From,
                        To = ba.To,

                        Bed = new GetBedDto
                        {
                            Id = ba.Bed.Id,

                            BedType = new GetBedTypeDto
                            {
                                Id = ba.Bed.BedType.Id,
                                Name = ba.Bed.BedType.Name,
                                Description = ba.Bed.BedType.Description
                                
                            },
                            
                            Room = new GetRoomDto
                            {
                                Id= ba.Bed.Room.Id,
                                HasTv = ba.Bed.Room.HasTv,
                                
                                Ward = new GetWardDto
                                {
                                    Id = ba.Bed.Room.Ward.Id,
                                    Name = ba.Bed.Room.Ward.Name,
                                    Description = ba.Bed.Room.Ward.Description
                                }
                            }
                        }
                    }).ToList()
            }).ToListAsync();
    }

    public async Task AssignBedAsync(string pesel, PostCreateBedAssignmentDto dto)
    {
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Pesel == pesel);

        if (patient == null)
        {
            throw new NotFoundException();
        }

        if (dto.To != null && dto.To < dto.From)
        {
            throw new Exception("Invalid date range");
        }

        var bed =  await _context.Beds
            .Include(b => b.BedType)
            .Include(b => b.Room)
            .ThenInclude(r => r.Ward)
            .Include(b => b.BedAssignments)
            .FirstOrDefaultAsync(b =>
                b.BedType.Name == dto.BedType
                &&
                b.Room.Ward.Name == dto.Ward
                &&
                !b.BedAssignments.Any(ba =>
                    dto.From <
                    (ba.To ?? DateTime.MaxValue)
                    &&
                    (dto.To ?? DateTime.MaxValue)
                    > ba.From
                ));

        if (bed == null)
        {
            throw new NotFoundException();
        }

        var assignment = new BedAssignment
        {
            PatientPesel = pesel,
            BedId = bed.Id,
            From = dto.From,
            To = dto.To
        };
        
        _context.BedAssignments.Add(assignment);
        
        await _context.SaveChangesAsync();
    }
}