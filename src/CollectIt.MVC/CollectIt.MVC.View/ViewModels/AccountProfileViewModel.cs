using System.ComponentModel.DataAnnotations;

namespace CollectIt.MVC.View.ViewModels;

public class ProfileAccountViewModel
{
    [Required]
    [MinLength(6)]
    public string Username { get; set; }

    [Required]
    [DataType(DataType.EmailAddress)]
    [MinLength(8)]
    public string Email { get; set; }

    public bool EmailConfirmed { get; set; }
}