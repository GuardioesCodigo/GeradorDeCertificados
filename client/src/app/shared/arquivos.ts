import { HttpErrorResponse } from '@angular/common/http';

import { ApiValidationProblemDetails } from '../auth/auth.models';

/** Dispara o download de um arquivo já carregado em memória. */
export function salvarArquivo(conteudo: Blob, nomeArquivo: string): void {
  const url = URL.createObjectURL(conteudo);
  const link = document.createElement('a');

  link.href = url;
  link.download = nomeArquivo;
  link.click();

  URL.revokeObjectURL(url);
}

/** Troca caracteres que não podem aparecer em nomes de arquivo. */
export function nomeDeArquivoSeguro(nome: string): string {
  return nome.replace(/[\\/:*?"<>|]/g, '_').trim() || 'arquivo';
}

/** Com responseType 'blob', o corpo do erro (ProblemDetails) também chega como Blob. */
export async function lerMensagemDeErroDeDownload(
  resposta: HttpErrorResponse,
  mensagemPadrao: string,
): Promise<string> {
  if (resposta.error instanceof Blob) {
    try {
      const problema = JSON.parse(await resposta.error.text()) as ApiValidationProblemDetails;

      if (problema.detail) return problema.detail;
    } catch {
      // mantém a mensagem padrão
    }
  }

  return mensagemPadrao;
}
