public class QuizViewModel
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; }
    public string UserAnswer { get; set; }
    public bool IsCorrect { get; set; }
    public int CorrectAnswersCount { get; set; }
    public int TotalQuestions { get; set; }
    public string CorrectAnswer { get; set; }

   
}