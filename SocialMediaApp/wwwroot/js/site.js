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
    if (previewPicture == null) return;
    document.getElementById('inputPfp').addEventListener('change', (event) => {
        if (!event.target.files[0]) return;
        previewPicture.src = URL.createObjectURL(event.target.files[0]);
    });

    const firstNameInput = document.getElementById('firstNameInput');
    const lastNameInput = document.getElementById('lastNameInput');

    const previewName = document.getElementById('previewName');

    firstNameInput.addEventListener('input', () => {
        previewName.innerHTML =
             '<b>' + firstNameInput.value + ' ' + lastNameInput.value + '</b>';
    });

    lastNameInput.addEventListener('input', () => {
        previewName.innerHTML =
            '<b>' + firstNameInput.value + ' ' + lastNameInput.value + '</b>';
    });

    let previewDescription = document.getElementById('previewDescription');
    let descriptionInput = document.getElementById('descriptionInput');

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

    let contentType = response.headers.get('content-type');

    if (!contentType || !contentType.includes('application/json')) {
        window.location.href = '/Identity/Account/Login';
        return;
    }

    if (!response.ok) {
        alert('Something went wrong');
        return;
    }

    const result = await response.json();

    if (!result.isFollowing) {
        window.location.reload();
        return;
    }

    button.dataset.following = result.isFollowing;
    button.textContent = 'Unsend follow request';
}

document.querySelectorAll('.follow-accept').forEach(b => {
    b.addEventListener('click', async (event) => {
        event.stopPropagation();
        let response = await fetch(`/FollowRequests/Accept/${b.dataset.userId}`, {
            method: 'POST'
        });

        if (!response.ok) {
            alert('Something went wrong');
            return;
        }

        b.parentElement.innerHTML = 
            ' <button class="btn btn-secondary" disabled> Accepted </button > '

    });
});

document.querySelectorAll('.follow-delete').forEach(b => {
    b.addEventListener('click', async (event) => {
        event.stopPropagation();
        let response = await fetch(`/FollowRequests/Delete/${b.dataset.userId}`, {
            method: 'POST'
        });

        if (!response.ok) {
            alert('Something went wrong');
            return;
        }

        let message = b.parentElement.parentElement;
        message.classList.add('bg-secondary-subtle')
        message.innerText = 'Follow request removed';

    });
});

document.querySelectorAll('.follow-request').forEach(fr => {
    let id = fr.firstElementChild.value;
    fr.addEventListener('click', (event) => {
        window.location = `/Users/Show/${id}`
    })
})

const imageBtn = document.getElementById("imageBtn");
const videoBtn = document.getElementById("videoBtn");

const imageInput = document.getElementById("imageInput");
const videoInput = document.getElementById("videoInput");

const imageSection = document.getElementById("imageSection");
const videoSection = document.getElementById("videoSection");

const imagePreview = document.getElementById("imagePreview");
const videoPreview = document.getElementById("videoPreview");

const clearImages = document.getElementById("clearImages");
const clearVideos = document.getElementById("clearVideos");

if (imageBtn != null) {
    imageBtn.addEventListener('click', () => imageInput.click());
}
if (videoBtn != null) {
    videoBtn.addEventListener('click', () => videoInput.click());
}

if (imageInput != null) {
    imageInput.addEventListener('change', () => {
        const files = Array.from(imageInput.files);
        imagePreview.innerHTML = "";

        if (files.length === 0) {
            imageSection.classList.add("d-none");
            return;
        }

        imageSection.classList.remove("d-none");

        files.forEach(file => {
            const img = document.createElement("img");
            img.src = URL.createObjectURL(file);
            imagePreview.appendChild(img);
        });
    });
}
if (videoInput != null) {
    videoInput.addEventListener('change', () => {
        const files = Array.from(videoInput.files);
        videoPreview.innerHTML = "";

        if (files.length === 0) {
            videoSection.classList.add("d-none");
            return;
        }

        videoSection.classList.remove("d-none");

        files.forEach(file => {
            const video = document.createElement("video");
            video.src = URL.createObjectURL(file);
            video.controls = true;
            videoPreview.appendChild(video);
        });
    });

}

if (clearImages != null) {
    clearImages.addEventListener("click", () => {
        imageInput.value = "";
        imagePreview.innerHTML = "";
        imageSection.classList.add("d-none");
    });
}


if (clearVideos != null) {
    clearVideos.addEventListener("click", () => {
        videoInput.value = "";
        videoPreview.innerHTML = "";
        videoSection.classList.add("d-none");
    });
}

// afisare imagini / videoclipuri din postari
document.addEventListener("DOMContentLoaded", () => {

    const container = document.getElementById("mediaContainer");
    console.log("salut");
    if (!container) return;

    const postId = container.dataset.postId;
    let mediaList = [];
    let currentIndex = 0;
    console.log("am intrat");

    fetch(`/Posts/GetMedia?id=${postId}`)
        .then(res => res.json())
        .then(data => {

            mediaList = [
                ...(data.images || []).map(i => ({ type: 'image', url: i.url })),
                ...(data.videos || []).map(v => ({ type: 'video', url: v.url }))
            ];

            if (mediaList.length === 0) return;

            showMedia(currentIndex);
        })
        .catch(err => console.error("Failed to load media", err));

    console.log(mediaList);
    function showMedia(index) {
        const item = mediaList[index];
        container.innerHTML = '';

        if (item.type === 'image') {
            const img = document.createElement('img');
            img.src = item.url;
            container.appendChild(img);
        } else if (item.type === 'video') {
            const video = document.createElement('video');
            video.src = item.url;
            video.controls = true;
            container.appendChild(video);
        }
    }


    const prevBtn = document.querySelector(".arrow-left-button");
    const nextBtn = document.querySelector(".arrow-right-button");

    prevBtn?.addEventListener("click", () => {
        if (!mediaList.length) return;
        currentIndex = (currentIndex - 1 + mediaList.length) % mediaList.length;
        showMedia(currentIndex);
    });

    nextBtn?.addEventListener("click", () => {
        if (!mediaList.length) return;
        currentIndex = (currentIndex + 1) % mediaList.length;
        showMedia(currentIndex);
    });

});
