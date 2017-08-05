using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;


namespace ChatterboxAPI.Models
{
    public class Message
    {
        public int Id { set; get; }
        public int AuthorId { get; set; }
        [Required]
        [ForeignKey("AuthorId")]
        public Member Author { set; get; }
        [Required]
        public string Content { set; get; }
        public DateTime Date { set; get; }
        public int RoomId { get; set; }
        [ForeignKey("RoomId")]
        public Room Room { set; get; }
    }
}