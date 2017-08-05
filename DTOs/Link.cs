using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Routing;

namespace ChatterboxAPI.DTOs
{
    public class Link
    {
        public string Href { get; set; }
        public string Rel { get; set; }
        public string Method { get; set; }
    }
}