using EsgTrainingManagement.Api.DTOs;
using EsgTrainingManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EsgTrainingManagement.Api.Controllers;

[ApiController, Authorize, Route("api/job-roles")]
public class TrainingRequirementsController(RequirementService service) : ControllerBase
{
    [HttpGet]
    public Task<PagedResponse<JobRoleResponse>> List([FromQuery] JobRoleQuery query, CancellationToken ct) => service.ListAsync(query, ct);

    [HttpPost("{jobRoleId:int}/trainings/{trainingId:int}"), Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Create(int jobRoleId, int trainingId, CreateRequirementRequest request, CancellationToken ct)
    {
        await service.CreateAsync(jobRoleId, trainingId, request, ct);
        return NoContent();
    }
}
