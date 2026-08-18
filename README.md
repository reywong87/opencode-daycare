# OpenDaycare

Interfaz web para la gestión y comunicación diaria de una guardería. La pantalla principal presenta el muro de publicaciones de la Sala Soles, con actividades, logros y anuncios para las familias.

## Tecnologías

- ASP.NET Core Blazor Web App
- .NET 10 (`net10.0`)
- Renderizado interactivo en servidor

## Requisitos

- .NET SDK 10.0 o posterior compatible con `net10.0`

## Ejecutar localmente

Desde la raíz del repositorio:

```bash
dotnet run --launch-profile https
```

La aplicación estará disponible en:

- `https://localhost:7088`
- `http://localhost:5138`

## Comandos

```bash
# Compilar el proyecto
dotnet build

# Ejecutar con el perfil HTTPS de desarrollo
dotnet run --launch-profile https
```

## Estructura

```text
Components/
  Pages/       Páginas enrutables de la aplicación
  Layout/      Diseño compartido
  Shared/      Componentes reutilizables
wwwroot/       Archivos estáticos
Program.cs     Configuración de la aplicación y del pipeline HTTP
```

## Desarrollo

La aplicación usa componentes Razor e interacción del lado del servidor. Las páginas se encuentran en `Components/Pages/`; los estilos específicos de un componente deben ubicarse junto a este en archivos `*.razor.css`.
