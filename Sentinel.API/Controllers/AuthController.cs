using System.IdentityModel.Tokens.Jwt;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Features.Auth.Login;
using Sentinel.Application.Features.Auth.Register;
using Sentinel.Application.Interfaces;

namespace Sentinel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;


    public AuthController(ICurrentUserService currentUserService, IMediator mediator)
    {
        _currentUserService = currentUserService;
        _mediator = mediator;
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            userId = _currentUserService.UserId,
            email = _currentUserService.Email,
            username = _currentUserService.Username
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var token = await _mediator.Send(command, cancellationToken);

        return Ok(token);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);

        return Ok(id);
    }
}