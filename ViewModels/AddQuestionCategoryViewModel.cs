using System.ComponentModel.DataAnnotations;

namespace MutantTrivia.ViewModels
{
    public class AddQuestionCategoryViewModel
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 20 characters.")]
        public string Name  { get; set; }
    }
}
