//Tomorrowify Javascript File


function signUp()
{
    console.log("Hallo World");
    SpotifyAuthRedirect();
}

async function RequestAccessToken(authCode) {
    console.log("Fetching access token using "+ authCode);
    const url = "https://accounts.spotify.com/api/token";
    var client_id = "0dd26d11f1f9480c926c221561c67c92";
    const path = window.location.href.split("?")[0];

    const form = new FormData();
    form.append("grant_type", "authorization_code");
    form.append("code", authCode.toString());
    form.append("redirect_uri", path);

    console.log("Sending form: ");
    for (var pair of form.entries()) {
      console.log(pair[0] + " - " + pair[1]);
    }

    const response = await fetch(url, {
      method: "POST",
      body: form,
      headers: {
        "Content-Type": "application/x-www-form-urlencoded",
        Authorization: "Basic " + btoa(client_id + ":" + authCode),
      },
    });

    console.log(response.json());
}

// Redirect the user to Spotify authorization endpoint
function SpotifyAuthRedirect() {
    var client_id = "0dd26d11f1f9480c926c221561c67c92";

    //Get the current URL
    var redirect_uri = window.location.href;
    console.log('Offering redirect to: ' + redirect_uri);
    var scope = 'playlist-modify-public playlist-modify-private playlist-read-private';//Probably need more permissions e.g. 

    var url = 'https://accounts.spotify.com/authorize';
    url += '?response_type=code';
    url += '&client_id=' + encodeURIComponent(client_id);
    url += (scope ? '&scope=' + encodeURIComponent(scope) : '');
    url += '&redirect_uri=' + encodeURIComponent(redirect_uri);
    // url += '&state=' + encodeURIComponent(state);

    // Redirect the user to Spotify authorization endpoint
    window.location.href = url;
}

function AddSignUpButton() {
    const button = document.createElement("button");
    button.innerHTML = "Sign up to Tomorrow-ify";
    button.onclick = signUp;
    document.getElementById("Signup").appendChild(button);
}

function AddSignedUpText() {
    const textElement = document.createElement("p");
    const textNode = document.createTextNode("You're all signed up!");
    textElement.appendChild(textNode);
    
    document.getElementById("Signup").appendChild(textElement);
}

// Parse the authorization code from the URL
window.onload = function() {
    // Parse the URL query string
    const queryString = window.location.search;
    const urlParams = new URLSearchParams(queryString);

    // Check if the authorization code is present
    const authCode = urlParams.get('code');

    if (authCode) {
        console.log('Authorization Code:', authCode);
        // Call to server function to swap authorization code for access token
        // This needs to be done server-side because it requires the client secret
        
        RequestAccessToken(authCode);
        AddSignedUpText();

    } else {
        console.log('Authorization code not found.');

        AddSignUpButton();
    }
};
