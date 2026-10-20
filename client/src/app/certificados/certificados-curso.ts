import { forkJoin, Subscription, switchMap, takeWhile, timer } from 'rxjs';

import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, DestroyRef, inject, input, OnInit, signal } from '@angular/core';
import { form, FormField, required } from '@angular/forms/signals';
import { LucideDownload, LucideLoaderCircle, LucideSend } from '@lucide/angular';

import { ApiValidationProblemDetails } from '../auth/auth.models';
import {
  CertificadoDto,
  StatusCertificado,
  StatusSolicitacao,
  StatusSolicitacaoDto,
} from './certificados.models';
import { CertificadosService } from './certificados.service';

const INTERVALO_ATUALIZACAO_MS = 2000;
const TAMANHO_MAXIMO_NOME = 200;

const ROTULOS_SOLICITACAO: Record<StatusSolicitacao, string> = {
  Pendente: 'Na fila',
  GerandoCertificados: 'Gerando certificados',
  GerandoZip: 'Compactando arquivos',
  Concluido: 'Concluído',
  Falha: 'Falhou',
};

const ROTULOS_CERTIFICADO: Record<StatusCertificado, string> = {
  Pendente: 'Pendente',
  Gerado: 'Gerado',
  Falha: 'Falhou',
};

const CLASSES_CERTIFICADO: Record<StatusCertificado, string> = {
  Pendente: 'bg-slate-100 text-slate-700',
  Gerado: 'bg-emerald-50 text-emerald-800',
  Falha: 'bg-rose-50 text-rose-800',
};


type EstadoEtapa = 'concluida' | 'atual' | 'pendente';

interface EtapaProcessamento {
  readonly rotulo: string;
  readonly estado: EstadoEtapa;
}

interface EmissaoFormModel {
  readonly nomes: string;
}

const ETAPAS: readonly { readonly status: StatusSolicitacao; readonly rotulo: string }[] = [
  { status: 'Pendente', rotulo: 'Na fila' },
  { status: 'GerandoCertificados', rotulo: 'Gerando certificados' },
  { status: 'GerandoZip', rotulo: 'Compactando arquivos' },
  { status: 'Concluido', rotulo: 'Concluído' },
];



@Component({
  imports: [FormField, LucideDownload, LucideLoaderCircle, LucideSend],
  selector: 'app-certificados-curso',
  templateUrl: './certificados-curso.html',
})
export class CertificadosCurso implements OnInit {
  readonly cursoId = input.required<string>();

  private readonly certificadosService = inject(CertificadosService);
  private readonly destroyRef = inject(DestroyRef);

  private acompanhamento?: Subscription;

  protected readonly status = signal<StatusSolicitacaoDto | null>(null);
  protected readonly certificados = signal<readonly CertificadoDto[]>([]);
  protected readonly carregando = signal(true);
  protected readonly enviandoFormulario = signal(false);
  protected readonly baixandoZip = signal(false);
  protected readonly mensagemErro = signal('');

  protected readonly emAndamento = computed(() => {
    const status = this.status()?.status;

    return status === 'Pendente' || status === 'GerandoCertificados' || status === 'GerandoZip';
  });

  protected readonly percentualProcessado = computed(() => {
    const status = this.status();

    if (!status || status.totalCertificados === 0) return 0;

    const processados = status.certificadosGerados + status.certificadosComFalha;

    return Math.round((processados / status.totalCertificados) * 100);
  });

  /** Linha do tempo do processamento; some quando a solicitação falha. */
  protected readonly etapas = computed<readonly EtapaProcessamento[]>(() => {
    const status = this.status()?.status;

    if (!status || status === 'Falha') return [];

    const indiceAtual = ETAPAS.findIndex((etapa) => etapa.status === status);

    return ETAPAS.map((etapa, indice) => ({
      rotulo: etapa.rotulo,
      estado:
        indice < indiceAtual || status === 'Concluido'
          ? 'concluida'
          : indice === indiceAtual
            ? 'atual'
            : 'pendente',
    }));
  });

  /** O certificado gerado mais recentemente vem primeiro; os ainda pendentes ficam no fim. */
  protected readonly certificadosOrdenados = computed(() =>
    [...this.certificados()].sort((a, b) => {
      const dataA = a.dataGeracao ? new Date(a.dataGeracao).getTime() : Number.NEGATIVE_INFINITY;
      const dataB = b.dataGeracao ? new Date(b.dataGeracao).getTime() : Number.NEGATIVE_INFINITY;

      return dataB - dataA || a.nomeAluno.localeCompare(b.nomeAluno);
    }),
  );

  protected readonly modeloEmissao = signal<EmissaoFormModel>({ nomes: '' });
  protected readonly formularioEmissao = form(this.modeloEmissao, (modelo) => {
    required(modelo.nomes, { message: 'Informe ao menos um aluno, um nome por linha.' });
  });

  protected readonly quantidadeDeNomes = computed(
    () => this.extrairNomes(this.modeloEmissao().nomes).length,
  );

