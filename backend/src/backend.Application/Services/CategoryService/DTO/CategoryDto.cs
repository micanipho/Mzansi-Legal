using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using backend.Domains.LegalDocuments;
using System;

namespace backend.Services.CategoryService.DTO;

/// <summary>
/// Data transfer object for Category create, update, and read operations.
/// AutoMapper maps this to and from the Category domain entity automatically.
/// </summary>
[AutoMap(typeof(Category))]
public class CategoryDto : EntityDto<Guid>
{
    /// <summary>Display name of the category.</summary>
    public string Name { get; set; }

    /// <summary>Icon identifier string used by the frontend.</summary>
    public string Icon { get; set; }

    /// <summary>Domain classification: 1 = Legal, 2 = Financial.</summary>
    public DocumentDomain Domain { get; set; }

    /// <summary>Display ordering among sibling categories.</summary>
    public int SortOrder { get; set; }
}
