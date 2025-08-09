using SupportWay.API.DTOs;
using SupportWay.Data.Context;
using SupportWay.Data.Repositories.Interfaces;
using SupportWay.Repositories;

namespace SupportWay.Services
{
    public class UserService : IUserService
    {
        private readonly IUsersRepository _usersRepository;
       

        public UserService(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }
        public async Task<List<UserSearchDto>> SearchUsersByNameAsync(string name)
        {
            var users = await _usersRepository.SearchUsersByNameAsync(name);

            return users.Select(u => new UserSearchDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Photo = u.Profile.Photo
            }).ToList();
        }
    }
}
