namespace App.Application
{
    public record OperationCreateRequest(string OperationId, decimal Amount, string Currency, string? Description);
}