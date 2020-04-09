using AspNetCore.Mvc.Extensions.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Controllers.Api
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/test")]
    public class ApiTestController : ApiControllerBase
    {
        [Route("checkid/{id}")]
        [HttpGet]
        public IActionResult CheckId(int id)
        {
            if (id > 100)
            {
                return BadRequest("We cannot use IDs greater than 100.");
            }
            return Ok(id);
        }

        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Route("unauthorized")]
        [HttpGet]
        public IActionResult Unauthorized_401()
        {
            return Unauthorized();
        }

        [Route("challenge")]
        [HttpGet]
        public IActionResult ChallengeDefault()
        {
            return Challenge();
        }

        [Route("bad-request")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult BadDefault()
        {
            return BadRequest();
        }

        [Route("bad-request-object")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult BadRequestDefault()
        {
            return new BadRequestResult();
        }

        [Route("challenge-bearer")]
        [HttpGet]
        public IActionResult Challenge_Bearer()
        {
            //Login
            return Challenge(JwtBearerDefaults.AuthenticationScheme);
        }

        [Route("challenge-basic")]
        [HttpGet]
        public IActionResult Challenge_Basic()
        {
            //Login
            return Challenge(BasicAuthenticationDefaults.AuthenticationScheme);
        }

        [Route("forbid")]
        [HttpGet]
        public IActionResult ForbidDefault()
        {
            return Forbid();
        }

        //A challenge result should generally be used in cases where the current visitor is not logged in, but is trying to access an action that requires an authenticated user.It will prompt a challenge for credentials.It could also be used for an authenticated user, who is not authorised for the action, and where you want to prompt for higher privileged credentials.
        //A forbid result should be used in cases where the current visitor is logged in as a user in your system, but is trying to access an action that their account does not have permission to perform.
        //With the standard ASP.NET Core CookieAuthentication added by Identity, default paths are set to handle each case and the user gets redirected.
        //By default... Access denied - i.e.forbidden looks to redirect to /Account/AccessDenied Unauthenticated - i.e.challenge looks to redirect to /Account/Login
        // redirection, forbidden will return a 403 status code, challenge will return a 401.

        [Route("forbid-bearer")]
        [HttpGet]
        public IActionResult Forbid_Bearer()
        {
            return Forbid(JwtBearerDefaults.AuthenticationScheme);
        }

        [Route("forbidden")]
        [HttpGet]
        public IActionResult Forbidden_403()
        {
            return Forbidden();
        }

        [Route("ok")]
        [HttpGet]
        public IActionResult OK_200()
        {
            return Ok();
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("not-found")]
        [HttpGet]
        public IActionResult NotFound_404()
        {
            return NotFound();
        }

        [Route("throw-exception")]
        [HttpGet]
        public IActionResult ThrowException()
        {
            throw new Exception("");
        }

        [Route("error")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        public IActionResult InternalServerError_500()
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        [Route("cancel-operation")]
        [HttpGet]
        public async Task<IActionResult> CancelOperation()
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(100);

            await Task.Run(async () => await Task.Delay(10000), cts.Token);
            await Task.Run(async () => await Task.Delay(10000), cts.Token);

            return Ok();
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + BasicAuthenticationDefaults.AuthenticationScheme)]
        [Route("logged-in")]
        [HttpGet]
        public IActionResult LoggedIn()
        {
            return Ok();
        }
    }
}
