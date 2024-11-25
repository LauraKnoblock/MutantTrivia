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

            var randomIndex = new Random().Next(totalQuestions); // Random number between 0 and totalQuestions - 1
            var randomQuestion = context.Questions.Skip(randomIndex).FirstOrDefault();

            var model = new QuizViewModel
            {
                TotalQuestionsCount = totalQuestions,
                Question = randomQuestion
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

            // Retrieve the current question
            var question = context.Questions.FirstOrDefault(q => q.Id == model.QuestionId);
            if (question == null)
            {
                ModelState.AddModelError("", "Invalid question.");
                return View(model);
            }

            // Check the user's answer
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

            // Check if quiz is complete
            if (answeredIds.Count == model.TotalQuestionsCount)
            {
                model.IsComplete = true;
                model.FeedbackMessage = "Congratulations! You've completed the quiz!";
                model.CorrectAnswersCount = currentCorrectAnswersCount;
                return View(model);
            }

            // Get a random next question that has not been answered
            var remainingQuestionsCount = context.Questions.Count() - answeredIds.Count;
            if (remainingQuestionsCount > 0)
            {
                var randomIndex = new Random().Next(remainingQuestionsCount); // Generate random index
                var nextQuestion = context.Questions
                    .Where(q => !answeredIds.Contains(q.Id))
                    .Skip(randomIndex)
                    .FirstOrDefault();

                if (nextQuestion != null)
                {
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

            // Fallback for unexpected cases
            TempData["QuizComplete"] = "Congratulations! You've answered all questions!";
            return RedirectToAction("Index");
        }




    }
}
