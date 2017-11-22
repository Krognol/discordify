# discordify

There's a pre-built binary under releases for Windows. You just have to edit the `discordify.exe.config` with the client id's and the Spotify secret.

## Setting up

*  Clone the repository `git clone https://github.com/Krognol/discordify` and build the project with visual studio. (Might work with Omnisharp for VScode).
*  Create an application [at the discord dev pages](https://discordapp.com/developers/applications/me).
* * You can name it whatever, copy the `Client ID` and set that in `App.config:discord_client_id` .
* * Click on `Enable rich presence` at the bottom of the page.
* * Upload a large asset for an icon and name it `spotify_large`.
* * You'll also want to get the `discord-rpc-w32.dll` from the `lib` dir or from [the official discord-rpc repo](https://github.com/discordapp/discord-rpc).
* Create a Spotify api application, and copy the Client ID and secret and set that in `App.config:spotify_client_id/secret`
* Launch discordify and click on `Initialize`
* Go to your settings in Discord, and go to Games
* Click on `Add it!` next to `Not seeing your game?`
* Click on `Discordify.exe`
* Done!

You can add a lookup for another music application in the config file under the `appSettings` section.
E.g. 
```xml
<add key="other_process_name" value="other music program name">
<add key="other_process_sep" value="the track title, artist separator">
```

You should see a rich presence on your profile if you click on it after a few seconds.

![example](https://i.imgur.com/od6xC8j.png)

If you don't have any media keys, you don't need to worry, the application checks if there's been any change every 10 seconds.