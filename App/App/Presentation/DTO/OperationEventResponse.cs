using App.Domain.Enums;

namespace App.Presentation.DTO
{
    public record OperationEventResponse(int EventId, OperationStatus Type, OperationStatus? FromStatus, OperationStatus ToStatus, string Message, DateTime OccurredAt);
}
