# discordify

## Setting up

*  Clone the repository `git clone https://github.com/Krognol/discordify`.
*  Create an application [at the discord dev pages](https://discordapp.com/developers/applications/me).
* * You can name it whatever, copy the `Client ID` and set that in `Form1.cs:DISCORD_CLIENT_ID` .
* Upload a large asset for an icon and name it `spotify_large`.
* Create a Spotify api application, and copy the Client ID and secret and set that in `Form1.cs:SPOTIFY_CLIENT_ID/SECRET`
* Launch discordify and click on `Connect`
* Go to your settings in Discord, and go to Games
* Click on `Add it!` next to `Not seeing your game?`
* Click on `Drive:/where/you/put/it/discordify.exe`
* Done!

You should see a rich presence on your profile if you click on it after a few seconds.

![example](https://i.imgur.com/od6xC8j.png)

If you don't have any media keys, you don't need to worry, the application checks if there's been any change every 10 seconds.