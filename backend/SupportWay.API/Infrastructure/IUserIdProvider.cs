using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SupportWay.API.Infrastructure
{
	public class SignalRUserIdProvider : IUserIdProvider
	{
		public string? GetUserId(HubConnectionContext connection)
		{
			return connection.User?.FindFirstValue(ClaimTypes.NameIdentifier)
				?? connection.User?.FindFirstValue("nameid")
				?? connection.User?.FindFirstValue("sub");
		}
	}
}
