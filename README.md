# discordify
In this guide you can find exactly how to setup discordify for your rich presence needs.

There's a pre-built binary under releases for Windows. You just have to edit the `discordify.exe.config` with the client id's and the Spotify secret.

## Prerequisites
In order to use discordify, you'll need to prepare some things before getting started with discorify itself. You'll need to make both a Discord App and a Spotify App. This can be complicated to figure out yourself, but if you follow the steps below you'll be guaranteed to get everything working correctly.

### Spotify App
We'll start off with the easier of the two. We need to get Spotify Client ID and secret, so follow these steps to get those setup for yourself.

* Go to [the Spotify developer dashboard](https://beta.developer.spotify.com/dashboard/) and log in with your Spotify account.
* Click `Create an App`, followed by `No`. We're creating a non-commercial application.
* Enter an application name and description. People in Discord won't see this so make it anything you'd like.
* Click the 3 checkboxes and click `Create`.
* On the newly created application's page, copy the Client ID and save it for later.
* On the same page, click `Show Client Secret` and copy the Client Secret and save it for later.
* You're done!

### Discord App
This is a little harder than the Spotify App. However, it'll be easy if you just follow these instructs! Let's get started right away.

* Go to [the Discord developer portal](https://discordapp.com/developers/applications/me/create) and log in with your Discord account.
* Enter an application name. This will be visible as the "game" you're currently playing, so make it something good. A good example would simply be "Spotify".
* Click the `Create App` button.
* In the "App Details", copy the Client ID and save it for later.
* Scroll down and click the `Enable Rich Presence` button.
* (Optional) Do the following if you want to display the Spotify logo (or anything, for that matter) to the left of your currently playing music.
	* On the Discord developer portal, browse to the "Rich Presence Assets" section.
	* Enter the name `spotify_large`, select the type you want, and select an image from your computer.
		* Want the default Spotify logo? Save [this image](https://i.imgur.com/2sCvm0v.png) to your computer by right-clicking the link and clicking "Save link as".
	* Click the `Upload Asset` button.
* Click the `Save changes` (just to be sure everything went to plan).
* You're done!

## Quick-setup
If you're just a user looking to start using discorify, this section is for you!

* Go the [the Releases page](https://github.com/Krognol/discordify/releases) and download the latest release.
* Extract the 7-zip file using an archive extraction program, make sure you save it at a safe location.
* In the newly created discordify directory, open `discordify.exe.config` with a text editor such as Notepad.
* Replace `YOUR_DISCORD_CLIENT_ID`, `YOUR_SPOTIFY_CLIENT_ID` and `YOUR_SPOTIFY_CLIENT_SECRET` with the information you created at the **Prerequisites** section above.
* Launch discordify by double-clicking `Discordify.exe`.
* In Discord, go to the "User Settings > Games" page.
* Click on the "Add it!" link and select "Discorify".
* You're done! Click `Initialize` and see if everything works!

You should see a rich presence on your profile after a few seconds, like shown below:

![example](https://i.imgur.com/od6xC8j.png)

If you don't have any media keys, you don't need to worry, the application checks if there's been any change every 10 seconds.

## Other applications
**Note:** This is relatively advanced and shouldn't be done by a novice computer user. Not all programs may be compatible with discordify and you may experience crashes when that's the case.

If you don't like to use Spotify and would love to instead use a different program, you can change the program discorify looks for by adding the following to the `discordify.exe.config` file, right under the Spotify Client secret in the `appSettings` section:

```xml
<add key="other_process_name" value="PROGRAM_NAME_HERE">
<add key="other_process_sep" value="SEPARATOR_HERE">
```

You'll need to do a little figuring out on how this work with your program of choice. You'll need to replace `PROGRAM_NAME_HERE` with the program's process name. In Spotify's case this would be "Spotify". You can find out a program's process name by searching for it in the Task Manager. You'll also need to replace `SEPARATOR_HERE` with a separation character. This is used to distinguish the artist's name from the track's name. In Spotify's case this is " - ", but it could be different for your program.

## For developers
This section is for developers only, so if you're not a developer, please ignore this section.

*  Clone the repository `git clone https://github.com/Krognol/discordify` and build the project with visual studio. (Might work with Omnisharp for VScode).
* You'll also want to get the `discord-rpc-w32.dll` from the `lib` dir or from [the official discord-rpc repo](https://github.com/discordapp/discord-rpc).
* Launch discordify and click on `Initialize`
