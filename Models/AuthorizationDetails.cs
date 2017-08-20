using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatterboxAPI.Models
{
    public class AuthorizationDetails
    {
        /// <summary>
        /// User name used to register.
        /// </summary>
        string Username { set; get; }
        /// <summary>
        /// Password used to register.
        /// </summary>
        string Password { set; get; }
        /// <summary>
        /// By default set "grant_type" value "password".
        /// </summary>
        string GrantType { set; get; }
    }
}