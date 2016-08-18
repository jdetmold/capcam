# CapCam
CapCam is a Windows Command Line application that captures images
from a webcam or any camera connected via USB.

It is written in C# and runs under Windows XP, Windows 7, Windows 8 and Windows 10.

## Requirements
The program uses the [AForge.NET Framework](http://aforgenet.com/framework).

## Download
- Click button **Clone or Download** to clone or download the repository
- [Click here](https://github.com/michael4jonas/capcam/raw/master/exe/capcam-exe.zip) to download only the exe-files for WinXP, Win7, Win8 and Win10

## Usage
    c:\temp>capcam.exe
    Capture image from webcam with maximum resolution
      Devicename is case-insensitive, substring is ok
      Increase delay if first image is black
      Image filename is auto-generated
    Version 1.1 by michael4jonas (License gnu.org/licenses/lgpl-3.0)

    Usage: capcam.exe [options] deviceid or devicename
      Ex.: capcam.exe  0
           capcam.exe  camera -d4 -fbmp -oimage-100.jpg
           capcam.exe "usb camera" -d4 -o"image 100.jpg"

    Options:
      -h               help
      -q               quiet mode

      -dN              delay for N seconds before first capture (def. 2)
      -rN              N-th resolutionid instead of maximum resolution
      -nN              N is number of images alltogether (def. 1)
      -wN              wait for N seconds before next capture (def. 1)

      -fjpg|png|bmp    image format (def. jpg)
      -ofilename       image output filename

    Cameras:
      0: USB2.0 Camera

## Source Code
- Developed with Windows 7 and VS2015 Community
- Download [AForge.NET Framework-2.2.5.zip](http://aforge.googlecode.com/files/AForge.NET%20Framework-2.2.5.zip)
  from [AForge.NET Framework](http://aforgenet.com/framework)
- Unzip to "./vendor/aforge/"
- Structure afterwards (maybe adjust references in visual studio)
    - ./capcam.sln
    - ./vendor/aforge/release/aforge*.dll
    - ...
- Start capcam.sln and compile with f5
- Done

## License
[gnu.org/licenses/lgpl-3.0](https://www.gnu.org/licenses/lgpl-3.0)

