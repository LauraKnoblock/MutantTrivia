using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MutantTrivia.Data;
using MutantTrivia.Models;
using MutantTrivia.ViewModels;
using FuzzySharp;

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
            // Queryable to build the query step-by-step
            var questions = context.Questions.AsQueryable();

            // Total count of all questions
            int totalQuestions = questions.Count();

            // Apply filters
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

            // Count after filtering
            int visibleQuestions = questions.Count();

            // Build the view model
            var viewModel = new QuestionListViewModel
            {
                SearchModel = searchModel,
                Questions = questions.ToList(),
                TotalQuestions = totalQuestions, // Pass total count
                VisibleQuestions = visibleQuestions // Pass filtered count
            };

            // Populate categories for the dropdown
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

            var model = new QuizViewModel
            {
                TotalQuestionsCount = totalQuestions
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult StartQuiz(int selectedQuestionCount)
        {
            var totalQuestions = context.Questions.Count();

            // Validate selected count
            if (selectedQuestionCount <= 0 || selectedQuestionCount > totalQuestions)
            {
                selectedQuestionCount = Math.Min(5, totalQuestions);
            }

            // Reset session
            HttpContext.Session.SetInt32("CorrectAnswersCount", 0);
            HttpContext.Session.Remove("AnsweredQuestionIds");

            var random = new Random();
            var randomSkip = random.Next(0, totalQuestions);
            var randomQuestion = context.Questions
                .Skip(randomSkip)
                .Take(1)
                .FirstOrDefault();

            var model = new QuizViewModel
            {
                TotalQuestionsCount = totalQuestions,
                SelectedQuestionCount = selectedQuestionCount,
                Question = randomQuestion,
                CurrentQuestionNumber = 1,
            };

            return View("Quiz", model);
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

            // Check answer
            // Check answer with FuzzySharp
            var similarity = Fuzz.PartialRatio(model.UserAnswer?.Trim().ToLower(), question.Answer?.Trim().ToLower());
            var isCorrect = similarity >= 75;
            // Update correct answers count
            var currentCorrectAnswersCount = HttpContext.Session.GetInt32("CorrectAnswersCount") ?? 0;
            if (isCorrect)
            {
                currentCorrectAnswersCount++;
                HttpContext.Session.SetInt32("CorrectAnswersCount", currentCorrectAnswersCount);
            }

            // Track answered questions
            var answeredIds = HttpContext.Session.GetString("AnsweredQuestionIds")?.Split(',')
                .Select(int.Parse).ToList() ?? new List<int>();
            answeredIds.Add(model.QuestionId);
            HttpContext.Session.SetString("AnsweredQuestionIds", string.Join(",", answeredIds));

            // Check if quiz is complete
            if (answeredIds.Count >= model.SelectedQuestionCount)
            {
                var completionModel = new QuizViewModel
                {
                    IsComplete = true,
                    CorrectAnswersCount = currentCorrectAnswersCount,
                    SelectedQuestionCount = model.SelectedQuestionCount,
                    TotalQuestionsCount = model.TotalQuestionsCount,
                    FeedbackMessage = $"Congratulations! You've completed the quiz! You got {currentCorrectAnswersCount} out of {model.SelectedQuestionCount} questions correct!"
                };
                return View(completionModel);
            }

            // Get next question
            var remainingQuestions = context.Questions
       .Where(q => !answeredIds.Contains(q.Id))
       .ToList();  // Get list of remaining questions

            var random = new Random();
            var randomIndex = random.Next(0, remainingQuestions.Count);
            var nextQuestion = remainingQuestions[randomIndex];

            model.UserAnswer = string.Empty;

            model = new QuizViewModel
            {
                TotalQuestionsCount = model.TotalQuestionsCount,
                SelectedQuestionCount = model.SelectedQuestionCount,
                CorrectAnswersCount = currentCorrectAnswersCount,
                QuestionId = nextQuestion.Id,
                Question = nextQuestion,
                CurrentQuestionNumber =  answeredIds.Count + 1,
                FeedbackMessage = isCorrect ? "CORRECT!" : "Incorrect. The correct answer was " + question.Answer
            };

            return View(model);
        }



    }
}
