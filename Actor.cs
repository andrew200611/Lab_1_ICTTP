using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Movies_domain.Model;

public partial class Actor : Entity
{
    [Required(ErrorMessage = "Поле не може бути порожнім")]
    [Display(Name = "Ім'я актора")]
    public string ActName { get; set; } = null!;

    public virtual ICollection<Movie> Mvs { get; set; } = new List<Movie>();
}