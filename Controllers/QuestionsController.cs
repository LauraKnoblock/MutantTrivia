using Microsoft.AspNetCore.Mvc;
using MutantTrivia.Data;
using MutantTrivia.Models;

namespace MutantTrivia.Controllers
{
    public class QuestionsController : Controller
    {

        public IActionResult Index()
        {

            ViewBag.questions = QuestionData.GetAll();
            return View();
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [Route("Questions/Add")]
        public IActionResult NewQuestion(Question newQuestion)
        {
            QuestionData.Add(newQuestion);

            return Redirect("/Questions");
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
