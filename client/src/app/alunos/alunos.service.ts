import { Observable } from 'rxjs';

import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Service } from '@angular/core';

import { URL_API } from '../auth/auth.tokens';
import { AlunoDto, CertificadoDoAlunoDto } from './alunos.models';

@Service()
export class AlunosService {
  private readonly http = inject(HttpClient);
  private readonly urlApi = inject(URL_API);

  listar(busca: string): Observable<readonly AlunoDto[]> {
    const termo = busca.trim();
    const params = termo ? new HttpParams().set('busca', termo) : new HttpParams();

    return this.http.get<readonly AlunoDto[]>(`${this.urlApi}/alunos`, { params });
  }

  obterPorId(alunoId: string): Observable<AlunoDto> {
    return this.http.get<AlunoDto>(`${this.urlApi}/alunos/${alunoId}`);
  }

  listarCertificados(alunoId: string): Observable<readonly CertificadoDoAlunoDto[]> {
    return this.http.get<readonly CertificadoDoAlunoDto[]>(
      `${this.urlApi}/alunos/${alunoId}/certificados`,
    );
  }

  baixarCertificado(alunoId: string, certificadoId: string): Observable<Blob> {
    return this.http.get(
      `${this.urlApi}/alunos/${alunoId}/certificados/${certificadoId}/download`,
      {
        responseType: 'blob',
      },
    );
  }

  /** Lista vazia baixa todos os certificados do aluno. */
  baixarZip(alunoId: string, certificadoIds: readonly string[]): Observable<Blob> {
    return this.http.post(
      `${this.urlApi}/alunos/${alunoId}/certificados/download`,
      { certificadoIds },
      { responseType: 'blob' },
    );
  }
}
