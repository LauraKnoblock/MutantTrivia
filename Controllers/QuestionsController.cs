using Microsoft.AspNetCore.Mvc;

namespace MutantTrivia.Controllers
{
    public class QuestionsController : Controller
    {
       private static List<string> Questions = new List<string>();

        public IActionResult Index()
        {

            ViewBag.questions = Questions;
            return View();
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [Route("Questions/Add")]
        public IActionResult NewQuestion(string name)
        {
            Questions.Add(name);
            return Redirect("/Questions");
        }
    }
}
