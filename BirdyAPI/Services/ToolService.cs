﻿using System;
using System.Linq;
using System.Security.Authentication;
using BirdyAPI.DataBaseModels;
using BirdyAPI.Tools.Exceptions;
using BirdyAPI.Types;

namespace BirdyAPI.Services
{
    public class ToolService
    {
        private readonly BirdyContext _context;

        public ToolService(BirdyContext context)
        {
            _context = context;
        }

        public int GetUserIdByUniqueTag(string uniqueTag)
        {
            User currentUser = _context.Users.SingleOrDefault(k => k.UniqueTag == uniqueTag);
            if(currentUser == null)
                throw new ArgumentException();

            return currentUser.Id;
        }

        public void GetUserConfirmed(int userId)
        {
            User currentUser = _context.Users.Find(userId);
            currentUser.CurrentStatus = UserStatus.Confirmed;
            _context.Users.Update(currentUser);
            _context.SaveChanges();
        }
        public bool IsItUserFriend(int currentUserId, int userId)
        {
            Friend currentFriend = _context.Friends.Find(userId, currentUserId);
            if (currentFriend == null)
            {
                Friend currentInverseFriend = _context.Friends.Find(currentUserId, userId);
                if (currentInverseFriend.RequestAccepted)
                    return true;

                return false;
            }

            if (currentFriend.RequestAccepted)
                return true;

            return false;
        }
    }
}
