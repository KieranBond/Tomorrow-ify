//Tomorrowify Javascript File


function signUp()
{
    console.log("Hallo World");
    SpotifyAuthRedirect();

}

// Redirect the user to Spotify authorization endpoint
function SpotifyAuthRedirect() {
    var client_id = 'abc';

    //Get the current URL
    var redirect_uri = window.location.href;
    var scope = 'playlist-modify-public playlist-modify-private playlist-read-private';//Probably need more permissions e.g. 

    var url = 'https://accounts.spotify.com/authorize';
    url += '?response_type=code';
    url += '&client_id=' + encodeURIComponent(client_id);
    url += (scope ? '&scope=' + encodeURIComponent(scope) : '');
    url += '&redirect_uri=' + encodeURIComponent(redirect_uri);

    // Redirect the user to Spotify authorization endpoint
    window.location.href = url;
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

    } else {
        console.log('Authorization code not found.');
    }
};
