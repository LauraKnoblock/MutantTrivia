using Microsoft.AspNetCore.Mvc;
using MutantTrivia.Data;
using MutantTrivia.Models;
using MutantTrivia.ViewModels;

namespace MutantTrivia.Controllers
{

   
    public class QuestionsController : Controller

    {
        private QuestionDbContext context;

        public QuestionsController(QuestionDbContext dbContext)
        {
            context = dbContext;
        }

        public IActionResult Index()
        {

            List<Question> questions = context.Questions.ToList();
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
                context.Questions.Add(newQuestion);
                context.SaveChanges();


                return Redirect("/Questions");
            }
            return View(addQuestionViewModel);
        }

        [HttpGet]
        public IActionResult Delete()
        {
            ViewBag.questions = context.Questions.ToList();

            return View();
        }

        [HttpPost]
        public IActionResult Delete(int[] questionIds)
        {
            foreach (int questionId in questionIds)
            {
                Question? theQuestion = context.Questions.Find(questionId);
                context.Questions.Remove(theQuestion);
            }

            context.SaveChanges();

            return Redirect("/Questions");
        }
    }
}
