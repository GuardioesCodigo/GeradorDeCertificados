using FluentResults;
using GeradorCertificado.Aplicacao.Modulos.GeracaoCertificados.Mensageria;
using GeradorCertificado.Aplicacao.Modulos.GeracaoCertificados.Util;
using GeradorCertificado.Dominio.Compartilhado;
using GeradorCertificado.Dominio.Modulos.Cursos;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.Certificados;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.SolicitacaoCertificados;
using MassTransit;
using MediatR;

namespace GeradorCertificado.Aplicacao.Modulos.GeracaoCertificados;

public sealed record AlunoParaCertificadoCommand(
    string Nome
);

public sealed record SolicitarGeracaoCertificadosCommand(
    Guid CursoId,
    IReadOnlyList<AlunoParaCertificadoCommand> Alunos
) : IRequest<Result<Guid>>;

public sealed class SolicitarGeracaoCertificadosCommandHandler(
    IRepositorioCurso repositorioCurso,
    IRepositorioSolicitacaoCertificado repositorioSolicitacao,
    IPublishEndpoint publishEndpoint
) : IRequestHandler<SolicitarGeracaoCertificadosCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        SolicitarGeracaoCertificadosCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var erros = Validar(command);

        if (erros.Count > 0)
            return Result.Fail<Guid>(ErrosDeSolicitacaoCertificado.Validacao(erros));

        var curso = await repositorioCurso.SelecionarPorIdAsync(command.CursoId, cancellationToken);

        if (curso is null)
            return Result.Fail<Guid>(ErrosDeSolicitacaoCertificado.CursoNaoEncontrado(command.CursoId));

        var solicitacaoEmAndamento = await repositorioSolicitacao.SelecionarEmProcessamentoPorCursoAsync(
            command.CursoId,
            cancellationToken
        );

        if (solicitacaoEmAndamento is not null)
            return Result.Fail<Guid>(ErrosDeSolicitacaoCertificado.ProcessamentoEmAndamento(command.CursoId));

        var solicitacaoId = Guid.CreateVersion7();

        List<Certificado> certificados = command.Alunos
            .Select(aluno => new Certificado(
                Guid.CreateVersion7(),
                aluno.Nome,
                StatusCertificado.Pendente,
                caminhoArquivo: null,
                dataGeracao: null,
                solicitacaoId
            ))
            .ToList();

        var solicitacao = new SolicitacaoCertificado(solicitacaoId, command.CursoId, certificados);

        var errosDeDominio = solicitacao.Validar();

        if (errosDeDominio.Count > 0)
            return Result.Fail<Guid>(ErrosDeSolicitacaoCertificado.Validacao(errosDeDominio));

        await repositorioSolicitacao.CadastrarAsync(solicitacao, cancellationToken);

        await publishEndpoint.Publish(
            new GerarCertificadosMessage(solicitacao.Id, command.CursoId),
            cancellationToken
        );

        return Result.Ok(solicitacao.Id);
    }

    private static IReadOnlyList<ErroValidacao> Validar(SolicitarGeracaoCertificadosCommand command)
    {
        List<ErroValidacao> erros = [];

        if (command.CursoId == Guid.Empty)
            erros.Add(new(nameof(command.CursoId), "O curso é obrigatório."));

        if (command.Alunos is null || command.Alunos.Count == 0)
        {
            erros.Add(new(nameof(command.Alunos), "A solicitação deve possuir pelo menos um aluno."));

            return erros;
        }

        for (int indice = 0; indice < command.Alunos.Count; indice++)
        {
            var aluno = command.Alunos[indice];
            var campo = $"{nameof(command.Alunos)}[{indice}].{nameof(aluno.Nome)}";

            if (string.IsNullOrWhiteSpace(aluno.Nome))
                erros.Add(new(campo, "O nome do aluno é obrigatório."));

            else if (aluno.Nome.Length > 200)
                erros.Add(new(campo, "O nome do aluno deve possuir no máximo 200 caracteres."));
        }

        return erros;
    }
}
