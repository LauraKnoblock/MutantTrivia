using Microsoft.EntityFrameworkCore;
using MutantTrivia.Models;
namespace MutantTrivia.Data
{
    public class QuestionDbContext : DbContext
    {
        public DbSet<Question> Questions { get; set; } 

        public DbSet<QuestionCategory> Categories { get; set; }

        public QuestionDbContext(DbContextOptions<QuestionDbContext> options) : base(options) { }
    }
}
