﻿using System;
using System.Linq;
using System.Security.Authentication;
using BirdyAPI.DataBaseModels;
using BirdyAPI.Tools.Exceptions;
using BirdyAPI.Types;

namespace BirdyAPI.Services
{
    public class AccessService
    {
        private readonly BirdyContext _context;

        public AccessService(BirdyContext context)
        {
            _context = context;
        }

        public void CheckChatUserAccess(int userId, int chatNumber, ChatStatus statusToCheck)
        {
            ChatUser currentUserChat = _context.ChatUsers.SingleOrDefault(k => k.ChatNumber == chatNumber && k.UserInChatID == userId);
            if (currentUserChat == null || currentUserChat.Status < statusToCheck)
                throw new InsufficientRightsException();
        }

        public int ValidateToken(Guid token)
        {
            UserSession currentSession = _context.UserSessions.Find(token);
            if (currentSession == null)
                throw new AuthenticationException();

            return currentSession.UserId;
        }
    }
}
