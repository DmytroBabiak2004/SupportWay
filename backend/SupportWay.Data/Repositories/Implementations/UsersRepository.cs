using Microsoft.EntityFrameworkCore;
using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public async Task<List<User>> SearchUsersByNameAsync(string name)
        {        
            return await _context.Users
                .Include(u => u.Profile)
                .Where(u => u.UserName.Contains(name))
                .ToListAsync(); ;
        }
    }
}
