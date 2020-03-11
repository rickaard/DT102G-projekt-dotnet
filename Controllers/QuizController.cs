using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CourseProject.Data;
using CourseProject.Models;

namespace CourseProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly CourseProjectContext _context;

        public QuizController(CourseProjectContext context)
        {
            _context = context;
        }

        // GET: api/Quiz
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Quiz>>> GetQuiz()
        {
            return await _context.Quiz
                .Include(q => q.Questions)
                .ToListAsync();
        }

        // GET: api/Quiz/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Quiz>> GetQuiz(int id)
        {
            var quiz = await _context.Quiz
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.QuizId == id);

            if (quiz == null)
            {
                return NotFound();
            }

            return quiz;
        }

        // GET: api/quiz/questions/1
        // [HttpGet("questions/{quizId}")]
        // public async Task<ActionResult<Quiz>> GetQuestions(int quizId)
        // {
        //     var quiz = await _context.Quiz
        //         .FirstOrDefaultAsync(q => q.QuizId == quizId);

        //     if (quiz == null)
        //     {
        //         return NotFound();
        //     }

        //     var questions = await _context.Question
        //         .Where(q => q.QuizId == quizId)
        //         .ToListAsync();

        //     return quiz;
        // }

        // PUT: api/Quiz/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuiz(int id, Quiz quiz)
        {
            if (id != quiz.QuizId)
            {
                return BadRequest();
            }

            _context.Entry(quiz).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuizExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        /** POST: api/Quiz **  
        ** method PostQuiz
        ** <params>: AddQuiz model
        ** <task>: Add's new objects to DB
        ** <returns>: Message OK and created object
        */
        [HttpPost]
        public async Task<ActionResult<Quiz>> PostQuiz(AddQuiz quiz)
        {
            var theQuiz = new Quiz();

            theQuiz.UserId = quiz.UserId;
            theQuiz.Title = quiz.Quiz.Title;
            theQuiz.Description = quiz.Quiz.Description;

            var theQuestions = new List<Question>();
            foreach (var q in quiz.Question)
            {
                theQuestions.Add(new Question
                {
                    QuestionText = q.QuestionText,
                    CorrectAnswer = q.CorrectAnswer,
                    AlternativOne = q.AlternativOne,
                    AlternativTwo = q.AlternativTwo,
                    AlternativThree = q.AlternativThree,
                    AlternativFour = q.AlternativFour
                });
            }
            theQuiz.Questions = theQuestions;

            _context.Quiz.Add(theQuiz);
            await _context.SaveChangesAsync();

            return Ok(new { message = "OK", theQuiz });
        }






        // DELETE: api/Quiz/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Quiz>> DeleteQuiz(int id)
        {
            var quiz = await _context.Quiz.FindAsync(id);
            if (quiz == null)
            {
                return NotFound();
            }

            _context.Quiz.Remove(quiz);
            await _context.SaveChangesAsync();

            return quiz;
        }

        private bool QuizExists(int id)
        {
            return _context.Quiz.Any(e => e.QuizId == id);
        }
    }
}
