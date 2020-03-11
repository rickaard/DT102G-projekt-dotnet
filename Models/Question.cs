using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CourseProject.Models
{
    public class Question
    {
        public int QuestionId { get; set; }

        [Required]
        public string QuestionText { get; set; }

        [Required]
        public string CorrectAnswer { get; set; }

        [Required]
        public string AlternativOne { get; set; }

        [Required]
        public string AlternativTwo { get; set; }

        [Required]
        public string AlternativThree { get; set; }
        
        [Required]
        public string AlternativFour { get; set; }

        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }
    }
}