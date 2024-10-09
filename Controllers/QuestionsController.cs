using Microsoft.AspNetCore.Mvc;
using MutantTrivia.Data;
using MutantTrivia.Models;
using MutantTrivia.ViewModels;

namespace MutantTrivia.Controllers
{
    public class QuestionsController : Controller
    {

        public IActionResult Index()
        {

            List<Question> questions = new List<Question>(QuestionData.GetAll());
            return View(questions);
        }

        [HttpGet]
        public IActionResult Add()
        {
            AddQuestionViewModel addQuestionViewModel = new AddQuestionViewModel();
            return View(addQuestionViewModel);
        }

        [HttpPost]
        public IActionResult Add(AddQuestionViewModel addQuestionViewModel)
        {
            if (ModelState.IsValid)
            {
                Question newQuestion = new Question
                {
                    Name = addQuestionViewModel.Name,
                    Answer = addQuestionViewModel.Answer,
                    Type = addQuestionViewModel.Type
                };
                QuestionData.Add(newQuestion);

                return Redirect("/Questions");
            }
            return View(addQuestionViewModel);
        }

        [HttpGet]
        public IActionResult Delete()
        {
            ViewBag.questions = QuestionData.GetAll();

            return View();
        }

        [HttpPost]
        public IActionResult Delete(int[] questionIds)
        {
            foreach (int questionId in questionIds)
            {
                QuestionData.Remove(questionId);
            }

            return Redirect("/Questions");
        }
    }
}
