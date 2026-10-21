import { forkJoin } from 'rxjs';

import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { LucideDownload, LucideLoaderCircle } from '@lucide/angular';

import { ApiValidationProblemDetails } from '../../auth/auth.models';
import {
  lerMensagemDeErroDeDownload,
  nomeDeArquivoSeguro,
  salvarArquivo,
} from '../../shared/arquivos';
import { AlunoDto, CertificadoDoAlunoDto } from '../alunos.models';
import { AlunosService } from '../alunos.service';

@Component({
  imports: [LucideDownload, LucideLoaderCircle, RouterLink],
  selector: 'app-detalhes-aluno',
  templateUrl: './detalhes-aluno.html',
})
export class DetalhesAluno {
  private readonly route = inject(ActivatedRoute);
  private readonly alunosService = inject(AlunosService);

  protected readonly aluno = signal<AlunoDto | null>(null);
  protected readonly certificados = signal<readonly CertificadoDoAlunoDto[]>([]);
  protected readonly selecionados = signal<ReadonlySet<string>>(new Set());
  protected readonly carregando = signal(true);
  protected readonly baixando = signal(false);
  protected readonly mensagemErro = signal('');

  protected readonly todosSelecionados = computed(
    () =>
      this.certificados().length > 0 &&
      this.certificados().every((certificado) =>
        this.selecionados().has(certificado.certificadoId),
      ),
  );

  constructor() {
    const alunoId = this.route.snapshot.paramMap.get('alunoId');

    if (!alunoId) {
      this.mensagemErro.set('Identificador do aluno inválido.');
      this.carregando.set(false);
      return;
    }

    forkJoin({
      aluno: this.alunosService.obterPorId(alunoId),
      certificados: this.alunosService.listarCertificados(alunoId),
    }).subscribe({
      next: ({ aluno, certificados }) => {
        this.aluno.set(aluno);
        this.certificados.set(certificados);
        this.carregando.set(false);
      },
      error: (resposta: HttpErrorResponse) => {
        this.mensagemErro.set(this.obterMensagemErro(resposta));
        this.carregando.set(false);
      },
    });
  }

  protected alternarSelecao(certificadoId: string, marcado: boolean): void {
    const novos = new Set(this.selecionados());

    if (marcado) novos.add(certificadoId);
    else novos.delete(certificadoId);

    this.selecionados.set(novos);
  }

  protected alternarTodos(marcado: boolean): void {
    this.selecionados.set(
      marcado ? new Set(this.certificados().map((c) => c.certificadoId)) : new Set(),
    );
  }

  protected baixarPdf(certificado: CertificadoDoAlunoDto): void {
    const aluno = this.aluno();

    if (!aluno) return;

    this.iniciarDownload(
      this.alunosService.baixarCertificado(aluno.id, certificado.certificadoId),
      `Certificado - ${nomeDeArquivoSeguro(certificado.cursoNome)} - ${nomeDeArquivoSeguro(aluno.nome)}.pdf`,
    );
  }

  protected baixarZip(ids: readonly string[]): void {
    const aluno = this.aluno();

    if (!aluno) return;

    this.iniciarDownload(
      this.alunosService.baixarZip(aluno.id, ids),
      `certificados-${nomeDeArquivoSeguro(aluno.nome)}.zip`,
    );
  }

  protected baixarSelecionados(): void {
    this.baixarZip([...this.selecionados()]);
  }

  protected formatarData(data: string): string {
    return new Date(data).toLocaleDateString('pt-BR');
  }

  private iniciarDownload(
    requisicao: ReturnType<AlunosService['baixarCertificado']>,
    nomeArquivo: string,
  ): void {
    this.baixando.set(true);
    this.mensagemErro.set('');

    requisicao.subscribe({
      next: (arquivo) => {
        salvarArquivo(arquivo, nomeArquivo);
        this.baixando.set(false);
      },
      error: (resposta: HttpErrorResponse) => {
        void lerMensagemDeErroDeDownload(
          resposta,
          'Não foi possível baixar o arquivo. Tente novamente em instantes.',
        ).then((mensagem) => {
          this.mensagemErro.set(mensagem);
          this.baixando.set(false);
        });
      },
    });
  }

  private obterMensagemErro(resposta: HttpErrorResponse): string {
    const problema = resposta.error as ApiValidationProblemDetails | null;

    return problema?.detail ?? 'Não foi possível carregar o aluno. Tente novamente em instantes.';
  }
}
