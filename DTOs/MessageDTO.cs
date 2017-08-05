using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChatterboxAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace ChatterboxAPI.DTOs
{
    public class MessageDTO
    {
        public int Id { set; get; }
        [Required]
        public AuthorDTO Author { set; get; }
        [Required]
        public string Content { set; get; }
        public DateTime Date { set; get; }
        //public RoomNoMemberDTO Room { set; get; }
    }
}