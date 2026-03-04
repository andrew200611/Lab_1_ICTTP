using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Movies_domain.Model;

public partial class Genre : Entity
{
    [Required(ErrorMessage = "Поле не може бути порожнім")]
    [Display(Name = "Назва жанру")]
    public string GrName { get; set; } = null!;

    public virtual ICollection<Movie> Mvs { get; set; } = new List<Movie>();
}