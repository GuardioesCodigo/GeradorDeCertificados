using System.Net.Mail;
using FluentResults;
using GeradorCertificado.Aplicacao.Modulos.Auth.Util;
using GeradorCertificado.Dominio.Compartilhado;
using GeradorCertificado.Dominio.Compartilhado.Auth;
using MediatR;

namespace GeradorCertificado.Aplicacao.Modulos.Auth;

public sealed record CadastrarClienteCommand(
    string Email,
    string Senha
) : IRequest<Result<Guid>>;

public sealed class CadastrarClienteCommandHandler(
    IGerenciadorDeIdentidade gerenciadorDeIdentidade
) : IRequestHandler<CadastrarClienteCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CadastrarClienteCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var erros = Validar(command);

        if (erros.Count > 0)
            return Result.Fail(ErrosDeAutenticacao.Validacao(erros));

        try
        {
            var usuario = await gerenciadorDeIdentidade.CadastrarAsync(
                Guid.CreateVersion7(),
                command.Email.Trim(),
                command.Senha,
                TipoUsuario.Cliente
            );

            return Result.Ok(usuario.Id);
        }
        catch (ConflitoDeIdentidadeException ex)
        {
            return Result.Fail(ErrosDeAutenticacao.CadastroDuplicado(ex.Message));
        }
        catch (ValidacaoDeIdentidadeException ex)
        {
            return Result.Fail(
                ErrosDeAutenticacao.Validacao([new ErroValidacao(ex.Campo, ex.Message)])
            );
        }
    }

    private static IReadOnlyList<ErroValidacao> Validar(CadastrarClienteCommand command)
    {
        List<ErroValidacao> erros = [];

        if (string.IsNullOrWhiteSpace(command.Email))
            erros.Add(new(nameof(command.Email), "O campo \"Email\" é obrigatório."));
        else if (!EmailEhValido(command.Email.Trim()))
            erros.Add(new(nameof(command.Email), "O \"email\" informado não é válido."));

        if (string.IsNullOrWhiteSpace(command.Senha))
        {
            erros.Add(new(nameof(command.Senha), "O campo \"Senha\" é obrigatório."));
        }
        else
        {
            if (command.Senha.Length < 8)
                erros.Add(new(nameof(command.Senha), "A \"senha\" deve possuir no mínimo 8 caracteres."));

            if (!command.Senha.Any(char.IsDigit))
                erros.Add(new(nameof(command.Senha), "A \"senha\" deve conter ao menos um dígito."));

            if (command.Senha.All(char.IsLetterOrDigit))
                erros.Add(new(nameof(command.Senha), "A \"senha\" deve conter ao menos um caractere não alfanumérico."));
        }

        return erros;
    }

    private static bool EmailEhValido(string email)
    {
        try
        {
            return new MailAddress(email).Address == email;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
