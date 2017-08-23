using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Http.Description;
using ChatterboxAPI.Models;
using ChatterboxAPI.DTOs;
using AutoMapper;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using System.Web.Http.Cors;
using System.Text;
using ChatterboxAPI.App_Start;

namespace ChatterboxAPI.Controllers
{
    [RoutePrefix("api/rooms")]
    [Authorize]
    public class RoomsController : ApiController
    {
        private ApplicationDbContext _context;
        private ResponseHelper responseData;
        private ResponseMessage responseMessage;

        public RoomsController()
        {
            _context = new ApplicationDbContext();
            responseData = new ResponseHelper();
            responseMessage = new ResponseMessage();
        }

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
        private Member GetMemberBasedOnUserId()
        {
            responseData.RequestHelper = Request;

            string authorizedAccountId = User.Identity.GetUserId();
            Member authorizedMember = _context.Members.SingleOrDefault(m => m.Account.Id == authorizedAccountId);

            if(authorizedMember == null)
            {
                responseData.ThrowNotFoundResponse("member", authorizedAccountId);
            }

            return authorizedMember;
        }

        /// <summary>
        /// Get chats for current user.
        /// </summary>
        /// <returns>List of chat objects.</returns>
        [HttpGet, Route("")]
        [ResponseType(typeof(IEnumerable<RoomNoMemberDTO>))]
        public IHttpActionResult GetUserRooms()
        {
            responseData.RequestHelper = Request;
            Member authorizedMember = GetMemberBasedOnUserId();

            var roomCollection = _context.Rooms.Where(r => r.Members.Contains(authorizedMember));
            var roomDtoCollection = _context.Rooms.ToList().Select(Mapper.Map<Room, RoomNoMemberDTO>);

            return Ok(roomDtoCollection);
        }
        /// <summary>
        /// Get all chats in DB.
        /// </summary>
        /// <returns>List of chat objects.</returns>
        [HttpGet, Route("all")]
        [ResponseType(typeof(IEnumerable<RoomNoMemberDTO>))]
        public IHttpActionResult GetRooms()
        {
            var roomCollection = _context.Rooms.ToList();
            var roomDtoCollection = _context.Rooms.ToList().Select(Mapper.Map<Room, RoomNoMemberDTO>);

            return Ok(roomDtoCollection);
        }

        /// <summary>
        /// Get chat.
        /// </summary>
        /// <param name="id">Chat id.</param>
        /// <returns>Chat object.</returns>
        [HttpGet, Route("{id:int}")]
        [ResponseType(typeof(RoomDTO))]
        public IHttpActionResult GetRoomData(int id)
        {
            responseData.RequestHelper = Request;

            Room room = _context.Rooms.SingleOrDefault(r => r.Id == id);
            Member member = GetMemberBasedOnUserId();

            if(room == null)
            {
                return new ResponseMessageResult(responseData.CreateNotFoundResponse("room", id));
            }

            bool authorizedUserHasAccess = !room.IsPrivate || room.Members.Select(m => m.Id == member.Id).ToList().Count() != 0;
            if(!authorizedUserHasAccess)
            {
                return new ResponseMessageResult(responseData.CreateResponse(status: HttpStatusCode.Forbidden, message: "Only members can access private chat."));
            }

            RoomNoMemberDTO roomDTO = Mapper.Map<Room, RoomNoMemberDTO>(room);

            return Ok(roomDTO);
        }
        /// <summary>
        /// Update chat data (name/description).
        /// </summary>
        /// <param name="id">Chat id.</param>
        /// <param name="roomDTO">New data.</param>
        /// <returns></returns>
        [HttpPut, Route("{id}", Name = "UpdateRoom")]
        [ResponseType(typeof(RoomNoMemberDTO))]
        public IHttpActionResult UpdateRoom(int id, [FromBody]RoomNoMemberDTO roomDTO)
        {
            responseData.RequestHelper = Request;

            if(!ModelState.IsValid)
            {
                return new ResponseMessageResult(responseData.CreateNotValidResponse(modelState: ModelState));
            }

            Room room = _context.Rooms.SingleOrDefault(r => r.Id == id);

            if(room == null)
            {
                return new ResponseMessageResult(responseData.CreateNotFoundResponse("room", id));
            }

            Member member = GetMemberBasedOnUserId();

            bool isRoomCreator = room.MemberId == member.Id;

            if(!isRoomCreator)
            {
                return new ResponseMessageResult(responseData.CreateResponse(
                    message: "Only room creator can update room data.",
                    status: HttpStatusCode.Forbidden));
            }

            var updatedRoom = Mapper.Map<RoomNoMemberDTO, Room>(roomDTO, room);
            _context.SaveChanges();

            return Ok(Mapper.Map<Room, RoomNoMemberDTO>(room));
        }

