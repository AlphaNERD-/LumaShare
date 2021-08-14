# LumaShare
LumaShare is a simple tool that allows you to create nice-looking shareable pictures from your Luma3DS screenshots. The tool is written in C# and uses .NET Core. I have only tested it on Windows but *theoretically* it should run anywhere (that is if you replace the OpenFileDialog and SaveFileDialog with something more cross-platform compatible).

## Usage instructions
LumaShare should be pretty self-explanatory. Here's how LumaShare looks like:

![Screenshot für Github](https://user-images.githubusercontent.com/9458093/129462707-c808bc74-ef4f-4eb0-9b4d-e1deda05099b.PNG)

Click on the "..." Button next to the Top Screen or Bottom Screen textbox to open the corresponding image file on your PC. LumaShare looks got files ending with \_bot.bmp or \_top.bmp by default. If your screenshot doesn't end with \_bot.bmp or top.bmp, you can show all supported files by selecting "All supported files" in the combo box above the "Open" button.

If you open a \_bot.bmp or \_top.bmp file, LumaShare looks for the other corresponding file in the same directory and if available adds it automatically unless you already picked another file for the top or bottom screen.

If you want to make another shareable picture or picked the wrong file for the top or bottom screen, you can use "Clear Images" to remove the selected images.

You can choose between different profiles (2 included), a transparent background or blurry background generated from top or bottom screen and add a border between the console image and the image border to allow more space for the background. LumaShare stores your settings automatically in a config file in the same directory of the .exe file and loads it automatically when you restart it.

Once you're done creating the picture, click "Save Picture" to save the picture you created.

## Profiles
LumaShare wraps your screenshots neatly into a 3DS image. It does so by loading a profile which contains information about where to put the screenshots and how large they have to be. The profiles are stored in the "profiles" directory relative to the directory of the .exe file. Each profile contains an image file of the console you wish to show your screenshots on and the aforementioned XML file.

Here's a template to create your own profile from:

```xml
<LumaShareProfile Name="<Name of Profile>" ImageFileName="<Image filename of the console>">
	<TopScreen Start="<X,Y>" Size="<Width,Height>"/>
	<BottomScreen Start="<X,Y>" Size="<Width,Height>"/>
</LumaShareProfile>
```

## Build instructions
Since LumaShare is a very small tool with little dependencies, all you need to do is open it in Visual Studio or Visual Studio Code and hit "Run". Visual Studio automatically adds the profiles directory from the source directory to the output directory so you can start using LumaShare right away.
