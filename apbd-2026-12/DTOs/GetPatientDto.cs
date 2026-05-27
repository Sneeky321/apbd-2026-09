namespace apbd_2026_12.DTOs;

public class GetPatientDto
{
    public string Pesel { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Age { get; set; }
    public bool Sex { get; set; }

    public List<GetAdmissionDto> Admissions { get; set; } = 
        new List<GetAdmissionDto>();
    
    public List<GetBedAssignmentDto> BedAssignments { get; set; } = 
        new List<GetBedAssignmentDto>();
}