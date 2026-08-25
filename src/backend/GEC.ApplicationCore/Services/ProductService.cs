using GEC.ApplicationCore.DTOs;
using GEC.ApplicationCore.DTOs.Products;
using GEC.ApplicationCore.Exceptions;
using GEC.ApplicationCore.Interfaces.Persistence;
using GEC.ApplicationCore.Interfaces.Services;
using GEC.Domain.Entities;
using GEC.Domain.Enums;

namespace GEC.ApplicationCore.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // ─── Product Management ────────────────────────────────────────────────────

    public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var slug = NormalizeSlug(request.Slug);

        if (await _unitOfWork.Products.ExistsBySlugAsync(slug, cancellationToken))
            throw new ConflictException("Product slug");

        var product = new Product
        {
            Name = request.Name.Trim(),
            Slug = slug,
            BaseDescription = request.BaseDescription?.Trim() ?? string.Empty,
            Status = ProductStatus.Draft
        };

        _unitOfWork.Products.Add(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToProductResponse(product);
    }

    public async Task<ProductAttributeResponse> AddProductAttributeAsync(Guid productId, AddProductAttributeRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product", productId);

        EnsureProductNotArchived(product);

        var attribute = await _unitOfWork.AttributeDefinitions.GetByIdAsync(request.AttributeId, cancellationToken)
            ?? throw new NotFoundException("Attribute definition", request.AttributeId);

        if (await _unitOfWork.ProductAttributes.ExistsAsync(productId, request.AttributeId, cancellationToken))
            throw new ConflictException("Product attribute");

        var productAttribute = new ProductAttribute
        {
            ProductId = productId,
            AttributeId = request.AttributeId,
            IsRequired = request.IsRequired
        };

        _unitOfWork.ProductAttributes.Add(productAttribute);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProductAttributeResponse
        {
            ProductId = productId,
            AttributeId = request.AttributeId,
            AttributeName = attribute.Name,
            IsRequired = request.IsRequired
        };
    }

    public async Task RemoveProductAttributeAsync(Guid productId, Guid attributeId, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product", productId);
        EnsureProductNotArchived(product);

        var productAttribute = await _unitOfWork.ProductAttributes.GetAsync(productId, attributeId, cancellationToken)
            ?? throw new NotFoundException("Product attribute");

        // PDR §3.1.3: fail loudly if any variant still uses the attribute
        if (await _unitOfWork.VariantAttributeValues.ExistsForProductAttributeAsync(productId, attributeId, cancellationToken))
            throw new InvalidRequestException(
                "Cannot remove attribute: one or more variants still use it. " +
                "Reassign or retire those variants first.");

        _unitOfWork.ProductAttributes.Remove(productAttribute);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProductSpecificationResponse> AddProductSpecificationAsync(Guid productId, AddProductSpecificationRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product", productId);
        EnsureProductNotArchived(product);

        var spec = new ProductSpecification
        {
            ProductId = productId,
            Name = request.Name.Trim(),
            Value = request.Value.Trim()
        };

        _unitOfWork.ProductSpecifications.Add(spec);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProductSpecificationResponse
        {
            Id = spec.Id,
            ProductId = spec.ProductId,
            Name = spec.Name,
            Value = spec.Value
        };
    }

    // ─── Variant Management ────────────────────────────────────────────────────

    public async Task<VariantResponse> AddVariantAsync(Guid productId, CreateVariantRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product", productId);

        EnsureProductNotArchived(product);

        var sku = NormalizeSku(request.Sku);

        if (await _unitOfWork.ProductVariants.ExistsBySkuAsync(sku, cancellationToken: cancellationToken))
            throw new ConflictException("SKU");

        var variant = new ProductVariant
        {
            ProductId = productId,
            Sku = sku,
            PriceAmount = request.PriceAmount,
            Currency = request.Currency.ToUpperInvariant(),
            Status = ProductVariantStatus.Draft,
            VariantSignature = string.Empty
        };

        _unitOfWork.ProductVariants.Add(variant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToVariantResponse(variant, new List<VariantAttributeDto>());
    }

    public async Task<VariantResponse> RemoveVariantAsync(Guid productId, Guid variantId, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product", productId);
        EnsureProductNotArchived(product);

        var variant = await _unitOfWork.ProductVariants.GetByIdAsync(variantId, cancellationToken)
            ?? throw new NotFoundException("Variant", variantId);

        if (variant.ProductId != productId)
            throw new InvalidRequestException("Variant does not belong to the specified product.");

        var attributeDtos = (await _unitOfWork.VariantAttributeValues.GetByVariantIdAsync(variantId, cancellationToken))
            .Select(av => new VariantAttributeDto
            {
                AttributeId = av.AttributeId,
                AttributeValueId = av.AttributeValueId
            }).ToList();

        _unitOfWork.ProductVariants.Remove(variant);

        if (variant.Status == ProductVariantStatus.Active && product.Status == ProductStatus.Active)
        {
            var variants = await _unitOfWork.ProductVariants.GetByProductIdAsync(productId, cancellationToken);
            if (variants.Count(v => v.Status == ProductVariantStatus.Active) <= 1)
            {
                product.Status = ProductStatus.Inactive;
                _unitOfWork.Products.Update(product);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToVariantResponse(variant, attributeDtos);
    }

    public async Task<VariantResponse> AssignVariantAttributeValueAsync(Guid productId, Guid variantId, AssignVariantAttributeValueRequest request, CancellationToken cancellationToken = default)
    {
        var variant = await _unitOfWork.ProductVariants.GetByIdAsync(variantId, cancellationToken)
            ?? throw new NotFoundException("Variant", variantId);

        if (variant.ProductId != productId)
            throw new InvalidRequestException("Variant does not belong to the specified product.");

        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product", productId);
        EnsureProductNotArchived(product);

        var currentRowVersion = _unitOfWork.GetRowVersion(variant);
        if (currentRowVersion != request.RowVersion)
            throw new ConcurrencyException("Variant");

        if (!await _unitOfWork.ProductAttributes.ExistsAsync(productId, request.AttributeId, cancellationToken))
            throw new InvalidRequestException("The attribute is not enabled for this product.");

        var attrValue = await _unitOfWork.AttributeValues.GetByIdAsync(request.AttributeValueId, cancellationToken)
            ?? throw new NotFoundException("Attribute value", request.AttributeValueId);

        if (attrValue.AttributeId != request.AttributeId)
            throw new InvalidRequestException("The attribute value does not belong to the specified attribute.");

        if (await _unitOfWork.VariantAttributeValues.ExistsAsync(variantId, request.AttributeId, cancellationToken))
            throw new ConflictException("Variant attribute value (use Reassign to update an existing value)");

        var variantAttributeValue = new VariantAttributeValue
        {
            VariantId = variantId,
            AttributeId = request.AttributeId,
            AttributeValueId = request.AttributeValueId,
            ProductId = productId
        };

        _unitOfWork.VariantAttributeValues.Add(variantAttributeValue);

        var allValues = await _unitOfWork.VariantAttributeValues.GetByVariantIdAsync(variantId, cancellationToken);
        allValues.Add(variantAttributeValue);
        var newSignature = ComputeSignature(allValues);

        if (!string.IsNullOrEmpty(newSignature))
        {
            if (await _unitOfWork.ProductVariants.ExistsBySignatureAsync(productId, newSignature, variantId, cancellationToken))
                throw new ConflictException("Variant attribute combination (another variant already has this exact combination)");
        }

        variant.VariantSignature = newSignature;
        _unitOfWork.ProductVariants.Update(variant);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedValues = await _unitOfWork.VariantAttributeValues.GetByVariantIdAsync(variantId, cancellationToken);
        return MapToVariantResponse(variant, updatedValues.Select(v => new VariantAttributeDto
        {
            AttributeId = v.AttributeId,
            AttributeValueId = v.AttributeValueId
        }).ToList());
    }

    public async Task<VariantResponse> ReassignVariantAttributeValueAsync(Guid productId, Guid variantId, ReassignVariantAttributeValueRequest request, CancellationToken cancellationToken = default)
    {
        var variant = await _unitOfWork.ProductVariants.GetByIdAsync(variantId, cancellationToken)
            ?? throw new NotFoundException("Variant", variantId);

        if (variant.ProductId != productId)
            throw new InvalidRequestException("Variant does not belong to the specified product.");

        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product", productId);
        EnsureProductNotArchived(product);

        var currentRowVersion = _unitOfWork.GetRowVersion(variant);
        if (currentRowVersion != request.RowVersion)
            throw new ConcurrencyException("Variant");

        if (!await _unitOfWork.ProductAttributes.ExistsAsync(productId, request.AttributeId, cancellationToken))
            throw new InvalidRequestException("The attribute is not enabled for this product.");

        // Single fetch — this list and existingVav are the same objects we'll mutate and score. 
        var currentValues = await _unitOfWork.VariantAttributeValues.GetByVariantIdAsync(variantId, cancellationToken);
        var existingVav = currentValues.FirstOrDefault(v => v.AttributeId == request.AttributeId)
            ?? throw new InvalidRequestException("Variant does not have a value assigned for this attribute. Use Assign instead.");

        if (existingVav.AttributeValueId == request.NewAttributeValueId)
        {

            return MapToVariantResponse(variant, currentValues.Select(v => new VariantAttributeDto
            {
                AttributeId = v.AttributeId,
                AttributeValueId = v.AttributeValueId
            }).ToList());
        }

        var newAttrValue = await _unitOfWork.AttributeValues.GetByIdAsync(request.NewAttributeValueId, cancellationToken)
            ?? throw new NotFoundException("Attribute value", request.NewAttributeValueId);

        if (newAttrValue.AttributeId != request.AttributeId)
            throw new InvalidRequestException("The new attribute value does not belong to the specified attribute.");

        existingVav.AttributeValueId = request.NewAttributeValueId;

        // No second query — currentValues already reflects the mutation via existingVav. 
        var newSignature = ComputeSignature(currentValues);

        if (newSignature is not null &&
            await _unitOfWork.ProductVariants.ExistsBySignatureAsync(productId, newSignature, variantId, cancellationToken))
            throw new ConflictException("Variant attribute combination");


        variant.VariantSignature = newSignature;
        _unitOfWork.ProductVariants.Update(variant);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToVariantResponse(variant, currentValues.Select(v => new VariantAttributeDto
        {
            AttributeId = v.AttributeId,
            AttributeValueId = v.AttributeValueId
        }).ToList());
    }

    public async Task<VariantResponse> UpdateVariantAsync(Guid productId, Guid variantId, UpdateVariantRequest request, CancellationToken cancellationToken = default)
    {
        var variant = await _unitOfWork.ProductVariants.GetByIdAsync(variantId, cancellationToken)
            ?? throw new NotFoundException("Variant", variantId);

        if (variant.ProductId != productId)
            throw new InvalidRequestException("Variant does not belong to the specified product.");

        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product", productId);
        EnsureProductNotArchived(product);

        var currentRowVersion = _unitOfWork.GetRowVersion(variant);
        if (currentRowVersion != request.RowVersion)
            throw new ConcurrencyException("Variant");

        if (request.Sku is not null)
        {
            var sku = NormalizeSku(request.Sku);
            if (await _unitOfWork.ProductVariants.ExistsBySkuAsync(sku, variantId, cancellationToken))
                throw new ConflictException("SKU");
            variant.Sku = sku;
        }

        if (request.PriceAmount.HasValue)
        {
            EnsurePriceChangeAllowed(variant);
            variant.PriceAmount = request.PriceAmount.Value;
        }

        if (request.Currency is not null)
            variant.Currency = request.Currency.ToUpperInvariant();

        _unitOfWork.ProductVariants.Update(variant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var attrValues = await _unitOfWork.VariantAttributeValues.GetByVariantIdAsync(variantId, cancellationToken);
        return MapToVariantResponse(variant, attrValues.Select(v => new VariantAttributeDto
        {
            AttributeId = v.AttributeId,
            AttributeValueId = v.AttributeValueId
        }).ToList());
    }

    public async Task<VariantResponse> UpdateVariantPriceAsync(Guid productId, Guid variantId, UpdateVariantPriceRequest request, CancellationToken cancellationToken = default)
    {
        var variant = await _unitOfWork.ProductVariants.GetByIdAsync(variantId, cancellationToken)
            ?? throw new NotFoundException("Variant", variantId);

        if (variant.ProductId != productId)
            throw new InvalidRequestException("Variant does not belong to the specified product.");

        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product", productId);
        EnsureProductNotArchived(product);

        var currentRowVersion = _unitOfWork.GetRowVersion(variant);
        if (currentRowVersion != request.RowVersion)
            throw new ConcurrencyException("Variant");

        if (variant.Status == ProductVariantStatus.Discontinued)
            throw new InvalidRequestException("Cannot change price of a discontinued variant.");

        EnsurePriceChangeAllowed(variant);
        variant.PriceAmount = request.PriceAmount;
        variant.Currency = request.Currency.ToUpperInvariant();
        _unitOfWork.ProductVariants.Update(variant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var attrValues = await _unitOfWork.VariantAttributeValues.GetByVariantIdAsync(variantId, cancellationToken);
        return MapToVariantResponse(variant, attrValues.Select(v => new VariantAttributeDto
        {
            AttributeId = v.AttributeId,
            AttributeValueId = v.AttributeValueId
        }).ToList());
    }

    public async Task<VariantResponse> ChangeVariantStatusAsync(Guid productId, Guid variantId, ChangeVariantStatusRequest request, CancellationToken cancellationToken = default)
    {
        var variant = await _unitOfWork.ProductVariants.GetByIdAsync(variantId, cancellationToken)
                      ?? throw new NotFoundException("Variant", variantId);

        if (variant.ProductId != productId)
            throw new InvalidRequestException("Variant does not belong to the specified product.");

        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product", productId);
        EnsureProductNotArchived(product);
        
        var currentRowVersion = _unitOfWork.GetRowVersion(variant);
        if (currentRowVersion != request.RowVersion)
            throw new ConcurrencyException("Variant");

        ValidateVariantStatusTransition(variant.Status, request.Status);

        if (request.Status == ProductVariantStatus.Active && variant.Status == ProductVariantStatus.Draft)
        {
            if (string.IsNullOrEmpty(variant.VariantSignature))
                throw new InvalidRequestException(
                    "Cannot activate variant: no attribute values have been assigned (VariantSignature is empty).");

            var requiredAttrs = await _unitOfWork.ProductAttributes.GetRequiredByProductIdAsync(variant.ProductId, cancellationToken);
            var assignedAttrIds = (await _unitOfWork.VariantAttributeValues.GetByVariantIdAsync(variantId, cancellationToken))
                .Select(v => v.AttributeId)
                .ToHashSet();

            var missing = requiredAttrs.Where(ra => !assignedAttrIds.Contains(ra.AttributeId)).ToList();
            if (missing.Count > 0)
                throw new InvalidRequestException(
                    $"Cannot activate variant: {missing.Count} required attribute(s) not yet assigned.");
        }

        if (variant.Status == ProductVariantStatus.Active && request.Status != ProductVariantStatus.Active && product.Status == ProductStatus.Active)
        {
            var variants = await _unitOfWork.ProductVariants.GetByProductIdAsync(productId, cancellationToken);
            if (variants.Count(v => v.Status == ProductVariantStatus.Active) <= 1)
            {
                product.Status = ProductStatus.Inactive;
                _unitOfWork.Products.Update(product);
            }
        }

        variant.Status = request.Status;
        _unitOfWork.ProductVariants.Update(variant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var attrValues = await _unitOfWork.VariantAttributeValues.GetByVariantIdAsync(variantId, cancellationToken);
        return MapToVariantResponse(variant, attrValues.Select(v => new VariantAttributeDto
        {
            AttributeId = v.AttributeId,
            AttributeValueId = v.AttributeValueId
        }).ToList());
    }

    // ─── Product Lifecycle ─────────────────────────────────────────────────────

    public async Task<ProductResponse> ChangeProductStatusAsync(Guid productId, ChangeProductStatusRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product", productId);

        var currentRowVersion = _unitOfWork.GetRowVersion(product);
        if (currentRowVersion != request.RowVersion)
            throw new ConcurrencyException("Product");

        ValidateProductStatusTransition(product.Status, request.Status);

        if (request.Status == ProductStatus.Active)
        {
            if (!await _unitOfWork.Products.HasActiveVariantsAsync(productId, cancellationToken))
                throw new InvalidRequestException("Cannot activate product: it has no active variants.");
        }

        product.Status = request.Status;
        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToProductResponse(product);
    }

    public async Task<ProductResponse> ArchiveProductAsync(Guid productId, ArchiveProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product", productId);

        var currentRowVersion = _unitOfWork.GetRowVersion(product);
        if (currentRowVersion != request.RowVersion)
            throw new ConcurrencyException("Product");

        if (product.Status == ProductStatus.Archived)
            throw new InvalidRequestException("Product is already archived.");

        product.Status = ProductStatus.Archived;
        _unitOfWork.Products.Update(product);

        var variants = await _unitOfWork.ProductVariants.GetByProductIdAsync(productId, cancellationToken);
        foreach (var v in variants)
        {
            if (v.Status != ProductVariantStatus.Discontinued)
            {
                var variantEntity = await _unitOfWork.ProductVariants.GetByIdAsync(v.Id, cancellationToken);
                if (variantEntity != null)
                {
                    variantEntity.Status = ProductVariantStatus.Discontinued;
                    _unitOfWork.ProductVariants.Update(variantEntity);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToProductResponse(product);
    }

    // ─── Read Operations ───────────────────────────────────────────────────────

    public async Task<ProductDetailsResponse> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Products.GetDetailsByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product", productId);
        return result;
    }

    public async Task<PagedResult<ProductListItemResponse>> GetAllProductsAsync(ProductFilterParams filterParams, CancellationToken cancellationToken = default)
        => await _unitOfWork.Products.GetPagedAsync(filterParams, cancellationToken);

    public async Task<List<VariantListItemResponse>> GetAllVariantsByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        _ = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product", productId);
        return await _unitOfWork.ProductVariants.GetByProductIdAsync(productId, cancellationToken);
    }

    public async Task<VariantDetailsResponse> GetVariantByIdAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ProductVariants.GetDetailsByIdAsync(variantId, cancellationToken)
            ?? throw new NotFoundException("Variant", variantId);
        return result;
    }

    public async Task<VariantDetailsResponse> GetVariantBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        var normalizedSku = NormalizeSku(sku);
        var result = await _unitOfWork.ProductVariants.GetDetailsBySkuAsync(normalizedSku, cancellationToken)
            ?? throw new NotFoundException("Variant");
        return result;
    }

    public async Task<List<ProductAttributeResponse>> GetProductAttributesByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        _ = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product", productId);
        return await _unitOfWork.ProductAttributes.GetByProductIdAsync(productId, cancellationToken);
    }

    // ─── Private Helpers ───────────────────────────────────────────────────────

    private static string NormalizeSlug(string slug)
        => slug.Trim().ToLowerInvariant();

    private static void EnsureProductNotArchived(Product product)
    {
        if (product.Status == ProductStatus.Archived)
            throw new InvalidRequestException("Cannot modify an archived product or its variants.");
    }

    private static string NormalizeSku(string sku)
        => sku.Trim().ToUpperInvariant();

    /// <summary>
    /// Computes the canonical variant signature: sorted (AttributeId:AttributeValueId) pairs
    /// joined by '|'. Returns empty string if the list is empty.
    /// </summary>
    private static string ComputeSignature(IEnumerable<VariantAttributeValue> values)
    {
        var pairs = values
            .OrderBy(v => v.AttributeId)
            .Select(v => $"{v.AttributeId}:{v.AttributeValueId}");
        return string.Join("|", pairs);
    }

    private ProductResponse MapToProductResponse(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Slug = product.Slug,
        BaseDescription = product.BaseDescription,
        Status = product.Status,
        CreatedAt = product.CreatedAt,
        UpdatedAt = product.UpdatedAt,
        RowVersion = _unitOfWork.GetRowVersion(product)
    };

    private static VariantResponse MapToVariantResponse(ProductVariant variant, List<VariantAttributeDto> attributes) => new()
    {
        Id = variant.Id,
        ProductId = variant.ProductId,
        Sku = variant.Sku,
        PriceAmount = variant.PriceAmount,
        Currency = variant.Currency,
        Status = variant.Status,
        VariantSignature = variant.VariantSignature,
        CreatedAt = variant.CreatedAt,
        Attributes = attributes
    };

    private static void EnsurePriceChangeAllowed(ProductVariant variant)
    {
        if (variant.Status == ProductVariantStatus.Discontinued)
            throw new InvalidRequestException("Cannot change price of a discontinued variant.");
    }

    // ─── Lifecycle Validation ──────────────────────────────────────────────────

    private static void ValidateProductStatusTransition(ProductStatus current, ProductStatus next)
    {
        if (current == ProductStatus.Archived)
            throw new InvalidRequestException("Archived products cannot change status. They are terminal.");

        var valid = (current, next) switch
        {
            (ProductStatus.Draft, ProductStatus.Active) => true,
            (ProductStatus.Draft, ProductStatus.Inactive) => true,
            (ProductStatus.Active, ProductStatus.Inactive) => true,
            (ProductStatus.Active, ProductStatus.Archived) => true,
            (ProductStatus.Inactive, ProductStatus.Active) => true,
            (ProductStatus.Inactive, ProductStatus.Archived) => true,
            _ => false
        };

        if (!valid)
            throw new InvalidRequestException($"Invalid product status transition: {current} to {next}.");
    }

    private static void ValidateVariantStatusTransition(ProductVariantStatus current, ProductVariantStatus next)
    {
        if (current == ProductVariantStatus.Discontinued)
            throw new InvalidRequestException("Discontinued variants cannot change status. They are terminal.");

        var valid = (current, next) switch
        {
            (ProductVariantStatus.Draft, ProductVariantStatus.Active) => true,
            (ProductVariantStatus.Draft, ProductVariantStatus.Inactive) => true,
            (ProductVariantStatus.Active, ProductVariantStatus.Inactive) => true,
            (ProductVariantStatus.Active, ProductVariantStatus.Discontinued) => true,
            (ProductVariantStatus.Inactive, ProductVariantStatus.Active) => true,
            (ProductVariantStatus.Inactive, ProductVariantStatus.Discontinued) => true,
            _ => false
        };

        if (!valid)
            throw new InvalidRequestException($"Invalid variant status transition: {current} to {next}.");
    }
}
