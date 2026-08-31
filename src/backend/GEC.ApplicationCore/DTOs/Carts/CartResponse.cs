namespace GEC.ApplicationCore.DTOs.Carts;

public record CartResponse(
    Guid Id,
    IReadOnlyList<CartItemResponse> Items,
    decimal Total,
    string Currency);