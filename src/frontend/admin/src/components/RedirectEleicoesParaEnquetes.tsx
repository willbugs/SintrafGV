import { Navigate, useLocation } from 'react-router-dom';

/** Redireciona rotas antigas /eleicoes/* para /enquetes/* */
export function RedirectEleicoesParaEnquetes() {
  const { pathname, search, hash } = useLocation();
  const destino = pathname.replace(/^\/eleicoes(?=\/|$)/, '/enquetes') + search + hash;
  return <Navigate to={destino} replace />;
}
