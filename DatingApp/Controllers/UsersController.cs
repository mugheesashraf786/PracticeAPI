﻿using AutoMapper;
using DatingApp.Data;
using DatingApp.DTO;
using DatingApp.Entities;
using DatingApp.Extension;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
		private readonly IPhotoService _photoService;

		public UsersController( IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
		{
			_userRepository = userRepository;
			_mapper = mapper;
			_photoService = photoService;
		}

		[HttpGet]
		public async Task <ActionResult<IEnumerable<MemberDTO>>> GetUsers()
		{
			var users = await _userRepository.GetMemberssAsync();
				return Ok(users);

		}

	 
		///api/users/3
		[HttpGet("{username}",Name ="GetUser")]
		public async Task <ActionResult<MemberDTO>> GetUser(string username)
		{
		   return await _userRepository.GetMemberAsync(username);
		   
		  
		}

		[HttpPut]
		public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
		{
			var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

			_mapper.Map(memberUpdateDto, user);

			_userRepository.Update(user);

			if (await _userRepository.SaveAllAsync()) return NoContent();

			return BadRequest("Failed to update user");
		}

		[HttpPost("add-photo")]
		public async Task<ActionResult<PhotoDTO>>AddPhoto(IFormFile file)
		{
			var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
			var result = await _photoService.AddPhotoAsync(file);
			if (result.Error != null) return BadRequest(result.Error.Message);

			var photo = new Photo
			{
				Url = result.SecureUrl.AbsoluteUri,
				PublicId = result.PublicId
			};
			if(user.Photos.Count==0)
			{
				photo.IsMain = true;
			}
			user.Photos.Add(photo);
			if (await _userRepository.SaveAllAsync())
			{
				//return _mapper.Map<PhotoDTO>(photo);
				return CreatedAtRoute("GetUser",new { username =user.UserName}  , _mapper.Map<PhotoDTO>(photo));
			}
				
			return BadRequest("Problem Adding Photo");

		}

		[HttpPut("set-main-photo/{photoId}")]
		public async Task <ActionResult>SetMainPhoto(int photoId)
		{
			var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
			var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
			if (photo.IsMain) return BadRequest("tHIS IS ALREADY your main phto");
			var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
			if (currentMain != null) currentMain.IsMain = false;
			photo.IsMain = true;

			if (await _userRepository.SaveAllAsync()) return NoContent();
			return BadRequest("Failed to set Main Photo");
		}

		[HttpDelete("delete-photo/{photoId}")]
		public async Task<ActionResult>DeletePhoto(int photoId)
		{
			var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
			var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
			if (photo == null) return NotFound();
			if (photo.IsMain) return BadRequest("YouCannot delete main photo");
			if (photo.PublicId != null)
			{
				var result=await _photoService.DeletePhotoAsync(photo.PublicId);
				if (result.Error != null) return BadRequest(result.Error.Message);

			}

			user.Photos.Remove(photo);

			if (await _userRepository.SaveAllAsync()) return Ok();
			return BadRequest("Failed to Delete");
		}
	}
}