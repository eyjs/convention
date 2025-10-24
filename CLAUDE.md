# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET 8 ASP.NET Core backend with Vue 3 frontend (SPA) for convention/travel management with integrated RAG (Retrieval-Augmented Generation) capabilities. The system provides:
- Convention management (행사 관리)
- Guest management with custom attributes
- Schedule management with templates
- Real-time chat with SignalR
- RAG-powered AI chat using vector search
- Notice board with Quill editor
- Gallery and file management
- Survey system

## Development Commands

### Backend (.NET)

```bash
# Build the project
dotnet build

# Run in development mode (watch for changes)
dotnet watch run

# Run normally
dotnet run

# Database migrations
dotnet ef migrations add <MigrationName>
dotnet ef database update

# Run tests
dotnet test
```

The API runs on http://localhost:5000 by default (configured in appsettings.json Kestrel section).

### Frontend (Vue 3 + Vite)

```bash
cd ClientApp

# Install dependencies
npm install

# Run development server (proxies to backend at localhost:5000)
npm run dev

# Build for production
npm run build

# Preview production build
npm preview

# Lint and fix
npm run lint
```

Frontend dev server runs on http://localhost:3000 with API proxy to http://localhost:5000.

### Database Setup

```bash
# Setup database (runs migrations)
setup-database.bat

# Or manually
dotnet ef database update
```

Uses SQL Server LocalDB by default: `Server=(localdb)\\mssqllocaldb;Database=startour`

## Architecture Overview

### Layered Architecture Pattern

The backend follows a clean layered architecture:

