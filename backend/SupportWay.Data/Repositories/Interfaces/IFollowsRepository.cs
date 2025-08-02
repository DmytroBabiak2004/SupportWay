using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface IFollowRepository
    {
        Task FollowUserAsync (string followerId, string followedId); 
        Task UnfollowUserAsync (string followerId, string followedId);
        Task<bool> IsFollowingAsync (string followerId, string followedId);
        Task<int> GetFollowersCountAsync(string userId);
        Task<int> GetFollowingCountAsync(string userId);

    }
}
