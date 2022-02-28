using DatingApp.Extension;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace DatingApp.Helpers
{
	public class LogUserActivity : IAsyncActionFilter
	{
		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var resultContext = await next();
			if (!resultContext.HttpContext.User.Identity.IsAuthenticated) return;
			var userId = resultContext.HttpContext.User.GetUserId();
			var repo = resultContext.HttpContext.RequestServices.GetService<IUserRepository>();
			var user = await repo.GetUserByIdAsync(userId);
			user.LastActive = DateTime.Now;
			await repo.SaveAllAsync();
		}
	}
}
