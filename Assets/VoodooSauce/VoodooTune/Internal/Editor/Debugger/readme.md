# VoodooTune Editor Debugger
The VoodooTune Editor Debugger (also called **VTED**) will help you verify that the configurations you defined in VoodooTune are working as you want them to. 

You’ll be able to select the ABTests and Segments that defines your player. You'll then receive the configuration that this specific player would have received. And this, directly in the Unity editor to avoid having to get out of your working environment.

## Requirements
- `Newtonsoft.Json` v3.0.2 or above.
- `VoodooSauce` v7.0 or above.
- `VoodooTune API`

To help you, VTED will automatically offer you to download `Newtonsoft.Json` in v3.0.2 (provided by the Unity Package Manager) when you first install it.

It will not download the latest version as we never know what might have changed and that could break the code. However, you should be able to use an upper version without trouble in most cases.

It will communicate with the VoodooTune API which should obviously be up and running.

This tool was made for Unity 2020.3. However, it is retro-compatible with 2018.4, 2019.4 and all 2020 versions. It hasn’t been tested in 2021 yet.

## Distribution
You can get access to this tool by downloading the VoodooSauce (v6.5 and above) or directly from its [GitHub repository](https://github.com/VoodooTeam/VoodooTune-Unity-Toolbox)
It will always be accompanied by the VoodooTune Unity SDK as it relies on it.

## How to Use
In the Unity toolbar, go to `VoodooSauce/VoodooTune/Editor Debugger`.
It will open the tool window. All the tools features are in this window.
Select the cohort you want to test by choosing an abtest and a cohort inside of it.
Then click on the `Apply` button to get the configuration for this cohort.
You can choose multiple cohorts if your project contains multiple Layers (go to the VoodooTune dashboard for more details)
If you want to test a specific set of Remote Configs, you can define the segment you want to test and apply them.
If you want to get back to your default config, click on the `Reset Config` button.

Notes: The system uses the VoodooTune Unity SDK which relies on 3 x-api-keys :
- tech
- staging
- dev

If you encountered some authentication issues, it will probably be related to theses keys.
Be careful when regenerating them. They need to be in a specific format (only letters, numbers and dashes).


## Features
- Reload the app information.
- Change the environment `tech/staging/dev`.
- Change the target version `wip/live`. Other specific versions will be available in a future release.
- Select the platform `iOS/Android`. This value will not modify the current build target to avoid some very long loading time.
- Select one or more `Segment`.
- Select one or more cohorts (`abTest` + `cohort`).
- Apply the setup.
- Display the current setup (version, segments, abtests and cohorts).
- Display the configuration associated to the current setup
- Copy the URL of the configuration you are testing
- Display the differences between the default configuration and the current one.
- Ping a class in your Project view (it will ping the file associated to your class).

## FAQ

### I just installed the latest version and I have some errors related to NewClassInfo and TypeUtility not existing.
You most probably already have the TypeHelper and WebRequestUtility in your project. The `VoodooTune Unity SDK` is based on those packages and they need to be in the same assembly.
To fix that, you have to move those folders (probably located under Assets/Voodoo) into the VoodooTune Unity SDK folder (Assets/VoodooSauce/VoodooTune/Internal/SDK).
To be sure that this will not happen, I advise you to delete those folders before adding the package in your project.

### I don't have the possibility to select multiple abtests.
Your game possess only 1 layer. You can't get the configuration from multiple abtests if they are in the same layer.

