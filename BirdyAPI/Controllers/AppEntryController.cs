﻿using System;
using System.Data;
using System.Security.Authentication;
using BirdyAPI.Dto;
using BirdyAPI.Services;
using BirdyAPI.Tools.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BirdyAPI.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("app")]
    public class AppEntryController : ExtendedController
    {
        private readonly AppEntryService _appEntryService;

        public AppEntryController(BirdyContext context) : base(context)
        {
            _appEntryService = new AppEntryService(context);
        }

        //TODO :1 Decrease status codes count
        /// <summary>
        /// User authentication
        /// </summary>
        /// <response code = "200">Return user token</response>
        /// <response code = "401">User need to confirm email</response>
        /// <response code = "404">Invalid login or password</response>
        /// <response code = "500">Unexpected Exception (only for debug)</response>
        [HttpPost]
        [Route("auth")]
        [ProducesResponseType(statusCode: 200, type:typeof(SimpleAnswerDto))]
        [ProducesResponseType(statusCode: 401, type: typeof(void))]
        [ProducesResponseType(statusCode: 404, type: typeof(void))]
        [ProducesResponseType(statusCode: 500, type: typeof(ExceptionDto))]
        public IActionResult UserAuthentication([FromBody] AuthenticationDto user)
        {
            try
            {
                return Ok(_appEntryService.Authentication(user));
            }
            catch (AuthenticationException)
            {
                return Unauthorized();
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// User registration
        /// </summary>
        /// <response code = "200">Confirm message sent</response>
        /// <response code = "500">Unexpected Exception (only for debug)</response>
        /// <response code = "409">Duplicate account</response>
        /// <response code = "403">Duplicate unique tag</response>
        [HttpPost]
        [Route("reg")]
        [ProducesResponseType(statusCode: 200, type: typeof(void))]
        [ProducesResponseType(statusCode: 500, type: typeof(ExceptionDto))]
        [ProducesResponseType(statusCode: 403, type: typeof(void))]
        [ProducesResponseType(statusCode: 409, type: typeof(void))]
        public IActionResult UserRegistration([FromBody]RegistrationDto user)
        {
            try
            {
                if (!CheckUniqueTagAvailable(user.UniqueTag))
                    throw new DuplicateNameException();

                _appEntryService.CreateNewAccount(user);
                return Ok();
            }
            catch (DuplicateAccountException)
            {
                return Conflict();
            }
            catch (DuplicateNameException)
            {
                return Forbid();
            }
        }


        //Нужно что-то нормальное возвращать, чтобы юзер вообще понимал, что происходит
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        [Route("confirm")]
        public IActionResult EmailConfirming([FromQuery] string email, Guid token)
        {
            try
            {
                _appEntryService.GetUserConfirmed(email, token);
                return Ok("CurrentStatus = 1");
            }
            catch (ArgumentException)
            {
                return BadRequest("Invalid link");
            }
            catch (TimeoutException)
            {
                return Forbid("Timeout");
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        /// <response code = "200">Password changed</response>
        /// <response code = "500">Unexpected Exception (only for debug)</response>
        /// <response code = "400">Wrong password</response>
        /// <response code = "401">Invalid token</response>
        [HttpPut]
        [Route("password")]
        [ProducesResponseType(statusCode: 200, type: typeof(void))]
        [ProducesResponseType(statusCode: 400, type: typeof(void))]
        [ProducesResponseType(statusCode: 401, type: typeof(void))]
        [ProducesResponseType(statusCode: 500, type: typeof(ExceptionDto))]
        public IActionResult ChangePassword([FromHeader] Guid token, [FromBody] ChangePasswordDto passwordChanges)
        {
            try
            {
                int currentUserId = ValidateToken(token);
                _appEntryService.ChangePassword(currentUserId, passwordChanges);
                return Ok();
            }
            catch (AuthenticationException)
            {
                return Unauthorized();
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Terminate all sessions
        /// </summary>
        /// <response code = "200">All sessions stopped</response>
        /// <response code = "500">Unexpected Exception (only for debug)</response>
        /// <response code = "401">Invalid token</response>
        [HttpDelete]
        [Route("logout/all")]
        [ProducesResponseType(statusCode: 200, type: typeof(void))]
        [ProducesResponseType(statusCode: 500, type: typeof(ExceptionDto))]
        [ProducesResponseType(statusCode: 401, type: typeof(void))]
        public IActionResult FullExitApp([FromHeader] Guid token)
        {
            try
            {
                int currentUserId = ValidateToken(token);
                _appEntryService.TerminateAllSessions(currentUserId);
                return Ok();
            }
            catch (AuthenticationException)
            {
                return Unauthorized();
            }
        }

        /// <summary>
        /// Terminate current session
        /// </summary>
        /// <response code = "200">Current session stopped</response>
        /// <response code = "500">Unexpected Exception (only for debug)</response>
        /// <response code = "401">Invalid token</response>
        [HttpDelete]
        [Route("logout")]
        [ProducesResponseType(statusCode: 200, type: typeof(void))]
        [ProducesResponseType(statusCode: 500, type: typeof(ExceptionDto))]
        [ProducesResponseType(statusCode: 401, type: typeof(void))]
        public IActionResult ExitApp([FromHeader] Guid token)
        {
            try
            {
                int currentUserId = ValidateToken(token);
                _appEntryService.TerminateSession(token, currentUserId);
                return Ok();
            }
            catch (AuthenticationException)
            {
                return Unauthorized();
            }
        }
    }
}
