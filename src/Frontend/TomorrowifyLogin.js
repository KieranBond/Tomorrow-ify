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
    return true;
  } catch (error) {
    HandleRegistrationError(error);
    return false;
  }
}

function HandleRegistrationError(error)
{
  console.error("Failed to sign up user");
  console.error(error);
  window.location.href = window.location.pathname;
}

// Parse the authorization code from the URL
window.onload = async function () {
  // Parse the URL query string
  const queryString = window.location.search;
  const urlParams = new URLSearchParams(queryString);

  // Check if the authorization code is present
  const authCode = urlParams.get("code");

  if (authCode) {
    console.log(
      "Found authorization code. Registering user to Tomorrow-ify service."
    );
    await RegisterUser(authCode);
  } else {
    console.log("Authorization code not found.");
  }
};
