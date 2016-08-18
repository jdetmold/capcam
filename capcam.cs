// Capture image from webcam
// Copyright 2016 michael4jonas
// License gnu.org/licenses/lgpl-3.0
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using AForge.Controls;
using AForge.Video.DirectShow;

class CMain
{
    static string version = "1.1";
    static bool   quietly = false;

    // util
    static void Print(string s, params object[] p)  { if (!quietly) Console.Write(s,p); }
    static int  ToNumber(string s)  { try { return Int32.Parse(s); } catch { return 0; }}
    static bool IsNumber(string s)  { int number; return Int32.TryParse(s, out number); }

    // main
    static void Main(string[] args)
    {
        try
        {
            CapCam(args);
        }
        catch (ApplicationException ex)
        {
            Console.Write("Error: {0}\n", ex.Message);
            Environment.Exit(8);
        }
        catch (Exception ex)
        {
            Console.Write("Error: {0}\n", ex);
            Environment.Exit(9);
        }

        Environment.Exit(0);
    }

    // help
    static void HelpAndExit()
    {
        Console.Write("Capture image from webcam with maximum resolution\n");
        Console.Write("  Devicename is case-insensitive, substring is ok\n");
        Console.Write("  Increase delay if first image is black\n");
        Console.Write("  Image filename is auto-generated\n");
        Console.Write("Version " + version + " by michael4jonas (License gnu.org/licenses/lgpl-3.0)\n");
        Console.Write("\n");
        Console.Write("Usage: capcam.exe [options] deviceid or devicename\n");
        Console.Write("  Ex.: capcam.exe  0\n");
        Console.Write("       capcam.exe  camera -d4 -fbmp -oimage-100.jpg\n");
        Console.Write("       capcam.exe \"usb camera\" -d4 -o\"image 100.jpg\"\n");
        Console.Write("\n");
        Console.Write("Options:\n");
        Console.Write("  -h               help\n");
        Console.Write("  -q               quiet mode\n");
        Console.Write("\n");
        Console.Write("  -dN              delay for N seconds before first capture (def. 2)\n");
        Console.Write("  -rN              N-th resolutionid instead of maximum resolution\n");
        Console.Write("  -nN              N is number of images alltogether (def. 1)\n");
        Console.Write("  -wN              wait for N seconds before next capture (def. 1)\n");
        Console.Write("\n");
        Console.Write("  -fjpg|png|bmp    image format (def. jpg)\n");
        Console.Write("  -ofilename       image output filename\n");
        Console.Write("\n");
        Console.Write("Cameras:\n");

        FilterInfoCollection videoInputDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
        int i = 0;
        foreach (FilterInfo device in videoInputDevices)
        {
            Console.Write("{0,3}: {1}\n", i, device.Name);
            ++i;
        }
        if (i == 0)  Console.Write("  no cameras\n");

        Environment.Exit(0);
    }

    // capture webcam
    static void CapCam(string[] args)
    {
        // args
        string devicename = null;
        int    delay      = 2;     // default
        int    resid      = Int32.MinValue;
        int    number     = 1;     // default
        int    wait       = 1;     // default
        string format     = "jpg"; // default
        string filename   = null;
        for (int i = 0; i < args.Length; ++i)
        {
            if      (args[i] == "-h")  HelpAndExit();
            else if (args[i] == "-q")  quietly = true;
            else if (args[i].StartsWith("-d")  &&  args[i].Length > 2)  delay      = ToNumber(args[i].Substring(2));
            else if (args[i].StartsWith("-r")  &&  args[i].Length > 2)  resid      = ToNumber(args[i].Substring(2));
            else if (args[i].StartsWith("-n")  &&  args[i].Length > 2)  number     = ToNumber(args[i].Substring(2));
            else if (args[i].StartsWith("-w")  &&  args[i].Length > 2)  wait       = ToNumber(args[i].Substring(2));
            else if (args[i].StartsWith("-f")  &&  args[i].Length > 2)  format     = args[i].Substring(2);
            else if (args[i].StartsWith("-o")  &&  args[i].Length > 2)  filename   = args[i].Substring(2);
            else if (!args[i].StartsWith("-")  &&  devicename == null)  devicename = args[i];
            else throw new ApplicationException("Unknown argument: " + args[i]);
        }

        // check args
        if (devicename == null)
            HelpAndExit();
        if (delay < 0  ||  delay >= 12)
            throw new ApplicationException("wrong delay");
        if (number < 1)
            throw new ApplicationException("wrong number");
        if (wait < 1)
            throw new ApplicationException("wrong wait");

        // action
        Print("Capture image from webcam with maximum resolution (V{0}) (-h for help)\n", version);
        CaptureWebCam(devicename, delay, resid, number, wait, format, filename);
        Print("done\n");
    }

