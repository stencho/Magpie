using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Magpie.Engine;

namespace Magpie.Graphics {
    public static class DisplayInfo {
        #region WIN API HELL
        private struct POINTL {
            public Int32 x;
            public Int32 y;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct DEVMODE {
            private const int CCHDEVICENAME = 0x20;
            private const int CCHFORMNAME = 0x20;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public ScreenOrientation dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct DISPLAY_DEVICE {
            [MarshalAs(UnmanagedType.U4)]
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
            public string DeviceString;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
            public string DeviceKey;
        }

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        static extern bool EnumDisplayDevices(string lpDevice, int iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        private static extern bool EnumDisplaySettings(
              string deviceName, int modeNum, ref DEVMODE devMode);

        const int ENUM_CURRENT_SETTINGS = -1;

        const int ENUM_REGISTRY_SETTINGS = -2;

        #endregion
               

        public struct display_mode {
            public XYPair resolution;
            public float refresh_rate;

            public display_mode(XYPair resolution, float refresh_rate) {
                this.resolution = resolution;
                this.refresh_rate = refresh_rate;
            }
        }

        public static int get_primary_screen() {
            if (Screen.AllScreens.Length == 1) {
                return 0;
            } else if (Screen.AllScreens.Length == 0){
                throw new Exception("no screen plugged in??");
            }

            for (int i = 0; i < Screen.AllScreens.Length; i++) {
                if (Screen.AllScreens[i].Primary) {
                    return i;
                }
            }

            throw new Exception("what the fuck how do you not have a primary monitor");            
        }

        public static bool get_display_modes(int display, out List<display_mode> modes, out string name, out string device_string) {
            DEVMODE devmode = new DEVMODE();
            DISPLAY_DEVICE device = new DISPLAY_DEVICE();
            device.cb = Marshal.SizeOf(device);

            modes = new();
            name = "";
            device_string = "";

            EnumDisplayDevices(null, display-1, ref device, 0);
            
            if (string.IsNullOrEmpty(device.DeviceName)) return false;

            for (int id = 0; EnumDisplaySettings(device.DeviceName, id, ref devmode); id++) {
                modes.Add(new display_mode(new XYPair(devmode.dmPelsWidth, devmode.dmPelsHeight), devmode.dmDisplayFrequency));
            }

            name = device.DeviceName;
            device_string = device.DeviceString;
            
            modes = modes.OrderBy(a => a.resolution.X).ToList();
            GC.Collect();
            return true;
        }

        public static string list_display_modes(int display) {
            string s = "";
            string s2;
            List<display_mode> modes;
            var dm = get_display_modes(display, out modes, out s, out s2);
            s += " " + s2 + "\n";
            if (modes.Count == 0) return "";

            int i = 0;
            foreach (display_mode m in modes) {
                s += $"{m.refresh_rate}Hz {m.resolution.ToXString()}";
                if (i < modes.Count - 1) {
                    s += "\n";
                }
                i++;
            }

            GC.Collect();
            return s;
        }

        public static int find_display_mode_index(int w, int h, float rr, List<display_mode> modes) {
            int i = 0;
            foreach (display_mode m in modes) {
                if (m.resolution.X == w && m.resolution.Y == h && m.refresh_rate == rr) {
                    return i;
                }
                i++;
            }
            return -1;
        }
        public static int find_display_mode_index_highest_hz_at_res(int w, int h, List<display_mode> modes) {
            int i = 0;
            float hz = 0;
            int index = -1;

            foreach (display_mode m in modes) {
                if (m.resolution.X == w && m.resolution.Y == h) {
                    if (m.refresh_rate > hz) hz = m.refresh_rate;
                        index = i;
                }
                i++;
            }

            return index;
        }
        public static display_mode find_display_mode_highest_hz_at_res(int w, int h, List<display_mode> modes, out int index) {
            int i = 0;
            float hz = 0;
            index = -1;

            foreach (display_mode m in modes) {
                if (m.resolution.X == w && m.resolution.Y == h) {
                    if (m.refresh_rate > hz) hz = m.refresh_rate;
                    index = i;
                }
                i++;
            }

            return modes[index];
        }

        public static float highest_hz_supported_by_highest_res(List<display_mode> modes, out int index) {
            int h_index = -1;
            int h_w=-1;
            int h_h=-1;
            float h_hz=-1;
            int i=0;

            foreach (display_mode m in modes) {
                if (m.resolution.X >= h_w || m.resolution.Y >= h_h) {
                    h_w = m.resolution.X;
                    h_h = m.resolution.Y;
                    if (m.refresh_rate > h_hz) {
                        h_hz = m.refresh_rate;

                        h_index = i;
                    }
                }
                i++;
            }

            index = h_index;
            return h_hz;    
        }


        public static string test() {
            string s = "";

            DEVMODE devmode = new DEVMODE();

            DISPLAY_DEVICE device = new DISPLAY_DEVICE();
            device.cb = Marshal.SizeOf(device);

            for (int i = 0; EnumDisplayDevices(null, i, ref device, 0); i++) {
                s += $"{i}: {device.DeviceName} :: {device.DeviceString}\n";

                for (int id = 0; EnumDisplaySettings(device.DeviceName, id, ref devmode); id++) {
                    s += $"{devmode.dmFormName} : {devmode.dmDisplayFrequency} {devmode.dmPelsWidth}x{devmode.dmPelsHeight} \n";
                }
            }

            //Log.log(DisplayInfo.test())

            return s;

        }

    }
}
