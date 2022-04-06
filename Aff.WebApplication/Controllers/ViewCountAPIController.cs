using Aff.DataAccess;
using Aff.DataAccess.Repositories;
using Aff.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Aff.WebApplication.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/View")]
    public class ViewCountAPIController : ApiController
    {
        [HttpGet]
        [Route("Test")]
        public string Test()
        {
            return "Test";
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("Count")]
        public IHttpActionResult UpdateViewCount(ViewCountModel request)
        {
            if (request == null || string.IsNullOrEmpty(request.AffCode) || string.IsNullOrEmpty(request.IpAddress))
            {
                return BadRequest("BadRequest.");
            }
            TimaAffiliateEntities dbContext = new TimaAffiliateEntities();
            var repo = new ViewCountRepository(dbContext);
            var transaciton = repo.CreateViewCount(request.AffCode, request.IpAddress);
            return Ok(transaciton);
        }
    }
}
