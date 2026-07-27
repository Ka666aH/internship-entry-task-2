using App.Domain;
using App.Presentation.DTO;
using System.Collections;
using System.Runtime.CompilerServices;

namespace App.Presentation.Mappers
{
    public static class OperationEventMapper
    {
        public static OperationEventResponse ToDTO(this OperationEvent operationEvent) =>
            new(
                operationEvent.EventId,
                operationEvent.ToStatus,
                operationEvent.FromStatus,
                operationEvent.ToStatus,
                operationEvent.Message,
                operationEvent.OccurredAt
                );
        public static IEnumerable<OperationEventResponse> ToDTO(this IEnumerable<OperationEvent> operationEvents) =>
            operationEvents.Select(ToDTO);
        
    }
}
