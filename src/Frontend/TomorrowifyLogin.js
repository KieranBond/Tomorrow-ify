function GetUserAgreement() {
  // Redirect the user to Spotify authorization endpoint
  var client_id = "0dd26d11f1f9480c926c221561c67c92";

  //Get the current URL
  var redirect_uri = window.location.href;
  console.log("Offering redirect to: " + redirect_uri);
  var scope =
    "playlist-modify-public playlist-modify-private playlist-read-private";

  var url = "https://accounts.spotify.com/authorize";
  url += "?response_type=code";
  url += "&client_id=" + encodeURIComponent(client_id);
  url += scope ? "&scope=" + encodeURIComponent(scope) : "";
  url += "&redirect_uri=" + encodeURIComponent(redirect_uri);

  // TODO: Add State to increase security
  // url += '&state=' + encodeURIComponent(state);

  // Redirect the user to Spotify authorization endpoint
  window.location.href = url;
}

async function RegisterUser(client_secret) {
  try 
  {
    await fetch(
      `https://372m5i2vzr6ajx3jjod4zvtlwi0uskpo.lambda-url.eu-west-2.on.aws/signup/${client_secret}`,
      {
        method: "POST",
        mode: "cors",
        headers: {
          "Access-Control-Allow-Origin": "*",
        },
      }
    );
    console.log("User signed up successfully");
    FeedbackText("You're all signed up!");
    return true;

  } 
  catch (error) 
  {
      FeedbackText("Authentication failed. Please try again.");
      console.error("Failed to sign up user");
      console.error(error);
      window.location.href = window.location.pathname;
      return false;
  }
}

function AddSignUpButton() {
  const button = document.createElement("button");
  button.innerHTML = "Sign up to Tomorrow-ify";
  button.onclick = GetUserAgreement;
  document.getElementById("Signup").appendChild(button);
}

function FeedbackText(feedbackText) {

    const signupElement = document.getElementById("Signup");

    let textElement = signupElement.querySelector("p");

    if (!textElement) {
        textElement = document.createElement("p");
        signupElement.appendChild(textElement);
    }

    textElement.textContent = feedbackText;
}

// Parse the authorization code from the URL
window.onload = async function () {
  // Parse the URL query string
  const queryString = window.location.search;
  const urlParams = new URLSearchParams(queryString);

  // Check if the authorization code is present
  const authCode = urlParams.get("code");

    if (authCode) {
    FeedbackText("Please wait while we authenticate...");

    console.log("Found authorization code. Registering user to Tomorrow-ify service.");
    const success = await RegisterUser(authCode);
        if (success) FeedbackText("You're all signed up!");

    } else {
        console.log("Authorization code not found. Displaying sign up button.");
    }
    AddSignUpButton();
};
