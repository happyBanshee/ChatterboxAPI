using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;



namespace ChatterboxAPI.DTOs
{
  //  [ModelName("Author")]
    public class AuthorDTO
      
    {
        public int Id { set; get; }
        [Required]
        [StringLength(255)]
        public string Name { set; get; }
    }
}