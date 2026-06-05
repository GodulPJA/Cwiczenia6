using Microsoft.AspNetCore.Mvc;
using Cwiczenia6.Services;
namespace Cwiczenia6.Controllers;
using DTOs;

[Route("api/[Controller]")]
[ApiController]
public class PatientsController : ControllerBase
{
    private readonly IDbService _dbService;

    public PatientsController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPatients([FromQuery] string? search)
    {
        var patients = await _dbService.GetPatientsAsync(search);
        return Ok(patients);
    }

    [HttpPost("{pesel}/bedassignments")]
    public async Task<IActionResult> AddBedAssignment(string pesel, [FromBody] CreateBedAssignmentDto request)
    {
        if (!await _dbService.DoesPatientExistAsync(pesel))
            return NotFound($"Patient with PESEL '{pesel}' not found.");

        if (!await _dbService.IsBedAvailableAsync(request.BedType, request.Ward, request.From, request.To))
            return NotFound("No available beds this type.");

        await _dbService.AssignBedAsync(pesel, request);

        return Created(string.Empty, new { message = "Bed assignment created successfully." });
    }
}