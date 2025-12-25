window.mapHelper = {
    map: null,
    markers: [],

    initializeMap: function (mapId, centerLat, centerLng, zoom) {
        if (this.map) {
            this.map.remove();
        }

        this.map = L.map(mapId).setView([centerLat, centerLng], zoom);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
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
