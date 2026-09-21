using GeradorCertificado.Api.Compartilhado.Http;
using GeradorCertificado.Aplicacao.Modulos.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeradorCertificado.Api.Modulos.Auth;

[ApiController]
[Route("auth")]
[AllowAnonymous]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("cadastro")]
    [ProducesResponseType<CadastrarUsuarioResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<CadastrarUsuarioResponse>> Cadastrar(
        CadastrarUsuarioRequest request,
        CancellationToken cancellationToken
    )
    {
        var resultado = await mediator.Send(
            new CadastrarClienteCommand(request.Email, request.Senha),
            cancellationToken
        );

        if (resultado.IsFailed)
            return this.ProblemDetails(resultado);

        return StatusCode(
            StatusCodes.Status201Created,
            new CadastrarUsuarioResponse(resultado.Value, request.Email.Trim())
        );
    }

    [HttpPost("login")]
    [ProducesResponseType<AutenticarUsuarioResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AutenticarUsuarioResponse>> Login(
        AutenticarUsuarioRequest request,
        CancellationToken cancellationToken
    )
    {
        var resultado = await mediator.Send(
            new AutenticarClienteCommand(request.Email, request.Senha),
            cancellationToken
        );

        if (resultado.IsFailed)
            return this.ProblemDetails(resultado);

        var token = resultado.Value;

        return Ok(new AutenticarUsuarioResponse(token.Token, token.DataExpiracaoEmUtc));
    }
}
