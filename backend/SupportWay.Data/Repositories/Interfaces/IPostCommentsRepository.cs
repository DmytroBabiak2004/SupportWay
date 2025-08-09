using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SupportWay.Data.Models;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface IPostCommentsRepository
    {
        Task <IEnumerable<PostComment>> GetCommentsByPostAsync(Guid postId);
        Task <IEnumerable <PostComment>> GetCommentsByUserAsync(string userId);
        Task<PostComment> GetCommentByIdAsync(Guid id);
        Task AddCommentAsync(PostComment comment);
        Task DeleteCommentAsync(Guid id);
    }
}
