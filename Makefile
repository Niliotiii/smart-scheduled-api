
# Nome do projeto
PROJECT_NAME = smart-scheduled-api

# Diretório das migrations
MIGRATIONS_DIR = Data/Migrations

# String de conexão do banco de dados
CONNECTION_STRING = "Server=localhost;Database=smart_scheduled_db;User=root;Password=danilo12;Port=3306;"

# Comando para buildar o projeto
build:
	dotnet build

# Comando para criar uma nova migration
migration:
	@read -p "Enter migration name: " name; \
	dotnet ef migrations add $$name --output-dir $(MIGRATIONS_DIR)

# Comando para limpar o banco de dados
clean-db:
	dotnet ef database drop --force

# Comando para rodar as migrations
update-db:
	dotnet ef database update

# Comando para rodar a aplicação
run:
	dotnet run

# Comando para limpar o projeto
clean:
	dotnet clean

# Comando para restaurar as dependências do projeto
restore:
	dotnet restore

# Comando para testar o projeto
test:
	dotnet test

# Comando para formatar o código
format:
	dotnet format

# Comando para verificar o código
lint:
	dotnet format --check

# Comando padrão
default: build
