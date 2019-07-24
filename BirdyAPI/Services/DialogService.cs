﻿using System;
using System.Collections.Generic;
using System.Linq;
using BirdyAPI.DataBaseModels;
using BirdyAPI.Dto;

namespace BirdyAPI.Services
{
    public class DialogService
    {
        private readonly BirdyContext _context;

        public DialogService(BirdyContext context)
        {
            _context = context;
        }

        public List<DialogInfoDto> GetDialogs(int userId)
        {
            return _context.DialogUsers.Where(k => k.FirstUserID == userId)
                .Union(_context.DialogUsers.Where(k => k.SecondUserID == userId))
                .Select(k => GetDialogInfo(k.DialogID, userId)).ToList();
        }

        public DialogInfoDto GetDialogInfo(Guid dialogId, int currentUserId)
        {
            DialogUser currentDialog = _context.DialogUsers.Find(dialogId);

            Message lastMessage = _context.Messages.Where(k => k.ConversationID == dialogId)
                .OrderByDescending(k => k.SendDate).FirstOrDefault();

            return new DialogInfoDto
            {
                InterlocutorUniqueTag = GetInterlocutorUniqueTag(currentDialog, currentUserId),
                LastMessage = lastMessage?.Text,
                LastMessageTime = lastMessage?.SendDate ?? DateTime.MinValue
            };
        }
        private string GetUserUniqueTag(int userId)
        {
            return _context.Users.Find(userId).UniqueTag;
        }

        private string GetInterlocutorUniqueTag(DialogUser currentDialog ,int currentUserId)
        {
            if (currentDialog.FirstUserID == currentUserId)
                return GetUserUniqueTag(currentDialog.SecondUserID);

            return GetUserUniqueTag(currentDialog.FirstUserID);
        }
    }
}
