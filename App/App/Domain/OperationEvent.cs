using App.Domain.Enums;

namespace App.Domain
{
    public class OperationEvent
    {
        public int EventId { get; init; }
        public string OperationId { get; init; }
        public OperationStatus? FromStatus { get; init; }
        public OperationStatus ToStatus { get; init; }
        public string Message { get; init; }
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
        public OperationEvent(int eventId, string operationId, OperationStatus? fromStatus, OperationStatus toStatus, string message)
        {
            EventId = eventId;
            OperationId = operationId;
            FromStatus = fromStatus;
            ToStatus = toStatus;
            Message = message;
        }
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Рассмотрите возможность добавления модификатора "required" или объявления значения, допускающего значение NULL.
        private OperationEvent() { }
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Рассмотрите возможность добавления модификатора "required" или объявления значения, допускающего значение NULL.
    }
}
