using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CourseProject.Models
{
    public class Quiz
    {
        public int QuizId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }
        
        public List<Question> Questions { get; set; }

        public int UserId { get; set; } // FK to User
        public User User { get; set; }
    }

}