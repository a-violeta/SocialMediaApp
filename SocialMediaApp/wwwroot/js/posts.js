function toggleLike(element) {
    const postId = element.dataset.postId;

    fetch(`/Posts/ToggleLike?postId=${postId}`, {
        method: 'POST',
        headers: {
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    })
        .then(res => res.json())
        .then(data => {
            element.querySelector('.likes-count').innerText = data.likesCount;
            const heart = element.querySelector('.heart-button');
            if (data.liked) {
                heart.classList.add('text-danger');
                heart.classList.remove('bi-heart');
                heart.classList.add('bi-heart-fill');
            }
            else {
                heart.classList.add('bi-heart');
                heart.classList.remove('text-danger');
                heart.classList.remove('bi-heart-fill');
            }

        });
}

function StopEventPropagation(event) {
    event.stopPropagation();
}
