window.initMapPicker = (dotnetRef) => {
    if (!window.L) {
        console.error("Leaflet not loaded.");
        return;
    }

    const defaultLat = 9.9281;  // San José
    const defaultLon = -84.0907;

    const map = L.map('map').setView([defaultLat, defaultLon], 13);
    window._truckeroMapPicker = map; // Store reference globally

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
    }).addTo(map);

    // Track pin at center of map
    const onMove = () => {
        const center = map.getCenter();
        dotnetRef.invokeMethodAsync('OnMapMoved', center.lat, center.lng);
    };

    map.on('moveend', onMove);
    onMove(); // initial update
};

let userLocationMarker = null;

window.centerMapToUserLocation = () => {
    const map = window._truckeroMapPicker;
    if (!navigator.geolocation || !map) {
        alert("Geolocation is not supported or map is not ready.");
        return;
    }

    navigator.geolocation.getCurrentPosition(
        (position) => {
            const lat = position.coords.latitude;
            const lon = position.coords.longitude;

            map.setView([lat, lon], 15);

            if (userLocationMarker) {
                userLocationMarker.setLatLng([lat, lon]);
            } else {
                userLocationMarker = L.marker([lat, lon], {
                    title: "You are here",
                    icon: L.icon({
                        iconUrl: 'https://cdn-icons-png.flaticon.com/512/684/684908.png',
                        iconSize: [25, 25],
                        iconAnchor: [12, 12]
                    })
                }).addTo(map);
            }
        },
        (error) => {
            alert("Unable to fetch your location.");
            console.warn(error);
        }
    );
};
