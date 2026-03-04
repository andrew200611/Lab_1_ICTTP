using System.ComponentModel.DataAnnotations;

namespace Movies_domain.Model;

public partial class User : Entity
{
    [Required(ErrorMessage = "Поле не може бути порожнім")]
    [Display(Name = "Email")]
    public string UsEmail { get; set; } = null!;

    [Display(Name = "Пароль")]
    public string? UsPassword { get; set; }

    [Display(Name = "Імʼя користувача")]
    public string? UsName { get; set; }

    [Display(Name = "Роль")]
    public string? UsRole { get; set; }

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<Movie> FavMovies { get; set; } = new List<Movie>();
}