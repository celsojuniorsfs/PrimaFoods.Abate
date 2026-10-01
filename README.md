# PrimaFoods.Abate

Painel ASP.NET Core MVC (.NET 10) para calcular e consultar o pagamento de animais abatidos.

## Pré-requisitos

- SDK do .NET 10
- Docker Desktop (no Windows, com o backend WSL 2)
- Git
- Porta **14330** livre na máquina
- Opcionais: Visual Studio 2026 e SSMS

## Como rodar

Os comandos abaixo estão em PowerShell. No bash, a única diferença é `cp .env.example .env` no lugar de `Copy-Item`.

1. Crie o `.env` a partir do exemplo e defina a senha do `sa` em `MSSQL_SA_PASSWORD` (o arquivo não é versionado):

   ```powershell
   Copy-Item .env.example .env
   ```

   A senha precisa seguir a política do SQL Server: no mínimo 8 caracteres, com maiúscula, minúscula, número e símbolo. **Com uma senha fora da política, o container não sobe.**

2. Suba o banco, que cria o `dbRecruta` e carrega os dados:

   ```powershell
   docker compose up -d
   docker compose logs db-init   # deve terminar em "Banco pronto."
   ```

3. Guarde a connection string nos user-secrets, com a mesma senha do `.env`:

   ```powershell
   dotnet user-secrets set "ConnectionStrings:Connection" "Server=localhost,14330;Database=dbRecruta;User Id=sa;Password=<senha do .env>;TrustServerCertificate=True" --project src/PrimaFoods.Abate.Web
   ```

   O `TrustServerCertificate=True` existe porque o container usa um certificado autoassinado. Vale só para o ambiente local.

4. Rode a aplicação:

   ```powershell
   dotnet run --project src/PrimaFoods.Abate.Web --launch-profile https
   ```

   Ela abre em `https://localhost:7185`. Na tela, clique em **Processar pagamentos** para executar o cálculo.

No Visual Studio, o passo 3 também pode ser feito com botão direito no projeto Web → **Gerenciar Segredos do Usuário**, e o passo 4 com F5.

Para recriar o banco do zero:

```powershell
docker compose down -v
docker compose up -d
```

Em produção, informe a connection string pela variável de ambiente `ConnectionStrings__Connection`.

## Testes

```bash
dotnet test
```

Os testes de integração sobem um SQL Server próprio com Testcontainers e exigem o Docker em execução.

O workflow `.github/workflows/ci.yml` roda os testes unitários e os de integração, em jobs separados, a cada pull request (para qualquer branch de destino) e a cada push para `develop` e `main`. Os resultados (`.trx`) ficam como artefatos da execução.

## Estrutura do repositório

```
database/                    scripts SQL aplicados em ordem (01 a 04)
database/original/           script recebido no teste, sem alterações
src/
  PrimaFoods.Abate.Domain/          enums do negócio
  PrimaFoods.Abate.Application/     casos de uso e portas
  PrimaFoods.Abate.Infrastructure/  Dapper e SQL Server
  PrimaFoods.Abate.Web/             controllers, views e composição
tests/
  PrimaFoods.Abate.UnitTests/
  PrimaFoods.Abate.IntegrationTests/  SQL Server real com Testcontainers
.github/workflows/ci.yml     roda os testes no GitHub Actions
docker-compose.yml           SQL Server e carga dos scripts de database/
```

## Atendimento aos requisitos

| Item | Onde |
|---|---|
| 1 e 1.1 a 1.4 | `dbo.spCalculateAnimalPayments` (`database/04_spCalculateAnimalPayments.sql`), com os preços e percentuais vindos da seção `Payment` |
| 1.5 | Tabela `dbo.AnimalPayments`: valor base, ágio, deságio e valor a pagar, mais `AdjustmentType` e `AdjustmentReason` |
| 2.1 | Aba **Resumo por pedido**: agrupa por pedido, fornecedor e sexo, com quantidade, peso, arrobas, valor total e valor unitário da @, e uma linha de total geral |
| 2.2 e 2.3 | Cards de machos e de fêmeas, com quantidade e percentual |
| 2.4 e 2.5 | Cards de ágio e de deságio, com quantidade, percentual e valor total |
| 2.6 | Bootstrap 5 com AdminKit: os cards empilham no celular e as tabelas rolam na horizontal |
| 2.7 | Ágio em verde com `+` e deságio em vermelho com `-`, nos cards, no resumo e no detalhe |
| Extra | Aba **Detalhe por animal** (DataTables, com busca e ordenação), que mostra o motivo de cada ajuste |

## Regras de cálculo

