using System.ComponentModel.DataAnnotations;

namespace backend.Users.Dto;

public class ChangeUserLanguageDto
{
    [Required]
    public string LanguageName { get; set; }
}

