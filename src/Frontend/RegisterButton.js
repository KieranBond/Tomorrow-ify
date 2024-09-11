// JavaScript to handle modal open/close
const openModalBtn = document.getElementById('open-modal-btn');
const closeModalBtn = document.getElementById('close-modal-btn');
const emailModal = document.getElementById('email-modal');
const form = document.getElementById('signup-form');

openModalBtn.addEventListener('click', () => {
    emailModal.style.display = 'flex';
});

closeModalBtn.addEventListener('click', () => {
    emailModal.style.display = 'none';
});

// Optional: Close the modal if the user clicks outside of the modal content
window.addEventListener('click', (event) => {
    if (event.target === emailModal) {
        emailModal.style.display = 'none';
    }
});

function formMessage(message, isSuccess){

    // Message displays the message to the user
    // isSuccess = Success(True) / Error(False)

    const formFeed = document.getElementById("form-feedback");
    if(isSuccess){
        formFeed.classList = "success-message";
    
    } else {
        formFeed.classList = "error-message";
    }

    formFeed.innerHTML = message;

}

