namespace App.Application.DTO
{
    public record OperationCreateRequest(string OperationId, string Amount, string Currency, string? Description);
}