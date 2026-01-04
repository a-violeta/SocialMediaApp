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
            const heart = element.querySelector('i');
            if (data.liked) heart.classList.add('text-danger');
            else heart.classList.remove('text-danger');
        });
}
