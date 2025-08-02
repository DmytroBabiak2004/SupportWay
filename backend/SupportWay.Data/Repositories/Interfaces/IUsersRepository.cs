using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface IUsersRepository
    {
        Task<List<User>> SearchUsersByNameAsync(string name);
    }
}
