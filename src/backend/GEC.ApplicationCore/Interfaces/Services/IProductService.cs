using GEC.ApplicationCore.DTOs;
using GEC.ApplicationCore.DTOs.Products;

namespace GEC.ApplicationCore.Interfaces.Services;

/// <summary>
/// Application service for the Product aggregate and its variants.
/// Covers all use cases defined in Catalog PDR v3.1 §3.
/// </summary>
public interface IProductService
{
    // --- Product Management (§3.1) ---

    /// <summary>Creates a new product in Draft status.</summary>
    Task<ProductResponse> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default);

    /// <summary>Enables a global attribute for a product and marks it required or optional.</summary>
    Task<ProductAttributeResponse> AddProductAttributeAsync(Guid productId, AddProductAttributeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a product attribute. Fails if any variant of the product still uses that attribute.
    /// </summary>
    Task RemoveProductAttributeAsync(Guid productId, Guid attributeId, CancellationToken cancellationToken = default);

    /// <summary>Appends a descriptive specification to a product (add-only, PDR §3.1.4).</summary>
    Task<ProductSpecificationResponse> AddProductSpecificationAsync(Guid productId, AddProductSpecificationRequest request, CancellationToken cancellationToken = default);

    // --- Variant Management (§3.2) ---

    /// <summary>Adds a new variant (SKU) to the product in Draft status with no attribute values assigned.</summary>
    Task<VariantResponse> AddVariantAsync(Guid productId, CreateVariantRequest request, CancellationToken cancellationToken = default);

    /// <summary>Removes a variant. Prefer status changes over hard deletion for normal business flows.</summary>
    Task<VariantResponse> RemoveVariantAsync(Guid productId, Guid variantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a value for a previously-unset attribute on a variant.
    /// Recomputes and duplicate-checks VariantSignature. Requires RowVersion.
    /// </summary>
    Task<VariantResponse> AssignVariantAttributeValueAsync(Guid productId, Guid variantId, AssignVariantAttributeValueRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the value of an already-assigned attribute on a variant.
    /// Recomputes and duplicate-checks VariantSignature. Requires RowVersion.
    /// </summary>
    Task<VariantResponse> ReassignVariantAttributeValueAsync(Guid productId, Guid variantId, ReassignVariantAttributeValueRequest request, CancellationToken cancellationToken = default);

    /// <summary>Updates variant SKU, price, and/or currency. Requires RowVersion.</summary>
    Task<VariantResponse> UpdateVariantAsync(Guid productId, Guid variantId, UpdateVariantRequest request, CancellationToken cancellationToken = default);

    /// <summary>Updates variant price and currency. Requires RowVersion.</summary>
    Task<VariantResponse> UpdateVariantPriceAsync(Guid productId, Guid variantId, UpdateVariantPriceRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes variant lifecycle status. Draft→Active requires all required attributes assigned
    /// and a non-empty VariantSignature. Requires RowVersion.
    /// </summary>
    Task<VariantResponse> ChangeVariantStatusAsync(Guid productId,Guid variantId, ChangeVariantStatusRequest request, CancellationToken cancellationToken = default);

    // --- Product Lifecycle (§3.3) ---

    /// <summary>Changes product lifecycle status. Active requires at least one active variant. Requires RowVersion.</summary>
    Task<ProductResponse> ChangeProductStatusAsync(Guid productId, ChangeProductStatusRequest request, CancellationToken cancellationToken = default);

    /// <summary>Archives a product, excluding it from normal customer browsing. Requires RowVersion.</summary>
    Task<ProductResponse> ArchiveProductAsync(Guid productId, ArchiveProductRequest request, CancellationToken cancellationToken = default);

    // --- Read Operations (§5) ---

    Task<ProductDetailsResponse> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductListItemResponse>> GetAllProductsAsync(GetProductsRequest request, CancellationToken cancellationToken = default);
    Task<List<VariantListItemResponse>> GetAllVariantsByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<VariantDetailsResponse> GetVariantByIdAsync(Guid variantId, CancellationToken cancellationToken = default);
    Task<VariantDetailsResponse> GetVariantBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<List<ProductAttributeResponse>> GetProductAttributesByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
}
