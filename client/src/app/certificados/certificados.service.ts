import { catchError, Observable, of } from 'rxjs';

import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Service } from '@angular/core';

import { URL_API } from '../auth/auth.tokens';
import {
  CertificadoDto,
  SolicitacaoCriadaDto,
  SolicitarCertificadosRequest,
  StatusSolicitacaoDto,
} from './certificados.models';

@Service()
export class CertificadosService {
  private readonly http = inject(HttpClient);
  private readonly urlApi = inject(URL_API);

  solicitar(cursoId: string, nomes: readonly string[]): Observable<SolicitacaoCriadaDto> {
    const requisicao: SolicitarCertificadosRequest = {
      alunos: nomes.map((nome) => ({ nome })),
    };

    return this.http.post<SolicitacaoCriadaDto>(
      `${this.urlApi}/cursos/${cursoId}/certificados`,
      requisicao,
    );
  }

  /** Devolve `null` quando o curso ainda não tem nenhuma solicitação (a API responde 404). */
  obterStatus(cursoId: string): Observable<StatusSolicitacaoDto | null> {
    return this.http
      .get<StatusSolicitacaoDto>(`${this.urlApi}/cursos/${cursoId}/status`)
      .pipe(catchError((erro: HttpErrorResponse) => this.tratarNaoEncontrado(erro, null)));
  }

  /** Devolve lista vazia quando o curso ainda não tem nenhuma solicitação. */
  listar(cursoId: string): Observable<readonly CertificadoDto[]> {
    return this.http
      .get<readonly CertificadoDto[]>(`${this.urlApi}/cursos/${cursoId}/certificados`)
      .pipe(catchError((erro: HttpErrorResponse) => this.tratarNaoEncontrado(erro, [])));
  }

  baixarZip(cursoId: string): Observable<Blob> {
    return this.http.get(`${this.urlApi}/cursos/${cursoId}/certificados/download`, {
      responseType: 'blob',
    });
  }

  private tratarNaoEncontrado<T>(erro: HttpErrorResponse, valorPadrao: T): Observable<T> {
    if (erro.status === 404) return of(valorPadrao);

    throw erro;
  }
}
