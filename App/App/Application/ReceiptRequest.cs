namespace App.Application
{
    public record ReceiptRequest(Guid ProviderPaymentId, string OperationId, string Result, string Message, DateTime OccurredAt);
}