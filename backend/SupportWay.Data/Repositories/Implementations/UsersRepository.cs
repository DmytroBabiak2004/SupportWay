using Microsoft.EntityFrameworkCore;
using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupportWay.Data.Repositories.Implementations
{
    public class UsersRepository : IUsersRepository
    {
        private readonly SupportWayContext _context;

        public UsersRepository(SupportWayContext context)
        {
            _context = context;
        }

        public async Task<List<User>> SearchUsersByNameAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<User>();

            return await _context.Users
                .Include(u => u.Profile) 
                .Where(u =>
                    u.UserName.Contains(query) ||
                    (u.Profile != null && (
                        u.Profile.Name.Contains(query) ||
                        u.Profile.FullName.Contains(query)
                    ))
                )
                .ToListAsync();
        }
    }
}