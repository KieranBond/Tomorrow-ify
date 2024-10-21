# Tomorrowify

For tomorrow, Spotify.

Giving you a playlist for tomorrow's listening pleasure.

## Local Development

### Website

To run the website locally, you can use the `Make` command `make run-website`. This will use node to host the website on a local server for you. You don't _have_ to host this way, but it makes life easier.

This requires you having `node` installed, and an `NPM` version of 5.2.0 or greater.

It should get the website running in your local browser (with hot reloads) on `http://localhost:8080`.

### Backend

To start the database, run `docker compose up` from the root folder. That will get an instance of [DynamoDB](https://aws.amazon.com/dynamodb/) running locally at `http://localhost:8000`.

You can then run the backend by using the `http` profile in `launchsettings.json`.

#### Spotify APIs
Go to developer.spotify.com, and sign up (or login). You'll need to have an app, so create one if necessary.

The key things to ensure are set are these callback URLs in your `appsettings.development.json`:
```
http://localhost:8080/signedup
http://127.0.0.1:8080/signedup
```

and the Web API enabled.

Once you have done that, retrieve the 'client secret' and run this command in the `src/Backend/Tomorrowify` folder:
`dotnet user-secrets set "ClientSecret" "12345-yoursecretid"`.

You should now be able to run the backend and interact with the Spotify API.


## Contributors

Shout out to those who have helped, through any form of contribution!

* @AshleyCheema
* @danielmccluskey
