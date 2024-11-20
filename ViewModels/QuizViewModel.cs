using MutantTrivia.Models;

public class QuizViewModel
{
    public int QuestionId { get; set; }
    public string? QuestionText { get; set; }
    public string? UserAnswer { get; set; }
    public bool IsCorrect { get; set; }
    public int CorrectAnswersCount { get; set; }
    public int TotalQuestionsCount { get; set; }
    public Question? Question { get; set; }
    public string? FeedbackMessage { get; set; }
    public bool IsComplete { get; set; }  // Add this

}