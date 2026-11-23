using EsgTrainingManagement.Api.DTOs;
using EsgTrainingManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EsgTrainingManagement.Api.Controllers;

[ApiController, Authorize, Route("api/training-records")]
public class TrainingRecordsController(TrainingRecordService service) : ControllerBase
{
    [HttpGet]
    public Task<PagedResponse<TrainingRecordResponse>>
        List([FromQuery] TrainingRecordQuery query, CancellationToken ct) => service.ListAsync(query, ct);

    [HttpGet("{recordId:int}")]
    public Task<TrainingRecordResponse> Get(int recordId, CancellationToken ct) => service.GetAsync(recordId, ct);

    [HttpPost, Authorize(Roles = "Admin")]
    [ProducesResponseType<TrainingRecordResponse>(201)]
    public async Task<ActionResult<TrainingRecordResponse>> Create(CreateTrainingRecordRequest request,
        CancellationToken ct)
    {
        var result = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { recordId = result.Id }, result);
    }
}