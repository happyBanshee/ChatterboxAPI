using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using ChatterboxAPI.Controllers;
using System.Net;
using Newtonsoft.Json.Linq;
using ChatterboxAPI.DTOs;

namespace ChatterboxAPI.App_Start
{
    public class ResponseMessage
    {
        public string Message { set; get; }
     //   public int Status { set; get; }
    }

    public class ResponseObject
    {
        //public string Message { set; get; }
           public int Id { set; get; }
    }

    public class ResponseHelper : ApiController
    {
        public HttpRequestMessage RequestHelper { set; get; }

        public HttpResponseMessage CreateResponse(HttpStatusCode status, string message)
        {
            ResponseMessage msg = new ResponseMessage {
                Message = message
            };

            HttpResponseMessage response = RequestHelper.CreateResponse(
               status,
               msg,
               "application/json"
              );
            return response;
        }

        public HttpResponseMessage CreateResponse(int id, HttpStatusCode status)
        {
            ResponseObject obj = new ResponseObject {
                Id = id
            };

            HttpResponseMessage response = RequestHelper.CreateResponse(
               status,
               obj,
               "application/json"
              );
            return response;
        }


        public HttpResponseMessage CreateResponse(HttpStatusCode status, RoomDTO roomDTO)
        {
            HttpResponseMessage response = RequestHelper.CreateResponse(
               HttpStatusCode.Created,
               roomDTO,
               "application/json"
              );

            return response;
        }

        public HttpResponseMessage CreateResponse(HttpStatusCode status, IEnumerable<MemberNoRoomDTO>  roomMembers)
        {
            HttpResponseMessage response = RequestHelper.CreateResponse(
               HttpStatusCode.Created,
               roomMembers,
               "application/json"
              );

            return response;
        }
        public HttpResponseMessage CreateResponse(HttpStatusCode status, IEnumerable< MessageDTO> messages)
        {
            HttpResponseMessage response = RequestHelper.CreateResponse(
               HttpStatusCode.Created,
               messages,
               "application/json"
              );

            return response;
        }


        public HttpResponseMessage CreateResponse(HttpStatusCode status, Uri uri)
        {
            HttpResponseMessage response = RequestHelper.CreateResponse(
               HttpStatusCode.Created,
               uri,
               "application/json"
              );

            return response;
        }

        public HttpResponseMessage CreateNotValidResponse( ModelStateDictionary modelState)
        {
            HttpResponseMessage response = RequestHelper.CreateResponse(
                HttpStatusCode.BadRequest,
                modelState);
            return response;
        }
       

        public HttpResponseMessage CreateNotFoundResponse(string itemName, int id)
        {
            HttpStatusCode status = HttpStatusCode.NotFound;

            ResponseMessage msg = new ResponseMessage {
                Message = "No " + itemName + " with ID = "+ id
               // Status = (int) status
            };

            HttpResponseMessage response = RequestHelper.CreateResponse(
               status,
               msg,
               "application/json"
              );
            return response;
        }


        public void ThrowNotFoundResponse(string itemName, string id)
        {
            var resp = new HttpResponseMessage(HttpStatusCode.NotFound) {
                Content = new StringContent(string.Format("No " + itemName + " with ID = {0}", id)),
                ReasonPhrase = "ID " + itemName + " Not Found"
            };
            throw new HttpResponseException(resp);
        }
        public void ThrowNotFoundResponse(string itemName, int id)
        {
            var resp = new HttpResponseMessage(HttpStatusCode.NotFound) {
                Content = new StringContent(string.Format("No " + itemName + " with ID = {0}", id)),
                ReasonPhrase = "ID " + itemName + " Not Found"
            };
            throw new HttpResponseException(resp);
        }
    }
}