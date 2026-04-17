# Spindle MVP

AI-assisted integration platform MVP (Flow/Boomi/MuleSoft style) built with:

- Backend: .NET 8, ASP.NET Core Minimal API, EF Core, PostgreSQL, Jint
- Frontend: Next.js 14 App Router, TypeScript, TailwindCSS, Zustand
- AI: OpenAI API endpoints for mapping, script generation, and flow drafting

## Project Structure

```text
/backend
  /Api
  /Application
  /Domain
  /Infrastructure
    /Migrations
  /Scripts
  /Flows
  /Mappings
  appsettings.json
  Program.cs
/frontend
  /app
    /flows
    /mappings
    /scripts
    /runs
    /auth
  /components
  /lib
  /store
  tailwind.config.js
```

## MVP Feature Coverage

1. Authentication
- Email/password signup/login
- JWT generation and protected API endpoints
- Single workspace per user

2. Flow Designer (simplified)
- Linear canvas-like step list with add/configure/reorder/save
- Step types: Webhook Trigger, Mapping Step, Script Step, HTTP Request Step

3. Mapping Engine (JSON -> JSON)
- Mapping rules stored as JSONB
- Source path to target path mappings
- JavaScript expression support in mapping rules (Jint)

4. JavaScript Scripting Engine
- Jint runtime with `message`, `log(string)`, `variables`
- Script returns transformed JSON payload

5. Connectors
- Webhook Trigger: `/webhooks/{webhookKey}`
- HTTP Request step with method/url/headers/body

6. Flow Execution
- Sequential step execution
- Payload hand-off between steps
- Run and step logs persisted
- Last 20 runs retained per flow

7. Monitoring
- Recent runs list
- Run detail with step-by-step statuses/logs
- Final output/error visibility

8. AI Features
- `/api/ai/mapping-suggestions`
- `/api/ai/script-generation`
- `/api/ai/flow-draft`

## Database

EF Core entities and migration included for:

- Users
- Workspaces
- Flows
- FlowSteps
- Mappings
- Scripts
- Executions
- ExecutionSteps

JSON payload and rules columns use PostgreSQL `jsonb`.

## Backend Setup

1. Install prerequisites
- .NET 8 SDK
- PostgreSQL (local or remote)

2. Configure:
- Edit `backend/appsettings.json`
  - `ConnectionStrings:DefaultConnection`
  - `Jwt:Key`
  - `OpenAI:ApiKey`

3. Restore and migrate:

```bash
cd backend
dotnet restore
dotnet ef database update
dotnet run
```

Backend default URL is typically `http://localhost:5000` (or as configured).

## Frontend Setup

1. Install prerequisites
- Node.js 20+

2. Configure env:

```bash
cd frontend
# optional
echo NEXT_PUBLIC_API_BASE=http://localhost:5000 > .env.local
```

3. Run:

```bash
npm install
npm run dev
```

Open `http://localhost:3000`.

## Main API Endpoints

- Auth
  - `POST /api/auth/signup`
  - `POST /api/auth/login`
- Flows
  - `GET /api/flows`
  - `POST /api/flows`
  - `GET /api/flows/{flowId}`
  - `PUT /api/flows/{flowId}/steps`
  - `POST /api/flows/{flowId}/execute`
- Runs
  - `GET /api/runs`
  - `GET /api/runs/{executionId}`
- Mapping/Scripts
  - `POST /api/mappings`
  - `POST /api/scripts`
- AI
  - `POST /api/ai/mapping-suggestions`
  - `POST /api/ai/script-generation`
  - `POST /api/ai/flow-draft`
- Webhook Trigger
  - `POST /webhooks/{webhookKey}`

## Notes

- Current flow designer is intentionally minimal and linear for MVP scope.
- Mapping UI supports editing/testing JSON mappings and AI suggestions.
- Script UI supports AI generation and manual JS editing.
- Execution monitoring shows run summaries and step logs.
