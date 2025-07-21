app_name = trapper-keeper
test_project = tests/TrapperKeeper.Tests/TrapperKeeper.Tests.csproj

.PHONY: clean build test publish run help

clean:
	dotnet clean src/TrapperKeeper/$(app_name).csproj
	dotnet clean $(test_project)
	dotnet restore

build:
	dotnet build src/TrapperKeeper/$(app_name).csproj

build-test:
	dotnet build $(test_project)

test: build-test
	dotnet test $(test_project) --verbosity normal

publish: clean
	dotnet publish src/TrapperKeeper/$(app_name).csproj -c Release
	@echo "Published to bin/Release/net9.0/publish/"

run: build
	dotnet run --project src/TrapperKeeper/$(app_name).csproj

compose-up: clean build
	docker compose up -d

compose-down:
	docker compose down

compose-reload: compose-down compose-up

help:
	@echo "build    - Build project and tests"
	@echo "test     - Run unit tests"
	@echo "publish  - Create production build"
	@echo "run      - Run development server"
	@echo "clean    - Clean solution"
