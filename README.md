WallpaperChanger
================

WallpaperChanger is a command line utility for changing the wallpaper (desktop background) in Windows.

The intent of this program is not to be a standalone wallpaper program (it is too cumbersome to use for that). It is intended to be used as a “helper” utility program for another program or script.

This utility works fine with Windows XP, Vista, 7, 8, and 10.

The compiled program is included as WallpaperChanger.exe

Leave comments or questions on this blog post: http://sg20.com/techblog/2011/06/23/wallpaper-changer-command-line-utility

## Use

The program takes two arguments: the file (including path) to use as wallpaper, and the style (Tiled, Centered, Stretched, Fit, Fill). Instead of a file, a directory can also be specified in which case a random image from the directory will be set as the wallpaper.
Syntax is: [file|directory] [style] [location]

```
  [file] is the complete path to the file
  [directory] is the complete path to a directory containing image files
    a random image from the directory will be set as the background
  [style] is an integer (if no style is specified it defaults to Stretched):
    0 for Tiled
    1 for Centered
    2 for Stretched
    3 for Fit (Windows 7 or later)
    4 for Fill (Windows 7 or later)
  [location] is the complete path to a directory for storing the generated file
    defaults to the temp folder which should be fine in most cases
```

If the style argument is not specified it will default to Stretched.

Optional flags:
*   -h, -help   - Display the usage help
*   -r, -remove - Remove the current wallpaper

Alternatively a config file can be placed in the same directory as the 
WallpaperChanger executable. The file should be named 'config' without 
any file extension.  Each line in the file should have the full path to 
an image and can optionally include the style code to use.  If the style
is not specified it will default to Stretched.

The file type can be any of the standard picture types (bmp, jpg, gif, etc.). The program will automatically convert the file to a bmp file (required by windows) and place it within the users temp directory. Storing the bmp file in the temp directory should be fine in most cases, however if you would prefer to use an alternative directory you can specify it as parameter number 3.

When using the config file it should be formatted like this:

```
C:\wallpaper1.jpg
C:\wallpaper\wallpaper2.jpg 3
D:\wallpaper3.png 2
"D:\wallpapers to use\image1.jpg"
etc.
```

Full paths to the image files should be used and if the path or filename contains spaces you will need to wrap it in quotes.

The program has a return code – 0 for normal, 1 for error

An error code will be returned if the program had an exception, for example invalid syntax, invalid file name, invalid style type, etc.

## Building the Program

I currently build the program using Microsoft Visual Studio 2013 Express edition.  It targets the .NET Framework v2.0 as that is all it requires.

## What’s New

* 1.5 - Wallpaper is now converted to png instead of bmp to avoid artifacts
* 1.4 – Added remove flag and config file support
* 1.3 – Added two new styles for Windows 7 or later (Fit and Fill)
