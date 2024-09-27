using MutantTrivia.Models;

namespace MutantTrivia.Data
{
    public class QuestionData
    {
        static private Dictionary<int, Question> 
            Questions = new Dictionary<int, Question>();

        public static IEnumerable<Question> GetAll()
        {
            return Questions.Values;
        }

        // Add a new question to my Dictionary

        public static void Add(Question newQuestion)
        {
            Questions.Add(newQuestion.Id, newQuestion);
        }

        public static void Remove(int id)
        {
            Questions.Remove(id);
        }

        // Fetch specific event

        public static Question GetById(int id)
        {
            return Questions[id];
        }
    }
}
