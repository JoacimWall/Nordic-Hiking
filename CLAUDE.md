# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Nordic-Hiking is a .NET 10.0 application that extracts hiking locations from YouTube videos using AI analysis and displays them on an interactive map. The project is in Swedish (UI text, seasons, README).

## Build & Run Commands

```bash
# Build entire solution (run from repo root)
dotnet build Nordic-Hiking.sln

# Run Admin app (Blazor Server, manages channels/videos/locations)
dotnet run --project src/Nordic-Hiking.Admin

# Run Public app (Blazor WASM, displays interactive map)
dotnet run --project src/Nordic-Hiking.Public

# Publish Public app for GitHub Pages
dotnet publish src/Nordic-Hiking.Public/Nordic-Hiking.Public.csproj -c Release -o publish

# Copy database from Admin to Public (required after processing new videos)
./update-public-db.sh

# EF Core migrations (from repo root)
dotnet ef migrations add <Name> --project src/Nordic-Hiking.Data --startup-project src/Nordic-Hiking.Admin
dotnet ef database update --project src/Nordic-Hiking.Data --startup-project src/Nordic-Hiking.Admin
```

No test projects exist yet.

## Architecture

Four projects in `src/`, all targeting `net10.0`:

- **Nordic-Hiking.Core** — Shared models (`Channel`, `Video`, `HikeLocation`) and service interfaces (`IYouTubeService`, `IAiAnalysisService`, `IGeocodingService`). No dependencies on other projects.
- **Nordic-Hiking.Data** — EF Core DbContext with SQLite. References Core. Database file lives at `src/Nordic-Hiking.Admin/hikes.db`.
- **Nordic-Hiking.Admin** — Blazor Server app for managing data. References Core and Data. Contains service implementations: `YouTubeService` (YoutubeExplode), `ClaudeAnalysisService` (Anthropic SDK), `GeocodingService` (Nominatim + Google Maps), `VideoProcessingService` (orchestrator).
- **Nordic-Hiking.Public** — Blazor WebAssembly app deployed to GitHub Pages. References Core only. Uses client-side SQL.js to query a static copy of `hikes.db` and Leaflet.js for the map. No server-side dependencies.

### Data Flow

1. Admin app adds YouTube channels and fetches their videos
2. `VideoProcessingService` extracts transcripts (YoutubeExplode), analyzes with Claude AI, geocodes locations, and stores `HikeLocation` entities in SQLite
3. `update-public-db.sh` copies the database to the Public app's `wwwroot/data/`
4. Public WASM app loads the database client-side via JS interop (`sqliteHelper.js`) and renders markers on a Leaflet map (`map.js`)

### Key JavaScript Files

- `src/Nordic-Hiking.Public/wwwroot/js/map.js` — Leaflet map initialization, marker management, layer controls (OpenStreetMap, Topographic, hiking/skiing/cycling trail overlays)
- `src/Nordic-Hiking.Public/wwwroot/js/sqliteHelper.js` — SQL.js database loading and query functions called via Blazor JS interop
- `src/Nordic-Hiking.Admin/wwwroot/js/editVideoMap.js` — Leaflet map for the video editing page (simpler than public map.js)

### Service DI Lifetimes

`GeocodingService` is registered as singleton (maintains rate limiter state). Other services are scoped. Keep this in mind when adding new services.

### Processing Logic

`VideoProcessingService` only creates a `HikeLocation` for the first successfully geocoded place (breaks after first match). This is important when modifying processing logic.

### JS Interop Patterns

Both apps use `window.*` global objects for Blazor JS interop:
- Public: `window.mapHelper` (map.js) and `window.sqliteHelper` (sqliteHelper.js)
- Admin: `window.editVideoMap` (editVideoMap.js)
- Called from C# via `IJSRuntime.InvokeAsync<T>` / `InvokeVoidAsync`

## Swedish Language Conventions

The project uses Swedish throughout. Key terms in UI/code:
- Seasons: Vår, Sommar, Höst, Vinter (Spring, Summer, Autumn, Winter)
- Difficulty: Lätt, Medel, Svår (Easy, Medium, Hard)
- Confidence: Hög, Medel, Låg (High, Medium, Low)

## Configuration & Secrets

API keys are stored via .NET User Secrets (UserSecretsId in Admin .csproj). Keys needed:
- `Claude:ApiKey` — Anthropic API key for video analysis
- `Google:MapsApiKey` — Google Maps Places API (optional, Nominatim is primary geocoder)

The Admin app uses `claude-sonnet-4-20250514` for AI analysis.

### Key NuGet Packages

Anthropic.SDK 5.8.0, YoutubeExplode 6.5.6, EF Core SQLite 10.0.1

## Dev Ports

- Admin: http://localhost:5272
- Public: http://localhost:5219

## Deployment

GitHub Actions workflow (`.github/workflows/static.yml`) publishes the Public WASM app to GitHub Pages on push to `main`.

### Base Href (Public App)

`$(BaseHref)` in `index.html` is replaced at build time via MSBuild property groups in the .csproj:
- **Debug**: `/` (for local development)
- **Release**: `/Nordic-Hiking/` (for GitHub Pages)
