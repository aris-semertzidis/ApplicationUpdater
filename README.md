# Application Updater
A simple program to upload your application and having clients to automatically update their files.
Consists of 2 essential elements. The AppBuilder and AppUpdater.

## How is working
AppBuilder is reading the local build folder and generating a BuildManifest which contains a list of all files and their respective hash.
Then uploads the files to a remote.
AppUpdater is reading the BuildManifest on the remote and checking the local files for any differences in hashes or missing files and then downloading them.

## Views
Created a console and WinForms application for each segment (Builder, Updater) where they can work agnostically and the application settings (ftp, https, urls etc) can be set on the build folder (ftp.json, http.json)

## Considerations
This is a work in progress program, but the basic functionality is working.

## Future Development
Extend the IWriter interface for AWS S3 storage.
Create a versioning system so can have multiple versions and environments (beta, release etc) before fetching the manifest.
