using EsgTrainingManagement.Api.DTOs;
using EsgTrainingManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EsgTrainingManagement.Api.Controllers;

[ApiController, Authorize, Route("api/compliance")]
public class ComplianceController(ComplianceService service) : ControllerBase
{
    [HttpGet("employees/{employeeId:guid}")]
    public Task<EmployeeComplianceResponse> Get(Guid employeeId, CancellationToken ct) =>
        service.GetAsync(employeeId, ct);

    [HttpGet("issues")]
    public Task<PagedResponse<ComplianceRow>> ListIssues([FromQuery] ComplianceQuery query, CancellationToken ct) =>
        service.ListIssuesAsync(query, ct);

    [HttpGet("esg-indicators")]
    public Task<IReadOnlyList<EsgIndicatorResponse>> Indicators(CancellationToken ct) => service.IndicatorsAsync(ct);
}