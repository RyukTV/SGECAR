# Arquitectura técnica

```text
Blazor WebAssembly
        ↓ HTTP/JSON
ASP.NET Core Web API
        ↓
Controllers
        ↓
Services
        ↓
Entity Framework Core
        ↓
SQL Server
```

`SistemaGestionEmpresarial.Contracts` se utiliza exclusivamente para compartir contratos HTTP entre el cliente Blazor y la API. El cliente no referencia el proyecto de la API: se comunica con él únicamente por HTTP.

Por ahora se descartó una arquitectura completa con proyectos `Domain`, `Application` e `Infrastructure`, porque no es necesaria para el alcance actual. Los servicios y las entidades se agregarán cuando el equipo defina las funcionalidades y el modelo de datos.
