namespace App.Application.Interfaces.Services
{
    public interface IReceiptService
    {
        Task<ReceiptResult> ProcessAsync(ReceiptRequest request, CancellationToken ct = default);
    }
}
