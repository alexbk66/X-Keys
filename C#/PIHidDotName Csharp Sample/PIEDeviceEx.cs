using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PIEHid32Net;

namespace PIHidDotName_Csharp_Sample
{
    public class PIEDeviceEx : IDisposable, PIEDataHandler, PIEErrorHandler
    {
        #region Private Properties

        public enum EPid
        {
            //Device 2 Keyboard, Joystick, Input and Output endpoints, PID #3
            PID1 = 1027,
            //Device 1 Keyboard, Joystick, Mouse and Output endpoints,. PID #2
            PID2 = 1028,
            //Device 0 Keyboard, Mouse, Input and Output endpoints (factory default), PID #1
            PID3 = 1029,
            //Device 3 Keyboard, Multimedia, Mouse and Output endpoints, PID #4
            PID4 = 1249,
        }

        PIEDevice device;

        byte[] wData = null; //write data buffer
        byte[] lastdata = null;


        #endregion Private Properties


        #region Public Properties


        public int hidusagepg => device.HidUsagePage;
        public int hidusage => device.HidUsage;
        public int WriteLength  => device.WriteLength;
        public int ReadLength  => device.ReadLength;

        public int Pid => device.Pid;
        public EPid PidType => (EPid)device.Pid;

        public string Model => device.ProductString;
        public int Version => device.Version;

        public string Description => ToString();


        public bool IsValidPID => _IsValidPID(Pid);

        public static bool _IsValidPID(int? pid)
        {
            return pid != null && Enum.IsDefined(typeof(EPid), pid);
        }

        public bool IsValid => _IsValid(device);

        public static bool _IsValid(PIEDevice dev)
        {
            return dev?.HidUsagePage == 0xc && dev?.WriteLength == 36;
        }

        #endregion Public Properties


        #region Construction


        public PIEDeviceEx(PIEDevice device)
        {
            this.device = device;
            device.SetupInterface();
            device.suppressDuplicateReports = false;
            Clear();
        }


        public void Clear()
        {
            if (wData == null)
            {
                wData = new byte[WriteLength];//size write array 
            }

            if (lastdata == null)
            {
                lastdata = new byte[ReadLength];
            }

            Array.Clear(wData, 0, wData.Length);
            Array.Clear(lastdata, 0, lastdata.Length);
        }


        public override string ToString()
        {
            if (IsValidPID)
            {
                return $"{Model} ({Pid} = {PidType})";
            }
            else
            {
                return $"Unknown Device: '{Model}' ({Pid})";
            }
        }


        public static List<PIEDeviceEx> EnumeratePIE()
        {
            return PIEDevice.EnumeratePIE()?
                   .Where(dev => _IsValid(dev))
                   .Select(dev => new PIEDeviceEx(dev))
                   .ToList();
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                try
                {
                    // Dispose managed resources here.
                    device?.CloseInterface();
                    device = null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CloseInterface: {ex}");
                }
            }

            // Dispose unmanaged resources here (if any).

            _disposed = true;
        }


        // Finalizer
        ~PIEDeviceEx()
        {
            Dispose(false);
        }


        #endregion Construction


        #region Methods


        public void SetupCallback()
        {
            if (device != null)
            {
                device.SetErrorCallback(this);
                device.SetDataCallback(this);
                device.callNever = false;
            }
        }


        void PIEDataHandler.HandlePIEHidData(byte[] data, PIEDevice sourceDevice, int error)
        {
        }

        void PIEErrorHandler.HandlePIEHidError(PIEDevice sourceDevices, int error)
        {
        }


        #endregion Methods


        #region Control Methods


        /// <summary>
        /// Sending this command toggles the backlights
        /// </summary>
        /// <returns></returns>
        public int ToggleBacklights()
        {
            return WriteData(0, 184);
        }


        /// <summary>
        /// Turn on the red LED
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public int RedLED()
        {
            byte saveled = wData[2]; //save the current value of the LED byte

            Clear();

            wData[1] = 186;
            wData[2] = (byte)(saveled | 128);

            return WriteData();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctrl">0=off, 1=on, 2=flash</param>
        /// <returns></returns>
        public int SetRedLED(int ctrl)
        {
            // 6 for green, 7 for red
            return SetLED(ctrl, 7);
        }

        public int SetGreenLED(int ctrl)
        {
            // 6 for green, 7 for red
            return SetLED(ctrl, 6);
        }


        /// <summary>
        /// 179 (0xb3)
        /// </summary>
        /// <param name="ctrl">0=off, 1=on, 2=flash</param>
        /// <param name="clr">6 for green, 7 for red</param>
        /// <returns></returns>
        private int SetLED(int ctrl, byte clr)
        {
            return WriteData(0, 179, clr, (byte)ctrl);
        }



        public int WriteData(params byte[] data)
        {
            Clear();

            int i = 0;
            foreach (byte b in data)
            {
                wData[i++] = b;
            }

            return WriteData();
        }


        public int WriteData()
        {
            if (device == null) return -1;

            int result = 404;
            while (result == 404)
            { 
                result = device.WriteData(wData);
            }
            return result;
        }


        #endregion Control Methods
    }
}
