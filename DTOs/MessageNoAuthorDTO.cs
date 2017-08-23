using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace ChatterboxAPI.DTOs
{
    public class MessageNoAuthorDTO
    {
        public int Id { set; get; }
        [Required]
        public string Content { set; get; }
        public DateTime Date { set; get; }
    }
}