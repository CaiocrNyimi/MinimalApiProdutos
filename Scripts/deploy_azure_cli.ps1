# ================================
# DEPLOY NA AZURE
# ================================

# 🔧 Variáveis de configuração (DECLARE OS VALORES DE TODOS ENTRE AS ASPAS DUPLAS!)
$RG_NAME = ""
$LOCATION = ""
$APP_NAME = ""
$SQL_SERVER_NAME = ""
$SQL_DB_NAME = ""
$SQL_ADMIN_USER = ""
$SQL_ADMIN_PASS = ""
$ACR_NAME = ""
$PUBLIC_IP = "999.999.999.999" # Substitua pelo seu IP real

# ================================
# ETAPA 1: Criar recursos Azure
# ================================

az group create --name $RG_NAME --location $LOCATION

az sql server create `
    --name $SQL_SERVER_NAME `
    --resource-group $RG_NAME `
    --location $LOCATION `
    --admin-user $SQL_ADMIN_USER `
    --admin-password $SQL_ADMIN_PASS

az sql db create `
    --resource-group $RG_NAME `
    --server $SQL_SERVER_NAME `
    --name $SQL_DB_NAME `
    --service-objective Basic `
    --backup-storage-redundancy Local

az sql server firewall-rule create `
    --resource-group $RG_NAME `
    --server $SQL_SERVER_NAME `
    --name AllowAllWindowsAzure `
    --start-ip-address 0.0.0.0 `
    --end-ip-address 0.0.0.0

az sql server firewall-rule create `
    --resource-group $RG_NAME `
    --server $SQL_SERVER_NAME `
    --name AllowMyPersonalIP `
    --start-ip-address $PUBLIC_IP `
    --end-ip-address $PUBLIC_IP

az monitor app-insights component create `
    --app "$APP_NAME-ai" `
    --location $LOCATION `
    --kind web `
    --resource-group $RG_NAME `
    --application-type web

# ================================
# ETAPA 2: Criar Azure Container Registry
# ================================

az acr create `
    --resource-group $RG_NAME `
    --name $ACR_NAME `
    --sku Basic `
    --admin-enabled true `
    --location $LOCATION

# ================================
# ETAPA 3: Build e Push da Imagem Docker
# ================================

docker build -t minimal-api-produtos .
docker tag minimal-api-produtos "$ACR_NAME.azurecr.io/minimal-api-produtos:latest"
az acr login --name $ACR_NAME
docker push "$ACR_NAME.azurecr.io/minimal-api-produtos:latest"

# ================================
# ETAPA 4: Deploy na VM Linux
# ================================

# Obter credenciais do ACR
az acr credential show `
    --name $ACR_NAME `
    --resource-group $RG_NAME `
    --query "{Username:username, Password:passwords[0].value}"

# Login no ACR (executar na VM Linux)
# docker login <SEU_ACR_NAME>.azurecr.io

# Parar e remover container antigo
# docker stop produtos-api
# docker rm produtos-api

# Puxar imagem atualizada
# docker pull <SEU_ACR_NAME>.azurecr.io/minimal-api-produtos:latest

# Rodar container com variáveis de ambiente
# docker run -d -p 80:8080 --name produtos-api `
#     -e ConnectionStrings__DefaultConnection="Server=tcp:$SQL_SERVER_NAME.database.windows.net,1433;Initial Catalog=$SQL_DB_NAME;User ID=$SQL_ADMIN_USER;Password=$SQL_ADMIN_PASS;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" `
#     -e ApplicationInsights__ConnectionString="InstrumentationKey=xxxxxx;IngestionEndpoint=https://brazilsouth-0.in.applicationinsights.azure.com/" `
#     <SEU_ACR_NAME>.azurecr.io/minimal-api-produtos:latest

# Verificar status do container
# docker ps -a

# Acesse o Swagger em: http://<IP-DA-VM>/swagger
