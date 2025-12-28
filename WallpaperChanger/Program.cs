// Developed by Phillip Hansen
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
//   -m, -monitor <index> - Set the image on the specified monitor (0 indexed)
//      When using this option the full syntax is:
//          -m <index> <file|directory> <location>
//   -d, -depth <depth limit> - Set the depth for searching directories for image files (e.g. depth 2 will search the initial directory and 1 level deeper). Default is 1.
//      When using this option the full syntax is:
//          -d <depth limit> <file|directory> <location>
//
// Alternatively a config file can be placed in the same directory as the WallpaperChanger executable.
// The file should be named 'config' without any file extension.  Each line in the file should have
// the full path to an image or directory and can optionally include the monitor index, directory depth, or the style code to use. 
// If the style is not specified it will default to Stretched.  If the directory depth is not specified it will default to 1.
//
// When setting the monitor index in the config file the format of the line should be: <file> -m <index>
// 
// This program is intended to be used as a "helper" program that is executed from other programs

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
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

        public static int Set(string file, Style style, string storagePath)
        {
            try
            {
                System.Drawing.Image img = System.Drawing.Image.FromFile(file);
                // convert and save the image as a png/bmp file (png format should work better than bmp to avoid artifacts, but only available on Win8 or higher)
                if (storagePath.EndsWith("png"))
                    img.Save(storagePath, System.Drawing.Imaging.ImageFormat.Png);
                else
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
                Console.WriteLine("<unexpected error>\n\n" + ex.Message);
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// Saves the file to the storage path location and sets it as the background for the specified monitor
        /// </summary>
        /// <param name="monitorIndex"></param>
        /// <param name="file"></param>
        /// <param name="storagePath"></param>
        /// <returns></returns>
        public static int SetMonitor(int monitorIndex, string file, string storagePath)
        {
            try
            {
                if (!IsWin8OrHigher())
                {
                    Console.WriteLine("Specifying a monitor is only supported on Windows 8 or higher\n");
                    return 1;
                }

                IDesktopWallpaper wallpaper = (IDesktopWallpaper)new DesktopWallpaper();
                System.Drawing.Image img = System.Drawing.Image.FromFile(file);
                // convert and save the image as a png file (png format should work better than bmp to avoid artifacts, but only available on Win8 or higher)
                img.Save(storagePath, System.Drawing.Imaging.ImageFormat.Png);
                wallpaper.SetWallpaper(wallpaper.GetMonitorDevicePathAt((uint)monitorIndex), storagePath);
            }
            catch (Exception ex)
            { // catch everything just in case
                Console.WriteLine("<unexpected error>\n\n" + ex.Message);
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
                Console.WriteLine("<unexpected error>\n\n" + ex.Message);
                return 1;
            }

            return 0;
        }

        public static int Main(string[] args)
        {
            string help = "\nCopyright (c) 2005-" + DateTime.Now.Year.ToString() + " Phillip Hansen  http://sg20.com (version 1.9)\n"
                + "Source available at: https://github.com/philhansen/WallpaperChanger\n\nSyntax is: <file|directory> <style> <location>\n\n"
                + "  <file> is the complete path to the file\n"
                + "  <directory> is the complete path to a directory containing image files\n"
                + "    a random image from the directory will be set as the background\n"
                + "  <style> is an integer (if no style is specified it defaults to Stretched):\n"
                + "    0 for Tiled\n    1 for Centered\n    2 for Stretched\n    3 for Fit (Windows 7 or later)\n    4 for Fill (Windows 7 or later)\n"
                + "  <location> is the complete path to a directory for storing the generated file\n"
                + "    defaults to the temp folder which should be fine in most cases";
            help += "\n\nIf the style argument is not specified it will default to Stretched.";
            help += "\n\nOptional flags:\n"
                + "  -h, -help   - Display the usage help\n"
                + "  -r, -remove - Remove the current wallpaper\n"
                + "  -m, -monitor <index> - Set the image on the specified monitor (0 indexed)\n"
                + "     When using this option the full syntax is:\n"
                + "       -m <index> <file|directory> <location>\n"
                + "  -d, -depth <depth limit> - Set the depth for searching directories for image files (e.g. depth 2 will search the initial directory and 1 level deeper). Default is 1.\n"
                + "     When using this option the full syntax is:\n"
                + "       -d <depth limit> <file|directory> <location>";
            help += "\n\nAlternatively a config file can be placed in the same directory as the WallpaperChanger executable. "
                + "The file should be named 'config' without any file extension.  Each line in the file should have the full path "
                + "to an image or directory and can optionally include the monitor index, directory depth, or the style code to use.  If the style is not specified it will default to Stretched. "
                + "If the directory depth is not specified it will default to 1."
                + "\n\nWhen setting the monitor index in the config file the format of the line should be: <file> -m <index>";
            help += "\n";

            string path = "";
            bool usingConfig = false;
            bool setMonitor = false;
            int monitorIndex = 0;
            int depthLimit = 1;
            Style style = Style.Stretched; // default value
            // Use png file for Win 8 or higher, otherwise use bmp file
            string fileType = "png";
            if (!IsWin8OrHigher())
                fileType = "bmp";
            // get the path to the user's temp folder
            string storagePath = Path.Combine(Path.GetTempPath(), "wallpaper." + fileType);

            // check the arguments
            if (args.Length == 0)
            {
                // a config file can be stored in the same directory as the wallpaper changer
                string configFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "config");
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
                // flexible parsing: flags may appear anywhere; collect non-flag positional args
                List<string> nonFlagArgs = new List<string>();

                for (int i = 0; i < args.Length; i++)
                {
                    string a = args[i];
                    // help flag
                    if (a == "-h" || a == "--help" || a == "-help")
                    {
                        Console.WriteLine(help);
                        return 1;
                    }
                    // remove wallpaper flag
                    else if (a == "-r" || a == "-remove")
                    {
                        return Remove();
                    }
                    // monitor flag consumes the next argument as the index
                    else if (a == "-m" || a == "-monitor")
                    {
                        if (!IsWin8OrHigher())
                        {
                            Console.WriteLine("Specifying a monitor is only supported on Windows 8 or higher\n");
                            return 1;
                        }
                        if (i + 1 >= args.Length)
                        {
                            Console.WriteLine(help);
                            return 1;
                        }
                        if (!int.TryParse(args[i + 1], out monitorIndex))
                        {
                            Console.WriteLine(help);
                            return 1;
                        }
                        setMonitor = true;
                        i++; // skip the index argument
                    }
                    // monitor flag consumes the next argument as the index
                    else if (a == "-d" || a == "-depth")
                    {
                        if (i + 1 >= args.Length)
                        {
                            Console.WriteLine(help);
                            return 1;
                        }
                        if (!int.TryParse(args[i + 1], out depthLimit))
                        {
                            Console.WriteLine(help);
                            return 1;
                        }
                        i++; // skip the index argument
                    }
                    else
                    {
                        // collect non-flag arguments (file/directory, style, location)
                        nonFlagArgs.Add(a);
                    }
                }

                // process positional (non-flag) arguments in order
                if (nonFlagArgs.Count >= 1)
                {
                    path = nonFlagArgs[0];
                }
                if (nonFlagArgs.Count >= 2)
                {
                    // try to parse style (name or numeric); if it parses, use it
                    string s = nonFlagArgs[1];
                    bool parsed = false;
                    try
                    {
                        style = (Wallpaper.Style)Enum.Parse(typeof(Wallpaper.Style), s, true);
                        parsed = true;
                    }
                    catch { }
                    if (!parsed)
                    {
                        int si;
                        if (int.TryParse(s, out si) && Enum.IsDefined(typeof(Wallpaper.Style), si))
                        {
                            style = (Wallpaper.Style)si;
                            parsed = true;
                        }
                    }
                    // if it wasn't a style but is an existing directory, treat it as the storage location
                    if (!parsed && Directory.Exists(nonFlagArgs[1]))
                    {
                        storagePath = Path.Combine(nonFlagArgs[1], "wallpaper." + fileType);
                    }
                }
                if (nonFlagArgs.Count >= 3)
                {
                    // third positional argument is always treated as the storage location
                    if (!Directory.Exists(nonFlagArgs[2]))
                    {
                        Console.WriteLine("\n{0} is not a valid directory.", nonFlagArgs[2]);
                        return 1;
                    }
                    storagePath = Path.Combine(nonFlagArgs[2], "wallpaper." + fileType);
                }
            }

            if (usingConfig)
            {
                string file;
                ProcessConfig(path, out file, out style, out setMonitor, out monitorIndex);
                if (file == null)
                {
                    Console.WriteLine("\nNo valid images found in the config file: {0}", path);
                    return 1;
                }

                int status = 0;
                if (setMonitor)
                    status = Wallpaper.SetMonitor(monitorIndex, file, storagePath);
                else
                    status = Wallpaper.Set(file, style, storagePath);
                
                if (status == 1)
                    return 1;
            }
            else if (File.Exists(path))
            {
                int status = 0;
                if (setMonitor)
                    status = Wallpaper.SetMonitor(monitorIndex, path, storagePath);
                else
                    status = Wallpaper.Set(path, style, storagePath);
                if (status == 1)
                    return 1;
            }
            else if (Directory.Exists(path))
            {
                var file = ProcessDirectory(path, depthLimit);
                if (file == null)
                {
                    Console.WriteLine("\nNo valid images found in {0}.", path);
                    return 1;
                }
                int status = 0;
                if (setMonitor)
                    status = Wallpaper.SetMonitor(monitorIndex, file, storagePath);
                else
                    status = Wallpaper.Set(file, style, storagePath);
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
        ///     Get the list of files from the given directory and choose a random image file
        /// </summary>
        /// <param name="path">
        ///     The path to process
        /// </param>
        /// <param name="depthLimit">
        ///     The depth limit
        /// </param>
        /// <returns>
        ///     Returns the chosen image file
        /// </returns>
        public static string ProcessDirectory(string path, int depthLimit = 1)
        {
            var files = FindImageFiles(path, depthLimit: depthLimit);

            if (files.Count == 0) // ignore empty directories
            {
                return null;
            }

            // initialize a Random object with a unique seed (based on the current time)
            Random randomObject = new Random(((int)DateTime.Now.Ticks));
            
            // pick a random index from the list
            int index = randomObject.Next(0, files.Count);
            return files[index];
        }

        /// <summary>
        ///     Recursively finds all image files in the given directory and sub-directories
        /// </summary>
        /// <param name="path">
        ///     The path to process
        /// </param>
        /// <param name="currentDepth">
        ///     The current depth
        /// </param>
        /// <param name="depthLimit">
        ///     The depth limit
        /// </param>
        /// <returns>
        ///     Returns a list of image files
        /// </returns>
        public static List<string> FindImageFiles(string path, int currentDepth = 1, int depthLimit = 1)
        {
            // get the list of image files in the directory
            var files = Directory.GetFiles(path).Where(f =>
                f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                || f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                || f.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase)
                ).ToList();

            if (currentDepth < depthLimit)
            {
                foreach (var d in Directory.GetDirectories(path))
                {
                    files.AddRange(FindImageFiles(d, currentDepth + 1, depthLimit));
                }
            }

            return files;
        }

        /// <summary>
        /// Get the list of files from the config file and choose a random image
        /// </summary>
        /// <param name="path">config file</param>
        /// <param name="file">filled in with the selected image file</param>
        /// <param name="style">filled in with the style</param>
        /// <param name="setMonitor">filled in with true if a monitor index was specified, false otherwise</param>
        /// <param name="monitorIndex">filled in with the monitor index (when specified)</param>
        public static void ProcessConfig(string path, out string file, out Style style, out bool setMonitor, out int monitorIndex)
        {
            file = null;
            style = Style.Stretched;
            setMonitor = false;
            monitorIndex = 0;
            int depthLimit = 1;
            var files = new List<string>();
            bool finished = false;
            
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    string line;
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

                var f = files[index];
                // parse the line
                // can contain file/directory and optionally the monitor index or style
                // split on the space between them, and combine values within double quotes (e.g. files with a space in the path)
                MatchCollection matches = Regex.Matches(f, @"(?<match>[^\s""]+)|""(?<match>[^""]*)""");
                var fileOrDirectory = matches[0].Groups["match"].Value;

                // parse the other options; -m and -d can appear in any order, style is expected to be last
                if (matches.Count >= 2)
                {
                    for (int i = 1; i < matches.Count; i++)
                    {
                        string value = matches[i].Groups["match"].Value;
                        if (value == "-m" || value == "-monitor")
                        {
                            if (i + 1 < matches.Count && IsWin8OrHigher() && int.TryParse(matches[i + 1].Groups["match"].Value, out monitorIndex))
                            {
                                setMonitor = true;
                                i++; // skip index
                            }
                        }
                        else if (value == "-d" || value == "-depth")
                        {
                            if (i + 1 < matches.Count && int.TryParse(matches[i + 1].Groups["match"].Value, out depthLimit))
                            {
                                i++; // skip depth value
                            }
                        }
                        else if (i == matches.Count - 1)
                        {
                            // treat last token as the style
                            style = (Wallpaper.Style)Enum.Parse(typeof(Wallpaper.Style), value);
                        }
                    }
                }
                
                // if it is a directory, find all image files and pick one
                if (Directory.Exists(fileOrDirectory))
                {
                    file = ProcessDirectory(fileOrDirectory, depthLimit);
                    if (file == null)
                    {
                        files.RemoveAt(index);
                        continue;
                    }

                    finished = true;
                }
                // make sure the image exists
                else if (File.Exists(fileOrDirectory))
                {
                    file = fileOrDirectory;
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

        /// <summary>
        /// Determine if the Operating system is Windows 8 or higher
        /// </summary>
        /// <returns>True if Win 8 or higher, False otherwise</returns>
        public static bool IsWin8OrHigher()
        {
            OperatingSystem OS = Environment.OSVersion;
            return (OS.Platform == PlatformID.Win32NT) && ((OS.Version.Major > 6) || ((OS.Version.Major == 6) && (OS.Version.Minor >= 2)));
        }
    }
}
