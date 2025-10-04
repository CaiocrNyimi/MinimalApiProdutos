# 💸 Projeto Dimdim — API de Gerenciamento Financeiro

Este projeto é uma **Minimal API em .NET** desenvolvida para realizar deploy na nuvem com Microsoft Azure. Ele realiza o gerenciamento de categorias e produtos, com persistência em banco de dados Azure SQL e deploy automatizado via Azure CLI e Docker.

## 📁 Estrutura do Repositório
```
MinimalApiProdutos/
├── Data/
│ └── AppDbContext.cs
├── Models/
│ ├── Categoria.cs
│ └── Produto.cs
├── Program.cs
├── Scripts/
│ ├── deploy_azure_cli.ps1
│ └── ddl_dimdim.sql
├── README.md
└── .dockerignore / Dockerfile / .gitignore
```

## 🗃️ Script de Banco — DDL das Tabelas

Arquivo: `Scripts/ddl_dimdim.sql`

```sql
CREATE TABLE Categorias (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nome NVARCHAR(100) NOT NULL
);

CREATE TABLE Produtos (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nome NVARCHAR(150) NOT NULL,
    Preco DECIMAL(18,2) NOT NULL,
    Estoque INT NOT NULL,
    CategoriaId INT NOT NULL,
    CONSTRAINT FK_Produtos_Categorias_CategoriaId FOREIGN KEY (CategoriaId)
        REFERENCES Categorias(Id)
        ON DELETE CASCADE
);

CREATE INDEX IX_Produtos_CategoriaId ON Produtos(CategoriaId);
```

## 🧠 Tecnologias Utilizadas

- .NET 8 (Minimal API)
- Entity Framework Core 9
- Azure SQL Server (PaaS)
- Azure CLI
- Docker
- Application Insights
- Swagger (OpenAPI)

## 🚀 Scripts de Deploy via Azure CLI

Arquivo: `Scripts/deploy_azure_cli.ps1`

Contém comandos para:

- Criar grupo de recursos
- Criar Azure SQL Server e banco
- Configurar regras de firewall
- Criar Application Insights
- Criar Azure Container Registry
- Build, tag e push da imagem Docker
- Deploy em VM Linux

## 🧩 JSON das Operações da API

### Categorias

### GET /categorias
- Não necessita JSON.

### POST /categorias
```json
{
  "nome": "Eletrônicos"
}
```

### PUT /categorias/{id}
```json
{
  "nome": "Smartphones"
}
```

### DELETE /categorias/{id}
- Não necessita JSON.

### Produtos

#### GET /produtos
- Não necessita JSON.

#### POST /produtos
```json
{
  "nome": "IPhone 6 Pro Max",
  "preco": 499.99,
  "estoque": 10,
  "categoriaId": 1
}
```

#### PUT /produtos/{id}
```json
{
  "nome": "Samsung Galaxy S22",
  "preco": 4999.99,
  "estoque": 20,
  "categoriaId": 1
}
```

#### DELETE /produtos/{id}
- Não necessita JSON.

## ☁️ How-to: Implantação na Nuvem

* Troque os valores entre <> para valores próprios. Exemplo:

Antes:
```bash
az group create --name <RG-NAME> --location brazilsouth
```
Depois:
```bash
az group create --name meu-grupo-de-recursos --location brazilsouth
```

1. Fazer Login no Azure CLI:
```bash
az login
```

2. Criar Grupo de Recursos:
```bash
az group create --name <RG-NAME> --location brazilsouth
```

3. Criar SQL Server:
```bash
az sql server create \
  --name <SQL-SERVER-NAME> \
  --resource-group <RG-NAME> \
  --location brazilsouth \
  --admin-user <SQL-SERVER-USERNAME> \
  --admin-password <SQL-SERVER-PASSWORD>
```

4. Criar SQL Database:
```bash
az sql db create \
  --resource-group <RG-NAME> \
  --server <SQL-SERVER-NAME> \
  --name <DB-NAME> \
  --service-objective Basic \
  --backup-storage-redundancy Local
```

5. Criar Regra para firewall:
```bash
az sql server firewall-rule create \
  --resource-group <RG-NAME> \
  --server <SQL-SERVER-NAME> \
  --name <FIREWALL-RULE-NAME> \
  --start-ip-address <IP-DA-MAQUINA> \
  --end-ip-address <IP-DA-MAQUINA>
```

6. Criar App Insights:
```bash
az monitor app-insights component create \
  --app <APP-NAME>-ai \
  --location brazilsouth \
  --kind web \
  --resource-group <RG-NAME> \
  --application-type web
```

7. Criar ACR:
```bash
az acr create --resource-group <RG-NAME> --name <ACR-NAME> --sku Basic --admin-enabled true
```

8. Criar/Publicar imagem Docker (Na sua máquina física):
```bash
docker build -t minimal-api-produtos .
docker tag minimal-api-produtos <ACR-NAME>.azurecr.io/minimal-api-produtos:latest
az acr login --name <ACR-NAME>
docker push <ACR-NAME>.azurecr.io/minimal-api-produtos:latest
```

9. Deploy (Dentro da VM Linux):
```bash
docker login <ACR-NAME>.azurecr.io
docker stop <CONTAINER-NAME>
docker rm <CONTAINER-NAME>
docker pull <ACR-NAME>.azurecr.io/minimal-api-produtos:latest

docker run -d -p 80:8080 --name <CONTAINER-NAME> \
  -e ConnectionStrings__DefaultConnection="Server=tcp:<SQL-SERVER-NAME>.database.windows.net,1433;Initial Catalog=<DB-NAME>;User ID=<SQL_SERVER_USERNAME>;Password=<SQL_SERVER_PASSWORD>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" \
  -e ApplicationInsights__ConnectionString="InstrumentationKey=xxxxxx;IngestionEndpoint=https://brazilsouth-0.in.applicationinsights.azure.com/" \
  <ACR-NAME>.azurecr.io/minimal-api-produtos:latest
```

10. Verificar monitoramento:

No portal da Azure:

- Acesse o recurso Application Insights
- Vá em Live Metrics Stream para ver requisições em tempo real
- Vá em Logs (Analytics) e execute:
```
requests
| order by timestamp desc
| take 10
```


11. Acessar Swagger:
```
http://<IP-DA-VM>/swagger
```
