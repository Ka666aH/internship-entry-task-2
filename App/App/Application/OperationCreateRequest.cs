namespace App.Application
{
    public record OperationCreateRequest(string OperationId, string Amount, string Currency, string? Description);
}