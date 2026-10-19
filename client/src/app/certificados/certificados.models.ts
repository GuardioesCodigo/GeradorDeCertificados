/** Valores devolvidos pela API em `status` (enum serializado como texto). */
export type StatusSolicitacao =
  'Pendente' | 'GerandoCertificados' | 'GerandoZip' | 'Concluido' | 'Falha';

export type StatusCertificado = 'Pendente' | 'Gerado' | 'Falha';

export interface AlunoRequest {
  readonly nome: string;
}

export interface SolicitarCertificadosRequest {
  readonly alunos: readonly AlunoRequest[];
}

/** Resposta de POST /cursos/{cursoId}/certificados (202 Accepted). */
export interface SolicitacaoCriadaDto {
  readonly solicitacaoId: string;
  readonly cursoId: string;
}

/** Resposta de GET /cursos/{cursoId}/status. */
export interface StatusSolicitacaoDto {
  readonly solicitacaoId: string;
  readonly cursoId: string;
  readonly status: StatusSolicitacao;
  readonly dataSolicitacao: string;
  readonly totalCertificados: number;
  readonly certificadosGerados: number;
  readonly certificadosComFalha: number;
}

/** Item de GET /cursos/{cursoId}/certificados. */
export interface CertificadoDto {
  readonly id: string;
  readonly nomeAluno: string;
  readonly status: StatusCertificado;
  readonly caminhoArquivo: string | null;
  readonly dataGeracao: string | null;
}