| Item | Regra |
|---|---|
| Arroba | 15 kg |
| Preço base da @ | Macho R$ 100,00 · Fêmea R$ 50,00 |
| Valor base | `peso / 15 * preço da @` |
| Ágio | animal com **0 dentes**: +10% sobre o valor base |
| Deságio | animal com **6 ou mais dentes**: -10% sobre o valor base |
| Valor a pagar | `valor base + ágio - deságio` |

Preços e percentuais são configuráveis na seção `Payment` do `appsettings.json` (`MaleArrobaPrice`, `FemaleArrobaPrice`, `PremiumPercentage`, `DiscountPercentage`) e enviados como parâmetros da stored procedure. A aplicação não sobe se os valores forem inválidos (preço ≤ 0 ou percentual fora de 0–100). Os 15 kg por arroba ficam fixos na procedure.

Cada cálculo apaga e regrava `dbo.AnimalPayments`; só o último cálculo é mantido. Animais com sexo fora de M/F, ou sem peso, dentes ou pedido, não geram pagamento; a tela mostra um aviso com quantos foram ignorados.

### Premissas

O enunciado deixa alguns pontos em aberto. Estas foram as interpretações adotadas:

- **Arroba de 15 kg.** O enunciado fala em peso multiplicado por valor, mas o valor é por arroba.
- **Ágio e deságio incidem sobre o valor base do próprio animal.**
- **Arredondamento em 2 casas por animal.** O ajuste é calculado sobre o valor base já arredondado, então a soma das linhas bate com o total.
- **1, 3 e 5 dentes** não aparecem no enunciado, que cobre 0, 2 ou 4 e 6 ou mais. Foram tratados como sem ajuste. Nenhum animal da base tem esses valores.
- **Valor unitário da @ no resumo = valor total ÷ arrobas**, ou seja, o valor efetivo, que já inclui ágio e deságio. No pedido 1, os machos saem a R$ 104,10 e não a R$ 100,00.
- **Animais com dados inválidos** (sexo fora de M/F, sem peso, sem dentes ou sem pedido) são ignorados e contados à parte.

## Observações sobre o script original

O script recebido está intacto em `database/original/Script_Teste_SQLServer.sql`. O que a aplicação executa são os scripts de `database/`, com estas diferenças:

- O original começa com `\c dbRecruta`, que é comando do `psql` (PostgreSQL), e não tem separadores `GO`. Assim, não roda no SQL Server. O `01_create_base.sql` cria o banco, usa `USE` e separa os lotes com `GO`.
- O comentário `--criar foreing key` virou a FK `FK_Animals_Orders` (`02_foreign_keys.sql`), junto com um índice em `Animals.OrderId`, porque o SQL Server não indexa a coluna da FK sozinho.
- **Tabelas e colunas foram traduzidas para o inglês**, para seguir o padrão do código: `tabAnimais` → `Animals`, `tabPedido` → `Orders`, `Animal` → `AnimalId`, `Sexo` → `Sex`, `codPedido` → `OrderId`, `qtdDentes` → `TeethCount`, `peso` → `Weight` e `Fornecedor` → `Supplier`. Os dados não mudaram.
- As colunas do original aceitam `NULL`, e daí vem o tratamento de dados inválidos na procedure.
- Alguns fornecedores vêm sem acento no próprio script ("Simo Silva", "Thom Silva", "Estevo Silva"). Foram mantidos como recebidos.

## Arquitetura

```
Web → Application ← Infrastructure
          ↓
        Domain (enums)
```

- **Domain**: enums do negócio (`Sex`, `AdjustmentType`).
- **Application**: casos de uso, portas (`IPaymentCalculator`, `IAnimalPaymentReader`) e o modelo de leitura `AnimalPaymentView`.
- **Infrastructure**: implementação com Dapper e SQL Server.
- **Web**: controllers, views e composição de dependências.

### Decisão: a regra de cálculo fica na stored procedure

A regra de pagamento (preço da arroba, ágio, deságio) é implementada **somente** em `database/04_spCalculateAnimalPayments.sql`. A aplicação .NET dispara o cálculo (`IPaymentCalculator`) e lê o resultado (`IAnimalPaymentReader`); não recalcula nada.

- **Por quê**: o cálculo é em lote e roda no banco, evitando trafegar todos os animais para a aplicação.
- **Consequência**: a regra não tem teste unitário em C#. Ela deve ser coberta por testes de integração contra um SQL Server real.
- **Alternativa descartada**: mover a regra para o Domain (`PaymentPolicy`). Pode ser reavaliada se a regra crescer ou precisar de testes sem banco.
