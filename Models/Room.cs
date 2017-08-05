using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;



namespace ChatterboxAPI.Models
{
    public class Room
    {
        public int Id { set; get; }
        [Required]
        public string Name { set; get; }
        public string Description { set; get; }
        public bool IsPrivate { get; set; }
        public ICollection<Member> Members { set; get; }

        public Room()
        {
            Members = new HashSet<Member>();
        }
    }
}