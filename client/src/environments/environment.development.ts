// Em desenvolvimento o client chama /api, e o dev server encaminha para a API local
// (veja proxy.conf.json). Isso evita CORS, que a API ainda não configura.
export const environment = {
  urlApi: '/api',
};
