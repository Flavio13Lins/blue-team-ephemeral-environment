echo off

echo Limpando cache...
dotnet clean

echo Buscando packages...
dotnet restore
cd src/Deception.Sensor.Api

echo Iniciando a API...
dotnet watch run --no-hot-reload