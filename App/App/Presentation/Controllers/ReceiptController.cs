using App.Application;
using App.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace App.Presentation.Controllers
{
    [ApiController]
    [Route("receipts")]
    public class ReceiptController : ControllerBase
    {
        private readonly IReceiptService _receiptService;

        public ReceiptController(IReceiptService receiptService)
        {
            _receiptService = receiptService;
        }

        [HttpPost]
        public async Task<IActionResult> Receive([FromBody] ReceiptRequest request, CancellationToken ct)
        {
            ReceiptResult result = await _receiptService.ProcessAsync(request, ct);
            return result switch
            {
                ReceiptResult.Success => NoContent(),
                ReceiptResult.Conflict => Conflict(),
                ReceiptResult.Processed => NoContent(),
                _ => NotFound(),
            };
        }
    }
}
