using apbd_2026_12.DTOs;
using Microsoft.AspNetCore.Mvc;
using apbd_2026_12.Services;
using apbd_2026_12.Exceptions;

namespace apbd_2026_12.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IDbService _service;
    
    public PatientsController(IDbService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetPatients([FromQuery] string? search)
    {
        var result = await _service.GetPatientsAsync(search);
        
        return Ok(result);
    }

    [HttpPost("{pesel}/bedassignments")]
    public async Task<IActionResult> GetBedAssignments(
        [FromQuery] string pesel,
        [FromBody] PostCreateBedAssignmentDto dto)
    {
        try
        {
            await _service.AssignBedAsync(pesel, dto);

            return Created();
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}