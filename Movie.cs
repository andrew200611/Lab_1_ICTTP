using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Movies_domain.Model;

public partial class Movie : Entity
{
    [Required(ErrorMessage = "Поле не може бути порожнім")]
    [Display(Name = "Назва фільму")]
    public string MvName { get; set; } = null!;

    [Display(Name = "Опис")]
    public string? MvDescription { get; set; }

    [Display(Name = "Рік виходу")]
    public int? MvYear { get; set; }

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<Actor> Acts { get; set; } = new List<Actor>();
    public virtual ICollection<User> FavUsers { get; set; } = new List<User>();
    public virtual ICollection<Genre> Grs { get; set; } = new List<Genre>();
}