// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.querySelectorAll('.clickable-post').forEach(post => {
    post.addEventListener('click', function () {
        const selection = window.getSelection();
        if (selection && selection.toString().length > 0) {
            return;
        }
        window.location = this.dataset.url;
    });
});

document.addEventListener('DOMContentLoaded', () => {
    let previewPicture = document.getElementById('previewPfp');

    document.getElementById('inputPfp').addEventListener('change', (event) => {
        if (!event.target.files[0]) return;
        previewPicture.src = URL.createObjectURL(event.target.files[0]);
    });

    const firstNameInput = document.getElementById('firstNameInput');
    const lastNameInput = document.getElementById('lastNameInput');

    const previewName = document.getElementById('previewName');

    firstNameInput.addEventListener('input', () => {
        previewName.innerHTML =
             "<b>" + firstNameInput.value + ' ' + lastNameInput.value + "</b>";
    });

    lastNameInput.addEventListener('input', () => {
        previewName.innerHTML =
            "<b>" + firstNameInput.value + ' ' + lastNameInput.value + "</b>";
    });

    let previewDescription = document.getElementById("previewDescription");
    let descriptionInput = document.getElementById("descriptionInput");

    descriptionInput.addEventListener('input', () => {
        previewDescription.textContent =
            descriptionInput.value;
    });
    
});


async function toggleFollow(button) {
    let userId = button.dataset.userId;

    let response = await fetch(`/Users/Follow/${userId}`, {
        method: 'POST'
    });

    let contentType = response.headers.get("content-type");

    if (!contentType || !contentType.includes("application/json")) {
        window.location.href = '/Identity/Account/Login';
        return;
    }

    if (!response.ok) {
        alert("Something went wrong");
        return;
    }

    const result = await response.json();

    button.dataset.following = result.isFollowing;
    button.textContent = result.isFollowing ? "Unsend follow request" : "Follow";
}