        /// <summary>
        /// Delete chat.
        /// </summary>
        /// <param name="id">Chat id.</param>
        /// <returns>Status code 200 OK.</returns>
        [HttpDelete, Route("{id}", Name = "DeleteRoom")]
        [ResponseType(typeof(ResponseMessageResult))]
        public IHttpActionResult DeleteRoom(int id)
        {
            responseData.RequestHelper = Request;
            Room room = _context.Rooms.SingleOrDefault(r => r.Id == id);

            if(room == null)
            {
                responseData.CreateNotFoundResponse("room", id);
            }

            Member member = GetMemberBasedOnUserId();

            bool isRoomCreator = room.MemberId == member.Id;

            if(!isRoomCreator)
            {
                return new ResponseMessageResult(responseData.CreateResponse(
                    message: "Only room creator can delete a room.",
                    status: HttpStatusCode.Forbidden));
            }

            _context.Rooms.Remove(room);
            _context.SaveChanges();

            return new ResponseMessageResult(responseData.CreateResponse(status: HttpStatusCode.OK, message: "Room with ID " + id + " was succesfully deleted."));
        }

        /// <summary>
        /// Add new chat.
        /// </summary>
        /// <param name="roomDTO">Object type RoomDTO.</param>
        /// <returns>Status code 201 Created.</returns>
        [HttpPost, Route("~/api/room", Name = "CreateRoom")]
        [ResponseType(typeof(RoomDTO))]
        public HttpResponseMessage CreateRoom(RoomDTO roomDTO)
        {
            responseData.RequestHelper = Request;

            if(!ModelState.IsValid)
            {
                responseData.CreateNotValidResponse(modelState: ModelState);
                // throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            string authorizedAccountId = User.Identity.GetUserId();
            Member authorizedMember = _context.Members.SingleOrDefault(m => m.Account.Id == authorizedAccountId);

            if(authorizedMember == null)
            {
                responseData.ThrowNotFoundResponse("member", authorizedAccountId);
            }

            roomDTO.MemberId = authorizedMember.Id;

            Room room = Mapper.Map<RoomDTO, Room>(roomDTO);
            room.Members.Add(authorizedMember);

            _context.Rooms.Add(room);
            _context.SaveChanges();

            roomDTO.Id = room.Id;

            return responseData.CreateResponse(status: HttpStatusCode.Created, id: room.Id);
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
            //authorization
            responseData.RequestHelper = Request;
            Room room = _context.Rooms.SingleOrDefault(r => r.Id == id);

            if(room == null)
            {
                return new ResponseMessageResult(responseData.CreateNotFoundResponse("room", id));
            }

            Member member = GetMemberBasedOnUserId();
            bool isRoomMember = room.Members.SingleOrDefault(m => m.Id == member.Id) != null;

            if(!isRoomMember)
            {
                return new ResponseMessageResult(responseData.CreateResponse(message: "Only chat members can see chat data.", status: HttpStatusCode.Forbidden));
            }

            var allmembers = from members in _context.Members
                             from rooms in members.Rooms
                             where rooms.Id == id
                             select members;
            IEnumerable<MemberNoRoomDTO> roomMembers = allmembers.ToList().Select(Mapper.Map<Member, MemberNoRoomDTO>);

            return Ok(roomMembers);
        }

        /// <summary>
        /// Join the chat.
        /// </summary>
        /// <param name="roomId">Chat id.</param>
        /// <returns>Status code 200 OK.</returns>
        [HttpPut, Route("{roomId:int}/member", Name = "AddMemberToRoom")]
        [ResponseType(typeof(string))]
        public IHttpActionResult AddMemberToRoom(int roomId)
        {
            responseData.RequestHelper = Request;

            Member authorizedMember = GetMemberBasedOnUserId();
            Room room = _context.Rooms.Include(r => r.Members).SingleOrDefault(r => r.Id == roomId);

            if(room == null)
            {
                return new ResponseMessageResult(responseData.CreateNotFoundResponse("room", roomId));
            }

            bool isAlreadyRoomMember = room.Members.Where(m => m.Id == authorizedMember.Id).ToList().Count() > 0;
            if(isAlreadyRoomMember)
            {
                return new ResponseMessageResult(responseData.CreateResponse(
                    //  message: "Room ID " + roomId + " already has member ID " + authorizedMember.Id + ".",
                    message: "Member with ID " + authorizedMember.Id + " has already joinув the chat.",

                    status: HttpStatusCode.BadRequest));
            }

            room.Members.Add(authorizedMember);
            _context.SaveChanges();

            return new ResponseMessageResult(responseData.CreateResponse(
                message: "Member with ID " + authorizedMember.Id + " was successfully added to the chat with ID " + roomId + ".",
                status: HttpStatusCode.OK));
        }

        /// <summary>
        /// Leave the chat.
        /// </summary>
        /// <param name="roomId">Chat id.</param>
        /// <returns>Status code 200 OK.</returns>
        [HttpDelete, Route("{id:int}/member", Name = "LeaveRoom")]
        [ResponseType(typeof(string))]
        public IHttpActionResult LeaveRoom(int roomId)
        {
            responseData.RequestHelper = Request;
            Room room = _context.Rooms.Include(r => r.Members).SingleOrDefault(r => r.Id == roomId);
            if(room == null)
            {
                return new ResponseMessageResult(responseData.CreateNotFoundResponse(
                    itemName: "room",
                    id: roomId));
            }

            Member member = GetMemberBasedOnUserId();
            bool isRoomMember = room.Members.SingleOrDefault(m => m.Id == member.Id) != null;

            if(!isRoomMember)
            {
                return new ResponseMessageResult(responseData.CreateNotFoundResponse(
                    itemName: "member",
                    id: roomId));
            }

            room.Members.Remove(member);
            _context.SaveChanges();

            return new ResponseMessageResult(responseData.CreateResponse(
                message: "User was successfully removed from the chat.",
                status: HttpStatusCode.OK));
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
            responseData.RequestHelper = Request;
            Room room = _context.Rooms.Include(r => r.Members).SingleOrDefault(r => r.Id == id);

            if(room == null)
            {
                return new ResponseMessageResult(responseData.CreateNotFoundResponse("room", id));
            }

            Member member = GetMemberBasedOnUserId();
            bool isChatMember = room.Members.SingleOrDefault(m => m.Id == member.Id) != null;


            if(!isChatMember)
            {
                return new ResponseMessageResult(responseData.CreateResponse(
                    message: "Only chat member can see this data.",
                    status: HttpStatusCode.Unauthorized)
                    );
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
            responseData.RequestHelper = Request;
            Room room = _context.Rooms.Include(r => r.Members).SingleOrDefault(r => r.Id == id);

            if(room == null)
            {
                return new ResponseMessageResult(responseData.CreateNotFoundResponse("room", id));
            }

            Member member = GetMemberBasedOnUserId();
            bool isChatMember = room.Members.SingleOrDefault(m => m.Id == member.Id) != null;

            if(!isChatMember)
            {
                return new ResponseMessageResult(responseData.CreateResponse(
                    message: "Only chat member can see this data.",
                    status: HttpStatusCode.Unauthorized)
                    );
            }

            var messages =
                _context.Messages.Include(m => m.Author)
                .Where(m => m.Room.Id == id)
                .Where(m => m.Date >= dateTime).ToList();

            if(count == true)
                return new ResponseMessageResult(responseData.CreateResponse(message: messages.Count.ToString(), status: HttpStatusCode.OK));
            else
                return Ok(messages.Select(Mapper.Map<Message, MessageDTO>));
        }

        /// <summary>
        /// Post message.
        /// </summary>
        /// <param name="id">Chat id.</param>
        /// <param name="msgDTO">Object type MessageDTO.</param>
        /// <returns></returns>
        [HttpPost, Route("{id:int}/messages", Name = "PostMessage")]
        [ResponseType(typeof(ResponseMessageResult))]
        public IHttpActionResult PostMessage(int id, MessageNoAuthorDTO msgDTO)
        {
            responseData.RequestHelper = Request;

            Member authorizedMember = GetMemberBasedOnUserId();
            Room room = _context.Rooms.SingleOrDefault(r => r.Id == id);

            if(room == null)
            {
                return new ResponseMessageResult(
                responseData.CreateNotFoundResponse(
                    itemName: "room",
                    id: id));
            }

            bool isChatMember = room.Members.SingleOrDefault(m => m.Id == authorizedMember.Id) != null;

            if(!isChatMember)
            {
                return new ResponseMessageResult(
                responseData.CreateResponse(
                    message: "Only chat member can access this data.",
                    status: HttpStatusCode.Forbidden));
            }

            Message message = Mapper.Map<MessageNoAuthorDTO, Message>(msgDTO);

            message.AuthorId = authorizedMember.Id;

            if(!ModelState.IsValid)
            {
                return new ResponseMessageResult(
                    responseData.CreateNotValidResponse(modelState: ModelState));
            }

            message.Room = room;

            _context.Messages.Add(message);
            _context.SaveChanges();

            msgDTO.Id = message.Id;

            return new ResponseMessageResult(
                responseData.CreateResponse(
                    status: HttpStatusCode.Created, id:
                    msgDTO.Id));
        }

        /// <summary>
        /// Delete message.
        /// </summary>
        /// <param name="id">Chat id.</param>
        /// <param name="msgId">Message id.</param>
        /// <returns>Status code 200 OK.</returns>
        [HttpDelete, Route("{id:int}/messages", Name = "DeleteMessage")]
        public IHttpActionResult DeleteMessage(int id, int msgId)
        {
            responseData.RequestHelper = Request;
            Member authorizedMember = GetMemberBasedOnUserId();
            Room room = _context.Rooms.SingleOrDefault(r => r.Id == id);

            if(room == null)
            {
                return new ResponseMessageResult(
                responseData.CreateNotFoundResponse(
                    itemName: "room",
                    id: id));
            }

            bool isChatMember = room.Members.SingleOrDefault(m => m.Id == authorizedMember.Id) != null;

            if(!isChatMember)
            {
                return new ResponseMessageResult(
                responseData.CreateResponse(
                    message: "Only chat member can access this data.",
                    status: HttpStatusCode.Forbidden));
            }

            Message msg = _context.Messages.Where(m => m.Room.Id == id).SingleOrDefault(m => m.Id == msgId);

            if(msg == null)
            {
                return new ResponseMessageResult(
                    responseData.CreateNotFoundResponse(
                        itemName: "message",
                        id: id));
            }

            bool isMessageAuthor = msg.AuthorId == authorizedMember.Id;
            if(!isMessageAuthor)
            {
                return new ResponseMessageResult(
                responseData.CreateResponse(
                    message: "Message was created by another member.",
                    status: HttpStatusCode.Forbidden));
            }

            _context.Messages.Remove(msg);
            _context.SaveChanges();

            return new ResponseMessageResult(
                responseData.CreateResponse(
                    message: "Message was successfully deleted.",
                    status: HttpStatusCode.OK));
        }
    }
}
