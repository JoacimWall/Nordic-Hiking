window.mapHelper = {
    map: null,
    markers: [],
    layerControl: null,

    initializeMap: function (mapId, centerLat, centerLng, zoom) {
        if (this.map) {
            this.map.remove();
        }

        this.map = L.map(mapId).setView([centerLat, centerLng], zoom);

        // Baslager - OpenStreetMap
        const osmBase = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
        }).addTo(this.map);

        // OpenTopoMap - bra för vandring
        const topoMap = L.tileLayer('https://{s}.tile.opentopomap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://opentopomap.org">OpenTopoMap</a> contributors'
        });

        // Vandringsleder från Waymarked Trails
        const hikingTrails = L.tileLayer('https://tile.waymarkedtrails.org/hiking/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://waymarkedtrails.org">Waymarked Trails</a>',
            opacity: 0.7
        }).addTo(this.map);

        // Skidleder/vinterleder
        const skiingTrails = L.tileLayer('https://tile.waymarkedtrails.org/slopes/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://waymarkedtrails.org">Waymarked Trails</a>',
            opacity: 0.7
        });

        // Cykelleder
        const cyclingTrails = L.tileLayer('https://tile.waymarkedtrails.org/cycling/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://waymarkedtrails.org">Waymarked Trails</a>',
            opacity: 0.7
        });

        // Baslager-val
        const baseMaps = {
            "OpenStreetMap": osmBase,
            "Topografisk karta": topoMap
        };

        // Overlay-lager
        const overlayMaps = {
            "🥾 Vandringsleder": hikingTrails,
            "⛷️ Skidleder": skiingTrails,
            "🚴 Cykelleder": cyclingTrails
        };

        // Lägg till lagerväljare
        this.layerControl = L.control.layers(baseMaps, overlayMaps, {
            collapsed: false,
            position: 'topright'
        }).addTo(this.map);

        return true;
    },

    addMarker: function (lat, lng, popupHtml) {
        const marker = L.marker([lat, lng]).addTo(this.map);
        marker.bindPopup(popupHtml);
        this.markers.push(marker);
        return true;
    },

    clearMarkers: function () {
        this.markers.forEach(marker => this.map.removeLayer(marker));
        this.markers = [];
        return true;
    },

    fitBounds: function () {
        if (this.markers.length > 0) {
            const group = L.featureGroup(this.markers);
            this.map.fitBounds(group.getBounds().pad(0.1));
        }
        return true;
    }
};
