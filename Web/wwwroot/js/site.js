function submitAnimation() {
    var element = document.activeElement; // Get the button that triggered the form submission

    // Prevent double-clicks if animation is already running
    if (element.classList.contains('is-loading')) {
        return false;
    }

    var spinner = document.createElement('div');
    const innerText = element.innerText;

    element.innerText = "";
    spinner.classList.add('spinner');
    element.appendChild(spinner);

    // 🎯 THE FIX: Use a class instead of 'disabled = true'
    element.classList.add('is-loading');
    element.style.pointerEvents = 'none'; // Prevents further taps/clicks
    spinner.style.display = 'inline-block';

    setTimeout(function () {
        element.classList.remove('is-loading');
        element.style.pointerEvents = 'auto';
        spinner.style.display = 'none';
        element.innerText = innerText;
    }, 3000);
}

function FillPageId(id) {
    $("#Page").val(id);

    searchForm();
}

function searchForm() {
    document.getElementById("search-form").submit();
}