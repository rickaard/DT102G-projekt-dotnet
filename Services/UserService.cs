using System;
using System.Collections.Generic;
using System.Linq;
using CourseProject.Helpers;
using CourseProject.Models;
using CourseProject.Data;

namespace CourseProject.Services
{
    public interface IUserService
    {

        IEnumerable<User> GetAll();
        User GetById(int id);

    }

    public class UserService : IUserService
    {
        private CourseProjectContext _context;

        public UserService(CourseProjectContext context)
        {
            _context = context;
        }



        public IEnumerable<User> GetAll()
        {
            return _context.Users;
        }

        public User GetById(int id)
        {
            return _context.Users.Find(id);
        }

    }
}