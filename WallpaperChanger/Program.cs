// Copyright (c) 2014 Phillip Hansen
//
// This is a command line program to change the wallpaper.  The program takes two arguments: the file (including path) to use as wallpaper, and the style (Tiled, Centered, Stretched, Fit, Fill)
// Syntax is: <file|directory> <style> <location>
//
//  <file> is the complete path to the file
//  <directory> is the complete path to a directory containing image files
//    a random image from the directory will be set as the background
//  <style> is an integer (if no style is specified it defaults to Stretched):
//    0 for Tiled
//    1 for Centered
//    2 for Stretched
//    3 for Fit (Windows 7 or later)
//    4 for Fill (Windows 7 or later)
//  <location> is the complete path to a directory for storing the generated file
//    defaults to the temp folder which should be fine in most cases
//
// If the style argument is not specified it will default to Stretched.
//
// Optional flags:
//   -h, -help   - Display the usage help
//   -r, -remove - Remove the current wallpaper
//
// Alternatively a config file can be placed in the same directory as the WallpaperChanger executable.
// The file should be named 'config' without any file extension.  Each line in the file should have
// the full path to an image and can optionally include the style code to use.  If the style is not
// specified it will default to Stretched.
// 
// This program is intended to be used as a "helper" program that is executed from other programs

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.IO;
using System.Data;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace WallpaperChanger
{

    class Wallpaper
    {
        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        // this is the system DLL for doing wallpaper stuff
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public enum Style : int
        {
            Tiled,
            Centered,
            Stretched,
            Fit,
            Fill
        }

        public static int Set(String file, Style style, String storagePath)
        {
            try
            {
                System.Drawing.Image img = System.Drawing.Image.FromFile(file);
                // convert and save the image as a bmp file (bmp format is required for the wallpaper)
                img.Save(storagePath, System.Drawing.Imaging.ImageFormat.Bmp);
            
                // update the regsitry
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
                if (style == Style.Tiled)
                {
                    key.SetValue(@"WallpaperStyle", 0.ToString());
                    key.SetValue(@"TileWallpaper", 1.ToString());
                }
                if (style == Style.Centered)
                {
                    key.SetValue(@"WallpaperStyle", 0.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                }
                if (style == Style.Stretched)
                {
                    key.SetValue(@"WallpaperStyle", 2.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                }
                if (style == Style.Fit)
                {
                    key.SetValue(@"WallpaperStyle", 6.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                }
                if (style == Style.Fill)
                {
                    key.SetValue(@"WallpaperStyle", 10.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                }

                // set the wallpaper using the external method
                SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, storagePath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
            }
            catch (OutOfMemoryException ex)
            {  // thrown when the file does not have a valid image format or the decoder does not support the pixel format of the file
                Console.WriteLine("\nInvalid file format or the file format is not supported");
                return 1;
            }
            catch (Exception ex)
            { // catch everything else just in case
                Console.WriteLine("<unexpected error>\n\n" + ex.ToString());
                return 1;
            }

            return 0;
        }

        public static int Remove()
        {
            try
            {
                // remove the wallpaper using the external method
                SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, "", SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
            }
            // catch everything just in case
            catch (Exception ex)
            {
                Console.WriteLine("<unexpected error>\n\n" + ex.ToString());
                return 1;
            }

            return 0;
        }

        static int Main(string[] args)
        {
            String help = "\nCopyright (c) 2014 Phillip Hansen  http://sg20.com (version 1.4)\n\nSyntax is: <file|directory> <style> <location>\n\n  <file> is the complete path to the file\n  <directory> is the complete path to a directory containing image files\n    a random image from the directory will be set as the background\n  <style> is an integer (if no style is specified it defaults to Stretched):\n    0 for Tiled\n    1 for Centered\n    2 for Stretched\n    3 for Fit (Windows 7 or later)\n    4 for Fill (Windows 7 or later)\n  <location> is the complete path to a directory for storing the generated file\n    defaults to the temp folder which should be fine in most cases";
            help += "\n\nIf the style argument is not specified it will default to Stretched.";
            help += "\n\nOptional flags:\n  -h, -help   - Display the usage help\n  -r, -remove - Remove the current wallpaper";
            help += "\n\nAlternatively a config file can be placed in the same directory as the WallpaperChanger executable. The file should be named 'config' without any file extension.  Each line in the file should have the full path to an image and can optionally include the style code to use.  If the style is not specified it will default to Stretched.";

            String path = "";
            bool usingConfig = false;

            // check the arguments
            if (args.Length == 0)
            {
                // a config file can be stored in the same directory as the wallpaper changer
                String configFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "config");
                if (File.Exists(configFile))
                {
                    path = configFile;
                    usingConfig = true;
                }
                else
                {
                    Console.WriteLine(help);
                    return 1;
                }
            }

            if (!usingConfig)
            {
                // special check for a help flag
                if (args[0] == "-h" || args[0] == "--help" || args[0] == "-help")
                {
                    Console.WriteLine(help);
                    return 1;
                }
                // remove wallpaper flag
                else if (args[0] == "-r" || args[0] == "-remove")
                {
                    return Remove();
                }

                // retrieve file/directory if we are not using config file
                path = args[0];
            }
            
            Style style = Style.Stretched; // default value
            // get the path to the user's temp folder
            String storagePath = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");

            if (args.Length >= 2)
            {
                style = (Wallpaper.Style)Enum.Parse(typeof(Wallpaper.Style), args[1]);
            }
            
            if (args.Length == 3)
            {
                // make sure it's a directory
                if (!Directory.Exists(args[2]))
                {
                    Console.WriteLine("\n{0} is not a valid directory.", args[2]);
                    return 1;
                }
                storagePath = Path.Combine(args[2], "wallpaper.bmp");
            }

            if (usingConfig)
            {
                String file;
                ProcessConfig(path, out file, out style);
                if (file == null)
                {
                    Console.WriteLine("\nNo valid images found in the config file: {0}", path);
                    return 1;
                }
                int status = Wallpaper.Set(file, style, storagePath);
                if (status == 1)
                    return 1;
            }
            else if (File.Exists(path))
            {
                int status = Wallpaper.Set(path, style, storagePath);
                if (status == 1)
                    return 1;
            }
            else if (Directory.Exists(path))
            {
                String file = ProcessDirectory(path);
                if (file == null)
                {
                    Console.WriteLine("\nNo valid images found in {0}.", path);
                    return 1;
                }
                int status = Wallpaper.Set(file, style, storagePath);
                if (status == 1)
                    return 1;
            }
            else
            {
                Console.WriteLine("\n{0} is not a valid file or directory.", path);
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// Get the list of files from the given directory and choose a random image file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static String ProcessDirectory(String path)
        {
            ArrayList files = new ArrayList();
            bool finished = false;
            String file = null;

            // get the list of files in the directory
            foreach (String tmpFile in Directory.GetFiles(path))
            {
                files.Add(tmpFile);
            }

            if (files.Count == 0) // ignore empty directories
            {
                return null;
            }

            // initialize a Random object with a unique seed (based on the current time)
            Random randomObject = new Random(((int)DateTime.Now.Ticks));

            // select a random file and check it against the list of file types
            while (!finished)
            {
                // pick a random index from the list
                int index = randomObject.Next(0, files.Count);

                String f = (String)files[index];
                // this is an image file
                if (f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                {
                    file = f;
                    finished = true;
                }
                // this is not an image, remove it from the list
                else
                {
                    files.RemoveAt(index);
                }

                // if the files list is now empty, end the loop
                if (files.Count == 0)
                {
                    finished = true;
                }
            }
            
            return file;
        }

        /// <summary>
        /// Get the list of files from the config file and choose a random image
        /// </summary>
        /// <param name="path">config file</param>
        /// <param name="file">filled in with the selected image file</param>
        /// <param name="style">filled in with the style</param>
        static void ProcessConfig(String path, out String file, out Style style)
        {
            file = null;
            style = Style.Stretched;
            ArrayList files = new ArrayList();
            bool finished = false;
            
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        files.Add(line);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading from config file\n\n" + ex.ToString());
                return;
            }

            if (files.Count == 0)
            {
                return;
            }

            // initialize a Random object with a unique seed (based on the current time)
            Random randomObject = new Random(((int)DateTime.Now.Ticks));

            // select a random file and check it
            while (!finished)
            {
                // pick a random index from the list
                int index = randomObject.Next(0, files.Count);

                String f = (String)files[index];
                // parse the file
                // can contain file and style
                // split on the space between them, and combine values within double quotes (e.g. files with a space in the path)
                MatchCollection matches = Regex.Matches(f, @"(?<match>[^\s""]+)|""(?<match>[^""]*)""");
                String tmpFile = matches[0].Groups["match"].Value;
                if (matches.Count == 2)
                {
                    style = (Wallpaper.Style)Enum.Parse(typeof(Wallpaper.Style), matches[1].Groups["match"].Value);
                }

                // make sure the image exists
                if (File.Exists(tmpFile))
                {
                    file = tmpFile;
                    finished = true;
                }
                // remove it from the list
                else
                {
                    files.RemoveAt(index);
                }

                // if the files list is now empty, end the loop
                if (files.Count == 0)
                {
                    finished = true;
                }
            }
        }
    }
}
