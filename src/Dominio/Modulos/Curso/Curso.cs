using System;
using GeradorCertificado.Dominio.Compartilhado;

namespace GeradorCertificado.Dominio.Modulos.Curso;

public sealed class Curso : EntidadeBase<Curso>
{
    public string Nome {get; private set;} = string.Empty;
    public string Descricao {get; private set;} = string.Empty;
    public int CargaHoraria {get; private set;}
    public DateTime DataConclusao {get; private set;}

    private Curso() { }

    public Curso(Guid id, string nome, string descricao, int cargaHoraria, DateTime dataConclusao)
    {
        Id = id;
        Nome = nome;
        Descricao = descricao;
        CargaHoraria = cargaHoraria;
        DataConclusao = dataConclusao;
    }

    public override IReadOnlyList<ErroValidacao> Validar()
    {
        List<ErroValidacao> erros = [];

        if (string.IsNullOrWhiteSpace(Nome))
            erros.Add(new(nameof(Nome), "O campo \"Nome\" do Curso é obrigatório."));

        else if (Nome.Length > 200)
            erros.Add(new(nameof(Nome), "O \"nome\" do Curso deve possuir no máximo 200 caracteres."));

        if (!string.IsNullOrWhiteSpace(Descricao) &&
            Descricao.Length > 500)
            erros.Add(new(nameof(Descricao), "A \"descrição\" do Curso deve possuir no máximo 500 caracteres."));

        if (CargaHoraria <= 0)
            erros.Add(new(nameof(CargaHoraria), "A \"carga horária\" do Curso deve ser maior que zero."));

        if (DataConclusao == default)
            erros.Add(new(nameof(DataConclusao), "A \"data de conclusão\" do Curso é obrigatória."));

        return erros;
    }

    public override void Atualizar(Curso entidadeAtualizada)
    {
        Nome = entidadeAtualizada.Nome;
        Descricao = entidadeAtualizada.Descricao;
        CargaHoraria = entidadeAtualizada.CargaHoraria;
        DataConclusao = entidadeAtualizada.DataConclusao;

    }
}
