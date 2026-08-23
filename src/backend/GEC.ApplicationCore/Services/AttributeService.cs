using GEC.ApplicationCore.DTOs;
using GEC.ApplicationCore.DTOs.Attributes;
using GEC.ApplicationCore.Exceptions;
using GEC.ApplicationCore.Interfaces.Persistence;
using GEC.ApplicationCore.Interfaces.Services;
using GEC.Domain.Entities;

namespace GEC.ApplicationCore.Services;

public class AttributeService : IAttributeService
{
    private readonly IUnitOfWork _unitOfWork;

    public AttributeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AttributeDefinitionResponse> AddAttributeDefinitionAsync(
        CreateAttributeDefinitionRequest request, CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();

        if (await _unitOfWork.AttributeDefinitions.ExistsByNameAsync(name, cancellationToken))
            throw new ConflictException("Attribute definition");

        var attribute = new AttributeDefinition { Name = name };
        _unitOfWork.AttributeDefinitions.Add(attribute);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AttributeDefinitionResponse { Id = attribute.Id, Name = attribute.Name };
    }

    public async Task<AttributeDefinitionResponse> RemoveAttributeDefinitionAsync(
        Guid attributeId, CancellationToken cancellationToken = default)
    {
        var attribute = await _unitOfWork.AttributeDefinitions.GetByIdAsync(attributeId, cancellationToken)
            ?? throw new NotFoundException("Attribute definition", attributeId);

        // Cannot delete if any product still has this attribute enabled
        if (await _unitOfWork.ProductAttributes.ExistsByAttributeIdAsync(attributeId, cancellationToken))
            throw new InvalidRequestException(
                "Cannot delete attribute definition: it is still attached to one or more products.");

        // Cannot delete if any attribute values still exist (safety; cascade is DB-level but warn early)
        if (await _unitOfWork.AttributeValues.ExistsByAttributeIdAsync(attributeId, cancellationToken))
            throw new InvalidRequestException(
                "Cannot delete attribute definition: it still has attribute values. Remove them first.");

        _unitOfWork.AttributeDefinitions.Remove(attribute);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AttributeDefinitionResponse { Id = attribute.Id, Name = attribute.Name };
    }

    public async Task<PagedResult<AttributeDefinitionResponse>> GetAttributeDefinitionsAsync(
        GetAttributeDefinitionsRequest request, CancellationToken cancellationToken = default)
        => await _unitOfWork.AttributeDefinitions.GetPagedAsync(request, cancellationToken);

    public async Task<AttributeValueResponse> AddAttributeValueAsync(
        Guid attributeId, CreateAttributeValueRequest request, CancellationToken cancellationToken = default)
    {
        _ = await _unitOfWork.AttributeDefinitions.GetByIdAsync(attributeId, cancellationToken)
            ?? throw new NotFoundException("Attribute definition", attributeId);

        var value = request.Value.Trim();

        if (await _unitOfWork.AttributeValues.ExistsAsync(attributeId, value, cancellationToken))
            throw new ConflictException("Attribute value");

        var attrValue = new AttributeValue
        {
            AttributeId = attributeId,
            Value = value
        };

        _unitOfWork.AttributeValues.Add(attrValue);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AttributeValueResponse
        {
            Id = attrValue.Id,
            AttributeId = attrValue.AttributeId,
            Value = attrValue.Value
        };
    }

    public async Task<AttributeValueResponse> RemoveAttributeValueAsync(
        Guid attributeId, Guid valueId, CancellationToken cancellationToken = default)
    {
        var attrValue = await _unitOfWork.AttributeValues.GetByIdAsync(valueId, cancellationToken)
            ?? throw new NotFoundException("Attribute value", valueId);

        if (attrValue.AttributeId != attributeId)
            throw new InvalidRequestException("Attribute value does not belong to the specified attribute.");

        // Cannot delete if any variant is still using this value
        if (await _unitOfWork.VariantAttributeValues.ExistsByAttributeValueIdAsync(valueId, cancellationToken))
            throw new InvalidRequestException(
                "Cannot delete attribute value: it is still assigned to one or more variants.");

        _unitOfWork.AttributeValues.Remove(attrValue);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AttributeValueResponse
        {
            Id = attrValue.Id,
            AttributeId = attrValue.AttributeId,
            Value = attrValue.Value
        };
    }

    public async Task<List<AttributeValueResponse>> GetAttributeValuesByAttributeIdAsync(
        Guid attributeId, CancellationToken cancellationToken = default)
    {
        _ = await _unitOfWork.AttributeDefinitions.GetByIdAsync(attributeId, cancellationToken)
            ?? throw new NotFoundException("Attribute definition", attributeId);

        return await _unitOfWork.AttributeValues.GetByAttributeIdAsync(attributeId, cancellationToken);
    }
}
