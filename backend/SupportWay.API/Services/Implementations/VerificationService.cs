using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SupportWay.API.DTOs;
using SupportWay.API.Services.Interface;
using SupportWay.Data.Context;
using SupportWay.Data.Models;

namespace SupportWay.API.Services.Implementations
{
    public class VerificationService : IVerificationService
    {
        private readonly SupportWayContext _db;
        private readonly UserManager<User> _userManager;

        public VerificationService(SupportWayContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<VerificationRequestDto?> GetMyPendingRequestAsync(string userId)
        {
            var req = await _db.VerificationRequests
                .Include(v => v.User).ThenInclude(u => u.Profile)
                .Where(v => v.UserId == userId)
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefaultAsync();
            return req == null ? null : MapToDto(req);
        }

        public async Task<Guid> SubmitRequestAsync(string userId, SubmitVerificationDto dto)
        {
            if (dto.VerificationType == VerificationType.User)
                throw new InvalidOperationException("Заявку можна подати тільки на роль волонтера або військового.");

            var hasPending = await _db.VerificationRequests
                .AnyAsync(v => v.UserId == userId && v.Status == VerificationStatus.Pending);
            if (hasPending)
                throw new InvalidOperationException("У вас вже є активна заявка на верифікацію.");

            var req = new VerificationRequest
            {
                UserId = userId,
                VerificationType = dto.VerificationType,
                Notes = dto.Notes,
                Status = VerificationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _db.VerificationRequests.Add(req);
            await _db.SaveChangesAsync();
            return req.Id;
        }

        public async Task<IEnumerable<VerificationRequestDto>> GetAllAsync(VerificationStatus? status = null)
        {
            var query = _db.VerificationRequests
                .Include(v => v.User).ThenInclude(u => u.Profile)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(v => v.Status == status.Value);

            var list = await query.OrderByDescending(v => v.CreatedAt).ToListAsync();
            return list.Select(MapToDto);
        }

        public async Task DecideAsync(Guid id, DecideVerificationDto dto, string adminId)
        {
            var req = await _db.VerificationRequests
                .Include(v => v.User).ThenInclude(u => u.Profile)
                .FirstOrDefaultAsync(v => v.Id == id)
                ?? throw new KeyNotFoundException("Заявку не знайдено.");

            req.Status = dto.Approved ? VerificationStatus.Approved : VerificationStatus.Rejected;
            req.AdminComment = dto.AdminComment;
            req.DecidedAt = DateTime.UtcNow;

            if (dto.Approved)
            {
                await ApplyApprovedRoleAsync(req.User, req.VerificationType);

                if (req.User.Profile != null)
                {
                    req.User.Profile.IsVerified = true;
                    req.User.Profile.VerifiedAs = req.VerificationType;
                }
            }

            await _db.SaveChangesAsync();
        }

        private async Task ApplyApprovedRoleAsync(User user, VerificationType verificationType)
        {
            if (!await _userManager.IsInRoleAsync(user, "User"))
                await _userManager.AddToRoleAsync(user, "User");

            var specialRoles = new[] { "Volunteer", "Military" };
            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToRemove = currentRoles.Where(r => specialRoles.Contains(r)).ToArray();
            if (rolesToRemove.Length > 0)
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            var roleToAdd = verificationType switch
            {
                VerificationType.Volunteer => "Volunteer",
                VerificationType.Military => "Military",
                _ => null
            };

            if (!string.IsNullOrWhiteSpace(roleToAdd) && !await _userManager.IsInRoleAsync(user, roleToAdd))
                await _userManager.AddToRoleAsync(user, roleToAdd);
        }

        private static VerificationRequestDto MapToDto(VerificationRequest v) => new()
        {
            Id = v.Id,
            UserId = v.UserId,
            Username = v.User?.UserName ?? string.Empty,
            PhotoBase64 = v.User?.Profile?.Photo != null ? Convert.ToBase64String(v.User.Profile.Photo) : null,
            VerificationType = v.VerificationType,
            Status = v.Status,
            Notes = v.Notes,
            AdminComment = v.AdminComment,
            CreatedAt = v.CreatedAt,
            DecidedAt = v.DecidedAt
        };
    }
}
