window._truckeroMapPickers = window._truckeroMapPickers || {};

window.initMapPicker = (elementId, dotnetRef, apiBaseUrl) => {
    const defaultLat = 9.9281;
    const defaultLon = -84.0907;
    const zoomLevel = 13;
    const normalizedApiBase = (apiBaseUrl || "").replace(/\/$/, "");
    const tileProviders = [
        {
            name: "api-proxy",
            url: `${normalizedApiBase}/api/map/tiles/{z}/{x}/{y}.png`,
            options: {
                attribution: "&copy; OpenStreetMap contributors",
                maxZoom: 19
            }
        },
        {
            name: "osm-single-host",
            url: "https://tile.openstreetmap.org/{z}/{x}/{y}.png",
            options: {
                attribution: "&copy; OpenStreetMap contributors",
                maxZoom: 19
            }
        },
        {
            name: "carto",
            url: "https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png",
            options: {
                attribution: "&copy; OpenStreetMap contributors &copy; CARTO",
                subdomains: "abcd",
                maxZoom: 20
            }
        }
    ];

    let attempts = 0;

    const tryInit = () => {
        const host = document.getElementById(elementId);
        if (!window.L || !host) {
            attempts++;
            if (attempts < 20) {
                setTimeout(tryInit, 150);
            } else {
                console.error("Leaflet or map host element was not available.", elementId);
            }
            return;
        }

        const existing = window._truckeroMapPickers[elementId];
        if (existing?.map) {
            if (existing.resizeObserver) {
                existing.resizeObserver.disconnect();
            }

            if (existing.fallbackTimer) {
                clearTimeout(existing.fallbackTimer);
            }

            existing.map.remove();
            delete window._truckeroMapPickers[elementId];
        }

        const map = L.map(host, {
            zoomControl: true,
            preferCanvas: true
        }).setView([defaultLat, defaultLon], zoomLevel);

        const state = {
            map,
            userLocationMarker: null,
            dotnetRef,
            tileLayer: null,
            tileProviderIndex: 0,
            tileErrorCount: 0,
            sawTileLoad: false,
            fallbackTimer: null,
            resizeObserver: null
        };

        window._truckeroMapPickers[elementId] = state;

        const setMapSize = () => map.invalidateSize({ animate: false });

        const cleanupTileLayerHandlers = () => {
            if (!state.tileLayer) {
                return;
            }

            state.tileLayer.off("tileload");
            state.tileLayer.off("tileerror");
        };

        const applyTileLayer = (providerIndex, reason) => {
            cleanupTileLayerHandlers();

            if (state.tileLayer) {
                map.removeLayer(state.tileLayer);
            }

            state.tileProviderIndex = providerIndex;
            state.tileErrorCount = 0;
            state.sawTileLoad = false;

            const provider = tileProviders[providerIndex];
            const tileLayer = L.tileLayer(provider.url, provider.options);
            state.tileLayer = tileLayer;

            tileLayer.on("tileload", () => {
                state.sawTileLoad = true;
                state.tileErrorCount = 0;
            });

            tileLayer.on("tileerror", () => {
                state.tileErrorCount++;

                const shouldFallback =
                    !state.sawTileLoad &&
                    state.tileErrorCount >= 8 &&
                    state.tileProviderIndex < tileProviders.length - 1;

                if (shouldFallback) {
                    applyTileLayer(state.tileProviderIndex + 1, "tileerror");
                }
            });

            tileLayer.addTo(map);

            if (state.fallbackTimer) {
                clearTimeout(state.fallbackTimer);
            }

            state.fallbackTimer = setTimeout(() => {
                const noTilesLoaded = !state.sawTileLoad;
                const hasFallback = state.tileProviderIndex < tileProviders.length - 1;

                if (noTilesLoaded && hasFallback) {
                    applyTileLayer(state.tileProviderIndex + 1, "timeout");
                }
            }, 5000);

            if (reason) {
                console.warn("Map tile provider switched:", provider.name, "reason:", reason);
            }

        };

        applyTileLayer(0, null);

        const onMove = () => {
            const center = map.getCenter();
            dotnetRef.invokeMethodAsync("OnMapMoved", center.lat, center.lng);
        };

        map.on("moveend", onMove);

        setTimeout(setMapSize, 50);
        setTimeout(setMapSize, 250);
        setTimeout(setMapSize, 800);

        host.addEventListener("transitionend", setMapSize, { passive: true });
        host.addEventListener("animationend", setMapSize, { passive: true });

        if (window.ResizeObserver) {
            state.resizeObserver = new ResizeObserver(() => setMapSize());
            state.resizeObserver.observe(host);
        }

        onMove();
    };

    tryInit();
};