1. **Controllers/** - API endpoints (thin controllers)
2. **Services/** - Business logic layer organized by domain
3. **Repositories/** - Data access layer with Unit of Work pattern
4. **Data/** - EF Core DbContext and entity configurations
5. **Entities/** - Domain models
6. **DTOs/** - Data transfer objects
7. **Interfaces/** - Service and repository contracts
8. **Middleware/** - Request pipeline components
9. **Providers/** - External service integrations (LLM providers)
10. **Storage/** - Vector storage implementations

### Key Architectural Patterns

**Unit of Work + Repository Pattern**
- `IUnitOfWork` orchestrates multiple repositories in a single transaction
- Each entity has its own repository (`IConventionRepository`, `IGuestRepository`, etc.)
- Always use `await _unitOfWork.SaveChangesAsync()` to commit changes
- Access repositories via `_unitOfWork.Conventions`, `_unitOfWork.Guests`, etc.

**Service Layer Organization** (Services/ directory)
- `Services/Ai/` - RAG, embedding, LLM provider management, indexing
- `Services/Auth/` - Authentication, SMS verification, JWT
- `Services/Chat/` - Convention chat logic, intent routing, RAG search
- `Services/Convention/` - Convention, notice, gallery, survey management
- `Services/Guest/` - Guest-related services, legacy schedule uploads
- `Services/Upload/` - **NEW:** Refactored Excel upload system (guests, schedules, attributes, group mapping)
- `Services/Shared/` - Cross-cutting concerns (file upload, user context)
- `Services/Shared/Builders/` - Builder pattern for complex objects (prompts, documents)

**Factory Pattern for LLM Providers**
- `LlmProviderManager` dynamically selects active LLM provider from database settings
- Supports multiple providers: Llama3 (Ollama), Gemini
- Interface: `ILlmProvider` with methods `GenerateResponseAsync()`, `ClassifyIntentAsync()`
- Configured via `LlmSettings` table in database (not just appsettings.json)

**Strategy Pattern for Vector Storage**
- `IVectorStore` interface with two implementations:
  - `InMemoryVectorStore` (Development) - registered as Singleton
  - `MssqlVectorStore` (Production) - registered as Scoped, uses SQL Server
- Automatically selected based on environment in Program.cs:78-101

### RAG (Retrieval-Augmented Generation) Architecture

The RAG system enables semantic search over convention data:

1. **Embedding Pipeline**
   - `IEmbeddingService` generates vector embeddings from text
   - Two implementations: `LocalEmbeddingService` (default), `OnnxEmbeddingService` (requires model file)
   - Configured via `EmbeddingSettings:UseOnnx` in appsettings.json

2. **Vector Storage**
   - Development: `InMemoryVectorStore` (fast, ephemeral)
   - Production: `MssqlVectorStore` backed by `VectorDataEntries` table
   - Stores: content, embedding (varbinary), metadata (JSON), conventionId

3. **RAG Service Flow**
   - `RagService` coordinates embedding + vector search + LLM generation
   - `IndexingService` auto-indexes conventions, guests, schedules into vector store
   - `ConventionDocumentBuilder` transforms entities into searchable text chunks

4. **Chat Integration**
   - `ConventionChatService` handles chat requests
   - `SourceIdentifier` determines required data sources (DB, RAG, or general LLM)
   - `RagSearchService` performs semantic search over convention vectors
   - `ChatPromptBuilder` constructs context-aware prompts
   - `LlmResponseService` generates final response with context

### Authentication & Authorization

- JWT-based authentication configured in Program.cs:160-192
- `IAuthService` handles login, token generation, refresh tokens
- `IUserContextFactory` extracts user context from HttpContext
- Guest access token system for convention-specific access
- `ConventionAccessService` verifies guest permissions

### Database Context Important Details

**DbContext Configuration** (Data/ConventionDbContext.cs)
- Uses UTC timestamps with `getdate()` defaults
- Cascade delete configured for parent-child relationships
- Unique indexes on critical fields (LoginId, AccessToken, etc.)
- Query filters for soft deletes (IsDeleted)

**Key Entity Relationships**
- `Convention` 1:N `Guests`, `Schedules`, `Features`, `Menus`, `Notices`, etc.
- `Guest` has custom `GuestAttributes` (key-value pairs)
- `ScheduleTemplate` with `ScheduleItems` for reusable schedules
- `GuestScheduleTemplate` (many-to-many) assigns schedules to guests
- `Notice` with `Comments`, `FileAttachments`, categorized by `NoticeCategory`
- `ConventionAction` with `GuestActionStatus` for dynamic action tracking

**Migrations Note**
Several migrations are excluded from compilation in LocalRAG.csproj:52-61. If adding new migrations, ensure they don't conflict with excluded ones.

## Frontend Architecture (ClientApp/)

**Vue 3 + Composition API**
- Vite build tool with HMR
- Pinia for state management
- Vue Router for navigation
- Axios for API calls
- Tailwind CSS for styling
- Quill editor for rich text (notices)
- SignalR for real-time chat
- v-viewer for image galleries

**Key Directories**
- `src/views/` - Page components
- `src/components/` - Reusable UI components
- `src/stores/` - Pinia state stores
- `src/services/` - API service modules
- `src/router/` - Route definitions

**API Proxy**
Vite proxies `/api` and `/chathub` to backend (localhost:5000) in development.

## Configuration

### appsettings.json Key Sections

**LlmSettings**
- Provider selection: "llama3" or "gemini"
- Llama3 connects to local Ollama (http://localhost:11434)
- Gemini requires API key
- Temperature, TopP for response tuning

**EmbeddingSettings**
- UseOnnx: false (default LocalEmbeddingService), true (OnnxEmbeddingService)
- ModelPath: ONNX model location
- Dimensions: 384 (all-MiniLM-L6-v2 default)

**RagSettings**
- DefaultTopK: Number of similar documents to retrieve (default: 5)
- AutoIndexing: Auto-index conventions/guests/schedules on creation

**JwtSettings**
- SecretKey must be 32+ characters
- Token expiration times configurable

**ConnectionStrings**
- DefaultConnection: SQL Server connection string
- Supports both LocalDB and full SQL Server

### Service Lifetimes (Critical for DI)

- **Singleton**: `IEmbeddingService`, `InMemoryVectorStore` (dev), `ISmsService`, `IVerificationService`, `JwtSettings`
- **Scoped**: All repositories, `ILlmProvider`, `IRagService`, `MssqlVectorStore` (prod), most business services
- **Transient**: None currently

Always inject `IUnitOfWork` and services as Scoped in service constructors.

## Working with LLM Providers

### Adding a New LLM Provider

1. Implement `ILlmProvider` interface in `Providers/` directory
2. Add configuration section to appsettings.json
3. Register provider in Program.cs (Scoped lifetime)
4. Update `LlmProviderManager` to support new provider
5. Add database record to `LlmSettings` table

### LLM Provider Interface
```csharp
Task<string> GenerateResponseAsync(string prompt, string? context = null,
    List<ChatRequestMessage>? history = null, string? systemInstructionOverride = null);
Task<string> ClassifyIntentAsync(string question, List<ChatRequestMessage>? history = null);
```

## Testing & Debugging

**Swagger UI**: Available at https://localhost:5001/swagger (dev only)

**Health Checks**: `/health` endpoint checks:
- Database connectivity
- LLM provider status
- Vector store availability
- Embedding service health

**Logging**:
- Serilog configured to Console + File (logs/log.txt)
- Daily rolling logs
- Log level configured per namespace in appsettings.json

## Build & Deployment

### Production Build

```bash
# Backend: Build and publish
dotnet publish -c Release -o ./publish

# Frontend: Build into wwwroot
cd ClientApp
npm run build
# Output goes to ClientApp/dist, then copied to wwwroot during publish
```

The .csproj has a `PublishVueApp` target that automatically builds Vue app during publish.

### Environment-Specific Behavior

Program.cs adjusts based on environment:
- Development: Swagger enabled, HTTPS redirect disabled, InMemory vector store, API 404 fallback
- Production: HTTPS redirect enabled, MSSQL vector store, SPA fallback to index.html

## Common Development Patterns

**Adding a New Entity**
1. Create entity class in `Entities/`
2. Add DbSet to `ConventionDbContext`
3. Configure relationships in `OnModelCreating`
4. Create migration: `dotnet ef migrations add Add<EntityName>`
5. Create repository interface in `Repositories/IUnitOfWork.cs`
6. Implement repository in `Repositories/<EntityName>Repository.cs`
7. Register repository in `ServiceCollectionExtensions.cs`
8. Update `UnitOfWork` to include new repository property

**Adding a New Service**
1. Create interface in `Interfaces/I<ServiceName>.cs`
2. Implement in appropriate `Services/` subdirectory
3. Register in Program.cs as Scoped
4. Inject `IUnitOfWork` and other required services
5. Use async/await for all DB operations

**Adding a New Controller**
1. Create in `Controllers/` directory
2. Inherit from `ControllerBase`
3. Use `[ApiController]` and `[Route("api/[controller]")]` attributes
4. Inject required services via constructor
5. Use DTOs for request/response, never expose entities directly

## Important Conventions

- All dates should be handled as UTC in the database
- Soft delete pattern: Use `IsDeleted` flag, not physical delete
- Always use `CancellationToken` in async repository methods
- API responses should use consistent DTO patterns
- File uploads go to `d:\home` directory (configured for Azure App Service)
- Session timeout: 2 hours (configured in Program.cs:43-48)
- CORS configured for localhost:3000 and localhost:5173

## Troubleshooting

**Ollama Connection Issues**
- Ensure Ollama is running: `ollama serve`
- Check Llama3 model is pulled: `ollama list`
- Verify BaseUrl in appsettings.json matches Ollama port (default 11434)

**Database Connection Issues**
- Ensure LocalDB is installed (comes with Visual Studio)
- Run `setup-database.bat` to create database
- Check connection string in appsettings.json

**Vector Store Issues**
- Development uses InMemory (data lost on restart)
- Check `EmbeddingSettings:UseOnnx` if using ONNX embeddings
- Verify model file exists at specified ModelPath if UseOnnx=true

**Frontend Build Issues**
- Delete `ClientApp/node_modules` and `ClientApp/dist`
- Run `npm install` in ClientApp directory
- Ensure Node.js >= 18.0.0

## Excel Upload System (Refactored - Oct 2024)

### Overview
The Excel upload system has been refactored to separate concerns and enable scalable group-based schedule mapping.

**Old System:** Single monolithic service (`ScheduleUploadService`) handling guests + schedules together

**New System:** Four independent services
1. **Guest Upload** (`GuestUploadService`) - Upload participants with group assignment
2. **Schedule Template Upload** (`ScheduleTemplateUploadService`) - Upload schedule templates as ConventionActions
3. **Attribute Upload** (`AttributeUploadService`) - Upload guest metadata with statistics
4. **Group-Schedule Mapping** (`GroupScheduleMappingService`) - Bulk assign schedules to groups

### Key Changes

**Guest Entity**
- Added `GroupName` field (nullable string, max 100 chars)
- Groups enable bulk schedule assignment (e.g., "A그룹", "VIP", "일반참석자")

**ConventionAction as Schedule Template**
- Schedule templates are stored as `ConventionAction` entities
- `ActionType`: `SCHEDULE_0001`, `SCHEDULE_0002`, etc. (auto-increment)
- `MapsTo`: `"SCHEDULE"`
- `Config`: JSON with schedule date/time

### Excel Formats

**Guest Upload:** `[소속|부서|이름|사번(주민번호)|전화번호|그룹]`
- **Required:** 이름, 전화번호, 그룹
- Creates or updates guests based on name + phone

**Schedule Template Upload:** `[일정헤더|상세내용]`
- **Header format:** `월/일(요일)_일정명_시:분` (e.g., `11/03(일)_조식_07:30`)
- **Detail:** Supports Excel internal line breaks (Shift+Enter), converts to `<br/>`

**Attribute Upload:** `[이름|전화번호|속성1|속성2|...]`
- Stores guest metadata in `GuestAttribute` table
- Returns statistics (distribution by attribute value) for event planning

### API Endpoints

```
POST /api/upload/conventions/{id}/guests
POST /api/upload/conventions/{id}/schedule-templates
POST /api/upload/conventions/{id}/attributes
POST /api/upload/schedule-mapping/group
GET  /api/upload/conventions/{id}/groups
```

### Workflow Example

1. Upload guests (with GroupName assignment)
2. Upload schedule templates (creates ConventionActions)
3. Map groups to schedules (bulk assign via `GuestActionStatus`)
4. Optionally upload attributes for statistics

### Database Migration

```sql
-- Add GroupName to Guests table
ALTER TABLE [Guests] ADD [GroupName] NVARCHAR(100) NULL;
CREATE INDEX [IX_Guest_GroupName] ON [Guests]([GroupName]);
```

Or run: `Migrations/manual_add_groupname.sql`

### Documentation

See `docs/UPLOAD_SYSTEM_REFACTORING.md` for detailed documentation, API examples, and migration guide.

### Sample Files

- `Sample/참석자업로드_샘플.xlsx`
- `Sample/일정업로드_샘플.xlsx`
- `Sample/속성업로드_샘플.xlsx`

## External Dependencies

- Ollama (optional, for Llama3 provider)
- Google Gemini API key (optional, for Gemini provider)
- SQL Server or LocalDB
- ONNX model file (optional, for ONNX embeddings)
