using apbd_2026_12.DTOs;

namespace apbd_2026_12.Services;

public interface IDbService
{
    Task<IEnumerable<GetPatientDto>> GetPatientsAsync(string? search);

    Task AssignBedAsync(string pesel, PostCreateBedAssignmentDto dto);
}