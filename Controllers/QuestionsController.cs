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

        public IActionResult Edit(int id)
        {
            var question = context.Questions.Find(id);
            if (question == null)
            {
                return NotFound();
            }

            var editViewModel = new EditQuestionViewModel
            {
                Id = question.Id,
                Name = question.Name,
                Answer = question.Answer,
                CategoryId = question.CategoryId
            };

            ViewBag.Categories = context.Categories.ToList();
            return View(editViewModel);
        }

        [HttpPost]
        public IActionResult Edit(EditQuestionViewModel editQuestionViewModel)
        {
            if (ModelState.IsValid)
            {
                var question = context.Questions.Find(editQuestionViewModel.Id);
                if (question == null)
                {
                    return NotFound();
                }

                question.Name = editQuestionViewModel.Name;
                question.Answer = editQuestionViewModel.Answer;
                question.CategoryId = editQuestionViewModel.CategoryId;

                context.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.Categories = context.Categories.ToList();
            return View(editQuestionViewModel);
        }

        public IActionResult Quiz()
        {
            var totalQuestions = context.Questions.Count();
            if (totalQuestions == 0)
            {
                TempData["Error"] = "No questions available.";
                return RedirectToAction("Index");
            }

            // Reset session for a new quiz
            HttpContext.Session.SetInt32("CorrectAnswersCount", 0);
            HttpContext.Session.Remove("AnsweredQuestionIds");

            var model = new QuizViewModel
            {
                TotalQuestionsCount = totalQuestions,
                Question = context.Questions.OrderBy(q => Guid.NewGuid()).FirstOrDefault()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Quiz(QuizViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var question = context.Questions.FirstOrDefault(q => q.Id == model.QuestionId);
            if (question == null)
            {
                ModelState.AddModelError("", "Invalid question.");
                return View(model);
            }

            // Answer checking logic
            var isCorrect = string.Equals(model.UserAnswer?.Trim(), question.Answer?.Trim(), StringComparison.OrdinalIgnoreCase);

            // Update correct answers count
            var currentCorrectAnswersCount = HttpContext.Session.GetInt32("CorrectAnswersCount") ?? 0;
            if (isCorrect)
            {
                currentCorrectAnswersCount++;
                HttpContext.Session.SetInt32("CorrectAnswersCount", currentCorrectAnswersCount);
            }

            // Get previously answered question IDs
            var answeredIds = HttpContext.Session.GetString("AnsweredQuestionIds")?.Split(',')
                .Select(int.Parse).ToList() ?? new List<int>();

            answeredIds.Add(model.QuestionId);
            HttpContext.Session.SetString("AnsweredQuestionIds", string.Join(",", answeredIds));

            // Check if quiz is complete (all questions answered)
            if (currentCorrectAnswersCount == model.TotalQuestionsCount ||
          answeredIds.Count == model.TotalQuestionsCount)
            {
                model.IsComplete = true;  // Add this property to QuizViewModel
                model.FeedbackMessage = "Congratulations! You've completed the quiz!";
                return View(model);
            }

            // Get next question
            var nextQuestion = context.Questions
                .Where(q => !answeredIds.Contains(q.Id))
                .OrderBy(q => Guid.NewGuid())
                .FirstOrDefault();

            // If no next question (shouldn't happen, but just in case)
            if (nextQuestion == null)
            {
                TempData["QuizComplete"] = "Congratulations! You've answered all questions!";
                return RedirectToAction("Index");
            }

            // Prepare model for next question
            model = new QuizViewModel
            {
                TotalQuestionsCount = model.TotalQuestionsCount,
                CorrectAnswersCount = currentCorrectAnswersCount,
                Question = nextQuestion,
                FeedbackMessage = isCorrect ? "CORRECT!" : "Incorrect."
            };

            return View(model);
        }



    }
}
