using Microsoft.AspNetCore.Mvc;
using MutantTrivia.Data;
using MutantTrivia.Models;
using MutantTrivia.ViewModels;

namespace MutantTrivia.Controllers
{

    public class QuestionCategoryController : Controller
    {
        private QuestionDbContext context;

        public QuestionCategoryController(QuestionDbContext dbContext)
        {
            context = dbContext;
        }

        public IActionResult Index()
        {
            List<QuestionCategory> categories = context.Categories.ToList();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            AddQuestionCategoryViewModel viewModel = new AddQuestionCategoryViewModel();
            return View(viewModel);
        }

        public IActionResult ProcessCreateQuestionCategoryForm(AddQuestionCategoryViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                QuestionCategory newCategory = new QuestionCategory
                {
                    Name = viewModel.Name
                };

                context.Categories.Add(newCategory);
                context.SaveChanges();

                return RedirectToAction("Index");

            }
            return View("Create", viewModel);
        }


    }
}
