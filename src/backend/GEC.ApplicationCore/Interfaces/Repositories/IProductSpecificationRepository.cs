using GEC.Domain.Entities;

namespace GEC.ApplicationCore.Interfaces.Repositories;

/// <summary>
/// Specifications are add-only in this revision (PDR §3.1.4).
/// </summary>
public interface IProductSpecificationRepository : IBaseRepository<ProductSpecification>
{
}
