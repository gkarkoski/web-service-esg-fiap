using EsgTrainingManagement.Api.DTOs;
using EsgTrainingManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EsgTrainingManagement.Api.Controllers;

[ApiController, Authorize, Route("api/trainings")]
public class TrainingsController(TrainingService service) : ControllerBase
{
    [HttpGet]
    public Task<PagedResponse<TrainingResponse>> List([FromQuery] TrainingQuery query, CancellationToken ct) => service.ListAsync(query, ct);

    [HttpGet("{trainingId:int}")]
    public Task<TrainingResponse> Get(int trainingId, CancellationToken ct) => service.GetAsync(trainingId, ct);

    [HttpPost, Authorize(Roles = "Admin")]
    [ProducesResponseType<TrainingResponse>(201)]
    public async Task<ActionResult<TrainingResponse>> Create(CreateTrainingRequest request, CancellationToken ct)
    {
        var result = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { trainingId = result.Id }, result);
    }
}
