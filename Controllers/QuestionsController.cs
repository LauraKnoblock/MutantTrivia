using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public IActionResult Index(QuestionSearchViewModel searchModel)
        {
            var questions = context.Questions.AsQueryable();

            if (!string.IsNullOrEmpty(searchModel.Name))
            {
                questions = questions.Where(q => q.Name.Contains(searchModel.Name));
            }

            if (!string.IsNullOrEmpty(searchModel.Answer))
            {
                questions = questions.Where(q => q.Answer.Contains(searchModel.Answer));
            }

            if (searchModel.CategoryId.HasValue)
            {
                questions = questions.Where(q => q.CategoryId == searchModel.CategoryId.Value);
            }

            var viewModel = new QuestionListViewModel
            {
                SearchModel = searchModel,
                Questions = questions.ToList()
            };

            ViewBag.Categories = context.Categories.ToList();

            return View(viewModel);
        }


        [HttpGet]
        public IActionResult Add()
        {
            List<QuestionCategory> categories = context.Categories.ToList();
            AddQuestionViewModel addQuestionViewModel = new AddQuestionViewModel(categories);
            return View(addQuestionViewModel);
        }

        [HttpPost]
        public IActionResult Add(AddQuestionViewModel addQuestionViewModel)
        {
            if (ModelState.IsValid)
            {
                QuestionCategory theCategory = 
             context.Categories.Find(addQuestionViewModel.CategoryId);
                Question newQuestion = new Question
                {
                    Name = addQuestionViewModel.Name,
                    Answer = addQuestionViewModel.Answer,
                    Category = theCategory
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

      public IActionResult Quiz()
        {
            // Get the total number of questions in the database
            var totalQuestions = context.Questions.Count();

            // Initialize the correct answers count from the session or default to 0
            int correctAnswersCount = HttpContext.Session.GetInt32("CorrectAnswersCount") ?? 0;

            // Get a random question from the database
            var allQuestions = context.Questions.ToList();
            var randomIndex = new Random().Next(allQuestions.Count);
            var randomQuestion = allQuestions[randomIndex];

            if (randomQuestion == null)
            {
                // Handle case where no questions exist
                return RedirectToAction("Index");
            }

            var quizModel = new QuizViewModel
            {
                QuestionId = randomQuestion.Id,
                QuestionText = randomQuestion.Name,
                CorrectAnswersCount = correctAnswersCount,
                TotalQuestions = totalQuestions
            };

            HttpContext.Session.SetInt32("CorrectAnswersCount", correctAnswersCount);


            return View(quizModel);
        }

        [HttpPost]
        public IActionResult Quiz(QuizViewModel model)
        {
            var question = context.Questions.FirstOrDefault(q => q.Id == model.QuestionId);
            if (question == null)
            {
                ModelState.AddModelError("", "Invalid question.");
                return View(model);
            }

            // Check if the user's answer matches the correct answer
            model.IsCorrect = string.Equals(model.UserAnswer, question.Answer, StringComparison.OrdinalIgnoreCase);

            // Update correct answers count if correct
            if (model.IsCorrect)
            {
                model.CorrectAnswersCount++;
            }

            // Display the result
            model.CorrectAnswer = question.Answer;

            // If the user has answered 10 questions correctly, show a success message
            if (model.CorrectAnswersCount == 10)
            {
                TempData["QuizComplete"] = "Congratulations! You have answered 10 questions correctly!";
            }

            HttpContext.Session.SetInt32("CorrectAnswersCount", model.CorrectAnswersCount);


            return View(model);
        }


    }
}
