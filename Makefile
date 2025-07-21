app_name = trapper-keeper

clean:
	dotnet clean src/TrapperKeeper/$(app_name).csproj
	dotnet restore src/TrapperKeeper/$(app_name).csproj

build:
	make clean &
	dotnet build src/TrapperKeeper/$(app_name).csproj

publish:
	make clean &
	dotnet publish src/TrapperKeeper/$(app_name).csproj
	./bin/Release/net9.0/publish/$(app_name)

run:
	make clean &
	dotnet run --project src/TrapperKeeper/$(app_name).csproj

help:
	@echo "build"
	@echo "publish"
