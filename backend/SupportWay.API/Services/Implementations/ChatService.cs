using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;
using SupportWay.Services.Interfaces;

namespace SupportWay.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatsRepository _chatRepository;

        public ChatService(IChatsRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public async Task<Chat?> GetByIdAsync(int chatId)
        {
            return await _chatRepository.GetByIdAsync(chatId);
        }

        public async Task<IEnumerable<Chat>> GetChatsByUserIdAsync(string userId)
        {
            return await _chatRepository.GetChatsByUserIdAsync(userId);
        }

        public async Task AddChatAsync(Chat chat)
        {
            await _chatRepository.AddAsync(chat);
        }

        public async Task DeleteChatAsync(int chatId)
        {
            var chat = await _chatRepository.GetByIdAsync(chatId);
            if (chat is not null)
            {
                await _chatRepository.DeleteAsync(chat);
            }
        }

        public async Task<bool> IsUserInChatAsync(int chatId, string userId)
        {
            return await _chatRepository.IsUserInChatAsync(chatId, userId);
        }
    }
}
