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

namespace ChatterboxAPI.Controllers
{
    [RoutePrefix("api/messages")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]

    public class MessagesController : ApiController
    {
        private ApplicationDbContext _context;

        public MessagesController()
        {
            _context = new ApplicationDbContext();
        }

        [HttpGet, Route("")]
        public IHttpActionResult GetMessages()
        {
            var messages = _context.Messages.Include(m => m.Room).Include(m => m.Author).ToList();

            //return Ok(messages);
            return Ok(messages.Select(Mapper.Map<Message, MessageDTO>));
        }


    }
}
