using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CourseProject.Models
{
    public class AddQuiz
    {
        public int UserId { get; set; }
        public Quiz Quiz { get; set; }
        public IEnumerable<Question> Question { get; set; }
    }
}