  ngOnInit(): void {
    this.destroyRef.onDestroy(() => this.acompanhamento?.unsubscribe());

    this.acompanhar();
  }

  protected atualizar(): void {
    this.acompanhar();
  }

  protected emitirCertificados(event: SubmitEvent): void {
    event.preventDefault();

    const formulario = this.formularioEmissao();

    if (formulario.invalid()) {
      formulario.markAsTouched();
      return;
    }

    const nomes = this.extrairNomes(this.modeloEmissao().nomes);

    if (nomes.length === 0) {
      this.mensagemErro.set('Informe ao menos um aluno, um nome por linha.');
      return;
    }

    const nomeLongo = nomes.find((nome) => nome.length > TAMANHO_MAXIMO_NOME);

    if (nomeLongo) {
      this.mensagemErro.set(
        `Cada nome pode ter no máximo ${TAMANHO_MAXIMO_NOME} caracteres ("${nomeLongo.slice(0, 30)}…").`,
      );
      return;
    }

    this.enviandoFormulario.set(true);
    this.mensagemErro.set('');

    this.certificadosService.solicitar(this.cursoId(), nomes).subscribe({
      next: () => {
        this.enviandoFormulario.set(false);
        this.modeloEmissao.set({ nomes: '' });
        this.acompanhar();
      },
      error: (resposta: HttpErrorResponse) => {
        this.enviandoFormulario.set(false);
        this.mensagemErro.set(
          this.obterMensagemErroApi(
            resposta,
            'Não foi possível solicitar os certificados. Tente novamente em instantes.',
          ),
        );
      },
    });
  }

  protected baixarZip(): void {
    const cursoId = this.cursoId();

    this.baixandoZip.set(true);
    this.mensagemErro.set('');

    this.certificadosService.baixarZip(cursoId).subscribe({
      next: (arquivo) => {
        const url = URL.createObjectURL(arquivo);
        const link = document.createElement('a');

        link.href = url;
        link.download = `certificados-${cursoId}.zip`;
        link.click();

        URL.revokeObjectURL(url);
        this.baixandoZip.set(false);
      },
      error: (resposta: HttpErrorResponse) => void this.tratarErroDeDownload(resposta),
    });
  }

  protected rotuloSolicitacao(status: StatusSolicitacao): string {
    return ROTULOS_SOLICITACAO[status];
  }

  protected rotuloCertificado(status: StatusCertificado): string {
    return ROTULOS_CERTIFICADO[status];
  }

  protected classesCertificado(status: StatusCertificado): string {
    return CLASSES_CERTIFICADO[status];
  }

  protected formatarDataHora(data: string): string {
    return new Date(data).toLocaleString('pt-BR');
  }

  /** Consulta o status e a lista a cada 2s enquanto houver processamento em andamento. */
  private acompanhar(): void {
    const cursoId = this.cursoId();

    this.acompanhamento?.unsubscribe();
    this.carregando.set(true);

    this.acompanhamento = timer(0, INTERVALO_ATUALIZACAO_MS)
      .pipe(
        switchMap(() =>
          forkJoin({
            status: this.certificadosService.obterStatus(cursoId),
            certificados: this.certificadosService.listar(cursoId),
          }),
        ),
        takeWhile(
          ({ status }) =>
            status !== null &&
            (status.status === 'Pendente' ||
              status.status === 'GerandoCertificados' ||
              status.status === 'GerandoZip'),
          true,
        ),
      )
      .subscribe({
        next: ({ status, certificados }) => {
          this.status.set(status);
          this.certificados.set(certificados);
          this.carregando.set(false);
        },
        error: (resposta: HttpErrorResponse) => {
          this.carregando.set(false);
          this.mensagemErro.set(
            this.obterMensagemErroApi(
              resposta,
              'Não foi possível consultar os certificados. Tente atualizar em instantes.',
            ),
          );
        },
      });
  }

  private extrairNomes(texto: string): string[] {
    return texto
      .split(/\r?\n/)
      .map((nome) => nome.trim())
      .filter((nome) => nome.length > 0);
  }

  private async tratarErroDeDownload(resposta: HttpErrorResponse): Promise<void> {
    let mensagem = 'Não foi possível baixar o arquivo. Tente novamente em instantes.';

    // Com responseType 'blob', o corpo do erro (ProblemDetails) também chega como Blob.
    if (resposta.error instanceof Blob) {
      try {
        const problema = JSON.parse(await resposta.error.text()) as ApiValidationProblemDetails;

        if (problema.detail) mensagem = problema.detail;
      } catch {
        // mantém a mensagem padrão
      }
    }

    this.mensagemErro.set(mensagem);
    this.baixandoZip.set(false);
  }

  private obterMensagemErroApi(resposta: HttpErrorResponse, mensagemPadrao: string): string {
    const problema = resposta.error as ApiValidationProblemDetails | null;

    if (problema?.errors) {
      const mensagens = Object.values(problema.errors).flat();

      if (mensagens.length > 0) return mensagens.join('\n');
    }

    return problema?.detail ?? mensagemPadrao;
  }
}
