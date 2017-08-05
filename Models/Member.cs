using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ChatterboxAPI.Models
{
    public class Member
    {
        public int Id { set; get; }
        [Required]
        [StringLength(255)]
        public string Name { set; get; }

        [Required]
        public DateTime Birthdate { set; get; }
        public ICollection<Room> Rooms { set; get; }

        public Member()
        {
            Rooms = new HashSet<Room>();
        }
    }
}