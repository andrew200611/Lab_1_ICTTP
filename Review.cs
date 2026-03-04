using System.ComponentModel.DataAnnotations;

namespace Movies_domain.Model;

public partial class Review : Entity
{
    [Display(Name = "Фільм")]
    public int RwMovie { get; set; }

    [Display(Name = "Користувач")]
    public int RwUser { get; set; }

    [Display(Name = "Дата")]
    public DateOnly? RwDate { get; set; }

    [Display(Name = "Текст відгуку")]
    [MaxLength(1000)]
    public string? RwText { get; set; }

    [Display(Name = "Оцінка")]
    [Range(1, 10, ErrorMessage = "Оцінка має бути від 1 до 10")]
    public int? RwRate { get; set; }

    public virtual Movie RwMovieNavigation { get; set; } = null!;
    public virtual User RwUserNavigation { get; set; } = null!;
}