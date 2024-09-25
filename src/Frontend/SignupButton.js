function GetUserAgreement(event) {
  event.preventDefault();
  // Redirect the user to Spotify authorization endpoint
  var client_id = "0dd26d11f1f9480c926c221561c67c92";

  //Get the current URL
  var redirect_uri = window.location.origin + "/signedup";
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

function HandleRegistrationError(error) {
  console.error("Failed to sign up user");
  console.error(error);
  window.location.href = window.location.pathname;
}

function ListenToSignUpButton() {
  const button = document.getElementsByClassName("cta-button")[0];
  button.onclick = GetUserAgreement;
}

// Parse the authorization code from the URL
window.onload = async function () {
  ListenToSignUpButton();
};
