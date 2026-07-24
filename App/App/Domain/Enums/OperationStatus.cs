using System.Text.Json;
using System.Text.Json.Serialization;

namespace App.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OperationStatus
    {
        Created,
        Processing,
        Completed,
        Rejected
    }
}
