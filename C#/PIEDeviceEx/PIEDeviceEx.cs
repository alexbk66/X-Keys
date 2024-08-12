using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using PIEHid32Net;

namespace PIEDeviceLib
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


        #endregion Private Properties


        #region Public Properties


        public int hidusagepg => device.HidUsagePage;
        public int hidusage => device.HidUsage;
        public int WriteLength  => device.WriteLength;
        public int ReadLength  => device.ReadLength;

        public int Pid => device.Pid;
        public EPid PidType => (EPid)device.Pid;

        public string SerialNumber => device.SerialNumberString;
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


        public class MessageEventArgs : EventArgs
        {
            public MessageEventArgs(string msg, int? error = null)
            {
                this.msg = msg;
                this.error = error;
            }

            public MessageEventArgs(byte[] data, int? error = null)
            {
                this.data = data;
                this.error = error;
            }

            public string msg { get; set; }

            public Byte[] data { get; set; }
                
            public int? error { get; set; }
        }


        public event EventHandler<MessageEventArgs> OnError;
        public event EventHandler<MessageEventArgs> OnMessage;


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

            Array.Clear(wData, 0, wData.Length);

            //if (lastdata == null)
            //{
            //    lastdata = new byte[ReadLength];
            //}

            //Array.Clear(lastdata, 0, lastdata.Length);
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


        public static List<PIEDeviceEx> EnumeratePIE()
        {
            return PIEDevice.EnumeratePIE()?
                   .Where(dev => _IsValid(dev))
                   .Select(dev => new PIEDeviceEx(dev))
                   .ToList();
        }


        public void SetupCallback()
        {
            if (device != null)
            {
                device.SetErrorCallback(this);
                device.SetDataCallback(this);
                device.callNever = false;
            }
        }

        
        DataStruct data_struct;
        DataStruct? old_data_struct;


        //data callback    
        void PIEDataHandler.HandlePIEHidData(byte[] data, PIEDevice sourceDevice, int error)
        {
            Debug.Assert(sourceDevice?.Path == device?.Path);

            try
            {
                data_struct = new DataStruct(data);

                Button? changed = data_struct.Changed(old_data_struct);

                old_data_struct = data_struct;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new MessageEventArgs($"Exception: {ex}"));
            }

        //    OnMessage?.Invoke(sourceDevice, new MessageEventArgs(data, error));
        }


        //error callback
        void PIEErrorHandler.HandlePIEHidError(PIEDevice sourceDevice, int error)
        {
            Debug.Assert(sourceDevice?.Path == device?.Path);

            OnError?.Invoke(this, new MessageEventArgs($"Error: {error}", error));
        }


        #endregion Methods


        #region Control Methods


        /// <summary>
        /// 177 (0xb1)
        /// After sending this command a general incoming data report will be given with
        /// the 3rd byte (Data Type) 2nd bit set.  If program switch is up byte 3 will be 2
        /// and if it is pressed byte 3 will be 3.  This is useful for getting the initial state
        /// or unit id of the device before it sends any data.
        /// </summary>
        /// <returns></returns>
        public int GetDataNow()
        {
            return WriteData(0, 177);
        }


        /// <summary>
        /// Sets the frequency of flashing for both the LEDs and backlighting
        /// </summary>
        /// <param name="freq"></param>
        /// <returns></returns>
        public int SetFlashFrequency(byte freq)
        {
            return WriteData(0, 180, freq);
        }


        /// <summary>
        /// Turns on or off, depending on value of ChkRedOnOff, ALL red/blue BLs using current intensity
        /// 
        /// </summary>
        /// <param name="which">0 for Blue, 1 for Red</param>
        /// <param name="onoff">
        /// 0 or 255
        /// OR turn individual rows on or off using bits.  1st bit=row 1, 2nd bit=row 2, 3rd bit =row 3, etc
        /// </param>
        /// <returns></returns>
        public int SetBankOnOff(int which, byte onoff)
        {
            return WriteData(0, 182, (byte)which, onoff);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="bank1">0-255 for brightness of bank 1 bl leds</param>
        /// <param name="bank2">0-255 for brightness of bank 2 bl leds</param>
        /// <returns></returns>
        public int SetBankBrightness(byte bank1, byte bank2)
        {
            return WriteData(0, 187, bank1, bank2);
        }


        /// <summary>
        /// 181 (b5)
        /// Use the Set Flash Freq to control frequency of blink
        //Key Index for XK-24 (in decimal)
        //Columns-->
        //  0   8   16  24
        //  1   9   17  25
        //  2   10  18  26
        //  3   11  19  27
        //  4   12  20  28
        //  5   13  21  29
        /// </summary>
        /// <param name="ctrl">0=off, 1=on, 2=flash</param>
        /// <param name="index"></param>
        /// <returns></returns>
        public int SetBtnLED(int ctrl, byte index)
        {
            return WriteData(0, 181, index, (byte)ctrl);
        }


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
        //public int RedLED()
        //{
        //    //save the current value of the LED byte
        //    byte saveled = wData[2]; 
        //    return WriteData(0, 186, (byte)(saveled | 128));
        //}


        /// <summary>
        /// Write current state of backlighting to EEPROM.  
        /// NOTE: Is it not recommended to do this frequently as 
        /// there are a finite number of writes to the EEPROM allowed
        /// </summary>
        /// <returns></returns>
        public int SaveBacklighting()
        {
            // anything other than 0 will save bl state to eeprom, default is 0
            return WriteData(0, 199, 1);
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
