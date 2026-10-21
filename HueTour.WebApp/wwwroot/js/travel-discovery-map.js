window.hueDiscoveryMap = (() => {
    const maps = new Map();
    const markerGroups = new Map();

    function escapeHtml(value) {
        const element = document.createElement("span");
        element.textContent = value ?? "";
        return element.innerHTML;
    }

    function render(elementId, latitude, longitude, places) {
        if (!window.L) throw new Error("Leaflet was not loaded.");
        const element = document.getElementById(elementId);
        if (!element) throw new Error("The map container is not available yet.");

        let map = maps.get(elementId);
        if (!map) {
            map = L.map(elementId, { scrollWheelZoom: false }).setView([latitude, longitude], 13);
            L.tileLayer("https://tile.openstreetmap.org/{z}/{x}/{y}.png", {
                maxZoom: 19,
                attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
            }).addTo(map);
            maps.set(elementId, map);
        } else {
            map.setView([latitude, longitude], 13);
        }

        const oldGroup = markerGroups.get(elementId);
        if (oldGroup) oldGroup.remove();
        const group = L.featureGroup();
        const markersById = new Map();

        L.circleMarker([latitude, longitude], {
            radius: 8,
            color: "#fff",
            weight: 3,
            fillColor: "#b98b39",
            fillOpacity: 1
        }).bindPopup("Điểm xuất phát").addTo(group);

        for (const place of (places ?? []).slice(0, 80)) {
            const detailsUrl = /^https:\/\/www\.openstreetmap\.org\//i.test(place.detailsUrl ?? "") ? place.detailsUrl : "https://www.openstreetmap.org";
            const popup = "<strong>" + escapeHtml(place.name) + "</strong>"
                + "<br><span>" + escapeHtml(place.category) + "</span>"
                + "<br><small>" + Number(place.distance).toFixed(1) + " km</small>"
                + '<br><a href="' + escapeHtml(detailsUrl) + '" target="_blank" rel="noopener noreferrer">Chi tiết trên OpenStreetMap</a>';
            const marker = L.marker([place.lat, place.lon]).bindPopup(popup).addTo(group);
            markersById.set(place.id, marker);
        }

        group.addTo(map);
        markerGroups.set(elementId, group);
        map._hueTourMarkers = markersById;
        const bounds = group.getBounds();
        if (bounds.isValid()) map.fitBounds(bounds.pad(0.14), { maxZoom: 15 });
        window.setTimeout(() => map.invalidateSize(), 80);
        return true;
    }

    function focus(latitude, longitude, placeId) {
        const map = maps.get("hue-discovery-map");
        if (!map) return false;
        map.setView([latitude, longitude], 16, { animate: true });
        const marker = map._hueTourMarkers?.get(placeId);
        if (marker) marker.openPopup();
        document.getElementById("map")?.scrollIntoView({ behavior: "smooth", block: "start" });
        return true;
    }

    function getCurrentPosition() {
        return new Promise((resolve, reject) => {
            if (!navigator.geolocation) {
                reject("Trình duyệt không hỗ trợ lấy vị trí hiện tại.");
                return;
            }
            navigator.geolocation.getCurrentPosition(
                position => resolve({ latitude: position.coords.latitude, longitude: position.coords.longitude }),
                error => reject(error.message || "Không thể lấy vị trí hiện tại."),
                { enableHighAccuracy: false, timeout: 10000, maximumAge: 300000 }
            );
        });
    }

    return { render, focus, getCurrentPosition };
})();
