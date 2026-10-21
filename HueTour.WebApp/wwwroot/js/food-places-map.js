window.hueFoodMap = (() => {
    const maps = new Map();
    const colors = {
        restaurant: "#582c83",
        cafe: "#7a4eab",
        bakery: "#d4af37",
        noodle: "#b8860b",
        bar: "#451e6b",
        dessert: "#a65f86",
        hotpot: "#9b4d32",
        grill: "#72502c",
        seafood: "#347c83",
        vegetarian: "#4b7b55"
    };

    function escapeHtml(value) {
        const element = document.createElement("span");
        element.textContent = value ?? "";
        return element.innerHTML;
    }

    function markerColor(categoryCode) {
        return colors[categoryCode] ?? "#582c83";
    }

    function setActiveMarker(state, placeId) {
        state.activeId = placeId;
        for (const [id, marker] of state.markers) {
            marker.getElement()?.classList.toggle("food-map-marker-active", id === placeId);
        }
    }

    function renderLegend(element, places) {
        const legend = element.parentElement?.querySelector(".food-map-legend");
        if (!legend) {
            return;
        }

        const categories = new Map();
        for (const place of places) {
            if (place.categoryCode && place.category) {
                categories.set(place.categoryCode, place.category);
            }
        }

        legend.replaceChildren();
        legend.hidden = categories.size === 0;
        for (const [code, name] of categories) {
            const item = document.createElement("span");
            item.className = "food-map-legend-item";
            const swatch = document.createElement("span");
            swatch.className = "food-map-legend-swatch";
            swatch.style.backgroundColor = markerColor(code);
            swatch.setAttribute("aria-hidden", "true");
            const label = document.createElement("span");
            label.textContent = name;
            item.append(swatch, label);
            legend.append(item);
        }
    }

    function popupContent(place) {
        const url = /^https:\/\/www\.openstreetmap\.org\//i.test(place.url ?? "")
            ? place.url
            : "https://www.openstreetmap.org";
        const details = [
            `<span class="food-popup-category">${escapeHtml(place.category)}</span>`,
            place.address ? `<span>${escapeHtml(place.address)}</span>` : "",
            place.signatureDish ? `<span><strong>Món đặc trưng:</strong> ${escapeHtml(place.signatureDish)}</span>` : "",
            place.priceLabel ? `<span><strong>Giá tham khảo:</strong> ${escapeHtml(place.priceLabel)}</span>` : "",
            Number.isFinite(place.distanceKm)
                ? `<span class="food-popup-distance">Cách bạn ~${Number(place.distanceKm).toFixed(1)} km</span>`
                : "",
            `<a href="${escapeHtml(url)}" target="_blank" rel="noopener noreferrer">Chi tiết trên OpenStreetMap</a>`
        ].filter(Boolean);

        return `<div class="food-map-popup"><strong>${escapeHtml(place.name)}</strong>${details.map(line => `<span>${line}</span>`).join("")}</div>`;
    }

    function onViewportMove(state) {
        if (Date.now() < state.suppressViewportUntil) {
            return;
        }

        window.clearTimeout(state.viewportTimer);
        state.viewportTimer = window.setTimeout(() => {
            if (!state.dotnetReference) {
                return;
            }

            const bounds = state.map.getBounds();
            state.dotnetReference.invokeMethodAsync(
                "OnViewportChanged",
                bounds.getWest(),
                bounds.getSouth(),
                bounds.getEast(),
                bounds.getNorth());
        }, 700);
    }

    function render(elementId, places, dotnetReference) {
        if (!window.L) {
            return false;
        }

        const element = document.getElementById(elementId);
        if (!element) {
            return false;
        }

        const items = (places ?? []).filter(place => Number.isFinite(place.latitude) && Number.isFinite(place.longitude));
        let state = maps.get(elementId);
        const isNewMap = !state;
        if (isNewMap) {
            const map = L.map(elementId, { scrollWheelZoom: false }).setView([16.4637, 107.5909], 13);
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
                viewportTimer: null,
                suppressViewportUntil: Date.now() + 1000,
                viewportHandler: null
            };
            state.viewportHandler = () => onViewportMove(state);
            map.on("moveend", state.viewportHandler);
            maps.set(elementId, state);
        } else {
            state.dotnetReference = dotnetReference;
        }

        state.group?.remove();
        state.group = L.featureGroup();
        state.markers = new Map();

        for (const place of items) {
            const color = markerColor(place.categoryCode);
            const marker = L.circleMarker([place.latitude, place.longitude], {
                radius: 7,
                color: "#ffffff",
                weight: 2,
                fillColor: color,
                fillOpacity: 0.94,
                className: "food-map-marker"
            })
                .bindPopup(popupContent(place), { maxWidth: 280, autoPanPadding: [12, 18] })
                .bindTooltip(escapeHtml(place.name), { direction: "top", offset: [0, -7], opacity: 0.96 });

            marker.on("click", () => {
                setActiveMarker(state, place.id);
                scrollToCard(place.id);

                state.dotnetReference?.invokeMethodAsync("OnMarkerSelected", place.id);
            });

            marker.addTo(state.group);
            state.markers.set(place.id, marker);
        }

        state.group.addTo(state.map);
        renderLegend(element, items);

        if (isNewMap) {
            const bounds = state.group.getBounds();
            state.suppressViewportUntil = Date.now() + 1000;
            if (bounds.isValid()) {
                state.map.fitBounds(bounds.pad(0.12), { maxZoom: 15, animate: false });
            } else {
                state.map.setView([16.4637, 107.5909], 13, { animate: false });
            }
        }

        if (state.activeId !== null) {
            setActiveMarker(state, state.activeId);
        }
        window.setTimeout(() => state.map.invalidateSize(), 80);
        return true;
    }

    function focus(elementId, placeId) {
        const state = maps.get(elementId);
        const marker = state?.markers.get(placeId);
        if (marker && state) {
            setActiveMarker(state, placeId);
            state.suppressViewportUntil = Date.now() + 1000;
            state.map.setView(marker.getLatLng(), 15, { animate: true });
            marker.openPopup();
        }
    }

    function highlight(elementId, placeId) {
        const state = maps.get(elementId);
        if (state?.markers.has(placeId)) {
            setActiveMarker(state, placeId);
        }
    }

    function focusCenter(elementId, latitude, longitude) {
        const state = maps.get(elementId);
        if (state) {
            state.suppressViewportUntil = Date.now() + 1000;
            state.map.setView([latitude, longitude], 14, { animate: true });
        }
    }

    function scrollToCard(placeId) {
        const card = document.querySelector(`[data-food-id="${placeId}"]`);
        if (card) {
            document.querySelectorAll(".food-card.is-highlight").forEach(item => item.classList.remove("is-highlight"));
            card.classList.add("is-highlight");
            card.scrollIntoView({ behavior: "smooth", block: "center" });
        }
    }

    function requestLocation(dotnetReference) {
        if (!window.isSecureContext && location.hostname !== "localhost" && location.hostname !== "127.0.0.1") {
            dotnetReference.invokeMethodAsync("OnUserLocationError", "insecure-context");
            return false;
        }
        if (!navigator.geolocation) {
            dotnetReference.invokeMethodAsync("OnUserLocationError", "not-supported");
            return false;
        }

        navigator.geolocation.getCurrentPosition(
            position => dotnetReference.invokeMethodAsync(
                "OnUserLocated",
                position.coords.latitude,
                position.coords.longitude),
            error => {
                const errorCode = error.code === 1
                    ? "permission-denied"
                    : error.code === 2
                        ? "position-unavailable"
                        : error.code === 3
                            ? "timeout"
                            : "unknown";
                dotnetReference.invokeMethodAsync("OnUserLocationError", errorCode);
            },
            { enableHighAccuracy: true, timeout: 12000, maximumAge: 60000 });
        return true;
    }

    function invalidateSize(elementId) {
        const state = maps.get(elementId);
        if (state) {
            window.requestAnimationFrame(() => state.map.invalidateSize({ pan: false }));
        }
    }

    function dispose(elementId) {
        const state = maps.get(elementId);
        if (!state) {
            return;
        }

        window.clearTimeout(state.viewportTimer);
        state.map.off("moveend", state.viewportHandler);
        state.group?.remove();
        state.map.remove();
        maps.delete(elementId);
    }

    return { render, focus, highlight, focusCenter, requestLocation, scrollToCard, invalidateSize, dispose };
})();
