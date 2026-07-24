using App.Domain.Enums;

namespace App.Domain
{
    public class Operation
    {
        public string OperationId { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; }
        public string? Description { get; init; }
        public OperationStatus Status { get; private set; } = OperationStatus.Created;
        public Guid? ProviderPaymentId { get; private set; } = null;
        public Operation(string operationId ,decimal amount, string currency, string? description)
        {
            OperationId = operationId;
            Amount = amount;
            Currency = currency;
            Description = description;
        }
        public void Submit() => Status = OperationStatus.Processing;
        public void Complete() => Status = OperationStatus.Completed;
        public void Reject() => Status = OperationStatus.Rejected;
        public void SetProviderPaymentId(Guid providerPaymentId)
        {
            if (ProviderPaymentId is not null) 
                throw new InvalidOperationException("ProviderPaymentId уже записан.");
            ProviderPaymentId = providerPaymentId;
        }

    }
}