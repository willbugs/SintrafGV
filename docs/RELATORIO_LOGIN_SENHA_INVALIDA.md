# Relatório: Sem aviso quando senha inválida no login

## Causa

O interceptor de resposta do axios (`api.ts`) trata **todo** 401 assim:

```javascript
if (error.response?.status === 401) {
  localStorage.removeItem('sintrafgv_token');
  localStorage.removeItem('sintrafgv_user');
  window.location.href = '/login';  // ← REDIRECIONA
}
return Promise.reject(error);
```

Quando o usuário erra a senha na tela de login:
1. A API retorna 401 (Credenciais inválidas)
2. O interceptor faz `window.location.href = '/login'`
3. A página recarrega antes do toast ser exibido
4. O usuário não vê nenhuma mensagem de erro

O `LoginPage` até chama `toast.error()` no `catch`, mas o recarregamento da página impede que o toast apareça.

## Correção

Não redirecionar para `/login` quando o 401 vier da própria requisição de login (`/api/auth/login`). Assim o erro segue para o `catch` e o toast é exibido normalmente.
