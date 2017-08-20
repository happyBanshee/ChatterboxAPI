using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChatterboxAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace ChatterboxAPI.DTOs
{
    public class MemberDTO
    {
        public int Id { set; get; }
        [Required]
        [StringLength(255)]
        public string Name { set; get; }

        [Required]
        public DateTime Birthdate { set; get; }
        public IEnumerable<RoomNoMemberDTO> Rooms { set; get; }

        public MemberDTO() {

            Rooms = new HashSet<RoomNoMemberDTO>();
        }

    }
}