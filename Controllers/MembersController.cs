using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ChatterboxAPI.Models;
using ChatterboxAPI.DTOs;
using AutoMapper;
using System.Web.Http.Cors;
using System.Data.Entity;
using System.Web.Http.Description;

namespace ChatterboxAPI.Controllers
{
    [RoutePrefix("api/members")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]

    public class MembersController : ApiController
    {
        private ApplicationDbContext _context;
        public MembersController()
        {
            _context = new ApplicationDbContext();
        }

        private void ThrowNotFoundException(string itemName, int id)
        {
            var resp = new HttpResponseMessage(HttpStatusCode.NotFound) {
                Content = new StringContent(string.Format("No " + itemName + " with ID = {0}", id)),
                ReasonPhrase = "ID " + itemName + " Not Found"
            };
            throw new HttpResponseException(resp);
        }
        /// <summary>
        /// Get all users in the system.
        /// </summary>
        /// <returns>List of users in DB.</returns>
        [HttpGet, Route("",Name = "GetMembers")]
        [ResponseType(typeof(List<MemberNoRoomDTO>))]
        public IHttpActionResult GetMembers() {
            var members = _context.Members.ToList();

            return Ok(members.Select(Mapper.Map<Member, MemberNoRoomDTO>));
        }
        /// <summary>
        /// Get specified user info excluding rooms.
        /// </summary>
        /// <param name="id">User id</param>
        /// <returns>User details excluding rooms.</returns>
        [HttpGet, Route("{id}/", Name = "GetMemberInfo")]
        public IHttpActionResult GetMemberInfo(int id)
        {
            var member = _context.Members.SingleOrDefault(m => m.Id == id);

            if(member == null)
            {
                ThrowNotFoundException("member", id);
            }

            return Ok(Mapper.Map<Member, MemberNoRoomDTO>(member));
        }
        /// <summary>
        /// Get all chats the user participates in.
        /// </summary>
        /// <param name="id">User id</param>
        /// <returns>List of rooms.</returns>
        [HttpGet, Route("{id}/rooms", Name = "GetMemberChats")]
        public IHttpActionResult GetMemberChats(int id)
        {
            var member = _context.Members.Include(m=>m.Rooms).SingleOrDefault(m => m.Id == id);

            if(member == null)
            {
                ThrowNotFoundException("member", id);
            }

            return Ok(member.Rooms.Select(Mapper.Map<Room, RoomNoMemberDTO>));
        }
        /// <summary>
        /// [Temporary solution] Remove user from DB.
        /// </summary>
        /// <param name="id">USer id</param>
        /// <returns>Users amount left.</returns>
        [HttpDelete, Route("{id}/", Name = "DeleteMember")]
        public IHttpActionResult DeleteMember(int id)
        {
            var member = _context.Members.Include(m => m.Rooms).SingleOrDefault(m => m.Id == id);

            if(member == null)
            {
                ThrowNotFoundException("member", id);
            }

            _context.Members.Remove(member);
            _context.SaveChanges();

            return Ok("Members amount: " + _context.Members.Count());
        }
    }
}
