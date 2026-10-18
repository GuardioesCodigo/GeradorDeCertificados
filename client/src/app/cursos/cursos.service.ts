import { catchError, forkJoin, map, Observable, of, tap } from 'rxjs';

import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';

import { AuthService } from '../auth/auth.service';
import { URL_API } from '../auth/auth.tokens';
import { CriarCursoRequest, CursoCadastradoDto, CursoDto } from './cursos.models';

/**
 * A API não possui `GET /cursos` (listagem). Enquanto ela não existir, o client guarda no
 * navegador os ids dos cursos cadastrados por cada usuário e carrega cada um por
 * `GET /cursos/{id}`. Quando a API ganhar a listagem, basta trocar o corpo de `listar()`
 * por `this.http.get<readonly CursoDto[]>(`${this.urlApi}/cursos`)`.
 */
@Service()
export class CursosService {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly urlApi = inject(URL_API);

  listar(): Observable<readonly CursoDto[]> {
    const ids = this.lerIdsDosCursos();

    if (ids.length === 0) return of([]);

    return forkJoin(
      ids.map((id) => this.obterPorId(id).pipe(catchError(() => of<CursoDto | null>(null)))),
    ).pipe(
      map((cursos) =>
        cursos
          .filter((curso): curso is CursoDto => curso !== null)
          .sort((a, b) => a.nome.localeCompare(b.nome)),
      ),
    );
  }

  cadastrar(requisicao: CriarCursoRequest): Observable<CursoCadastradoDto> {
    return this.http
      .post<CursoCadastradoDto>(`${this.urlApi}/cursos`, requisicao)
      .pipe(tap((curso) => this.registrarId(curso.id)));
  }

  obterPorId(cursoId: string): Observable<CursoDto> {
    return this.http.get<CursoDto>(`${this.urlApi}/cursos/${cursoId}`);
  }

  private chaveArmazenamento(): string | null {
    const usuarioId = this.authService.sessao()?.usuario.id;
    const email = this.authService.sessao()?.usuario.email;
    const identificador = usuarioId || email;

    return identificador ? `certifica+.cursos.${identificador}` : null;
  }

  private lerIdsDosCursos(): string[] {
    const chave = this.chaveArmazenamento();

    if (!chave) return [];

    try {
      const ids = JSON.parse(localStorage.getItem(chave) ?? '[]') as unknown;

      return Array.isArray(ids) ? ids.filter((id): id is string => typeof id === 'string') : [];
    } catch {
      return [];
    }
  }

  private registrarId(cursoId: string): void {
    const chave = this.chaveArmazenamento();

    if (!chave) return;

    const ids = this.lerIdsDosCursos();

    if (!ids.includes(cursoId)) {
      localStorage.setItem(chave, JSON.stringify([...ids, cursoId]));
    }
  }
}
