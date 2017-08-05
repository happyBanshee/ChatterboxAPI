using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using ChatterboxAPI.Models;
using ChatterboxAPI.DTOs;
using AutoMapper;
using System.Web.Http.Cors;
using System.Data.Entity;


namespace ChatterboxAPI.Controllers
{
    [RoutePrefix("api/rooms")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]

    public class RoomsController : ApiController
    {
        private ApplicationDbContext _context;
        [NonAction]
        private IEnumerable<Link> CreateLinks(RoomDTO room)
        {
            var links = new [] {

                new Link{
                    Href = Url.Link("GetRoom", new { id=room.Id}),
                    Rel = "self",
                    Method ="GET"
                },
                  new Link{
                    Href = Url.Link("UpdateRoom", new { id=room.Id}),
                    Rel = "self",
                    Method ="PUT"
                },
                  new Link{
                        Href = Url.Link("DeleteRoom", new { id=room.Id}),
                    Rel = "self",
                    Method ="DELETE"
                  },
                new Link{
                   Href = Url.Link("CreateRoom", new {}),
                    Rel = "self",
                    Method ="POST"
                },
                new Link{
                    Href = Url.Link("GetRoomMembers", new {id=room.Id }),
                    Rel = "members",
                    Method ="GET"
                }
            };

            return links;
        }
        [NonAction]
        private void ThrowNotFoundException(string itemName, int id)
        {
            var resp = new HttpResponseMessage(HttpStatusCode.NotFound) {
                Content = new StringContent(string.Format("No " + itemName + " with ID = {0}", id)),
                ReasonPhrase = "ID " + itemName + " Not Found"
            };
            throw new HttpResponseException(resp);

        }

