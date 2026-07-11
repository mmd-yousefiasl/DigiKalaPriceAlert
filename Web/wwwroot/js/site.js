function submitAnimation() {
    var element = document.activeElement; // Get the button that triggered the form submission
    var spinner = document.createElement('div');
    const innerText = element.innerText;

    element.innerText = "";
    spinner.classList.add('spinner');
    element.appendChild(spinner);

    element.disabled = true;
    spinner.style.display = 'inline-block';

    setTimeout(function () {
        element.disabled = false;
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