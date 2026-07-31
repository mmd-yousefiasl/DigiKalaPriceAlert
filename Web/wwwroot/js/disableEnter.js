document.addEventListener("DOMContentLoaded", function () {
    // 1. Target only inputs that are NOT OTP fields
    const inputs = document.querySelectorAll("form input:not(.code-input)");

    inputs.forEach(function (inputElement) {
        inputElement.addEventListener("keypress", function (event) {
            if (event.key === "Enter" || event.keyCode === 13) {
                event.preventDefault();
                return false;
            }
        });
    });

    // 2. Global form submission interceptor
    const form = document.querySelector("form");
    if (form) {
        form.addEventListener("submit", function (event) {
            const activeElement = document.activeElement;
            
            // If the user is pressing Enter while focused on a normal input (like phone number)
            if (activeElement && 
                activeElement.tagName === 'INPUT' && 
                !activeElement.classList.contains('code-input')) {
                
                // Allow the OTP auto-submit logic (in login-code.js) to handle submission 
                // by stopping the native Enter key submission here.
                event.preventDefault();
                return false;
            }
            // Otherwise, allow the form to submit naturally!
        }, true); 
    }
});