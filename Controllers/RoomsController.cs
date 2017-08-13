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
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using System.Web.Http.Cors;
using System.Text;
using ChatterboxAPI.App_Start;

namespace ChatterboxAPI.Controllers
{
    [RoutePrefix("api/rooms")]
    [Authorize]
    [EnableCors(origins: "*", headers: "*", methods: "*")]

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

        /// <summary>
        /// Get all chats in DB.
        /// </summary>
        /// <returns>List of chat objects.</returns>
        [HttpGet, Route("")]
        [ResponseType(typeof(IEnumerable<RoomNoMemberDTO>))]
        public IHttpActionResult GetRooms()
        {
            responseData.RequestHelper = Request;
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
        [Authorize]
        [ResponseType(typeof(RoomDTO))]
        public HttpResponseMessage GetRoom(int id)
        {
            responseData.RequestHelper = Request;
            Room room = _context.Rooms.SingleOrDefault(r => r.Id == id);

            if(room == null)
            {
                return responseData.CreateNotFoundResponse("room", id);
            }

            //TODO: authorization

            RoomDTO roomDTO = Mapper.Map<Room, RoomDTO>(room);

            return responseData.CreateResponse(status: HttpStatusCode.OK, roomDTO: roomDTO);
        }
        /// <summary>
        /// Update chat data.
        /// </summary>
        /// <param name="id">Chat id.</param>
        /// <param name="roomDTO">New data.</param>
        /// <returns></returns>
        [HttpPut, Route("{id}", Name = "UpdateRoom")]
        [Authorize]
        public HttpResponseMessage UpdateRoom(int id, [FromBody]RoomDTO roomDTO)
        {
            responseData.RequestHelper = Request;
            if(!ModelState.IsValid)
            {
                return responseData.CreateNotValidResponse(modelState: ModelState);
            }

            //TODO: authorization

            Room room = _context.Rooms.SingleOrDefault(r => r.Id == id);

            if(room == null)
            {
                return responseData.CreateNotFoundResponse("room", id);
            }

            var updatedRoom = Mapper.Map<RoomDTO, Room>(roomDTO, room);
            _context.SaveChanges();

            return responseData.CreateResponse(status: HttpStatusCode.OK, roomDTO: roomDTO);
        }
        /// <summary>
        /// Remove chat from DB.
        /// </summary>
        /// <param name="id">Chat id.</param>
        /// <returns>Status code 200 OK.</returns>
        [HttpDelete, Route("{id}", Name = "DeleteRoom")]
        public HttpResponseMessage DeleteRoom(int id)
        {
            responseData.RequestHelper = Request;
            Room room = _context.Rooms.SingleOrDefault(r => r.Id == id);

            if(room == null)
            {
                responseData.CreateNotFoundResponse("room", id);
            }

            //TODO: authorization

            _context.Rooms.Remove(room);
            _context.SaveChanges();

            return responseData.CreateResponse(status: HttpStatusCode.OK, message: "Room with ID " + id + " was removed.");
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
            // TODO authorization
            string creatorId = User.Identity.GetUserId();

            roomDTO.CreatorId = creatorId;

            Room room = Mapper.Map<RoomDTO, Room>(roomDTO);

            _context.Rooms.Add(room);
            _context.SaveChanges();

            roomDTO.Id = room.Id;

            return responseData.CreateResponse(status: HttpStatusCode.Created, uri: new Uri(Request.RequestUri + "/" + room.Id));//, roomDTO);
        }

        /// <summary>
        /// Get members of the chat.
        /// </summary>
        /// <param name="id">Chat id.</param>
        /// <returns>List of object type MemberNoRoomDTO.</returns>
        [HttpGet, Route("{id:int}/members", Name = "GetRoomMembers")]
        [ResponseType(typeof(IEnumerable<MemberNoRoomDTO>))]
        public HttpResponseMessage GetRoomMembers(int id)
        {
            //authorization
            responseData.RequestHelper = Request;
            Room room = _context.Rooms.SingleOrDefault(r => r.Id == id);

            if(room == null)
            {
                return responseData.CreateNotFoundResponse("room", id);
            }

            var allmembers = from members in _context.Members
                             from rooms in members.Rooms
                             where rooms.Id == id
                             select members;
            IEnumerable<MemberNoRoomDTO> roomMembers = allmembers.ToList().Select(Mapper.Map<Member, MemberNoRoomDTO>);

            return responseData.CreateResponse(status: HttpStatusCode.Created, roomMembers: roomMembers);
        }

        /// <summary>
        /// Add user to chat.
        /// </summary>
        /// <param name="roomId">Chat id.</param>
        /// <param name="memberId">User Id.</param>
        /// <returns>Status code 200 OK.</returns>
        [HttpPut, Route("{id:int}/members/{memberId:int}", Name = "AddMemberToRoom")]
        [ResponseType(typeof(string))]
        public HttpResponseMessage AddMemberToRoom(int roomId, int memberId)
        {
            responseData.RequestHelper = Request;
            //TODO: authorizations
            var room = _context.Rooms.Include(r => r.Members).SingleOrDefault(r => r.Id == roomId);

            if(room == null)
            {
                return responseData.CreateNotFoundResponse("room", roomId);
            }
            var member = _context.Members.Include(r => r.Rooms).SingleOrDefault(m => m.Id == memberId);

            if(member == null)
            {
                return responseData.CreateNotFoundResponse("member", memberId);
            }

            var memberInRoom = room.Members.Where(m => m.Id == memberId).ToList();
            if(memberInRoom.Count() > 0)
            {
                return responseData.CreateResponse(message: "Room ID " + roomId + " already has member ID " + memberId, status: HttpStatusCode.BadRequest);
            }

            room.Members.Add(member);
            _context.SaveChanges();

            return responseData.CreateResponse(message: "Member with ID " + memberId + "was successfully added to the chat with ID " + roomId, status: HttpStatusCode.OK);
        }
        /// <summary>
        /// Remove user from chat.
        /// </summary>
        /// <param name="roomId">Chat id.</param>
        /// <param name="memberId">User id.</param>
        /// <returns>Status code 200 OK.</returns>
        [HttpDelete, Route("{id:int}/members/{memberId:int}", Name = "DeleteMemberFromRoom")]
        [Authorize]
        public HttpResponseMessage DeleteMemberFromRoom(int roomId, int memberId)
        {
            responseData.RequestHelper = Request;
            Room room = _context.Rooms.Include(r => r.Members).SingleOrDefault(r => r.Id == roomId);
            if(room == null)
            {
                return responseData.CreateNotFoundResponse(itemName: "room", id: roomId);
            }

            string authorizedUserId = User.Identity.GetUserId();
            string roomCreatorId = room.CreatorId;

            if(authorizedUserId != roomCreatorId)
            {
                return responseData.CreateResponse(message: "Only chat creator can remove users.", status: HttpStatusCode.Unauthorized);
            }

            Member member = _context.Members.Include(r => r.Rooms).SingleOrDefault(m => m.Id == memberId);

            if(member == null)
            {
                return responseData.CreateNotFoundResponse("member", roomId);
            }
            room.Members.Remove(member);
            _context.SaveChanges();

            return responseData.CreateResponse(message: "User with ID " + memberId + " was successfully removed from the chat.", status: HttpStatusCode.OK);
        }

        /// <summary>
        /// Get all messages in chat.
        /// </summary>
        /// <param name="id">Chat id.</param>
        /// <returns>List of objects type MessageDTO.</returns>
        [HttpGet, Route("{id:int}/messages", Name = "GetRoomMessages")]
        [Authorize]
        [ResponseType(typeof(IEnumerable<MessageDTO>))]
        public HttpResponseMessage GetRoomMessages(int id)
        {
            responseData.RequestHelper = Request;
            Room room = _context.Rooms.Include(r => r.Members).SingleOrDefault(r => r.Id == id);

            if(room == null)
            {
                responseData.CreateNotFoundResponse("room", id);
            }

            string authorizedUserId = User.Identity.GetUserId();
            //Member chatMember = room.Members.SingleOrDefault(m => m.Id == authorizedUserId);

            //if(chatMember == null)
            //{
            //    return responseData.CreateResponse(message: "Only chat member can see chat messages.", status: HttpStatusCode.Unauthorized);
            //    //ThrowUnauthorizedException();
            //}

            var allMessages = from messages in _context.Messages.Include(m => m.Author)
                              where messages.Room.Id == id
                              select messages;
            var messagesDTO = allMessages.ToList().Select(Mapper.Map<Message, MessageDTO>);

            return responseData.CreateResponse(message: "Only chat member can see chat messages.", status: HttpStatusCode.Unauthorized);
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
        [Authorize]
        public HttpResponseMessage GetMessagesFromDate(int id, DateTime dateTime, bool count = false)
        {
            responseData.RequestHelper = Request;

            var messages =
                _context.Messages.Include(m => m.Author)
                .Where(m => m.Room.Id == id)
                .Where(m => m.Date >= dateTime).ToList();
            if(count == true)
                return responseData.CreateResponse(message: messages.Count.ToString(), status: HttpStatusCode.OK);
            else
                return responseData.CreateResponse(messages: messages.Select(Mapper.Map<Message, MessageDTO>), status: HttpStatusCode.OK);
        }
        /// <summary>
        /// Update existing messaged.
        /// </summary>
        /// <param name="id">Chat id.</param>
        /// <param name="msgDTO">Object type MessageDTO.</param>
        /// <returns></returns>
        [HttpPost, Route("{id:int}/messages", Name = "PostMessage")]
        [Authorize]
        public HttpResponseMessage PostMessage(int id, MessageDTO msgDTO)
        {
            responseData.RequestHelper = Request;

            if(!ModelState.IsValid)
            {
                return responseData.CreateNotValidResponse(modelState: ModelState);
            }

            Member author = _context.Members.SingleOrDefault(m => m.Id == msgDTO.Author.Id);
            Room room = _context.Rooms.SingleOrDefault(r => r.Id == id);
            Message message = Mapper.Map<MessageDTO, Message>(msgDTO);

            if(author == null)
            {
                responseData.CreateNotFoundResponse(itemName: "author", id: msgDTO.Author.Id);
            }
            if(room == null)
            {
                responseData.CreateNotFoundResponse(itemName: "room", id: id);
            }

            message.Author = author;
            message.Room = room;

            _context.Messages.Add(message);
            _context.SaveChanges();

            msgDTO.Id = message.Id;

            return responseData.CreateResponse(message: "Message was successfuly added.", status: HttpStatusCode.Created);
        }
        /// <summary>
        /// Remove posted message.
        /// </summary>
        /// <param name="id">Chat id.</param>
        /// <param name="msgId">Message id.</param>
        /// <returns>Status code 200 OK.</returns>

        [HttpDelete, Route("{id:int}/messages", Name = "DeleteMessage")]
        [Authorize]
        public HttpResponseMessage DeleteMessage(int id, int msgId)
        {
            responseData.RequestHelper = Request;

            Message msg = _context.Messages.Where(m => m.Room.Id == id).SingleOrDefault(m => m.Id == msgId);

            if(msg == null)
            {
            return    responseData.CreateNotFoundResponse("message", id);
            }

            _context.Messages.Remove(msg);
            _context.SaveChanges();

            return responseData.CreateResponse(message: "Message was successfully deleted.", status: HttpStatusCode.OK);
        }
    }
}
