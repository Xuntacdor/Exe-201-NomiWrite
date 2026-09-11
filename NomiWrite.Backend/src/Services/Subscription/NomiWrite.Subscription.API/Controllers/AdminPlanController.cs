using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NomiWrite.Subscription.Application.DTOs;
using NomiWrite.Subscription.Application.Interfaces;

namespace NomiWrite.Subscription.API.Controllers;

[ApiController]
[Route("api/admin/plans")]
[Authorize(Roles = "Admin")]
public class AdminPlanController : ControllerBase
{
    private readonly IAdminPlanService _adminPlanService;

    public AdminPlanController(IAdminPlanService adminPlanService) => _adminPlanService = adminPlanService;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminPlanListItemDto>>> GetPlans()
    {
        var plans = await _adminPlanService.GetPlansAsync();
        return Ok(plans);
    }

    [HttpPost]
    public async Task<ActionResult<AdminPlanListItemDto>> CreatePlan([FromBody] CreatePlanRequestDto request)
    {
        var result = await _adminPlanService.CreatePlanAsync(request);
        return CreatedAtAction(nameof(GetPlanById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminPlanListItemDto>> GetPlanById(Guid id)
    {
        var plans = await _adminPlanService.GetPlansAsync();
        var plan = plans.FirstOrDefault(p => p.Id == id);
        return plan is null ? NotFound() : Ok(plan);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AdminPlanListItemDto>> UpdatePlan(Guid id, [FromBody] UpdatePlanRequestDto request)
    {
        var result = await _adminPlanService.UpdatePlanAsync(id, request);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<AdminPlanListItemDto>> UpdatePlanStatus(Guid id, [FromBody] UpdatePlanStatusRequestDto request)
    {
        var result = await _adminPlanService.UpdatePlanStatusAsync(id, request.IsActive);
        return Ok(result);
    }
}