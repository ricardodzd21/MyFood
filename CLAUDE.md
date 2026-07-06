# MyFood — Meu catálogo de comidas e bebidas

App **pessoal** para catalogar tudo que você consome (vinhos, cervejas, comidas, cafés...). Um único admin (você) cadastra e administra tudo. Segue o mesmo template dos projetos InterConsult (Carlink/ServiceLink/GoIndaiatuba), porém **enxuto**: sem pagamento, planos, cupons ou multiusuário.

## Identidade
- **Marca**: MyFood
- **Cor principal**: âmbar (#f59e0b) sobre neutro stone/pedra; header escuro (#1c1917)
- **Ícone**: UtensilsCrossed (lucide-react)
- **Código/infra**: "MyFood" (namespace `MyFood_api`, localStorage `myfood_token`, containers `myfood-*`)

## Portas
| Serviço | Container | Porta |
|---------|-----------|-------|
| API | myfood-api | 8080 (interna, não exposta no host) |
| Front | myfood-front | 3021 → 80 |
| DB | Postgres Docker | 172.17.0.1:35432, Database=MyFood |

## Stack
- **Backend**: .NET 8 Minimal API + EF Core 8 + PostgreSQL (Npgsql), JWT + BCrypt. Um arquivo `Program.cs` com todos os endpoints, `Models.cs`, `Data/AppDbContext.cs`.
- **Frontend**: React 19 + Vite 6 + TypeScript + Tailwind v4 (`@tailwindcss/vite`) + react-router 7 + axios + lucide-react.

## Entidades
| Entidade | Função |
|----------|--------|
| User | Só login. Admin único criado no boot (config `Admin:Email`/`Admin:Password`) |
| Category | Vinhos, Cervejas, Destilados, Comidas, Cafés... (Icon emoji, Color, Order) |
| Subcategory | Tinto Suave, IPA, Massa, Sobremesa... (pertence a uma Category) |
| CategoryAttribute | Atributos **sugeridos** por categoria (ex: Vinhos → Teor, Origem, Safra) |
| Item | O item catalogado: Name, Category, Subcategory?, Description?, **Rating (0-5)**, **IsFavorite**, ConsumedAt? |
| ItemPhoto | Fotos do item — **máximo 3** (validado no backend), uma IsMain |
| ItemAttribute | Atributos **flexíveis** nome→valor (Teor=13%, Origem=Chile, Estabelecimento=Restaurante X) |

**Modelo de atributos flexíveis**: cada item guarda pares nome/valor livres, então qualquer categoria cabe sem mudar o schema. Vinho tem Teor/Origem/Safra; comida tem Estabelecimento (texto livre, sem pré-cadastro)/Ingredientes. A categoria só *sugere* os campos; nada é obrigatório.

## Endpoints (todos exigem JWT, exceto login)
- `POST /api/auth/login` · `GET /api/auth/me`
- `GET/POST/PUT/DELETE /api/categories` · `POST/PUT/DELETE /api/subcategories`
- `GET /api/items?category=&subcategory=&favorite=&minRating=&q=&sort=` · `GET /api/items/{id}`
- `POST/PUT/DELETE /api/items` · `POST /api/items/{id}/toggle-favorite`
- `GET /api/stats` (dashboard) · `POST /api/upload` (fotos → wwwroot/uploads)
- `GET /api/ai/status` · `POST /api/ai/analyze` (IA lê foto e devolve campos)

## IA — preencher item a partir da foto (opcional, gratuito)
- Provedor: **Google Gemini Flash** (free tier). Chave grátis em https://aistudio.google.com/apikey
- Config: `Gemini__ApiKey` (+ `Gemini__Model`, padrão `gemini-2.5-flash`). **Sem chave o app funciona normal**; com chave aparece o botão "Analisar foto" no formulário.
- Fluxo: foto (multipart) → `POST /api/ai/analyze` → Gemini com a lista de categorias existentes → JSON `{ name, category, subcategory, description, attributes[] }` → o form é pré-preenchido para revisão. A **nota** e o **favorito** ficam sempre com o usuário.
- `Services/GeminiService.cs` usa `responseSchema` (saída estruturada) e mapeia a categoria sugerida para uma existente pelo nome.

## Rotas Frontend
`/login` · `/` (início/stats) · `/catalogo` (grid + filtros) · `/item/:id` · `/novo` · `/editar/:id` · `/categorias`

## Desenvolvimento local
- Backend: `cd MyFood-api && dotnet run` (porta 5000; `appsettings.Development.json` aponta pra Postgres local). Migration inicial já criada (`dotnet ef database update` ou o Migrate() no boot aplica).
- Frontend: `cd MyFood-front && npm run dev` (porta 5173; proxy `/api` e `/uploads` → localhost:5000).
- Admin de dev: `ricardodzd21@gmail.com` / `200461` (fallback; sobreponha com `Admin:Email`/`Admin:Password`).

## Deploy (padrão InterConsult)
- `docker compose up -d --build`. Front (3021) faz proxy `/api` e `/uploads` → `myfood-api:8080`.
- Secrets em `server.env` (ver `server.env.example`), nunca comitado.
- Uploads persistidos em volume `./uploads`.

## Padrões
- Backend: `ItemQuery(db)` (Includes), `MapItem(item)` (entidade→DTO), fotos cortadas em 3 no create/update, atributos via RemoveRange+reinsert na edição.
- Frontend: `useAuth()` (token em `myfood_token`), `lib/api.ts` (axios + interceptor 401), atributos sugeridos carregados ao escolher categoria, upload sequencial (máx 3).