    // capture webcam
    static void CaptureWebCam(string devicename, int delay, int resid, int number, int wait, string format, string filename)
    {
        // format
        ImageFormat imageformat;
        if      (format.ToLower() == "jpg") imageformat = ImageFormat.Jpeg;
        else if (format.ToLower() == "png") imageformat = ImageFormat.Png;
        else if (format.ToLower() == "bmp") imageformat = ImageFormat.Bmp;
        else throw new ApplicationException("wrong format");

        // get video devices
        FilterInfoCollection videoInputDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
        if (videoInputDevices.Count == 0)
            throw new ApplicationException("no cameras");

        // list all devices
        int i = 0;
        foreach (FilterInfo device in videoInputDevices)
        {
            Print("{0,3}: {1}\n", i, device.Name);
            ++i;
        }

        // get deviceid
        int deviceid = 0; // default
        if (devicename != null)
        {
            if (IsNumber(devicename))
                deviceid = ToNumber(devicename);
            else
            {
                bool found = false;
                foreach (FilterInfo device in videoInputDevices)
                {
                    if (device.Name.ToLower().Contains(devicename.ToLower()))  { found = true; break; }
                    ++deviceid;
                }
                if (!found)
                    throw new ApplicationException("wrong devicename");
            }
        }
        if (deviceid < 0  ||  deviceid >= videoInputDevices.Count)
            throw new ApplicationException("wrong deviceid");

        // init capture device
        VideoCaptureDevice videoCaptureDevice = new VideoCaptureDevice(videoInputDevices[deviceid].MonikerString);
        if (videoCaptureDevice.VideoCapabilities.Length == 0)
            throw new ApplicationException("no camera resolutions");

        // list all resolutions
        Print("Camera {0} resolutions\n", deviceid);
        i = 0;
        foreach (VideoCapabilities capabilty in videoCaptureDevice.VideoCapabilities)
        {
            Print("{0,3}: {1} x {2}\n", i, capabilty.FrameSize.Width, capabilty.FrameSize.Height);
            ++i;
        }

        // get max resolutionid
        if (resid == Int32.MinValue)
        {
            i = 0;
            Size max = new Size(0, 0);
            foreach (VideoCapabilities capabilty in videoCaptureDevice.VideoCapabilities)
            {
                if (capabilty.FrameSize.Width > max.Width  ||  (capabilty.FrameSize.Width == max.Width  &&  capabilty.FrameSize.Height > max.Height))
                {
                    resid = i; // default
                    max   = capabilty.FrameSize;
                }
                ++i;
            }
        }
        if (resid < 0  ||  resid >= videoCaptureDevice.VideoCapabilities.Length)
            throw new ApplicationException("wrong resolutionid");
        videoCaptureDevice.VideoResolution = videoCaptureDevice.VideoCapabilities[resid];

        // init player
        VideoSourcePlayer videoSourcePlayer = new VideoSourcePlayer();
        videoSourcePlayer.VideoSource       = videoCaptureDevice;
        videoSourcePlayer.Start();

        // waiting for first capture
        Print("waiting [{0} -d{1} -r{2} -n{3} -w{4} -f{5}] ", deviceid, delay, resid, number, wait, format);
        bool captured = false;
        for (i = 0; i < 25; ++i)
        {
            Print(".");
            Thread.Sleep(delay * 1000); // increase delay if image is black
            if (videoCaptureDevice.IsRunning
             && videoSourcePlayer.IsRunning
             && videoSourcePlayer.GetCurrentVideoFrame() != null)
            {
                captured = true;
                break;
            }
        }
        if (!captured)
            Print("\n  capture failed, increase delay");
        Print("\n");

        // captures images
        for (i = 0; i < number; ++i)
        {
            Bitmap bitmap = videoSourcePlayer.GetCurrentVideoFrame();
            if (bitmap == null)
                Print("  capture failed\n");
            else
            {
                string filenamefull;
                if (filename == null)
                    filenamefull = "capcam" + deviceid + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + "." + format.ToLower(); // default
                else
                {
                    string pathname  = Path.GetDirectoryName(filename);
                    string basename  = Path.GetFileNameWithoutExtension(filename);
                    string extension = Path.GetExtension(filename);
                    if (basename     == "")  basename  = "capcam";
                    if (extension    == "")  extension = format.ToLower();
                    if (extension[0] == '.') extension = extension.Substring(1);
                    if (number > 1)
                        basename += DateTime.Now.ToString("yyyyMMdd-HHmmss");
                    filenamefull = Path.Combine(pathname, basename) + "." + extension;
                }
                bitmap.Save(filenamefull, imageformat);
                Print("  {0}\n", filenamefull);
            }
            Thread.Sleep(wait * 1000);
        }

        // cleanup
        videoSourcePlayer.SignalToStop();
        videoSourcePlayer.WaitForStop();
        videoSourcePlayer.VideoSource = null;
        videoCaptureDevice.SignalToStop();
        videoCaptureDevice.WaitForStop();
    }
}
//eof
