namespace GEC.ApplicationCore.DTOs.Carts;

public record CartItemResponse(
    Guid Id,
    Guid VariantId,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal,
    string Currency,
    bool IsAvailable);