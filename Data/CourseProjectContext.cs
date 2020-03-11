using Microsoft.EntityFrameworkCore;
using CourseProject.Models;

namespace CourseProject.Data
{
    public class CourseProjectContext : DbContext
    {
        public CourseProjectContext(DbContextOptions<CourseProjectContext> options)
            : base(options)
        {
        }

        public DbSet<TodoItem> TodoItems { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<CourseProject.Models.Quiz> Quiz { get; set; }
        public DbSet<Question> Question { get; set; }
    }
}

