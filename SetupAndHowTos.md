# Introduction #

This page gives a quick tutorial for setting up the software you'll need to contribute, and some how-tos for common tasks such as pulling and pushing changes.


# Setup #

  1. Install Visual Studio, either the full version from MSDNAA, or the C# Express edition from http://www.microsoft.com/express/Downloads/#2010-Visual-CS
  1. While VS is downloading/installing, install TortoiseHg from http://tortoisehg.bitbucket.org/ (64-bit link just below big download button).
  1. Create a folder on your computer where you'd like a copy of the code repository to go. Right-click on this folder and click TortoiseHg -> Clone...
  1. Paste this into the Source field: `https://kettering-cs320-procedural-graphics.googlecode.com/hg/`
  1. Put a trailing `\` at the end of the destination field -- this prevents an extra subfolder being created.
  1. Click Clone. You now have a local repository of the project.
  1. Wait for VS to finish installing.
  1. Optional (full VS only -- not Express): Install VisualHG from http://visualhg.codeplex.com/

Another nice thing to setup:
  1. Run TortoiseHg Workbench from the start menu
  1. Double-click the repository from the list on the left.
  1. Click View -> Synchronize from the menubar.
  1. The lower pane should change; it should show the the URL of this Google Code repository. Click the button with the padlock.
  1. Enter your GMail address as the user name.
  1. Find your commit password at: https://code.google.com/hosting/settings and enter it in the password field.
  1. Click save.

# Using Mercurial / TortoiseHg #

The clone you created in Setup is a fully self-contained Mercurial repository, with it's own history, etc. Mercurial tracks the history of files. When you add a new file, you need to tell Mercurial/TortoiseHg to track it. After you've finished making a set of changes, you need to Commit them. When you want to push your commits to (or pull down the latest version from) the Google Code repository, you need to Synchronize.

## Steps for pulling changes down from the project repo ##

  1. Run TortoiseHg Workbench from the start menu.
  1. Double-click the repository from the list on the left.
  1. Click View -> Synchronize from the menubar.
    * One time-setup: Click the "Post pull: None" button and change select the Update option (this saves you a step), then click Save.
  1. Click the icon with the arrow coming toward you, second button on the toolbar in the middle. This is the Pull Incoming Changesets button. (The first button, just to the left of it, is the Preview Incoming Changesets button)
  1. It should download the latest changeset history and automatically apply the latest version of the repository to your local copy.

## Committing changes ##

  1. Run TortoiseHg Workbench from the start menu, double-click the repo.
  1. Go to the Commit view (View->Commit, or the checkbox toolbar button).
  1. In the lower-left list will be a list of files that has changed since the last commit.
    1. Any files that were created will be magenta have a `?` next to it. Select the files you've created and right click->Add. They should now be green with an `A` next to it
      * (Mercurial does not track new files until you tell it to.)
      * (Unless you use VisualHG, in which case this step should be done for you when you create a new code file inside of Visual Studio.)
    1. Check all the files whose changes you wish to commit.
      * Clicking on the file will show your changes in the lower-right.
      * If you've added files please also check in the .csproj file
  1. Enter a short description of the changes in the middle-right box.
  1. Click the commit button at the middle-far-right.

## Pushing commits up to the project repo ##

  1. Open the workbench and the repo.
  1. Click View -> Synchronize.
  1. The third button on the middle toolbar is the Preview Outgoing Changesets button (small arrow going from the near cylinder to a grayed far cylinder).
  1. The fourth button is the Push Outgoing Changesets button. Click this to upload your commits to the project repo.
  1. Remember to update any issues on the issue tracker.