# Deploy — MyFood

Mesmo padrão Docker dos outros projetos InterConsult.

- **Servidor**: `69.197.168.215` (mesmo do Carlink/ServiceLink/GoIndaiatuba)
- **Path**: `/www/wwwroot/myfood`
- **Containers**: `myfood-api` (host 5033 → 8080), `myfood-front` (host 3004 → 80)
- **Banco**: Postgres em `172.17.0.1:35432`, database `MyFood`
- **Nginx do host**: `https://${DOMAIN}` → `127.0.0.1:3004` (front) → `myfood-api:8080` (api)
- **CI/CD**: push em `main` → GitHub Actions → SSH → `git pull && docker compose build --no-cache && up -d`

---

## 1. Criar o repositório no GitHub

1. Crie um repo **privado** chamado **`MyFood`** na conta `ricardodzd21`
   (https://github.com/new — Owner: ricardodzd21, Name: MyFood, Private, **sem** README/gitignore).
2. No projeto local, envie o código (ver seção 2).
3. Adicione os **secrets** (Settings → Secrets and variables → Actions):
   - `SERVER_HOST` = `69.197.168.215`
   - `SERVER_USER` = `root`
   - `SSH_PRIVATE_KEY` = conteúdo da sua chave SSH privada (a mesma dos outros projetos)
   - `SERVER_PORT` = `22` (opcional)

---

## 2. Enviar o código (primeira vez)

Já feito o `git init` + commit local. Só falta apontar o remote e dar push:

```bash
cd /c/Projects/Interconsult/MyFood
git remote add origin https://github.com/ricardodzd21/MyFood.git   # (já configurado pelo assistente)
git branch -M main
git push -u origin main
```

Se pedir login, use seu usuário GitHub + um Personal Access Token como senha
(ou `gh auth login` se tiver o GitHub CLI).

---

## 3. Comprar/apontar domínio

DNS no registrador → registro A:
- `SEU_DOMINIO` → `69.197.168.215`
- `www.SEU_DOMINIO` → `69.197.168.215`

---

## 4. Primeira instalação no servidor

```bash
ssh root@69.197.168.215
cd /tmp
git clone git@github.com:ricardodzd21/MyFood.git
cd MyFood
DOMAIN=seudominio.com ./deploy/setup.sh
```

O `setup.sh`:
1. Clona o repo em `/www/wwwroot/myfood`
2. Cria `server.env` (você edita: senha do Postgres, JwtSecret, Admin, Gemini opcional)
3. Garante o database `MyFood` no Postgres
4. Configura nginx do host + gera SSL Let's Encrypt
5. `docker compose build && up -d`

> As tabelas e os seeds (categorias, subcategorias, atributos) são criados **automaticamente** no boot da API (EF `Migrate()`).

---

## 5. Atualizações

Cada `git push origin main` dispara o GitHub Actions (SSH → pull → rebuild → up).

Manual no servidor:
```bash
cd /www/wwwroot/myfood
./deploy/update.sh    # pull + rebuild + up
./deploy/restart.sh   # só restart (após editar server.env)
```

---

## 6. IA (opcional)

Para ativar o "Analisar foto":
1. Pegue uma chave grátis em https://aistudio.google.com/apikey
2. No servidor, adicione ao `server.env`: `Gemini__ApiKey=SUA_CHAVE`
3. `./deploy/restart.sh`

Sem chave, o app funciona normalmente no preenchimento manual.

---

## 7. Secrets do GitHub (resumo)

| Secret | Valor |
|--------|-------|
| `SERVER_HOST` | `69.197.168.215` |
| `SERVER_USER` | `root` |
| `SSH_PRIVATE_KEY` | chave SSH privada (mesma dos outros) |
| `SERVER_PORT` | `22` (opcional) |
