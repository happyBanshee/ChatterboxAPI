using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ChatterboxAPI.DTOs
{
    public class MemberNoRoomDTO
    {
        public int Id { set; get; }
        [Required]
        [StringLength(255)]
        public string Name { set; get; }

        [Required]
        public DateTime Birthdate { set; get; }
    }
}