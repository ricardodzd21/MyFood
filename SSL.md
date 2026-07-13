# SSL / Let's Encrypt — servidor InterConsult (aaPanel)

Referência para renovar certificados dos domínios hospedados no servidor `69.197.168.215` (aaPanel).
Todos os sites são **proxy reverso para containers Docker** (`127.0.0.1:porta`), o que causa um problema recorrente na renovação — documentado aqui.

## Domínios e portas (front)

| Domínio | Container (proxy) |
|---------|-------------------|
| negociarcarro.com | 127.0.0.1:3002 |
| negociarservico.com | 127.0.0.1:3003 |
| interconsultdigital.com | 127.0.0.1:3004 |
| guaiba-go.com | 127.0.0.1:3050 |
| ebookami.com / ebookcami.com | 127.0.0.1:4002 |
| MyFood | 127.0.0.1:3021 (sem domínio ainda) |

## O problema recorrente

A validação **HTTP-01** do Let's Encrypt busca `http://DOMINIO/.well-known/acme-challenge/<token>`.
Como o site é proxy reverso para o container, o **nginx do container** responde esse caminho com o
`index.html` do SPA (`try_files ... /index.html`). O Let's Encrypt recebe HTML em vez do token e falha:

```
Invalid response ...: "<!doctype html><html>... /favicon..."
```

## A correção (permanente)

No **nginx do HOST** (aaPanel), servir o acme-challenge localmente **antes** de repassar ao container.

1. aaPanel → **Website** → clique no domínio → **Config file** (arquivo de configuração).
2. No bloco `server { listen 443 ... }`, antes do `location /` que tem `proxy_pass`, adicione:

   ```nginx
   location ^~ /.well-known/acme-challenge/ {
       root /www/wwwroot/SEU_DOMINIO;
       default_type "text/plain";
       allow all;
   }
   ```
   Troque `SEU_DOMINIO` (ex: `negociarservico.com`). O `^~` dá prioridade sobre o proxy.
3. Salvar (recarrega o nginx).
4. Aba **SSL → Let's Encrypt** → marcar domínio **+ www** → **Apply** (verificação por arquivo/HTTP).
5. Ligar **Force HTTPS**.

Depois disso a renovação automática do painel volta a funcionar.

## Casos especiais

- **negociarcarro.com**: em 2026-04 estava **sem registro A** (DNS). Conferir antes:
  `dig +short negociarcarro.com` → deve retornar `69.197.168.215`. Se não, corrigir o A no registrador.
- **ebookami.com / ebookcami.com**: estavam como **wildcard `*.ebookami.com`** (DNS-01) com TXT
  `_acme-challenge` antigo travando. Como cada um é um app único, trocar para **cert simples**
  (domínio + www) por HTTP — usa a correção acima e não depende mais de TXT. Wildcard exige DNS-01.

## Rate limit do Let's Encrypt

`The account has more than 5 failed orders within 1 hour` → **esperar 1 hora** entre tentativas.
Aplicar o fix, esperar, e reemitir **um domínio por vez**.

## Verificar validade

```bash
echo | openssl s_client -connect SEU_DOMINIO:443 2>/dev/null | openssl x509 -noout -dates
# notAfter deve estar ~3 meses à frente
```

## Renovação certbot (se algum domínio usar certbot em vez do painel)

```bash
sudo certbot certificates              # lista certs + validade
sudo certbot renew                     # renova os próximos a vencer
nginx -t && systemctl reload nginx     # ou: /www/server/nginx/sbin/nginx -s reload
```
