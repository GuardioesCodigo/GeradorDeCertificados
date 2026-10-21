using FluentResults;
using GeradorCertificado.Aplicacao.Modulos.GeracaoCertificados.Mensageria;
using GeradorCertificado.Aplicacao.Modulos.GeracaoCertificados.Util;
using GeradorCertificado.Dominio.Compartilhado;
using GeradorCertificado.Dominio.Modulos.Alunos;
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
    IRepositorioAluno repositorioAluno,
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

        List<string> nomes = command.Alunos
            .Select(aluno => Aluno.NormalizarNome(aluno.Nome))
            .ToList();

        Dictionary<string, Aluno> alunosPorNome = new(StringComparer.OrdinalIgnoreCase);

        foreach (string nome in nomes.Distinct(StringComparer.OrdinalIgnoreCase))
            alunosPorNome[nome] = await ObterOuCadastrarAlunoAsync(nome, cancellationToken);

        List<Certificado> certificados = nomes
            .Select(nome => new Certificado(
                Guid.CreateVersion7(),
                nome,
                StatusCertificado.Pendente,
                caminhoArquivo: null,
                dataGeracao: null,
                solicitacaoId,
                alunosPorNome[nome].Id
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

    /// <summary>
    /// O aluno é identificado pelo nome (sem diferenciar maiúsculas/minúsculas): se já existe,
    /// os novos certificados ficam ligados a ele; senão, ele é gravado agora.
    /// </summary>
    private async Task<Aluno> ObterOuCadastrarAlunoAsync(string nome, CancellationToken cancellationToken)
    {
        var existente = await repositorioAluno.SelecionarPorNomeAsync(nome, cancellationToken);

        if (existente is not null)
            return existente;

        var novo = new Aluno(Guid.CreateVersion7(), nome);

        try
        {
            await repositorioAluno.CadastrarAsync(novo, cancellationToken);

            return novo;
        }
        catch (ConflitoDePersistenciaException)
        {
            var criadoEmParalelo = await repositorioAluno.SelecionarPorNomeAsync(nome, cancellationToken);

            return criadoEmParalelo
                ?? throw new InvalidOperationException($"Não foi possível gravar o aluno \"{nome}\".");
        }
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
