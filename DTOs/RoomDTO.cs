using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;



namespace ChatterboxAPI.DTOs
{
    public class RoomDTO
    {
        public int Id { set; get; }
        [Required]
        public string Name { set; get; }
        public string Description { set; get; }
        public bool IsPrivate { get; set; }
        public IEnumerable<MemberNoRoomDTO> Members { set; get; }
       // public IEnumerable<Link> Links { set; get; }
    }
}