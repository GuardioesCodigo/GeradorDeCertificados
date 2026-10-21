import { debounceTime, distinctUntilChanged, Subject, switchMap, tap } from 'rxjs';

import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { LucideUsers } from '@lucide/angular';

import { ApiValidationProblemDetails } from '../../auth/auth.models';
import { AlunoDto } from '../alunos.models';
import { AlunosService } from '../alunos.service';

@Component({
  imports: [LucideUsers, RouterLink],
  selector: 'app-listar-alunos',
  templateUrl: './listar-alunos.html',
})
export class ListarAlunos implements OnInit {
  private readonly alunosService = inject(AlunosService);
  private readonly destroyRef = inject(DestroyRef);

  private readonly buscas = new Subject<string>();

  protected readonly alunos = signal<readonly AlunoDto[]>([]);
  protected readonly carregando = signal(true);
  protected readonly mensagemErro = signal('');
  protected readonly busca = signal('');

  ngOnInit(): void {
    this.buscas
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        tap(() => {
          this.carregando.set(true);
          this.mensagemErro.set('');
        }),
        switchMap((termo) => this.alunosService.listar(termo)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (alunos) => {
          this.alunos.set(alunos);
          this.carregando.set(false);
        },
        error: (resposta: HttpErrorResponse) => {
          this.mensagemErro.set(this.obterMensagemErro(resposta));
          this.carregando.set(false);
        },
      });

    this.buscas.next('');
  }

  protected atualizarBusca(texto: string): void {
    this.busca.set(texto);
    this.buscas.next(texto);
  }

  private obterMensagemErro(resposta: HttpErrorResponse): string {
    const problema = resposta.error as ApiValidationProblemDetails | null;

    return problema?.detail ?? 'Não foi possível carregar os alunos. Tente novamente em instantes.';
  }
}
