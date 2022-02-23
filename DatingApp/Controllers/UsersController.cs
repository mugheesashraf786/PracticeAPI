using AutoMapper;
using DatingApp.Data;
using DatingApp.DTO;
using DatingApp.Entities;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DatingApp.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
		private readonly IUserRepository _userRepository;
		private readonly IMapper _mapper;

		public UsersController( IUserRepository userRepository, IMapper mapper)
        {
			_userRepository = userRepository;
			_mapper = mapper;
		}

        [HttpGet]
        public async Task <ActionResult<IEnumerable<MemberDTO>>> GetUsers()
        {
            var users = await _userRepository.GetMemberssAsync();
                return Ok(users);

        }

     
        ///api/users/3
        [HttpGet("{username}")]
        public async Task <ActionResult<MemberDTO>> GetUser(string username)
        {
           return await _userRepository.GetMemberAsync(username);
           
          
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userRepository.GetUserByUsernameAsync(username);

            _mapper.Map(memberUpdateDto, user);

            _userRepository.Update(user);

            if (await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");
        }
    }
}