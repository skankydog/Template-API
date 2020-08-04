using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        
        public DatingRepository(DataContext context)
        {
            _context = context;
        }

        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<User> GetUser(int Id)
        {
            //var user = await _context.Users.Include(u => u.Photos).FirstOrDefaultAsync(u => u.Id == Id); using lazy loading - removed the include
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == Id);

            return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            //var users = _context.Users.Include(u => u.Photos) Removed include as now using lazy loading
            //    .OrderByDescending(u => u.LastActive).AsQueryable();
            var users = _context.Users.OrderByDescending(u => u.LastActive).AsQueryable();
            users = users.Where(u => u.Id != userParams.UserId);
            users = users.Where(u => u.Gender == userParams.Gender);

            if (userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserId, true);
                users = users.Where(u => userLikers.Contains(u.Id));
            }

            if (userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserId, false);
                users = users.Where(u => userLikees.Contains(u.Id));
            }

            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);
            var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);

            users = users.Where(u => u.DateOfBirth >= minDob);
            users = users.Where(u => u.DateOfBirth <= maxDob);

            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.Created);
                        break;

                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }

            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            var user = await _context.Users
              //  .Include(u => u.Likers) using lazy loading
              //  .Include(u => u.Likees)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (likers)
            {
                return user.Likers.Where(u => u.LikeeId == id).Select(u => u.LikerId);
            }
            else
            {
                return user.Likees.Where(u => u.LikerId == id).Select(u => u.LikeeId);
            }
        }
        public async Task<Photo> GetFirstPhoto(int userId)
            {
            var user = await _context.Users.Include(u => u.Photos).FirstOrDefaultAsync(u => u.Id == userId);
            var photo = user.Photos.FirstOrDefault();

            return(photo);
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);

            return photo;
        }

        public async Task<Photo> GetMainPhotoForUser(int Id)
        {
            return await _context.Photos.Where(u => u.UserId == Id).FirstOrDefaultAsync(p => p.IsMain);
        }

        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await _context.Likes.FirstOrDefaultAsync(u => u.LikerId == userId && u.LikeeId == recipientId);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            // removed - now using lazy loading...
         //   var messages = _context.Messages.Include(u => u.Sender).ThenInclude(u => u.Photos)
         //       .Include(u => u.Recipient).ThenInclude(u => u.Photos).AsQueryable();
            var messages = _context.Messages.AsQueryable();

            switch (messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(u => u.RecipientId == messageParams.UserId && u.RecipientDeleted == false);
                    break;

                case "Outbox":
                    messages = messages.Where(u => u.SenderId == messageParams.UserId && u.SenderDeleted == false);
                    break;

                default: // "Unread"
                    messages = messages.Where(u => u.RecipientId == messageParams.UserId && u.IsRead == false
                        && u.RecipientDeleted == false);
                    break;
            }

            messages = messages.OrderByDescending(m => m.MessageSent);

            return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
            var loggedInUserId = userId;
            var browsedUserId = recipientId;

            // removed includes... now lazy loading
            //var messages = await _context.Messages.Include(u => u.Sender).ThenInclude(u => u.Photos)
            //    .Include(u => u.Recipient).ThenInclude(u => u.Photos)
            var messages = await _context.Messages
                .Where(m => (m.RecipientId == loggedInUserId && m.SenderId == browsedUserId && m.RecipientDeleted == false) || 
                            (m.RecipientId == browsedUserId && m.SenderId == loggedInUserId && m.SenderDeleted == false))
                .OrderByDescending(m => m.MessageSent)
                .ToListAsync();

                return messages;
        }
    }
}