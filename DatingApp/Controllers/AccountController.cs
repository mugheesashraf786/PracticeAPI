using AutoMapper;
using DatingApp.Data;
using DatingApp.DTO;
using DatingApp.Entities;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DatingApp.Controllers
{
    public class AccountController : BaseApiController
    {
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;
		private readonly ITokenService _tokenService;
		private readonly IMapper _mapper;

		public AccountController(UserManager<AppUser> userManager,SignInManager<AppUser> signInManager, ITokenService tokenService, IMapper mapper)
        {
            
			_userManager = userManager;
			_signInManager = signInManager;
			_tokenService = tokenService;
			_mapper = mapper;
		}

        [HttpPost("register")]
        public async Task <ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
        {
            if (await UserExists(registerDTO.Username))
                return BadRequest("Username Already Taken");
            var user = _mapper.Map<AppUser>(registerDTO);
            //using var hmac = new HMACSHA512();

            user.UserName = registerDTO.Username;
            //user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password));
            //user.PasswordSalt = hmac.Key;


            var result = await _userManager.CreateAsync(user, registerDTO.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            var roleResult = await _userManager.AddToRoleAsync(user, "Member");
            if (!roleResult.Succeeded) return BadRequest(result.Errors);

            return new UserDTO
            {
                Username = user.UserName,
                Token = await _tokenService.createToken(user),
                KnownAs=user.KnownAs,
                Gender = user.Gender

            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(loginDTO loginDTO)
        {
            var user = await _userManager.Users
                .Include(p=>p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == loginDTO.Username.ToLower());
            if (user == null) return Unauthorized("Invalid Username");

            var result = await _signInManager.
                CheckPasswordSignInAsync(user, loginDTO.Password, false);

            if (!result.Succeeded) return Unauthorized();
            //using var hmac = new HMACSHA512( user.PasswordSalt);
            //var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

            //for (int i=0; i< computedHash.Length;i++)
            //{
            //    if (computedHash[i] != user.PasswordHash[i])
            //    {
            //        return Unauthorized("Invalid password");
            //    }
            //}
            return new UserDTO
            {
                Username = user.UserName,
                Token = await _tokenService.createToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }
        private async Task<bool>UserExists(string usernmae)
        {
            return await _userManager.Users.AnyAsync(x => x.UserName == usernmae.ToLower());
        }

        

    }
}
