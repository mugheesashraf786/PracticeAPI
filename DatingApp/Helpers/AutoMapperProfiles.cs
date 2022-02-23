using AutoMapper;
using DatingApp.DTO;
using DatingApp.Entities;
using DatingApp.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.Helpers
{
	public class AutoMapperProfiles : Profile
	{
		public AutoMapperProfiles()
		{
			CreateMap<AppUser, MemberDTO>()
				.ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.Photos
				.FirstOrDefault(x => x.IsMain).Url))
				.ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.
														 DateOfBirth.CalculateAge()));
			CreateMap<Photo, PhotoDTO>();
			CreateMap<MemberUpdateDto, AppUser>();
		}
	}
}
