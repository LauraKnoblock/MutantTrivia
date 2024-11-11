namespace MutantTrivia.ViewModels
{
    public class QuestionListViewModel
    {
        public QuestionSearchViewModel SearchModel { get; set; }
        public List<MutantTrivia.Models.Question> Questions { get; set; }

        public QuestionListViewModel()
        {
            SearchModel = new QuestionSearchViewModel();
            Questions = new List<MutantTrivia.Models.Question>();
        }
    }
}
