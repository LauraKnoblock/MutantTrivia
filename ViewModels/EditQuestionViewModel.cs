using Microsoft.AspNetCore.Mvc.Rendering;
using MutantTrivia.Models;
using System.ComponentModel.DataAnnotations;

namespace MutantTrivia.ViewModels
{
    public class EditQuestionViewModel
    {
        public int Id { get; set; } // Added to identify the question being edited

        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Question must be between 3-100 characters long.")]
        public string Name { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Answer must be between 1-50 characters long.")]
        public string Answer { get; set; }

        public QuestionType Type { get; set; } // Consider removing if not relevant for editing

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }
        public List<SelectListItem>? Categories { get; set; } // Optional if categories are not editable
    }
}