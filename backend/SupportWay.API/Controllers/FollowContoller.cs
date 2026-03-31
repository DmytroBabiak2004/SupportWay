using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportWay.Services.Interfaces;
using System.Security.Claims;

namespace SupportWay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FollowController : ControllerBase
    {
        private readonly IFollowService _followService;

        public FollowController(IFollowService followService)
        {
            _followService = followService;
        }

        [HttpPost("{followedId}")]
        public async Task<IActionResult> Follow(string followedId)
        {
            var followerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (followerId == null) return Unauthorized();
            if (followerId == followedId) return BadRequest("You cannot follow yourself.");

            await _followService.FollowUserAsync(followerId, followedId);
            var newCount = await _followService.GetFollowersCountAsync(followedId);
            return Ok(new { followersCount = newCount });
        }

        [HttpDelete("{followedId}")]
        public async Task<IActionResult> Unfollow(string followedId)
        {
            var followerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (followerId == null) return Unauthorized();

            await _followService.UnfollowUserAsync(followerId, followedId);
            return Ok();
        }

        [HttpGet("is-following/{followedId}")]
        public async Task<IActionResult> IsFollowing(string followedId)
        {
            var followerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (followerId == null) return Unauthorized();

            var isFollowing = await _followService.IsFollowingAsync(followerId, followedId);
            return Ok(isFollowing);
        }

        [HttpGet("{userId}/followers-count")]
        public async Task<IActionResult> GetFollowersCount(string userId)
        {
            var count = await _followService.GetFollowersCountAsync(userId);
            return Ok(count);
        }

        [HttpGet("{userId}/following-count")]
        public async Task<IActionResult> GetFollowingCount(string userId)
        {
            var count = await _followService.GetFollowingCountAsync(userId);
            return Ok(count);
        }

        [HttpGet("{userId}/followers")]
        public async Task<IActionResult> GetFollowers(string userId)
        {
            var list = await _followService.GetFollowersAsync(userId);
            return Ok(list);
        }

        [HttpGet("{userId}/following")]
        public async Task<IActionResult> GetFollowing(string userId)
        {
            var list = await _followService.GetFollowingAsync(userId);
            return Ok(list);
        }

        /// <summary>Remove a follower from my followers list (only own profile)</summary>
        [HttpDelete("remove-follower/{followerUserId}")]
        public async Task<IActionResult> RemoveFollower(string followerUserId)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ownerId == null) return Unauthorized();

            await _followService.RemoveFollowerAsync(ownerId, followerUserId);
            return Ok();
        }
    }
}
