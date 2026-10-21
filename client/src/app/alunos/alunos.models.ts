export interface AlunoDto {
  readonly id: string;
  readonly nome: string;
}

/** Item de GET /alunos/{alunoId}/certificados: um certificado já gerado, com o curso dele. */
export interface CertificadoDoAlunoDto {
  readonly certificadoId: string;
  readonly cursoId: string;
  readonly cursoNome: string;
  readonly cargaHoraria: number;
  readonly dataConclusao: string;
  readonly dataGeracao: string | null;
}
