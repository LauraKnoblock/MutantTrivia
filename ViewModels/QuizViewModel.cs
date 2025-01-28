using MutantTrivia.Models;

public class QuizViewModel
{
    public int QuestionId { get; set; }
    public string? QuestionText { get; set; }
    public string? UserAnswer { get; set; }
    public bool IsCorrect { get; set; }
    public int CorrectAnswersCount { get; set; }
    public int TotalQuestionsCount { get; set; }
    public int SelectedQuestionCount { get; set; }
    public int CurrentQuestionNumber { get; set; }
    public Question? Question { get; set; }
    public string? FeedbackMessage { get; set; }
    public bool IsComplete { get; set; }
    public QuizSearchModel SearchModel { get; set; } = new QuizSearchModel();

    public List<AnsweredQuestion> AnsweredQuestions { get; set; } = new List<AnsweredQuestion>();

    public class AnsweredQuestion
    {
        public string? QuestionText { get; set; }
        public string? UserAnswer { get; set; }
        public string? CorrectAnswer { get; set; }
        public bool IsCorrect { get; set; }
    }
}

public class QuizSearchModel
{
    public int? CategoryId { get; set; }
}