using Microsoft.AspNetCore.Identity;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;

public class ChatService : IChatService
{
    private readonly IChatsRepository _repo;
    private readonly UserManager<User> _userManager;

    public ChatService(IChatsRepository repo, UserManager<User> userManager)
    {
        _repo = repo;
        _userManager = userManager;
    }

    public async Task<IEnumerable<Chat>> GetChatsByUserIdAsync(string userId)
    {
        return await _repo.GetChatsByUserIdAsync(userId);
    }

    public async Task<Chat?> GetByIdAsync(Guid id)
    {
        return await _repo.GetByIdAsync(id);
    }
    public async Task<ChatDto> AddChatAsync(string user1Id, string user2Id)
    {
        var user1 = await _userManager.FindByIdAsync(user1Id);
        var user2 = await _userManager.FindByIdAsync(user2Id);

        if (user1 == null || user2 == null)
            throw new Exception("User not found");

        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Name = $"{user1.UserName} & {user2.UserName}",
            StartedAt = DateTime.UtcNow,
            UserChats = new List<UserChat>
        {
            new UserChat { UserId = user1.Id },
            new UserChat { UserId = user2.Id }
        }
        };

        await _repo.AddAsync(chat);

        return new ChatDto
        {
            Id = chat.Id,
            Name = chat.Name,
            StartedAt = chat.StartedAt
        };
    }


    public async Task DeleteChatAsync(Guid chatId)
    {
        var chat = await _repo.GetByIdAsync(chatId);
        if (chat == null) return;

        await _repo.DeleteAsync(chat);
    }

    public async Task<bool> IsUserInChatAsync(Guid chatId, string userId)
    {
        return await _repo.IsUserInChatAsync(chatId, userId);
    }
}
