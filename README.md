# dotnet-api-base – Framework modular para ASP.NET Core

[![NuGet](https://img.shields.io/badge/NuGet-MateusCizeski-%23004880)](https://www.nuget.org/packages?q=MateusCizeski&includeComputedFrameworks=true&prerel=true)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## 📖 Sobre o projeto
O ApiBase é um framework modular para ASP.NET Core, publicado como múltiplos pacotes NuGet independentes.
Ele fornece uma base genérica para criação de APIs reutilizáveis, seguindo princípios de Clean Architecture e DDD-light, garantindo baixo acoplamento e alta produtividade.

### 📦 Pacotes disponíveis
- `ApiBase.Controller` → Controllers genéricos reutilizáveis.  
- `ApiBase.Application` → Serviços de aplicação, DTOs, lógica de negócio aplicada.  
- `ApiBase.Infra` → Configurações de infraestrutura, contexto de dados, Unit of Work.  
- `ApiBase.Domain` → Entidades e regras de negócio puras.  
- `ApiBase.Repository` → Repositórios genéricos desacoplados do banco de dados.  
👉 Ver todos os pacotes no NuGet
---

## 🚀 Funcionalidades principais
- **CRUD genérico** com suporte a paginação, ordenação e filtros dinâmicos.  
- **Controllers genéricos** para operações padrão.  
- **Repositories genéricos + Unit of Work** para persistência desacoplada.  
- **Camada Application** para lógica de negócios e DTOs.  
- **Camada Domain** independente, contendo entidades puras.  
- **Pacotes NuGet** para integração plug-and-play em novos projetos.  

---

## 🛠️ Tecnologias e padrões
- .NET 8+ / ASP.NET Core  
- Entity Framework Core  
- Repository Pattern & Unit of Work  
- Clean Architecture / DDD-light  

---

## 📦 Instalação
Exemplo para instalar um dos pacotes:

```bash
dotnet add package ApiBase.Application
