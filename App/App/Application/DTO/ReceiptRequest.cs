namespace App.Application.DTO
{
    public record ReceiptRequest(Guid ProviderPaymentId, string OperationId, string Result, string Message, DateTime OccurredAt);
}