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
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using CourseProject.Helpers;
using CourseProject.Services;
using AutoMapper;
using Microsoft.Extensions.Options;

namespace CourseProject.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly CourseProjectContext _context;
        //private IUserService _userService;
        private readonly AppSettings _appSettings;
        //private IMapper _mapper;

        public UsersController(CourseProjectContext context, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _appSettings = appSettings.Value;
        }




        /** POST: api/users/authenticate **  
        * @desc: authenticates a user and then createas a JSON web token
        * @params: model Authenticate(string Email, string Password) 
        * @returns: Status Code 400 - Error message / Status Code 200 - the authenticated User with created token
        */
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]Authenticate model)
        {
            var user = Authenticate(model.Email, model.Password);

            if (user == null)
                return BadRequest(new { status = "error", message = "Username or password is incorrect" });

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserId.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // return basic user info and authentication token
            return Ok(new
            {
                status = "success",
                Id = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Token = tokenString
            });
        }


        /** POST: api/users/register **  
        * @desc: registers a new user - hashes and salts the inputted password - saves to DB
        * @params: model Register(string Name, string Email, string Password) - the model to be used
        * @returns: status 400 and error msg / status 200 and info about the created user as JSON object
        */
        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody]Register model)
        {
            // validation
            if (string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { status = "error", message = "Password is required" });

            if (_context.Users.Any(x => x.Email == model.Email))
                return BadRequest(new { status = "error", message = "Email \"" + model.Email + "\" is already taken" });


            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(model.Password, out passwordHash, out passwordSalt);

            User user = new User();
            user.Name = model.Name;
            user.Email = model.Email;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _context.Users.Add(user);
            _context.SaveChanges();
            var latestId = user.UserId;

            return Ok(new
            {
                status = "success",
                message = "New user created",
                Name = user.Name,
                Email = user.Email
            });
        }


        /** GET: api/Users **  
        * @desc: Fetches all users from DB, includes the user's Quizzes (if has any, otherwise empty array)
        * @params: 
        * @returns: Returns a list of all the users (userid, email and quizzes (if any) )
        */
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Quiz)
                .Select(u => new { u.UserId, u.Name, u.Email, u.Quiz })
                .ToListAsync();

            return Ok(users);
        }

        /* GET: api/Users/1 *
        * @desc: Fetches specific user from DB, includes the user's Quizzes (if has any, otherwise empty array)
        * @params: int id, the user Id
        * @returns: Status code 404 if not found / otherwise a list of the user (userid, email and quizzes (if any) )
        */
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users
                            .Select(u => new { u.UserId, u.Name, u.Email })
                            .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound(new { status = "error", message = "No user with that ID found" });
            }

            var quiz = await _context.Quiz
                        .Include(q => q.Questions)
                        .Where(q => q.UserId == id)
                        .Select(q => new { q.QuizId, q.Title, q.Description, q.Questions })
                        .ToListAsync();

            return Ok(new
            {
                user.UserId,
                user.Name,
                user.Email,
                quiz
            });
        }




        // PUT: api/Users/5
        // [HttpPut("{id}")]
        // public async Task<IActionResult> PutUser(int id, User user)
        // {
        //     if (id != user.UserId)
        //     {
        //         return BadRequest();
        //     }

        //     _context.Entry(user).State = EntityState.Modified;

        //     try
        //     {
        //         await _context.SaveChangesAsync();
        //     }
        //     catch (DbUpdateConcurrencyException)
        //     {
        //         if (!UserExists(id))
        //         {
        //             return NotFound();
        //         }
        //         else
        //         {
        //             throw;
        //         }
        //     }

        //     return NoContent();
        // }



        // DELETE: api/Users/5
        // [HttpDelete("{id}")]
        // public async Task<ActionResult<User>> DeleteUser(int id)
        // {
        //     var user = await _context.Users.FindAsync(id);
        //     if (user == null)
        //     {
        //         return NotFound();
        //     }

        //     _context.Users.Remove(user);
        //     await _context.SaveChangesAsync();

        //     return user;
        // }



        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }





        /* ** HELPER METHODS ** */



        /*
        * @desc: authenticates a user login attempt - check if user email exists in DB and then verifies password 
        * @params: string email, string password 
        * @returns: null if not passed auth / the user object if passed auth
        */
        public User Authenticate(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return null;

            var user = _context.Users.SingleOrDefault(x => x.Email == email);

            // check if username exists
            if (user == null)
                return null;

            // check if password is correct
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            // authentication successful
            return user;
        }



        /*
        * @desc: creates a hashed and salted password
        * @params: string password - the inputted password that needs to be hashed
        * @returns: hashed and salted password
        */
        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }



        /*
        * @desc: verifies password - compares the input password to the hashed and salted passwords
        * @params: string password - the input password to be compared with
        * @returns: falses if not passed verification / true if passed
        */
        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }



    }



}


