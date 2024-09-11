function GetUserAgreement(event) {
  event.preventDefault();
  // Redirect the user to Spotify authorization endpoint
  var client_id = "0dd26d11f1f9480c926c221561c67c92";

  //Get the current URL
  var redirect_uri = window.location.origin + window.location.pathname + "#cta";
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
  try {
    const response = await fetch(
      `https://372m5i2vzr6ajx3jjod4zvtlwi0uskpo.lambda-url.eu-west-2.on.aws/signup/${client_secret}`,
      {
        method: "POST",
        mode: "cors",
        headers: {
          "Access-Control-Allow-Origin": "*",
        },
      }
    );

    if(!response.ok)
    {
      HandleRegistrationError(response.statusText);
      return false;
    }

    console.log("User signed up successfully");
    FeedbackText("You're all signed up!");
    return true;
  } catch (error) {
    HandleRegistrationError(error);
    return false;
  }
}

function HandleRegistrationError(error)
{
  FeedbackText("Authentication failed. Please try again.");
  console.error("Failed to sign up user");
  console.error(error);
  window.location.href = window.location.pathname;
}

function ListenToSignUpButton() {
  const button = document.getElementsByClassName("cta-button")[0];
  button.onclick = GetUserAgreement;
}

function FeedbackText(feedbackText) {
  const signupElement = document.getElementsByClassName("cta-button")[0];
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

    console.log(
      "Found authorization code. Registering user to Tomorrow-ify service."
    );
    await RegisterUser(authCode);
  } else {
    console.log("Authorization code not found. Displaying sign up button.");
  }

  ListenToSignUpButton();
};
