using Abp.Domain.Entities;
using backend.MzansiLegal.RefLists;
using System;

namespace backend.MzansiLegal.Categories;

public class Category : Entity<Guid>
{
    public string Name { get; set; }

    /// <summary>
    /// JSON-serialised dictionary of locale -> label, e.g. {"en":"Employment","zu":"Ukuqashwa"}
    /// </summary>
    public string LocalizedLabels { get; set; }

    public string Icon { get; set; }

    public LegalDomain Domain { get; set; }

    public int SortOrder { get; set; }

    protected Category() { }

    public Category(Guid id, string name, string icon, LegalDomain domain, int sortOrder)
    {
        Id = id;
        Name = name;
        Icon = icon;
        Domain = domain;
        SortOrder = sortOrder;
        LocalizedLabels = "{}";
    }
}
