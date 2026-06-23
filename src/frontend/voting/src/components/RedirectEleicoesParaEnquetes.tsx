import { Navigate } from 'react-router-dom';

export function RedirectEleicoesParaEnquetes() {
  return <Navigate to="/enquetes" replace />;
}
