WallpaperChanger
================

WallpaperChanger is a command line utility for changing the wallpaper (desktop background) in Windows.

The intent of this program is not to be a standalone wallpaper program (it is too cumbersome to use for that). It is intended to be used as a “helper” utility program for another program or script.

This utility works fine with Windows XP (version 1.8), Vista (version 1.8), 7, 8, 10, and 11.

The compiled program is available on the Releases page: https://github.com/philhansen/WallpaperChanger/releases

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
*   -m, -monitor <index> - Set the image on the specified monitor (0 indexed)
*   -d, -depth <depth limit> - Set the depth for searching directories for image files (e.g. depth 2 will search the initial directory and 1 level deeper). Default is 1.

When using the monitor option, the full syntax is: `-m <index> <file|directory> <location>`  The style does not need to be specified since it appears to always default to Stretched.  Note this functionality is only available on Windows 8 or higher.

Alternatively a config file can be placed in the same directory as the 
WallpaperChanger executable. The file should be named 'config' without 
any file extension.  Each line in the file should have the full path to 
an image or directory and can optionally include the monitor index, directory depth, or the style code to use.  If the style
is not specified it will default to Stretched.  If the directory depth is not specified it will default to 1.

When setting the monitor index in the config file the format of the line should be: `<file> -m <index>`

The file type can be any of the standard picture types (png, bmp, jpg, gif, etc.). The program will automatically convert the file to a png/bmp file and place it within the users temp directory. Storing the png/bmp file in the temp directory should be fine in most cases, however if you would prefer to use an alternative directory you can specify it as parameter number 3.  On Windows 8 or higher it will use a png file for better quality.  It seems Windows 7 and lower cannot use a png file directly so a bmp file is used instead.  Note: On Windows 7 (and lower) if you are using extremely high resolution images then you may run into the "artifact" problem after Windows sets the file as the background.  If you notice artifacts then my recommendation is to use a separate program to lower the resolution of the original image or try saving it as a different file type first (e.g. jpg).

When using the config file it should be formatted like this:

```
C:\wallpaper1.jpg
C:\wallpaper\wallpaper2.jpg 3
D:\wallpaper3.png 2
"D:\wallpapers to use\image1.jpg"
C:\wallpaper4.jpg -m 1
C:\wallpaper -d 3
etc.
```

Full paths to the image files should be used and if the path or filename contains spaces you will need to wrap it in quotes.

The program has a return code – 0 for normal, 1 for error

An error code will be returned if the program had an exception, for example invalid syntax, invalid file name, invalid style type, etc.

## Building the Program

The program now targets .Net Framework 4.8 which supports Windows 7+.  Previous versions (until 1.8) targeted the .NET Framework v2.0 which supported Windows XP+.

## What’s New

* 1.9 - New depth parameter when searching for files in directories, config file supports directories, changed to .Net Framework 4.8 supporting Windows 7+
* 1.8 - Config file now supports the monitor index flag to set the wallpaper for a specific monitor (Windows 8 or higher) 
* 1.7 - Added ability to set the wallpaper for a specific monitor (Windows 8 or higher)
* 1.6 - Use png file for Windows 8 or higher, otherwise use bmp file
* 1.5 - Wallpaper is now converted to png instead of bmp to avoid artifacts
* 1.4 – Added remove flag and config file support
* 1.3 – Added two new styles for Windows 7 or later (Fit and Fill)
