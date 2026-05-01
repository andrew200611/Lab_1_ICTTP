using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Movies_infrastructure.ViewModels
{
    public class ActorListItem
    {
        public int Id { get; set; }

        [Display(Name = "Ім'я актора")]
        public string ActName { get; set; } = null!;

        [Display(Name = "Фільми")]
        public List<string> MovieNames { get; set; } = new List<string>();
    }
}
