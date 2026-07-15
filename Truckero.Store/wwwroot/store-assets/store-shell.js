(function () {
    if (window.__truckeroStoreShellInitialized) {
        return;
    }

    window.__truckeroStoreShellInitialized = true;

    const routes = {
        Dashboard: "/",
        Deliveries: "/deliveries/tracker",
        Fleet: "/fleet",
        Analytics: "/deliveries/history",
        Customers: "/staff",
        Staff: "/staff",
        Notifications: "/notifications",
        Settings: "/settings",
        Logout: "/",
        Back: "/staff"
    };

    const sidebarStateKey = "truckero.store.sidebar.expanded";

    function pageName() {
        const path = window.location.pathname || "";
        return decodeURIComponent(path.split("/").pop() || "").toLowerCase();
    }

    function normalizeText(value) {
        return (value || "")
            .replace(/\s+/g, " ")
            .trim()
            .toLowerCase();
    }

    function navigateTop(route) {
        if (!route) {
            return;
        }

        if (window.top && window.top !== window) {
            window.top.location.href = route;
            return;
        }

        window.location.href = route;
    }

    function launchExternal(url) {
        if (!url) {
            return;
        }

        if (window.top && window.top !== window) {
            window.top.location.href = url;
            return;
        }

        window.location.href = url;
    }

    function bindOnce(element, handler, preventDefault) {
        if (!element || element.dataset.storeActionBound === "true") {
            return;
        }

        element.dataset.storeActionBound = "true";
        element.addEventListener("click", function (event) {
            if (preventDefault) {
                event.preventDefault();
            }

            handler(event, element);
        });
    }

    function buttonsByText(text) {
        const expected = normalizeText(text);
        return Array.from(document.querySelectorAll("button")).filter((button) => normalizeText(button.textContent) === expected);
    }

    function wireRouteButtonByText(text, route) {
        buttonsByText(text).forEach((button) => {
            bindOnce(button, function () {
                navigateTop(route);
            }, true);
        });
    }

    function wireActionButtonByText(text, action) {
        buttonsByText(text).forEach((button) => {
            bindOnce(button, action, true);
        });
    }

    function wireActionButtonById(id, action) {
        const button = document.getElementById(id);
        bindOnce(button, action, true);
    }

    function wireAnchorBySelector(selector, route) {
        const anchor = document.querySelector(selector);
        if (!anchor) {
            return;
        }

        anchor.setAttribute("href", route);
        anchor.setAttribute("target", "_top");
    }

    function showToast(message) {
        const existing = document.getElementById("store-toast-message");
        if (existing) {
            existing.remove();
        }

        const toast = document.createElement("div");
        toast.id = "store-toast-message";
        toast.textContent = message;
        toast.style.position = "fixed";
        toast.style.bottom = "1.25rem";
        toast.style.right = "1.25rem";
        toast.style.zIndex = "9999";
        toast.style.background = "#0F172A";
        toast.style.color = "#FFFFFF";
        toast.style.padding = "0.7rem 1rem";
        toast.style.borderRadius = "0.75rem";
        toast.style.fontSize = "0.875rem";
        toast.style.boxShadow = "0 8px 20px rgba(0,0,0,0.25)";
        document.body.appendChild(toast);

        window.setTimeout(function () {
            toast.remove();
        }, 1800);
    }

    function setButtonState(button, active) {
        if (!button) {
            return;
        }

        if (active) {
            button.classList.add("bg-white", "text-truckero-orange", "shadow-sm");
            button.classList.remove("text-truckero-gray-500");
            return;
        }

        button.classList.remove("bg-white", "text-truckero-orange", "shadow-sm");
        button.classList.add("text-truckero-gray-500");
    }

    function wireAnchorRoutes(sidebar) {
        const links = sidebar.querySelectorAll("a[aria-label], #logo-container a");
        links.forEach((link) => {
            const label = (link.getAttribute("aria-label") || "").trim();
            const route = label ? routes[label] : routes.Dashboard;

            if (route) {
                link.setAttribute("href", route);
                link.setAttribute("target", "_top");
            }

            if (!label) {
                return;
            }

            if (!link.querySelector(".store-nav-text")) {
                const textNode = document.createElement("span");
                textNode.className = "store-nav-text";
                textNode.textContent = label;
                link.appendChild(textNode);
            }
        });

        const logoLink = sidebar.querySelector("#logo-container a");
        if (logoLink && !logoLink.querySelector(".store-logo-text")) {
            const logoText = document.createElement("span");
            logoText.className = "store-logo-text";
            logoText.textContent = "Truckero";
            logoLink.appendChild(logoText);
        }
    }

    function wireHeaderButtons() {
        const buttons = document.querySelectorAll("button[aria-label]");
        buttons.forEach((button) => {
            const label = (button.getAttribute("aria-label") || "").trim();
            const route = routes[label];
            if (!route || label === "Toggle menu") {
                return;
            }

            bindOnce(button, function () {
                navigateTop(route);
            }, true);
        });
    }

    function wireSidebarToggle(sidebar) {
        const toggleButton = document.querySelector('button[aria-label="Toggle menu"]');
        const isExpanded = window.localStorage.getItem(sidebarStateKey) === "true";
        if (isExpanded) {
            sidebar.classList.add("store-sidebar-expanded");
        }

        if (!toggleButton) {
            return;
        }

        bindOnce(toggleButton, function () {
            const expanded = sidebar.classList.toggle("store-sidebar-expanded");
            window.localStorage.setItem(sidebarStateKey, String(expanded));
        }, true);
    }

    function wireDashboardActions() {
        wireRouteButtonByText("Track Deliveries", "/deliveries/tracker");
        wireRouteButtonByText("Assign Job", "/deliveries/assignment");
        wireRouteButtonByText("Create Delivery", "/deliveries/create");
        wireRouteButtonByText("View All Jobs", "/deliveries/history");
    }

    function wireCompanySetupActions() {
        wireRouteButtonByText("Cancel", "/");
        wireActionButtonByText("Upload Logo", function () {
            showToast("Logo upload flow is pending API integration.");
        });
        wireActionButtonByText("Add New Store", function () {
            showToast("New store draft added.");
        });
        wireActionButtonByText("Save Changes", function () {
            showToast("Company settings saved.");
        });
    }

    function wireCreateDeliveryActions() {
        wireRouteButtonByText("Discard", "/deliveries/tracker");
        wireRouteButtonByText("Schedule Delivery", "/deliveries/assignment");
        wireAnchorBySelector("#header a", "/deliveries/tracker");

        const sizeButtons = ["Small", "Medium", "Large"].map(function (label) {
            return buttonsByText(label)[0];
        }).filter(Boolean);

        sizeButtons.forEach(function (button) {
            bindOnce(button, function () {
                sizeButtons.forEach(function (candidate) {
                    candidate.classList.remove("bg-truckero-orange-light", "border-truckero-orange", "text-truckero-orange", "font-semibold");
                    candidate.classList.add("border-truckero-gray-300", "text-truckero-gray-700");
                });

                button.classList.remove("border-truckero-gray-300", "text-truckero-gray-700");
                button.classList.add("bg-truckero-orange-light", "border-truckero-orange", "text-truckero-orange", "font-semibold");
            }, true);
        });
    }

    function wireCreateClerkActions() {
        wireRouteButtonByText("Cancel", "/staff");
        wireActionButtonById("complete-button", function () {
            navigateTop("/staff");
        });

        wireActionButtonByText("Send via Email", function () {
            const email = (document.getElementById("clerk-email") || {}).value || "clerk@example.com";
            launchExternal("mailto:" + encodeURIComponent(email) + "?subject=Truckero%20Clerk%20Activation");
        });

        wireActionButtonByText("Send via SMS", function () {
            const raw = (document.getElementById("clerk-phone") || {}).value || "";
            const phone = raw.replace(/[^\d+]/g, "") || "+15550000000";
            launchExternal("sms:" + phone);
        });

        wireActionButtonByText("Send via WhatsApp", function () {
            const raw = (document.getElementById("clerk-phone") || {}).value || "";
            const phone = raw.replace(/[^\d]/g, "") || "15550000000";
            launchExternal("https://wa.me/" + phone);
        });

        wireActionButtonByText("Download QR Code", function () {
            const canvas = document.getElementById("qr-canvas");
            if (!canvas || typeof canvas.toDataURL !== "function") {
                showToast("Generate the QR code first.");
                return;
            }

            const link = document.createElement("a");
            link.download = "clerk-activation-qr.png";
            link.href = canvas.toDataURL("image/png");
            link.click();
        });
    }

    function wireDeliveryAssignmentActions() {
        const internalTab = document.getElementById("internal-fleet-tab");
        const freelanceTab = document.getElementById("freelance-tab");
        const internalContent = document.getElementById("internal-fleet-content");
        const freelanceContent = document.getElementById("freelance-content");

        function setTab(showInternal) {
            if (internalContent) {
                internalContent.classList.toggle("hidden", !showInternal);
            }

            if (freelanceContent) {
                freelanceContent.classList.toggle("hidden", showInternal);
            }

            if (internalTab) {
                internalTab.classList.toggle("text-truckero-orange", showInternal);
                internalTab.classList.toggle("border-b-2", showInternal);
                internalTab.classList.toggle("border-truckero-orange", showInternal);
                internalTab.classList.toggle("text-truckero-gray-500", !showInternal);
            }

            if (freelanceTab) {
                freelanceTab.classList.toggle("text-truckero-orange", !showInternal);
                freelanceTab.classList.toggle("border-b-2", !showInternal);
                freelanceTab.classList.toggle("border-truckero-orange", !showInternal);
                freelanceTab.classList.toggle("text-truckero-gray-500", showInternal);
            }
        }

        bindOnce(internalTab, function () {
            setTab(true);
        }, true);

        bindOnce(freelanceTab, function () {
            setTab(false);
        }, true);

        wireActionButtonByText("Assign", function () {
            showToast("Driver assigned.");
            navigateTop("/deliveries/tracker");
        });
    }

    function wireDeliveryHistoryActions() {
        wireActionButtonByText("Export to CSV", function () {
            const table = document.querySelector("table");
            if (!table) {
                showToast("No data available to export.");
                return;
            }

            const csvRows = Array.from(table.querySelectorAll("tr")).map(function (row) {
                return Array.from(row.querySelectorAll("th, td")).map(function (cell) {
                    return "\"" + (cell.textContent || "").replace(/\s+/g, " ").trim().replace(/"/g, "\"\"") + "\"";
                }).join(",");
            });

            const blob = new Blob([csvRows.join("\n")], { type: "text/csv;charset=utf-8;" });
            const url = URL.createObjectURL(blob);
            const link = document.createElement("a");
            link.href = url;
            link.download = "delivery-history.csv";
            link.click();
            URL.revokeObjectURL(url);
        });

        wireActionButtonByText("Clear Filters", function () {
            document.querySelectorAll("input, select, textarea").forEach(function (input) {
                if (input.type === "checkbox" || input.type === "radio") {
                    input.checked = false;
                    return;
                }

                input.value = "";
            });

            showToast("Filters cleared.");
        });
    }

    function wireDeliveryTrackerActions() {
        const mapView = buttonsByText("Map View")[0];
        const listView = buttonsByText("List View")[0];
        bindOnce(mapView, function () {
            setButtonState(mapView, true);
            setButtonState(listView, false);
            showToast("Map view selected.");
        }, true);
        bindOnce(listView, function () {
            setButtonState(listView, true);
            setButtonState(mapView, false);
            showToast("List view selected.");
        }, true);

        wireActionButtonByText("Filters", function () {
            showToast("Filter panel is pending API integration.");
        });
    }

    function wireFleetActions() {
        wireRouteButtonByText("Add Driver", "/staff/create-clerk");
        wireActionButtonByText("Add Vehicle", function () {
            showToast("Vehicle setup flow is pending API integration.");
        });
        wireRouteButtonByText("New Delivery", "/deliveries/create");
    }

    function wireNotificationActions() {
        wireActionButtonByText("Mark all as read", function () {
            showToast("All notifications marked as read.");
        });
        wireActionButtonByText("Contact Customer", function () {
            launchExternal("mailto:customer@example.com?subject=Truckero%20Delivery%20Update");
        });
        wireRouteButtonByText("View Job Details", "/deliveries/tracker");
        wireRouteButtonByText("Accept & Assign", "/deliveries/assignment");
        wireActionButtonByText("Decline", function () {
            showToast("Request declined.");
        });
        wireActionButtonByText("Investigate", function () {
            navigateTop("/deliveries/history");
        });
        wireActionButtonByText("Load More", function () {
            showToast("Loaded more notifications.");
        });
    }

    function wireStaffActions() {
        wireRouteButtonByText("Create New Clerk", "/staff/create-clerk");
    }

    function wireSettingsActions() {
        wireActionButtonByText("Save Changes", function () {
            showToast("Settings saved.");
        });
        wireActionButtonByText("Same as Monday", function () {
            const timeInputs = Array.from(document.querySelectorAll('#business-hours-card input[type="time"]'));
            if (timeInputs.length >= 2) {
                const mondayStart = timeInputs[0].value;
                const mondayEnd = timeInputs[1].value;

                for (let i = 2; i + 1 < timeInputs.length; i += 2) {
                    timeInputs[i].value = mondayStart;
                    timeInputs[i + 1].value = mondayEnd;
                }
            }

            showToast("Copied Monday schedule.");
        });
        wireActionButtonByText("Add Payment Method", function () {
            showToast("Payment method flow is pending API integration.");
        });
    }

    function wireMainContentActions() {
        const current = pageName();
        switch (current) {
            case "storedashboard.html":
                wireDashboardActions();
                break;
            case "company & store setup.html":
                wireCompanySetupActions();
                break;
            case "create delivery job.html":
                wireCreateDeliveryActions();
                break;
            case "create new clerk screen.html":
                wireCreateClerkActions();
                break;
            case "deliveryassignment.html":
                wireDeliveryAssignmentActions();
                break;
            case "deliveryhistory.html":
                wireDeliveryHistoryActions();
                break;
            case "deliverytracker.html":
                wireDeliveryTrackerActions();
                break;
            case "fleetmanagement.html":
                wireFleetActions();
                break;
            case "notifications.html":
                wireNotificationActions();
                break;
            case "store staff managementrole.html":
                wireStaffActions();
                break;
            case "storesettings.html":
                wireSettingsActions();
                break;
            default:
                break;
        }
    }

    document.addEventListener("DOMContentLoaded", function () {
        const sidebar = document.getElementById("sidebar");
        if (!sidebar) {
            return;
        }

        wireAnchorRoutes(sidebar);
        wireHeaderButtons();
        wireSidebarToggle(sidebar);
        wireMainContentActions();
    });
})();
