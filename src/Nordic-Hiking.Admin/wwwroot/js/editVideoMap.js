window.editVideoMap = {
    map: null,
    markers: [],

    initializeMap: function (mapId, centerLat, centerLng, zoom) {
        if (this.map) {
            this.map.remove();
        }

        this.markers = [];
        this.map = L.map(mapId).setView([centerLat, centerLng], zoom);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
        }).addTo(this.map);

        return true;
    },

    addMarker: function (lat, lng, label) {
        const marker = L.marker([lat, lng]).addTo(this.map);
        if (label) {
            marker.bindPopup(`<strong>${label}</strong>`);
        }
        this.markers.push(marker);
        return true;
    },

    fitBounds: function () {
        if (this.markers.length > 0) {
            const group = L.featureGroup(this.markers);
            this.map.fitBounds(group.getBounds().pad(0.5), { maxZoom: 5 });
        }
        return true;
    }
};

