#!/bin/bash

# Script de inicialização do SQL Server com Seed
# Este script é executado quando o container SQL Server inicia

set -e

echo "Aguardando SQL Server estar pronto..."
sleep 15s

echo "Executando script de seed..."

/opt/mssql-tools/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P $SA_PASSWORD \
  -i /docker-entrypoint-initdb.d/InitDB.sql

echo "Database initialized successfully!"
