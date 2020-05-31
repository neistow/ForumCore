﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Forum.Core.Concrete.Models;

namespace Forum.Core.Abstract.Managers
{
    public interface IUserManager : IManager
    {
        Task<User> Authenticate(string username, string password);
        Task<List<User>> GetAll();
        ValueTask<User> GetById(int id);
        User Get(int id);
        bool CreateUser(User user, string password);
        void Delete(User user);
    }
}