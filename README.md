# Aluguer de Veículos

Aplicação web para a gestão de veículos, clientes e contratos de aluguer.

- **API:** ASP.NET Core 8 (C#), Entity Framework Core (Code-First) e SQL Server
- **Frontend:** Vue 3, TypeScript, Pinia, Vue Router e PrimeVue
- **Testes:** xUnit, com 149 testes automáticos

## Índice

1. [Funcionalidades](#funcionalidades)
2. [Como correr o projeto](#como-correr-o-projeto)
3. [Utilizadores de teste](#utilizadores-de-teste)
4. [Permissões](#permissões)
5. [Regras de negócio](#regras-de-negócio)
6. [Arquitetura e decisões](#arquitetura-e-decisões)
7. [Autenticação](#autenticação)
8. [Testes](#testes)
9. [Trabalho futuro](#trabalho-futuro)

## Funcionalidades

- **Veículos:** registo, edição, listagem com pesquisa e filtros, e estado calculado automaticamente (Disponível, Alugado ou Reservado).
- **Clientes:** registo, edição e listagem com pesquisa e filtro por estado.
- **Contratos:** criação, listagem com o estado calculado (Agendado, Ativo, Em atraso, Concluído ou Cancelado), devolução e cancelamento.
- **Funcionários:** criação e gestão de contas, reservada ao gestor.
- **Autenticação e autorização:** login com JWT e dois perfis (Gestor e Funcionário).

## Como correr o projeto

### Pré-requisitos

- [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB, Express ou outra edição)
- [Node.js](https://nodejs.org/) 22.18 ou superior

### 1. Configurar a API

As credenciais e a configuração local não fazem parte do repositório. Antes de arrancar a API, é preciso criar o ficheiro `API/.env.development` com o conteúdo abaixo e substituir os valores entre `< >`.

```
ConnectionStrings__DefaultConnection=<conexão da base de dados>
JwtSettings__Key=<chave secreta com pelo menos 32 caracteres>
JwtSettings__Issuer=<nome do emissor>
JwtSettings__Audience=<nome do público>
Manager__Name=<nome do primeiro gestor>
Manager__Email=<email do primeiro gestor>
Manager__Password=<password do primeiro gestor>
Seed__TestData=true
```

| Variável | Descrição |
|---|---|
| `ConnectionStrings__DefaultConnection` | Ligação ao SQL Server. A base de dados é criada automaticamente. |
| `JwtSettings__Key` | Chave usada para assinar os tokens (mínimo de 32 caracteres). |
| `JwtSettings__Issuer` e `JwtSettings__Audience` | Emissor e destinatário dos tokens. |
| `Manager__Name`, `Manager__Email`, `Manager__Password` | Conta do primeiro gestor, criada no arranque se ainda não existir nenhum utilizador. |
| `Seed__TestData` | Com `true`, e apenas em desenvolvimento, a base de dados é apagada e recriada com dados de demonstração **em cada arranque**. Com `false`, são apenas aplicadas as migrações e criado o primeiro gestor. |

Nos restantes ambientes é lido um ficheiro com as mesmas variáveis, sem o sufixo `.development`.

### 2. Arrancar a API

```bash
dotnet dev-certs https --trust
```

```bash
dotnet run --project API --launch-profile https
```

A API fica disponível em `https://localhost:7104`, com a documentação Swagger em `https://localhost:7104/swagger`. As migrações são aplicadas automaticamente no arranque.

### 3. Arrancar o frontend

```bash
cd Frontend/Web
```

```bash
npm install
```

```bash
npm run dev
```

A aplicação fica disponível em `http://localhost:5173`. O servidor de desenvolvimento reencaminha os pedidos de `/api` para a API em `https://localhost:7104`.

### 4. Correr os testes

```bash
dotnet test
```

## Utilizadores de teste

Com `Seed__TestData=true`, a base de dados é criada com veículos, clientes, um contrato em cada estado e os seguintes utilizadores, todos com a password `Strong123@`:

| Email | Perfil | Estado |
|---|---|---|
| `manager@test.com` | Gestor | Ativo |
| `employee@test.com` | Funcionário | Ativo |
| `inactive@test.com` | Funcionário | Inativo (não consegue iniciar sessão) |

As datas dos contratos de demonstração são relativas ao dia atual, pelo que existe sempre um contrato agendado, um ativo, um em atraso, dois concluídos e um cancelado.

## Permissões

| Ação | Gestor | Funcionário |
|---|---|---|
| Criar, desativar e reativar funcionários | Sim | Não |
| Registar, editar, desativar e reativar veículos | Sim | Não |
| Registar e editar clientes | Sim | Sim |
| Desativar e reativar clientes | Sim | Não |
| Criar contratos e registar devoluções | Sim | Sim |
| Cancelar contratos | Sim | Não |
| Consultar veículos, clientes e contratos | Sim | Sim |

## Regras de negócio

### Requisitos

**Veículos**
- Marca, modelo, matrícula, ano de fabrico e tipo de combustível são obrigatórios.
- O ano de fabrico não pode ser posterior ao ano atual.
- A matrícula é única.

**Clientes**
- Nome completo, email, telefone e carta de condução são obrigatórios.
- O email é único.
- O telefone só contém números e tem um formato válido.

**Contratos**
- Cliente, veículo, data de início, data de fim e quilometragem inicial são obrigatórios.
- A data de início não pode ser anterior à data atual.
- A data de fim é posterior à data de início.

**Listagens**
- O estado do veículo é atualizado automaticamente com base nos contratos.

### Regras adicionais

**Veículos**
- A matrícula é aceite nos formatos portugueses `AA-00-AA`, `00-00-AA` e `00-AA-00`, com ou sem hífenes, e é guardada normalizada.
- O formato da matrícula tem de ser compatível com o ano de fabrico: `00-00-AA` só para veículos até 2005 e `00-AA-00` até 2020. O formato reflete a data da matrícula em Portugal, por isso `AA-00-AA` é aceite para qualquer ano (um veículo importado recebe uma matrícula nova).
- O ano de fabrico não pode ser anterior a 1992, o início dos formatos de matrícula suportados.
- Além de Disponível e Alugado, o veículo pode estar **Reservado**, quando tem um contrato agendado que ainda não começou. Um veículo com um contrato em atraso continua Alugado.
- A última quilometragem registada é calculada a partir das devoluções.

**Clientes**
- O número da carta de condução é único e segue o formato `P-123456 7`.
- O telefone segue o formato português: 9 dígitos, a começar por 2 ou 9.
- Um cliente com um contrato em atraso não pode fazer novos contratos.

**Contratos**
- Um veículo só pode ter **um contrato por concluir** de cada vez. Só aceita um novo contrato depois de ser devolvido, o que evita que um atraso ou uma avaria deixe outro cliente sem veículo.
- Depois da devolução existe um **dia de preparação**: o veículo só pode voltar a ser alugado a partir do dia seguinte.
- A data de início não pode ser superior a **7 dias** a partir de hoje, para que uma reserva distante não imobilize o veículo.
- Só é possível criar contratos com clientes e veículos ativos.
- A quilometragem inicial não pode ser inferior à última registada para o veículo, e é preenchida automaticamente no formulário.
- O contrato guarda o **preço por dia** acordado; o total é calculado a partir do número de dias.
- O contrato guarda o **utilizador que o criou**, obtido do token de autenticação.
- A **devolução** regista a data e a quilometragem final: a data tem de estar entre o início do contrato e o dia atual, e a quilometragem final não pode ser inferior à inicial.
- O **cancelamento** só é possível até ao dia de início, inclusive, e nunca depois de uma devolução.
- Os contratos não são editados: para corrigir um contrato, cancela-se e cria-se outro.
- O estado do contrato (Agendado, Ativo, Em atraso, Concluído ou Cancelado) é calculado.

**Utilizadores**
- O email é único e a password tem de ter pelo menos 8 caracteres, com maiúscula, minúscula, número e símbolo.
- Um gestor não pode desativar a sua própria conta.
- Desativar um utilizador termina as suas sessões ativas.

**Geral**
- Nada é apagado: veículos, clientes e utilizadores são desativados, e os contratos são cancelados, para preservar o histórico.
- Não é possível desativar um veículo ou um cliente com contratos por concluir.

## Arquitetura e decisões

### Organização por funcionalidade

A API e o frontend estão organizados pelas mesmas funcionalidades.

```
API/
  AutenticacaoAutorizacao/   Controllers, DTOs, Exceptions, Interfaces, Services
  Utilizadores/
  Veiculos/
  Clientes/
  Contratos/
  Models/                    Entidades e enums
  Data/                      DbContext e dados iniciais
  Migrations/
  Shared/                    Resposta comum, exceções base e validações
  Utils/                     Tratamento de erros e extensões
Frontend/Web/src/
  AutenticacaoAutorizacao/   components, models, services, stores
  Utilizadores/ 
  Veiculos/ 
  Clientes/ 
  Contratos/
  Shared/                    Layout, cliente HTTP, tema e utilitários
  router/
Testes/UnitTests/            Testes organizados pelas mesmas funcionalidades
```

### Guardar factos, calcular estados

Os estados que dependem da passagem do tempo (disponibilidade do veículo e estado do contrato) não são guardados na base de dados. São calculados a partir de factos: as datas do contrato, a data de devolução e a data de cancelamento. Um estado guardado ficaria desatualizado sem que nada fosse alterado, por exemplo no dia em que um contrato começa. As regras estão nas próprias entidades (`Vehicle.GetAvailabilityOn`, `Contract.GetStatusOn`, `Contract.GetTotalPrice`).

### Validação em três camadas

| Camada | Responsabilidade | Exemplos |
|---|---|---|
| DTO | O que se valida apenas com os dados do pedido | Campos obrigatórios, formatos, datas, matrícula compatível com o ano |
| Service | O que precisa da base de dados | Unicidade, veículo com contrato por concluir, quilometragem |
| Base de dados | Garantia final de integridade | Índices únicos, chaves estrangeiras |

### Tratamento de erros

Os services lançam exceções de domínio que herdam de cinco exceções base (`NotFound`, `Conflict`, `BusinessRule`, `Unauthorized` e `Forbidden`). Um middleware converte-as no estado HTTP adequado. Todas as respostas, de sucesso ou de erro, seguem o mesmo formato (`ResponseDTO`), e os erros de validação são devolvidos por campo, para o frontend os mostrar junto de cada campo do formulário.

### Reservas simultâneas

A criação de um contrato decorre numa transação com isolamento `Serializable`, para impedir que dois pedidos em simultâneo reservem o mesmo veículo.

### Histórico preservado

As chaves estrangeiras usam `NoAction` e os registos são desativados em vez de apagados (`RecordStatus`), pelo que um contrato antigo nunca fica a apontar para um veículo ou cliente inexistente.

## Autenticação

- **Access token** JWT de 15 minutos, guardado apenas em memória no frontend.
- **Refresh token** de 7 dias, entregue num cookie `HttpOnly`, `Secure` e `SameSite=Strict`, e guardado na base de dados apenas como hash.
- O refresh token é **rodado** a cada utilização. A reutilização de um token já rodado é tratada como roubo e revoga a sessão.
- Cada access token está associado à sessão que o gerou, verificada em cada pedido, pelo que o logout e a desativação de uma conta têm efeito imediato.
- O login demora o mesmo tempo quer o email exista ou não, para não revelar que emails estão registados.
- Os endpoints de autenticação têm um limite de 15 pedidos por minuto por endereço IP.

## Testes

O projeto `Testes/UnitTests` tem 149 testes xUnit, que cobrem os requisitos funcionais e as regras adicionais:

- a validação dos DTOs;
- os services de veículos, clientes, contratos, utilizadores e autenticação;
- o cálculo de estados e de preços nas entidades.

Cada teste usa uma base de dados SQLite em memória própria, que respeita os índices únicos e as chaves estrangeiras.

## Trabalho futuro

Pontos considerados durante o desenvolvimento e deixados fora do âmbito:

- **Preço automático:** calcular o preço por dia a partir da gama, da quilometragem, do ano e de outros fatores, e cobrar os dias de atraso na devolução.
- **Registo de danos:** registar os danos detetados na devolução de um veículo que precise de reparação.
- **Estado "Em preparação"** para o dia da devolução.
- **Quilometragem no registo do veículo**, para os veículos que entram no sistema já com quilómetros.
- **Auditoria completa:** registar quem devolveu, cancelou ou desativou cada registo, e quando.
- **Marcas e modelos** em tabelas próprias, em vez de texto livre.
- **Escala:** paginação no servidor.
- **Testes** de interface e de ponta a ponta.
