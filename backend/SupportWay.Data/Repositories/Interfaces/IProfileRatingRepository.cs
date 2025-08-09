using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportWay.Data.Repositories.Interfaces
{
    public interface IProfileRatingRepository
    {
        Task RateProfileAsync(string raterUserId, Guid ratedProfileId, int value);
        Task<double?> GetAverageRatingAsync(Guid ratedProfileId);
    }

}
