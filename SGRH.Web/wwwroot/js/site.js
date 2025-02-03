// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function showSuccessAlert(message) {
    Swal.fire({
        title: 'Éxito',
        text: message,
        icon: 'success',
        timer: 5000,
        showConfirmButton: false
    });
}

function showErrorAlert(message) {
    Swal.fire({
        title: 'Error',
        text: message,
        icon: 'error'
    });
}