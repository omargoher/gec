namespace GEC.ApplicationCore.DTOs.Carts;

public record CartVariantInfo(
    Guid Id,
    decimal PriceAmount,
    string Currency,
    bool IsPurchasable);