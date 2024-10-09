using Microsoft.AspNetCore.Mvc.Rendering;
using MutantTrivia.Models;
using System.ComponentModel.DataAnnotations;

namespace MutantTrivia.ViewModels
{
    public class AddQuestionViewModel
    {
        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Question must be between 3-100 characters long.")]
        public string? Name { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Answer must be between 1-50 characters long.")]
        public string? Answer { get; set; }

        public QuestionType Type { get; set; }

        public List<SelectListItem> QuestionTypes { get; set; } = new List<SelectListItem>
   {
      new SelectListItem(QuestionType.History.ToString(), ((int)QuestionType.History).ToString()),
      new SelectListItem(QuestionType.Literature.ToString(), ((int)QuestionType.Literature).ToString()),
      new SelectListItem(QuestionType.Geography.ToString(), ((int)QuestionType.Geography).ToString()),
      new SelectListItem(QuestionType.Arts.ToString(), ((int)QuestionType.Arts).ToString()),
      new SelectListItem(QuestionType.Sports.ToString(), ((int)QuestionType.Sports).ToString()),
      new SelectListItem(QuestionType.Science.ToString(), ((int)QuestionType.Science).ToString()),




    };
    }
}