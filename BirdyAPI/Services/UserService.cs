﻿using System;
using System.Linq;
using BirdyAPI.DataBaseModels;
using BirdyAPI.Dto;
namespace BirdyAPI.Services
{
    public class UserService
    {
        private readonly BirdyContext _context;

        public UserService(BirdyContext context)   
        {
            _context = context;
        }
        public UserAccountDto GetUserInfo(int userId)
        {
            User user = _context.Users.Find(userId);

            return UserAccountDto.Create(user);
        }

        public int GetUserIdByUniqueTag(string uniqueTag)
        {
            User currentUser = _context.Users.SingleOrDefault(k => k.UniqueTag == uniqueTag);
            if (currentUser == null)
                throw new ArgumentException("User not found");

            return currentUser.Id;
        }
    }
}