        public RoomsController()
        {
            _context = new ApplicationDbContext();
        }
        /// <summary>
        /// Get all chats in DB.
        /// </summary>
        /// <returns>List of chat objects.</returns>
        [HttpGet, Route("")]
        [ResponseType(typeof(IEnumerable<RoomNoMemberDTO>))]
        public IHttpActionResult GetRooms()
        {
            var roomCollection = _context.Rooms.ToList();
            var roomDtoCollection = _context.Rooms.ToList().Select(Mapper.Map<Room, RoomNoMemberDTO>);
            //var roomDtoCollectionMapped = roomDtoCollection.Select(r => {
            //    r.Links = CreateLinks(r);
            //    return r;
            //});
            return Ok(roomDtoCollection);
        }
        /// <summary>
        /// Get chat.
        /// </summary>
        /// <param name="id">Chat id.</param>
        /// <returns>Chat object.</returns>
        [HttpGet, Route("{id:int}")]
        [ResponseType(typeof(RoomDTO))]
        public IHttpActionResult GetRoom(int id)
        {
            Room room = _context.Rooms.SingleOrDefault(r => r.Id == id);

            if(room == null)
            {
                ThrowNotFoundException("room", id);
            }

            RoomDTO roomDTO = Mapper.Map<Room, RoomDTO>(room);

            //IEnumerable<Link> links = CreateLinks(roomDTO);
            // roomDTO.Links = links;

            return Ok(roomDTO);
        }
        /// <summary>
        /// Update chat data.
        /// </summary>
        /// <param name="id">Chat id.</param>
        /// <param name="roomDTO">New data.</param>
        /// <returns></returns>
        [HttpPut, Route("{id}", Name = "UpdateRoom")]
        public IHttpActionResult UpdateRoom(int id, [FromBody]RoomDTO roomDTO)
        {
            if(!ModelState.IsValid)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            Room room = _context.Rooms.SingleOrDefault(r => r.Id == id);

            if(room == null)
            {
                ThrowNotFoundException("room", id);
            }

            var updatedRoom = Mapper.Map<RoomDTO, Room>(roomDTO, room);
            _context.SaveChanges();


            return Ok();
        }
        /// <summary>
        /// Remove chat from DB.
        /// </summary>
        /// <param name="id">Chat id.</param>
        /// <returns>Status code 200 OK.</returns>
        [HttpDelete, Route("{id}", Name = "DeleteRoom")]
                public IHttpActionResult DeleteRoom(int id)
        {

            Room room = _context.Rooms.SingleOrDefault(r => r.Id == id);

            if(room == null)
            {
                ThrowNotFoundException("room", id);
            }

            _context.Rooms.Remove(room);
            _context.SaveChanges();

            return Ok();

        }
        /// <summary>
        /// Add new chat.
        /// </summary>
        /// <param name="roomDTO">Object type RoomDTO.</param>
        /// <returns>Status code 201 Created.</returns>
        [HttpPost, Route("", Name = "CreateRoom")]
        [ResponseType(typeof(RoomDTO))]
        public IHttpActionResult CreateRoom(RoomDTO roomDTO)
        {
            if(!ModelState.IsValid)
            {

                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var room = Mapper.Map<RoomDTO, Room>(roomDTO);
            _context.Rooms.Add(room);
            _context.SaveChanges();

            roomDTO.Id = room.Id;

            return Created(new Uri(Request.RequestUri + "/" + room.Id), roomDTO);
        }
        /// <summary>
        /// Get members of the chat.
        /// </summary>
        /// <param name="id">Chat id.</param>
        /// <returns>List of object type MemberNoRoomDTO.</returns>
        [HttpGet, Route("{id:int}/members", Name = "GetRoomMembers")]
        [ResponseType(typeof(IEnumerable<MemberNoRoomDTO>))]
        public IHttpActionResult GetRoomMembers(int id)
        {
            Room room = _context.Rooms.SingleOrDefault(r => r.Id == id);

            if(room == null)
            {
                ThrowNotFoundException("room", id);
            }

            var allmembers = from members in _context.Members
                             from rooms in members.Rooms
                             where rooms.Id == id
                             select members;
            var roomMembers = allmembers.ToList().Select(Mapper.Map<Member, MemberNoRoomDTO>);

            return Ok(roomMembers);
        }
        /// <summary>
        /// Add user to chat.
        /// </summary>
        /// <param name="id">Chat id.</param>
        /// <param name="memberId">User Id.</param>
        /// <returns>Status code 200 OK.</returns>
        [HttpPut, Route("{id:int}/members/{memberId:int}", Name = "AddMemberToRoom")]
        [ResponseType(typeof(string))]
                public IHttpActionResult AddMemberToRoom(int id, int memberId)
        {
            var room = _context.Rooms.Include(r => r.Members).SingleOrDefault(r => r.Id == id);
            if(room == null)
            {
                ThrowNotFoundException("room", id);
            }
            var member = _context.Members.Include(r => r.Rooms).SingleOrDefault(m => m.Id == memberId);

            if(member == null)
            {
                ThrowNotFoundException("member", memberId);
            }

            var memberInRoom = room.Members.Where(m => m.Id == memberId).ToList();
            if(memberInRoom.Count() > 0)
            {
                return Ok("Room ID " + id + " already has member ID " + memberId);

            }
            
            room.Members.Add(member);
            _context.SaveChanges();

            return Ok();
            //return Ok("Members amount: " + room.Members.Count());
        }
        /// <summary>
        /// Remove user from chat.
        /// </summary>
        /// <param name="id">Chat id.</param>
        /// <param name="memberId">User id.</param>
        /// <returns>Status code 200 OK.</returns>
        [HttpDelete, Route("{id:int}/members/{memberId:int}", Name = "DeleteMemberFromRoom")]
        public IHttpActionResult DeleteMemberFromRoom(int id, int memberId)
        {
            var room = _context.Rooms.Include(r => r.Members).SingleOrDefault(r => r.Id == id);
            if(room == null)
            {
                ThrowNotFoundException("room", id);
            }
            var member = _context.Members.Include(r => r.Rooms).SingleOrDefault(m => m.Id == memberId);

            if(member == null)
            {
                ThrowNotFoundException("member", id);
            }
            room.Members.Remove(member);
            _context.SaveChanges();

            return Ok();
        }
        /// <summary>
        /// Get all messages in chat.
        /// </summary>
        /// <param name="id">Chat id.</param>
        /// <returns>List of objects type MessageDTO.</returns>
        [HttpGet, Route("{id:int}/messages", Name = "GetRoomMessages")]
        [ResponseType(typeof(IEnumerable<MessageDTO>))]

        public IHttpActionResult GetRoomMessages(int id)
        {
            Room room = _context.Rooms.SingleOrDefault(r => r.Id == id);

            if(room == null)
            {
                ThrowNotFoundException("room", id);
            }
            var allMessages = from messages in _context.Messages.Include(m => m.Author)
                              where messages.Room.Id == id
                              select messages;

            var messagesDTO = allMessages.ToList().Select(Mapper.Map<Message, MessageDTO>);

            return Ok(messagesDTO);
        }
        /// <summary>
        /// Get messages added later spesified date.
        /// </summary>
        /// <param name="id">Chat id.</param>
        /// <param name="dateTime">Date.</param>
        /// <param name="count">Return messages count.</param>
        /// <returns>List of objects type MessageDTO. Returns messages count if "count" flag is TRUE.</returns>
        [HttpGet, Route("{id:int}/messages/{count:int?}", Name = "GetMessagesFromDate")]
        [ResponseType(typeof(IEnumerable<MessageDTO>))]
        public IHttpActionResult GetMessagesFromDate(int id, DateTime dateTime, bool count = false)
        {
            var messages =
                _context.Messages.Include(m => m.Author)
                .Where(m => m.Room.Id == id)
                .Where(m => m.Date >= dateTime).ToList();
            if(count == true)
                return Ok(messages.Count);
            else
                return Ok(messages.Select(Mapper.Map<Message, MessageDTO>));
        }
        /// <summary>
        /// Update existing messaged.
        /// </summary>
        /// <param name="id">Chat id.</param>
        /// <param name="msgDTO">Object type MessageDTO.</param>
        /// <returns></returns>
        [HttpPost, Route("{id:int}/messages", Name = "PostMessage")]
        public HttpResponseMessage PostMessage(int id, MessageDTO msgDTO)
        {
            if(!ModelState.IsValid)
            {
                var errors = ModelState.Select(x => x.Value.Errors)
                          .Where(y => y.Count > 0)
                          .ToList();
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            Member author = _context.Members.SingleOrDefault(m => m.Id == msgDTO.Author.Id);
            Room room = _context.Rooms.SingleOrDefault(r => r.Id == id);
            Message message = Mapper.Map<MessageDTO, Message>(msgDTO);

            if(author == null)
            {
                ThrowNotFoundException("author", msgDTO.Author.Id);
            }
            if(author == null)
            {
                ThrowNotFoundException("room", id);
            }

            message.Author = author;
            message.Room = room;

            _context.Messages.Add(message);
            _context.SaveChanges();

            msgDTO.Id = message.Id;
            //var ur = new Uri(Request.RequestUri + "/" + message.Id);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
        /// <summary>
        /// Remove posted message.
        /// </summary>
        /// <param name="id">Chat id.</param>
        /// <param name="msgId">Message id.</param>
        /// <returns>Status code 200 OK.</returns>
        [HttpDelete, Route("{id:int}/messages", Name = "DeleteMessage")]
        public IHttpActionResult DeleteMessage(int id, int msgId)
        {
            Message msg = _context.Messages.Where(m => m.Room.Id == id).SingleOrDefault(m => m.Id == msgId);

            if(msg == null)
            {
                ThrowNotFoundException("message", id);
            }

            _context.Messages.Remove(msg);
            _context.SaveChanges();

            return Ok();
        }
    }
}
