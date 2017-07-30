//
// This implements the IDesktopWallpaper interface that was added with Windows 8:
//      https://msdn.microsoft.com/en-us/library/windows/desktop/hh706946(v=vs.85).aspx
//
// Implementation based on the code from https://bitbucket.org/ciniml/desktopwallpaper/src
//

using System;
using System.Runtime.InteropServices;

namespace WallpaperChanger
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    /// <summary>
    /// This enumeration is used to set and get slideshow options.
    /// </summary> 
    public enum DesktopSlideshowOptions
    {
        // When set, indicates that the order in which images in the slideshow are displayed can be randomized.
        ShuffleImages = 0x01,
    }


    /// <summary>
    /// This enumeration is used by GetStatus to indicate the current status of the slideshow.
    /// </summary>
    public enum DesktopSlideshowState
    {
        Enabled = 0x01,
        Slideshow = 0x02,
        DisabledByRemoteSession = 0x04,
    }


    /// <summary>
    /// This enumeration is used by the AdvanceSlideshow method to indicate whether to advance the slideshow forward or backward.
    /// </summary>
    public enum DesktopSlideshowDirection
    {
        Forward = 0,
        Backward = 1,
    }

    /// <summary>
    /// This enumeration indicates the wallpaper position for all monitors. (This includes when slideshows are running.)
    /// The wallpaper position specifies how the image that is assigned to a monitor should be displayed.
    /// </summary>
    public enum DesktopWallpaperPosition
    {
        Center = 0,
        Tile = 1,
        Stretch = 2,
        Fit = 3,
        Fill = 4,
        Span = 5,
    }

    [ComImport, Guid("B92B56A9-8B55-4E14-9A89-0199BBB6F93B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDesktopWallpaper
    {
        /// <summary>
        /// Set the wallpaper image on the designated monitor
        /// </summary>
        /// <param name="monitorID">The ID of the monitor. This value can be obtained through GetMonitorDevicePathAt. Set this value to NULL to set the wallpaper image on all monitors.</param>
        /// <param name="wallpaper">The full path of the wallpaper image file.</param>
        void SetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string monitorID, [MarshalAs(UnmanagedType.LPWStr)] string wallpaper);

        /// <summary>
        /// Get the current wallpaper on the designated monitor
        /// </summary>
        /// <param name="monitorID"></param>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.LPWStr)]
        string GetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string monitorID);

        /// <summary>
        /// Gets the monitor device path
        /// </summary>
        /// <param name="monitorIndex">Index of the monitor device in the monitor device list.</param>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.LPWStr)]
        string GetMonitorDevicePathAt(uint monitorIndex);
        
        /// <summary>
        /// Gets number of monitor device paths
        /// </summary>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.U4)]
        uint GetMonitorDevicePathCount();

        /// <summary>
        /// Retrieves the display rectangle of the specified monitor
        /// </summary>
        /// <param name="monitorID"></param>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.Struct)]
        Rect GetMonitorRECT([MarshalAs(UnmanagedType.LPWStr)] string monitorID);

        /// <summary>
        /// Sets the color that is visible on the desktop when no image is displayed or when the desktop background has been disabled.
        /// This color is also used as a border when the desktop wallpaper does not fill the entire screen.
        /// </summary>
        /// <param name="color"></param>
        void SetBackgroundColor([MarshalAs(UnmanagedType.U4)] uint color);

        /// <summary>
        /// Retrieves the color that is visible on the desktop when no image is displayed or when the desktop background has been disabled.
        /// This color is also used as a border when the desktop wallpaper does not fill the entire screen.
        /// </summary>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.U4)]
        uint GetBackgroundColor();

        /// <summary>
        /// Sets the display option for the desktop wallpaper image, determining whether the image should be centered, tiled, or stretched.
        /// </summary>
        /// <param name="position"></param>
        void SetPosition([MarshalAs(UnmanagedType.I4)] DesktopWallpaperPosition position);

        /// <summary>
        /// Retrieves the current display value for the desktop background image.
        /// </summary>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.I4)]
        DesktopWallpaperPosition GetPosition();

        /// <summary>
        /// Specifies the images to use for the desktop wallpaper slideshow.
        /// </summary>
        /// <param name="items"></param>
        void SetSlideshow(IntPtr items);

        /// <summary>
        /// Gets the images that are being displayed in the desktop wallpaper slideshow.
        /// </summary>
        /// <returns></returns>
        IntPtr GetSlideshow();

        /// <summary>
        /// Sets the desktop wallpaper slideshow settings for shuffle and timing.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="slideshowTick"></param>
        void SetSlideshowOptions(DesktopSlideshowDirection options, uint slideshowTick);

        /// <summary>
        /// Gets the current desktop wallpaper slideshow settings for shuffle and timing.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="slideshowTick"></param>
        /// <returns></returns>
        [PreserveSig]
        uint GetSlideshowOptions(out DesktopSlideshowDirection options, out uint slideshowTick);

        /// <summary>
        /// Switches the wallpaper on a specified monitor to the next image in the slideshow.
        /// </summary>
        /// <param name="monitorID"></param>
        /// <param name="direction"></param>
        void AdvanceSlideshow([MarshalAs(UnmanagedType.LPWStr)] string monitorID, [MarshalAs(UnmanagedType.I4)] DesktopSlideshowDirection direction);

        /// <summary>
        /// Gets the current status of the slideshow.
        /// </summary>
        /// <returns></returns>
        DesktopSlideshowDirection GetStatus();

        /// <summary>
        /// Enables or disables the desktop background.
        /// </summary>
        /// <returns></returns>
        bool Enable();
    }

    [ComImport, Guid("C2CF3110-460E-4fc1-B9D0-8A1C0C9CC4BD")]
    public class DesktopWallpaper
    {
    }
}
