using EsgTrainingManagement.Api.DTOs;
using EsgTrainingManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EsgTrainingManagement.Api.Controllers;

[ApiController, Authorize, Route("api/employees")]
public class EmployeesController(EmployeeService service) : ControllerBase
{
    [HttpGet]
    public Task<PagedResponse<EmployeeResponse>> List([FromQuery] EmployeeQuery query, CancellationToken ct) =>
        service.ListAsync(query, ct);

    [HttpGet("{employeeId:guid}")]
    public Task<EmployeeResponse> Get(Guid employeeId, CancellationToken ct) => service.GetAsync(employeeId, ct);

    [HttpPost, Authorize(Roles = "Admin")]
    [ProducesResponseType<EmployeeResponse>(201)]
    public async Task<ActionResult<EmployeeResponse>> Create(CreateEmployeeRequest request, CancellationToken ct)
    {
        var employee = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { employeeId = employee.Id }, employee);
    }
}