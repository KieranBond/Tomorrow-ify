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

form.addEventListener('submit', (event) => {
    event.preventDefault();

    // Derive formdata from the submit form
    const formData = new FormData(form);

    // Convert FormData to a plain object
    const dataObject = {};
    formData.forEach((value, key) => {
        dataObject[key] = value;
    });

    //Log Form Data
    console.log('Form Data:', dataObject);

    /* Example of submitting form to somewhere using fetch
        fetch('/submit-form', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(dataObject),
        })
        .then(response => response.json())
        .then(data => {
            console.log('Success:', data);
            formMessage("You have been added to our mailing list.", true);
        })
        .catch((error) => {
            console.error('Error:', error);
            formMessage("Oops, something went wrong... Please try again shortly.", false);
        });

        */

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

