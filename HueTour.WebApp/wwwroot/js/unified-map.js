window.hueUnifiedMap = (() => {
    const maps = new Map();
    const typeStyles = {
        attraction: { color: "#542675", label: "Tham quan" },
        food: { color: "#bd8b16", label: "Ẩm thực" },
        unknown: { color: "#687386", label: "Khác" }
    };

    function escapeHtml(value) {
        const element = document.createElement("span");
        element.textContent = value ?? "";
        return element.innerHTML;
    }

    function safeUrl(value) {
        try {
            const url = new URL(value ?? "", window.location.href);
            return url.protocol === "https:" || url.protocol === "http:" ? url.toString() : "";
        } catch {
            return "";
        }
    }

    function popupContent(place) {
        const rows = [];
        if (place.category) rows.push(`<span class="unified-popup-category">${escapeHtml(place.category)}</span>`);
        if (place.address) rows.push(`<span>${escapeHtml(place.address)}</span>`);
        if (place.signatureDish) rows.push(`<span><strong>Món đặc trưng:</strong> ${escapeHtml(place.signatureDish)}</span>`);
        if (place.description && place.description !== place.signatureDish) rows.push(`<span>${escapeHtml(place.description)}</span>`);
        if (place.priceLabel) rows.push(`<span><strong>Giá tham khảo:</strong> ${escapeHtml(place.priceLabel)}</span>`);
        if (place.ratingLabel) rows.push(`<span><strong>Đánh giá:</strong> ${escapeHtml(place.ratingLabel)}</span>`);
        if (place.openingHours) rows.push(`<span><strong>Giờ mở cửa:</strong> ${escapeHtml(place.openingHours)}</span>`);
        if (place.phone) rows.push(`<span><strong>Điện thoại:</strong> ${escapeHtml(place.phone)}</span>`);
        if (Number.isFinite(place.distanceKm)) rows.push(`<span class="unified-popup-distance">Cách tâm tìm kiếm ${Number(place.distanceKm).toFixed(1)} km</span>`);
        if (place.dataSource) rows.push(`<small>Nguồn: ${escapeHtml(place.dataSource)}</small>`);

        const imageUrl = safeUrl(place.imageUrl);
        const image = imageUrl
            ? `<img class="unified-popup-image" src="${escapeHtml(imageUrl)}" alt="Ảnh ${escapeHtml(place.name)}" loading="lazy" referrerpolicy="no-referrer">`
            : "";
        const detailUrl = safeUrl(place.detailUrl);
        const detail = detailUrl
            ? `<a href="${escapeHtml(detailUrl)}" target="_blank" rel="noopener noreferrer">Xem chi tiết nguồn <i class="bi bi-box-arrow-up-right"></i></a>`
            : "";

        return `<div class="unified-map-popup">${image}<strong>${escapeHtml(place.name)}</strong>${rows.map(row => `<span>${row}</span>`).join("")}${detail}</div>`;
    }

    function renderLegend(element, places) {
        const legend = element.parentElement?.querySelector(".unified-map-legend");
        if (!legend) return;

        const counts = new Map();
        for (const place of places) {
            const type = typeStyles[place.type] ? place.type : "unknown";
            counts.set(type, (counts.get(type) ?? 0) + 1);
        }

        legend.replaceChildren();
        legend.hidden = counts.size === 0;
        for (const [type, count] of counts) {
            const item = document.createElement("span");
            item.className = "unified-map-legend-item";
            const swatch = document.createElement("span");
            swatch.className = "unified-map-legend-swatch";
            swatch.style.backgroundColor = typeStyles[type].color;
            swatch.setAttribute("aria-hidden", "true");
            const label = document.createElement("span");
            label.textContent = `${typeStyles[type].label} · ${count}`;
            item.append(swatch, label);
            legend.append(item);
        }
    }

    function notifyViewport(state) {
        if (Date.now() < state.suppressViewportUntil) return;
        window.clearTimeout(state.viewportTimer);
        state.viewportTimer = window.setTimeout(() => {
            if (!state.dotnetReference) return;
            const bounds = state.map.getBounds();
            state.dotnetReference.invokeMethodAsync(
                "OnUnifiedViewportChanged",
                bounds.getWest(), bounds.getSouth(), bounds.getEast(), bounds.getNorth());
        }, 700);
    }

    function render(elementId, places, latitude, longitude, dotnetReference) {
        if (!window.L) throw new Error("Leaflet chưa được tải.");
        const element = document.getElementById(elementId);
        if (!element) throw new Error("Chưa tìm thấy vùng hiển thị bản đồ.");

        const validPlaces = (places ?? []).filter(place =>
            Number.isFinite(place.latitude) && Number.isFinite(place.longitude)
            && place.latitude >= -90 && place.latitude <= 90
            && place.longitude >= -180 && place.longitude <= 180);
        let state = maps.get(elementId);
        const isNew = !state;
        const center = `${Number(latitude).toFixed(5)}:${Number(longitude).toFixed(5)}`;
        const centerChanged = !isNew && state.center !== center;

        if (isNew) {
            const map = L.map(elementId, { scrollWheelZoom: false }).setView([latitude, longitude], 13);
            L.tileLayer("https://tile.openstreetmap.org/{z}/{x}/{y}.png", {
                maxZoom: 19,
                attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
            }).addTo(map);
            state = {
                map,
                group: null,
                markers: new Map(),
                dotnetReference,
                activeId: null,
                center,
                viewportTimer: null,
                suppressViewportUntil: Date.now() + 1200,
                viewportHandler: null
            };
            state.viewportHandler = () => notifyViewport(state);
            map.on("moveend", state.viewportHandler);
            maps.set(elementId, state);
        } else {
            state.dotnetReference = dotnetReference;
            if (centerChanged) {
                state.center = center;
                state.suppressViewportUntil = Date.now() + 1200;
                state.map.setView([latitude, longitude], 13, { animate: false });
            }
        }

        state.group?.remove();
        state.group = L.featureGroup();
        state.markers = new Map();

        for (const place of validPlaces) {
            const style = typeStyles[place.type] ?? typeStyles.unknown;
            const marker = L.circleMarker([place.latitude, place.longitude], {
                radius: 7,
                color: "#ffffff",
                weight: 2,
                fillColor: style.color,
                fillOpacity: 0.94,
                className: `unified-map-marker unified-map-marker-${place.type ?? "unknown"}`
            })
                .bindPopup(popupContent(place), { maxWidth: 310, autoPanPadding: [12, 18] })
                .bindTooltip(escapeHtml(place.name), { direction: "top", offset: [0, -7], opacity: 0.96 });

            marker.on("click", () => {
                setActiveMarker(state, place.id);
                scrollToCard(place.id);
                state.dotnetReference?.invokeMethodAsync("OnUnifiedMarkerSelected", place.id);
            });
            marker.addTo(state.group);
            state.markers.set(place.id, marker);
        }

        state.group.addTo(state.map);
        renderLegend(element, validPlaces);

        if (isNew || centerChanged) {
            const bounds = state.group.getBounds();
            state.suppressViewportUntil = Date.now() + 1200;
            if (bounds.isValid()) state.map.fitBounds(bounds.pad(0.12), { maxZoom: 15, animate: false });
            else state.map.setView([latitude, longitude], 13, { animate: false });
        }

        if (state.activeId !== null) setActiveMarker(state, state.activeId);
        window.setTimeout(() => state.map.invalidateSize({ pan: false }), 80);
        return true;
    }

    function setActiveMarker(state, placeId) {
        state.activeId = placeId;
        for (const [id, marker] of state.markers) {
            marker.getElement()?.classList.toggle("unified-map-marker-active", id === placeId);
        }
    }

    function focus(elementId, placeId) {
        const state = maps.get(elementId);
        const marker = state?.markers.get(placeId);
        if (!state || !marker) return false;
        setActiveMarker(state, placeId);
        state.suppressViewportUntil = Date.now() + 1200;
        state.map.setView(marker.getLatLng(), 16, { animate: true });
        marker.openPopup();
        return true;
    }

    function highlight(elementId, placeId) {
        const state = maps.get(elementId);
        if (state?.markers.has(placeId)) setActiveMarker(state, placeId);
    }

    function scrollToCard(placeId) {
        if (!String(placeId).startsWith("food/")) return;
        const numericId = String(placeId).slice("food/".length);
        const selectorId = window.CSS?.escape ? CSS.escape(numericId) : numericId.replace(/[^a-zA-Z0-9_-]/g, "");
        const card = document.querySelector(`[data-food-id="${selectorId}"]`);
        if (!card) return;
        document.querySelectorAll(".food-card.is-highlight").forEach(item => item.classList.remove("is-highlight"));
        card.classList.add("is-highlight");
        card.scrollIntoView({ behavior: "smooth", block: "center" });
    }

    function requestLocation(dotnetReference) {
        if (!window.isSecureContext && location.hostname !== "localhost" && location.hostname !== "127.0.0.1") {
            dotnetReference.invokeMethodAsync("OnUnifiedUserLocationError", "insecure-context");
            return false;
        }
        if (!navigator.geolocation) {
            dotnetReference.invokeMethodAsync("OnUnifiedUserLocationError", "not-supported");
            return false;
        }

        navigator.geolocation.getCurrentPosition(
            position => dotnetReference.invokeMethodAsync("OnUnifiedUserLocated", position.coords.latitude, position.coords.longitude),
            error => {
                const code = error.code === 1 ? "permission-denied"
                    : error.code === 2 ? "position-unavailable"
                        : error.code === 3 ? "timeout" : "unknown";
                dotnetReference.invokeMethodAsync("OnUnifiedUserLocationError", code);
            },
            { enableHighAccuracy: true, timeout: 12000, maximumAge: 60000 });
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
                { enableHighAccuracy: false, timeout: 10000, maximumAge: 300000 });
        });
    }

    function invalidateSize(elementId) {
        const state = maps.get(elementId);
        if (state) window.requestAnimationFrame(() => state.map.invalidateSize({ pan: false }));
    }

    function dispose(elementId) {
        const state = maps.get(elementId);
        if (!state) return;
        window.clearTimeout(state.viewportTimer);
        state.map.off("moveend", state.viewportHandler);
        state.group?.remove();
        state.map.remove();
        maps.delete(elementId);
    }

    return { render, focus, highlight, scrollToCard, requestLocation, getCurrentPosition, invalidateSize, dispose };
})();
