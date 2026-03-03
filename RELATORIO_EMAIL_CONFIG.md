# Relatório: Formulário → Banco → Envio de E-mail

## Fluxo verificado

### 1. Formulário (ConfiguracaoEmailPage.tsx)
- Campos: `smtpHost`, `smtpPort`, `usarSsl`, `smtpUsuario`, `smtpSenha`, `emailRemetente`, `nomeRemetente`, `habilitado`
- Ao salvar: `api.post('/api/configuracao-email', config)` — envia o objeto completo em camelCase

### 2. API Salvar (ConfiguracaoEmailController.Salvar)
- Recebe `ConfiguracaoEmailDto` com `UsarSsl` (mapeia de `usarSsl` do JSON)
- Monta `ConfiguracaoEmail` e chama `_repository.SalvarAsync(config)`

### 3. Repository (ConfiguracaoEmailRepository.SalvarAsync)
- Atualiza `existente.UsarSsl = config.UsarSsl` — **persiste corretamente**

### 4. API Testar (ConfiguracaoEmailController.Testar)
- Lê config do banco via `_emailService.TestarConfiguracaoAsync(destinatario)`
- O EmailService chama `_configRepository.ObterAsync()` — **usa o que está no banco**

### 5. EmailService.EnviarAsync
- Usa `config.SmtpHost`, `config.SmtpPort`, `config.UsarSsl`, etc.
- `ObterOpcoesSslParaTestar(config)` define quais opções de SSL tentar

---

## Problemas encontrados

### Problema 1: Lógica SSL invertida
**Arquivo:** `EmailService.cs` → `ObterOpcoesSslParaTestar`

```csharp
lista.Add(SecureSocketOptions.None);  // SEMPRE primeiro
if (config.UsarSsl)
    lista.Add(SecureSocketOptions.StartTls);  // ou SslOnConnect
```

- **UsarSsl = false:** tenta só `[None]`
- **UsarSsl = true:** tenta `[None, StartTls]` — **None é tentado primeiro**

Quando o usuário marca SSL, a primeira tentativa é sempre sem SSL. Se o servidor exigir StartTls na porta 587, a primeira tentativa falha. Se ambas falham com timeout, o erro final é o mesmo em ambos os casos.

**Correção:** Quando `UsarSsl = true`, tentar StartTls/SslOnConnect **primeiro**. Quando `UsarSsl = false`, tentar só None.

### Problema 2: Timeout do frontend (30s)
**Arquivo:** `api.ts` → `timeout: 30000`

O teste de e-mail faz várias tentativas SMTP (cada uma pode levar 30s+). O axios cancela a requisição após 30s. O usuário vê "timeout" mesmo que o backend ainda esteja tentando — e isso ocorre **independente** da configuração SSL.

**Correção:** Usar timeout maior na chamada do teste (ex: 90s).

### Problema 3: Mesmo erro em ambos os casos
Com SSL marcado: tenta None (timeout) → StartTls (timeout) → erro final: timeout  
Com SSL desmarcado: tenta None (timeout) → erro final: timeout  

O erro final é o mesmo (timeout) porque ambas as tentativas falham da mesma forma na sua rede.

---

## Resumo
- O formulário, o banco e o envio estão alinhados — os dados são persistidos e lidos corretamente.
- O que causa o mesmo erro é a **ordem das tentativas** (None sempre primeiro) e o **timeout do HTTP** (30s), que mascara o comportamento real.

---

## Correções aplicadas

1. **EmailService.ObterOpcoesSslParaTestar**
   - Antes: sempre tentava `None` primeiro; se UsarSsl, adicionava StartTls depois.
   - Depois: `UsarSsl = true` → só tenta StartTls (ou SslOnConnect na 465); `UsarSsl = false` → só tenta None.

2. **ConfiguracaoEmailPage.testar**
   - Timeout da requisição de teste aumentado de 30s para 90s.
