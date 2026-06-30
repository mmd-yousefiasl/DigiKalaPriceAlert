document.addEventListener("DOMContentLoaded", function () {
    // === 1. DOM Elements ===
    const container = document.getElementById("timerContainer");
    const timerTextField = document.getElementById("timerText");
    const resendLink = document.getElementById("btnResendSms");
    const inputs = document.querySelectorAll(".code-input");
    const submitButton = document.getElementById("submit-button");
    const form = document.querySelector("form"); // 🎯 FIXED: Declared cleanly here
    let timer;

    // === 2. Cooldown Timer Logic ===
    function initTimer() {
        const remainingSecondsStr = container.getAttribute("data-remaining-seconds");
        let remainingTime = parseInt(remainingSecondsStr, 10);

        if (isNaN(remainingTime) || remainingTime <= 0) {
            showLinkState();
            return;
        }

        startCountdown(remainingTime);
    }

    function startCountdown(duration) {
        let remainingTime = duration;

        resendLink.classList.add("hidden");
        timerTextField.classList.remove("hidden");

        clearInterval(timer);

        timer = setInterval(function () {
            remainingTime--;

            if (remainingTime < 0) {
                clearInterval(timer);
                showLinkState();
                return;
            }

            let minutes = Math.floor(remainingTime / 60);
            let seconds = remainingTime % 60;

            let formattedMinutes = String(minutes).padStart(2, '0');
            let formattedSeconds = String(seconds).padStart(2, '0');

            timerTextField.textContent = `ارسال مجدد کد تا (${formattedMinutes}:${formattedSeconds})`;
        }, 1000);
    }

    function showLinkState() {
        timerTextField.classList.add("hidden");
        resendLink.classList.remove("hidden");
    }

    // === Diagnostic Submission Check ===
    function checkAndSubmit() {
        const currentValues = Array.from(inputs).map(inp => inp.value);
        const isFullyFilled = currentValues.every(val => val.trim() !== "" && val.length > 0);

        if (isFullyFilled) {
            // 🚀 THE FIX: Explicitly focus the submit button before submission!
            // This guarantees document.activeElement is the button when submitAnimation() runs.
            if (submitButton) {
                submitButton.focus();
            }

            setTimeout(() => {
                if (form && submitButton) {
                    if (typeof form.requestSubmit === "function") {
                        form.requestSubmit(submitButton);
                    } else {
                        submitButton.click();
                    }
                }
            }, 50);
        }
    }

    // === 4. OTP Input Controls ===
    inputs.forEach((input, index) => {
        if (index === 0) input.focus();

        input.addEventListener("input", function () {
            this.value = this.value.replace(/[^0-9]/g, '');

            if (this.value.length >= 1 && index < inputs.length - 1) {
                inputs[index + 1].focus();
            }

            checkAndSubmit();
        });

        input.addEventListener("keydown", function (e) {
            if (e.key === "Backspace" && this.value.length === 0 && index > 0) {
                inputs[index - 1].focus();
            }
        });

        input.addEventListener("paste", function (e) {
            const pastedData = (e.clipboardData || window.clipboardData).getData("text").trim();

            if (/^\d{6}$/.test(pastedData)) {
                e.preventDefault();

                inputs.forEach((inp, idx) => {
                    inp.value = pastedData[idx];
                });

                checkAndSubmit();
            }
        });
    });

    initTimer();
});