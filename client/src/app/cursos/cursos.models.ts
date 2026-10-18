export interface CursoDto {
  readonly id: string;
  readonly nome: string;
  readonly descricao: string;
  readonly cargaHoraria: number;
  readonly dataConclusao: string;
}

export interface CriarCursoRequest {
  readonly nome: string;
  readonly descricao?: string;
  readonly cargaHoraria: number;
  readonly dataConclusao: string;
}

/** Resposta de POST /cursos: a API devolve apenas o id e o nome do curso criado. */
export interface CursoCadastradoDto {
  readonly id: string;
  readonly nome: string;
}
