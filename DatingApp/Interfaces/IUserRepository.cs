using DatingApp.DTO;
using DatingApp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.Interfaces
{
	public interface IUserRepository
	{
		void Update(AppUser user);
		Task<bool> SaveAllAsync();
		Task<IEnumerable<AppUser>> GetUserAsync();
		Task<AppUser> GetUserByIdAsync(int id);
		Task<AppUser> GetUserByUsernameAsync(string username);
		Task<IEnumerable<MemberDTO>> GetMemberssAsync();
		Task<MemberDTO> GetMemberAsync(string username);
	}
}
