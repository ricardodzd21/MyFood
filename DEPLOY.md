# Deploy — MyFood

Mesmo modelo do Carlink: **a pipeline compila e publica**. O GitHub Actions builda o front e a API no runner e envia os artefatos prontos por **SCP** (usando a chave SSH). O servidor só recebe os arquivos e roda `docker compose`. **Nada de git no servidor.**

- **Servidor**: `69.197.168.215` (aaPanel)
- **Path**: `/www/wwwroot/myfood`
- **Containers**: `myfood-api` (porta 8080 interna, **não exposta no host**), `myfood-front` (host **3021** → 80)
- **Banco**: Postgres do host — a API cria o database `MyFood` e as tabelas sozinha no boot (EF `Migrate()`)
- **Proxy + SSL**: feitos pelo **aaPanel** (site do domínio → proxy reverso p/ `127.0.0.1:3021`), igual aos outros sites

---

## 1. Secrets no GitHub (uma vez)

Repo → **Settings → Secrets and variables → Actions**:

| Secret | Valor |
|--------|-------|
| `SERVER_HOST` | `69.197.168.215` |
| `SERVER_USER` | `root` |
| `SSH_PRIVATE_KEY` | a mesma chave SSH usada nos outros projetos |
| `SERVER_PORT` | `22` (opcional) |

## 2. server.env no servidor (uma vez)

A pipeline **nunca** sobrescreve o `server.env` (ele guarda os secrets). Crie uma vez:

```bash
mkdir -p /www/wwwroot/myfood
nano /www/wwwroot/myfood/server.env
```

Conteúdo (ver `server.env.example`) — ajuste a connection string p/ o mesmo Postgres dos outros projetos:

```
ConnectionStrings__DefaultConnection=Host=172.17.0.1;Port=35432;Database=MyFood;Username=postgres;Password=SENHA_DO_POSTGRES
JwtSecret=uma-chave-longa-aleatoria-32+caracteres
Admin__Email=seu-email@exemplo.com
Admin__Password=sua-senha-forte
AllowedOrigins__0=https://SEU_DOMINIO
Gemini__ApiKey=            # opcional (chave grátis do Google AI Studio)
Gemini__Model=gemini-2.5-flash
```

## 3. Publicar

Só dar push na `main` (ou rodar o workflow manual em Actions → Deploy MyFood → Run workflow):

```bash
git push origin main
```

A pipeline: build front + API → SCP p/ `/www/wwwroot/myfood` → `docker compose build && up -d`.
> No **primeiríssimo** deploy, se o `server.env` ainda não existir, o passo final falha de propósito avisando. Crie o `server.env` (passo 2) e rode o deploy de novo.

## 4. Proxy + SSL no aaPanel (uma vez)

No painel, crie o site do domínio do MyFood e configure como **proxy reverso** para `http://127.0.0.1:3021` (container `myfood-front`), depois emita o **Let's Encrypt** — exatamente como você fez com carlink / servicelink / goindaiatuba. O A record do domínio deve apontar para `69.197.168.215`.

---

## Portas usadas (evitar conflito)
| Projeto | API | Front |
|---------|-----|-------|
| Carlink | 5031 | 3002 |
| ServiceLink | 5032 | 3003 |
| GoIndaiatuba | 5050 | 3050 |
| **MyFood** | interna (não exposta) | **3021** |

## IA (opcional)
Preencha `Gemini__ApiKey` no `server.env` (chave grátis em https://aistudio.google.com/apikey) e rode o deploy. Sem chave, o botão "Analisar foto" não aparece e o app funciona no manual.
