using System.ComponentModel.DataAnnotations;

namespace CourseProject.Models
{
    // Authenticate Model used to define the params for incoming POST to /users/authenticate
    public class Authenticate
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }

    // Register Model used to define the params for incoming POST to /users/register
    public class Register
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class SimpleUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}