using App.Application;
using App.Application.Interfaces.Services;
using App.Domain;
using Microsoft.AspNetCore.Mvc;

namespace App.Presentation.Controllers
{
    [ApiController]
    [Route("operations")]
    public class OperationController : ControllerBase
    {
        private readonly IOperationService _operationService;

        public OperationController(IOperationService operationService)
        {
            _operationService = operationService;
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OperationCreateRequest request, CancellationToken ct)
        {
            //validation
            if (request.Amount <= 0 || request.Amount % 0.01m != 0) return BadRequest("Amount must be positive with ≤ 2 decimal places");

            Operation? operation = await _operationService.CreateAsync(request, ct);
            if (operation == null) return Conflict();
            return Created($"/operations/{operation.OperationId}", operation);
        }
        [HttpPost("{id}/submit")]
        public async Task<IActionResult> Submit([FromRoute] string id, CancellationToken ct)
        {
            SubmitResult result = await _operationService.SubmitAsync(id, ct);
            return result switch
            {
                SubmitResult.Success => Accepted(),
                SubmitResult.Submitted => Ok(),
                _ => NotFound(),
            };
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute] string id, CancellationToken ct)
        {
            Operation? operation = await _operationService.GetAsync(id, ct);
            if (operation == null) return NotFound();
            return Ok(operation);
        }
        [HttpGet("{id}/events")]
        public Task<IActionResult> GetEvents([FromRoute] string id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
