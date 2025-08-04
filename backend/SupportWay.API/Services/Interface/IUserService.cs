using SupportWay.API.DTOs;

namespace SupportWay.Services
{
    public interface IUserService
    {
        public Task<List<UserSearchDto>> SearchUsersByNameAsync(string name);
    }
}
