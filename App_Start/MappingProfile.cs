using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using ChatterboxAPI.Models;
using ChatterboxAPI.DTOs;

namespace ChatterboxAPI.App_Start
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            Mapper.CreateMap<Member, MemberDTO>();
            Mapper.CreateMap<MemberDTO, Member>().ForMember(c => c.Id, opt => opt.Ignore());

            Mapper.CreateMap<Member, MemberNoRoomDTO>();
            Mapper.CreateMap<MemberNoRoomDTO, Member>().ForMember(c => c.Id, opt => opt.Ignore());
            Mapper.CreateMap<Member, AuthorDTO>();
            Mapper.CreateMap<AuthorDTO, Member>().ForMember(c => c.Id, opt => opt.Ignore());
            

            Mapper.CreateMap<Message, MessageDTO>();
            Mapper.CreateMap<MessageDTO, Message>().ForMember(m => m.Id, opt => opt.Ignore());
            Mapper.CreateMap<Room, RoomDTO>();
            Mapper.CreateMap<RoomDTO, Room>().ForMember(r => r.Id, opt => opt.Ignore());
            Mapper.CreateMap<Room, RoomNoMemberDTO>();
            Mapper.CreateMap<RoomNoMemberDTO, Room>().ForMember(r => r.Id, opt => opt.Ignore());
        }
    }
}