window.centerMapToUserLocation = (elementId) => {
    const state = window._truckeroMapPickers[elementId];
    const map = state?.map;
    if (!navigator.geolocation || !map) {
        console.warn("Geolocation is not supported or map is not ready.");
        return;
    }

    navigator.geolocation.getCurrentPosition(
        (position) => {
            const lat = position.coords.latitude;
            const lon = position.coords.longitude;

            map.setView([lat, lon], 15);

            if (state.userLocationMarker) {
                state.userLocationMarker.setLatLng([lat, lon]);
            } else {
                state.userLocationMarker = L.marker([lat, lon], {
                    title: "You are here",
                    icon: L.icon({
                        iconUrl: "https://cdn-icons-png.flaticon.com/512/684/684908.png",
                        iconSize: [25, 25],
                        iconAnchor: [12, 12]
                    })
                }).addTo(map);
            }
        },
        (error) => {
            console.warn("Unable to fetch your location.", error);
        }
    );
};

window.reverseGeocodeFromBrowser = async (lat, lon) => {
    const url = `https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=${lat}&lon=${lon}`;

    try {
        const response = await fetch(url, {
            method: "GET",
            headers: {
                "Accept": "application/json",
                "Accept-Language": "en-US,en;q=0.9"
            }
        });

        const body = await response.text();
        const status = `${response.status} ${response.statusText}`;

        if (!response.ok) {
            return {
                success: false,
                status,
                message: body,
                address: null,
                requestUrl: url
            };
        }

        let parsed;
        try {
            parsed = JSON.parse(body);
        } catch {
            return {
                success: false,
                status,
                message: "Could not parse JSON response.",
                address: null,
                requestUrl: url
            };
        }

        return {
            success: true,
            status,
            message: "",
            address: parsed?.display_name ?? null,
            requestUrl: url
        };
    } catch (error) {
        const jsonpResult = await reverseGeocodeWithJsonp(lat, lon);
        if (jsonpResult.success) {
            return jsonpResult;
        }

        return {
            success: false,
            status: "Request failed",
            message: `Fetch failed: ${error?.message ?? "Connection failure"}; JSONP failed: ${jsonpResult.message}`,
            address: null,
            requestUrl: url
        };
    }
};

const reverseGeocodeWithJsonp = (lat, lon) => {
    return new Promise((resolve) => {
        const callbackName = `__truckeroNominatimCb_${Date.now()}_${Math.floor(Math.random() * 100000)}`;
        const requestUrl =
            `https://nominatim.openstreetmap.org/reverse?format=json&addressdetails=1&accept-language=en` +
            `&lat=${encodeURIComponent(lat)}&lon=${encodeURIComponent(lon)}&json_callback=${callbackName}`;

        let timeoutId = null;

        const cleanup = (script) => {
            if (timeoutId) {
                clearTimeout(timeoutId);
            }

            try {
                delete window[callbackName];
            } catch {
                window[callbackName] = undefined;
            }

            if (script?.parentNode) {
                script.parentNode.removeChild(script);
            }
        };

        const script = document.createElement("script");
        script.src = requestUrl;
        script.async = true;
        script.referrerPolicy = "no-referrer";

        window[callbackName] = (payload) => {
            cleanup(script);

            const address = payload?.display_name ?? null;
            if (address) {
                resolve({
                    success: true,
                    status: "200 OK (JSONP)",
                    message: "Resolved via JSONP fallback.",
                    address,
                    requestUrl
                });
                return;
            }

            resolve({
                success: false,
                status: "JSONP response without address",
                message: "display_name missing from JSONP payload.",
                address: null,
                requestUrl
            });
        };

        script.onerror = () => {
            cleanup(script);
            resolve({
                success: false,
                status: "JSONP script error",
                message: "Script load failed (blocked or unreachable).",
                address: null,
                requestUrl
            });
        };

        timeoutId = setTimeout(() => {
            cleanup(script);
            resolve({
                success: false,
                status: "JSONP timeout",
                message: "No JSONP callback received within timeout.",
                address: null,
                requestUrl
            });
        }, 8000);

        document.head.appendChild(script);
    });
};
