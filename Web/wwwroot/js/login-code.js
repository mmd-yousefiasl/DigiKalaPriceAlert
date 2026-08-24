document.addEventListener("DOMContentLoaded", function () {
    const container = document.getElementById("timerContainer");
    const timerTextField = document.getElementById("timerText");
    const resendLink = document.getElementById("btnResendSms");

    let timer;

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

            let formattedMinutes = String(minutes).padStart(2, "0");
            let formattedSeconds = String(seconds).padStart(2, "0");

            timerTextField.textContent =
                `ارسال مجدد کد تا (${formattedMinutes}:${formattedSeconds})`;
        }, 1000);
    }

    function showLinkState() {
        timerTextField.classList.add("hidden");
        resendLink.classList.remove("hidden");
    }

    initTimer();
});