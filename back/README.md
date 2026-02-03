# FunkoApi 🎮✨

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)
![GraphQL](https://img.shields.io/badge/-GraphQL-E10098?style=for-the-badge&logo=graphql&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)
![Redis](https://img.shields.io/badge/redis-%23DD0031.svg?style=for-the-badge&logo=redis&logoColor=white)
![Docker](https://img.shields.io/badge/docker-%230db7ed.svg?style=for-the-badge&logo=docker&logoColor=white)

Una API robusta y moderna para la gestión de colecciones de Funkos, construida con **ASP.NET Core 10**. Ofrece soporte completo para consultas **GraphQL** y endpoints **REST**, integrando autenticación JWT, caché distribuida con Redis y persistencia en PostgreSQL.

---

## 🚀 Tecnologías Core

- **Framework:** .NET 10.0 (ASP.NET Core Web API)
- **API:** GraphQL (HotChocolate) & REST Controllers
- **Base de Datos:** PostgreSQL (Entity Framework Core)
- **Caché:** Redis (Distributed Cache)
- **Autenticación:** JWT (JSON Web Tokens)
- **Mensajería:** WebSockets para notificaciones en tiempo real
- **logging:** Serilog & NLog
- **Testing:** xUnit, FluentAssertions, y Bruno (para tests de API)
- **Despliegue:** Docker & Docker Compose

---

## 🛠️ Características Principales

- **Multi-API:** Accede a tus datos vía REST o mediante el potente lenguaje de consulta GraphQL.
- **Autenticación y Autorización:** Seguridad basada en roles con JWT y hashing de contraseñas (BCrypt).
- **Notificaciones en tiempo real:** Implementación de WebSockets para actualizaciones instantáneas.
- **Optimización:** Caché distribuida para mejorar los tiempos de respuesta.
- **Gestión de Archivos:** Servicio de almacenamiento integrado para imágenes de Funkos.
- **Resiliencia:** Manejo global de excepciones y rate limiting integrado.

---

## 📂 Estructura del Proyecto

```text
FunkoApi/
├── FunkoApi/               # Código fuente principal de la API
│   ├── Controllers/        # Endpoints REST
│   ├── Graphql/            # Queries, Mutations y Tipos de GraphQL
│   ├── Data/               # Contexto de BD y Migraciones
│   ├── Models/             # Entidades del dominio
│   ├── Service/            # Lógica de negocio
│   ├── Repository/         # Capa de acceso a datos
│   └── Infrastructures/    # Configuraciones de DI y servicios externos
├── TestFunko/              # Tests Unitarios e Integración (.NET)
├── TestBruno/              # Colecciones de tests para Bruno (GraphQL/REST)
└── compose.yaml            # Configuración de Docker Multi-contenedor
```

---

## 🏁 Empezando

### Requisitos Previos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (opcional para desarrollo local)

### Configuración Rápida (Docker)

La forma más sencilla de ejecutar el ecosistema completo es usando Docker Compose:

1. Clona el repositorio.
2. Asegúrate de tener un archivo `.env` configurado (ver `.env.example`).
3. Ejecuta el comando:
   ```powershell
   docker-compose up --build
   ```

La API estará disponible en `http://localhost:8080`.

---

## 🔍 Explorando la API

### GraphQL (Recomendado)
Puedes interactuar con la API GraphQL y explorar el esquema usando el playground integrado (**Banana Cake Pop**):
- **URL:** `http://localhost:8080/graphql`

### REST
Los endpoints clásicos están disponibles bajo la ruta `/api`.

---

## 🧪 Testing

### Tests de C#
Ejecuta los tests unitarios y de integración desde la terminal:
```powershell
dotnet test
```

### Tests de Bruno (API)
El proyecto incluye una configuración automática para ejecutar tests de Bruno en Docker. Los reportes se generan en el puerto `8060`.



## 👨‍💻 Autor

<div align="center">
  <table>
    <tr>
      <td align="center">
        <a href="https://github.com/Aragorn7372">
          <img src="https://github.com/Aragorn7372.png" width="100px;" alt="" style="border-radius: 50%"/>
        </a><br>
        <sub><b>Aragorn7372</b></sub><br>
        <a href="https://github.com/Aragorn7372" title="GitHub">
          <img src="https://img.shields.io/badge/GitHub-100000?style=for-the-badge&logo=github&logoColor=white" alt="GitHub"/>
        </a>
      </td>
    </tr>
  </table>
</div>


---


## 📝 Licencia

Este proyecto está bajo la Licencia MIT. ¡Siéntete libre de contribuir!
