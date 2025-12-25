#!/bin/bash

# Script för att kopiera hikes.db från Admin till Public-appen

SOURCE="src/Nordic-Hiking.Admin/hikes.db"
DEST="src/Nordic-Hiking.Public/wwwroot/data/hikes.db"

if [ ! -f "$SOURCE" ]; then
    echo "❌ Fel: Hittar inte $SOURCE"
    echo "Se till att du har kört Admin-appen först och processerat videor."
    exit 1
fi

mkdir -p "src/Nordic-Hiking.Public/wwwroot/data"

cp "$SOURCE" "$DEST"

if [ $? -eq 0 ]; then
    echo "✅ Databas kopierad!"
    echo "   $SOURCE"
    echo "   → $DEST"
    echo ""
    echo "Du kan nu köra Public-appen:"
    echo "   cd src/Nordic-Hiking.Public"
    echo "   dotnet run"
else
    echo "❌ Fel vid kopiering av databas"
    exit 1
fi
