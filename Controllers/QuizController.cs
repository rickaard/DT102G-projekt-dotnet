using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CourseProject.Data;
using CourseProject.Models;
using Microsoft.AspNetCore.Authorization;

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

        /** GET: api/Quiz/ **  
        * @desc: Fetches every Quiz with related Questions from DB
        * @params: 
        * @returns: Every Quiz with related Questions as JSON object
        */
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Quiz>>> GetQuiz()
        {
            try
            {
                var quizzes = await _context.Quiz
                .Include(q => q.Questions)
                .Include(u => u.User)
                .Select(q => new { q.QuizId, q.Title, q.Description, q.Questions, q.UserId, q.User.Name })
                .ToListAsync();

                return Ok(quizzes);
            }
            catch (Exception error)
            {

                return BadRequest(new { message = error });
            }

        }


        /** GET: api/Quiz/5 **  
        * @desc: Fetches specific quiz and includes related Questions from DB
        * @params: int id - the specific QuizId
        * @returns: Either Status Code 404 / Status Code 200 with correct Quiz as JSON obj
        */
        [HttpGet("{id}")]
        public async Task<ActionResult<Quiz>> GetQuiz(int id)
        {
            var quiz = await _context.Quiz
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.QuizId == id);

            if (quiz == null)
            {
                return NotFound(new { status = "error", message = "No quiz with that ID found" });
            }

            var questions = await _context.Question
                            .Where(q => q.QuizId == id)
                            .Select(q => new { q.QuizId, q.QuestionId, q.QuestionText, q.CorrectAnswer, q.AlternativOne, q.AlternativTwo, q.AlternativThree, q.AlternativFour })
                            .ToListAsync();


            return Ok(new
            {
                quiz.QuizId,
                quiz.UserId,
                quiz.Title,
                quiz.Description,
                questions
            });

        }



        /** PUT: api/Quiz/5 **  
        * @desc: Updates a specific quiz and the questions related to it
        * @params: int id, Quiz quiz - id of the quiz and Quiz model
        * @returns: Status code 400 if wrong ID / Status code of 204 if successfull 
        */
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuiz(int id, Quiz quiz)
        {
            if (id != quiz.QuizId)
            {
                return BadRequest(new { message = "fel quiz id" });
            }

            _context.Update(quiz);

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
        * @desc: Add's new entries in DB - a new Quiz and Questions related to that Quiz
        * @params: model AddQuiz - the model to be used 
        * @returns: 202 status code and JSON object  
        */
        [Authorize]
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

            return Ok(new
            {
                status = "success",
                message = "New quiz created!",
                quizId = theQuiz.QuizId,
                title = theQuiz.Title,
                description = theQuiz.Description
            });
        }




        /** DELETE: api/Quiz/5 **  
        * @desc: Delete specific Quiz with related Questions from DB
        * @params: int id - ID on the Quiz to be deleted
        * @returns: JSON object and status code 200
        */
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Quiz>> DeleteQuiz(int id)
        {
            var quiz = await _context.Quiz.FindAsync(id);
            if (quiz == null)
            {
                return NotFound(new { status = "error", message = "No quiz with that ID found" });
            }

            _context.Quiz.Remove(quiz);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = "success",
                message = "Quiz has been deleted",
                quiz.QuizId,
                quiz.Title
            });
        }

        private bool QuizExists(int id)
        {
            return _context.Quiz.Any(e => e.QuizId == id);
        }
    }
}
