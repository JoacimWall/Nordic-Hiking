# Public-app (Vandringskarta)

## Förberedelser

För att Public-appen ska fungera måste du kopiera SQLite-databasen från Admin-projektet:

### 1. Kopiera databas

Kör detta kommando från projektets rot-mapp:

```bash
cp src/YoutubeChannelMapPlotter.Admin/hikes.db src/YoutubeChannelMapPlotter.Public/wwwroot/data/hikes.db
```

**OBS:** Detta måste göras varje gång du har uppdaterat data i Admin-appen (processerat nya videor, redigerat platser, etc).

### 2. Kör Public-appen

```bash
cd src/YoutubeChannelMapPlotter.Public
dotnet run
```

Öppna: https://localhost:5001

## Funktionalitet

Public-appen visar:

- **Interaktiv karta** med alla vandringplatser
- **Markers** för varje plats som visar:
  - Platsnamn, region, land
  - Video thumbnail
  - Videotitel och kanal
  - Svårighetsgrad och varaktighet
  - Sammanfattning
  - Länk till YouTube-video
- **Automatisk zoom** för att visa alla platser

## Uppdatera data

1. Lägg till/processa videor i Admin-appen
2. Kopiera `hikes.db` till Public-appen (kommando ovan)
3. Starta om Public-appen

## Teknik

- Blazor WebAssembly
- SQLite med WASM-stöd
- Leaflet för kartvisning
- Bootstrap för styling
