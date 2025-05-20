/**
 * Form Debugger JavaScript functions
 * Provides functionality to capture and fill form data for testing
 */
console.log('FormDebugger.js is being loaded');

// Make sure we have a global FormDebugger object
window.FormDebugger = window.FormDebugger || {
    /**
     * Captures all form input values from the current page
     * @returns {Object} Dictionary of form field names/ids and their values
     */
    captureFormData: function() {
        const formData = {};
        
        // Get all input elements
        const inputs = document.querySelectorAll('input, select, textarea');
        
        inputs.forEach(input => {
            // Skip buttons and hidden inputs
            if (input.type === 'button' || input.type === 'submit' || input.type === 'reset') {
                return;
            }
            
            // Get the input name or id
            const key = input.name || input.id;
            if (!key) return;
            
            // Get the value based on input type
            let value;
            if (input.type === 'checkbox') {
                value = input.checked;
            } else if (input.type === 'radio') {
                if (input.checked) {
                    value = input.value;
                } else {
                    return; // Skip unchecked radio buttons
                }
            } else {
                value = input.value;
            }
            
            formData[key] = value;
        });
        
        return formData;
    },
    
    /**
     * Fills form fields with the provided data
     * @param {Object} formData Dictionary of form field names/ids and their values
     */
    fillFormData: function(formData) {
        // Iterate through the form data and fill in the form
        for (const [key, value] of Object.entries(formData)) {
            const input = document.querySelector(`[name="${key}"], #${key}`);
            if (!input) continue;
            
            if (input.type === 'checkbox') {
                input.checked = value === true;
            } else if (input.type === 'radio') {
                const radio = document.querySelector(`[name="${key}"][value="${value}"]`);
                if (radio) radio.checked = true;
            } else {
                input.value = value;
            }
            
            // Trigger change event to update any bound values
            const event = new Event('change', { bubbles: true });
            input.dispatchEvent(event);
            
            // For Blazor components, we need to trigger the input event as well
            const inputEvent = new Event('input', { bubbles: true });
            input.dispatchEvent(inputEvent);
        }
    },
    
    /**
     * Check if the FormDebugger is available
     * @returns {boolean} True if the FormDebugger is available
     */
    isAvailable: function() {
        console.log('FormDebugger.isAvailable called');
        return true;
    },
    
    /**
     * Save data to localStorage
     * @param {string} key The key to store the data under
     * @param {string} value The value to store
     */
    saveToLocalStorage: function(key, value) {
        try {
            localStorage.setItem(key, value);
            console.log(`Saved data to localStorage with key: ${key}`);
            return true;
        } catch (error) {
            console.error(`Error saving to localStorage: ${error.message}`);
            return false;
        }
    },
    
    /**
     * Load data from localStorage
     * @param {string} key The key to retrieve data from
     * @returns {string|null} The retrieved value or null if not found
     */
    loadFromLocalStorage: function(key) {
        try {
            const value = localStorage.getItem(key);
            console.log(`Loaded data from localStorage with key: ${key}`);
            return value;
        } catch (error) {
            console.error(`Error loading from localStorage: ${error.message}`);
            return null;
        }
    }
};

// Log when the script is fully loaded
console.log('FormDebugger.js loaded successfully, window.FormDebugger:', window.FormDebugger);
