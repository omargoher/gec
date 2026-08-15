namespace GEC.ApplicationCore.DTOs.Categories;

public record CheckSlugAvailabilityRequest(
    string Slug,
    Guid? ExcludeCategoryId = null);
