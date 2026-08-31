namespace GEC.ApplicationCore.DTOs.Carts;

public record AddCartItemRequest(Guid VariantId, int Quantity);
