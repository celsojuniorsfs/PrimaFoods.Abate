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
