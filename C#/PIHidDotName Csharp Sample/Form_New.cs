using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using PIEHid32Net;
using PIEDeviceLib;


namespace PIHidDotName_Csharp_Sample
{
    public partial class Form1
    {
        #region Init


        private void BtnEnumerate_Click(object sender, EventArgs e)
        {
            CboDevices.Items.Clear();

            devicesex = PIEDeviceEx.EnumeratePIE()?.ToArray();

            //enumerate and setupinterfaces for all devices
            devices = PIEDevice.EnumeratePIE();

            if (devicesex == null || devicesex.Length == 0)
            {
                toolStripStatusLabel1.Text = "No Devices Found";
            }
            else
            {
                foreach (var dev in devicesex)
                {
                    CboDevices.Items.Add(dev?.Description);
                }
            }

            if (CboDevices.Items.Count > 0)
            {
                // This causes CboDevices_SelectedIndexChanged
                CboDevices.SelectedIndex = 0;
            }
        }



        private void CboDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            Debug.Assert(devicesex != null && CboDevices.SelectedIndex < devicesex.Length);

            if (CurrentDevice != null)
            {
                CurrentDevice.OnError -= CurrentDevice_OnError;
                CurrentDevice.OnMessage -= CurrentDevice_OnMessage;
            }

            CurrentDevice = devicesex[CboDevices.SelectedIndex];

            if (CurrentDevice != null)
            {
                CurrentDevice.OnMessage += CurrentDevice_OnMessage;
                CurrentDevice.OnError += CurrentDevice_OnError;
            }
            
            LblVersion.Text = CurrentDevice?.Version.ToString();
        }


        private void BtnCallback_Click(object sender, EventArgs e)
        {
            //setup callback if there are devices found for each device found
            CurrentDevice?.SetupCallback();
        }


        void ShowResult(int? result, string func)
        {
            string msg = null;
            if (result == 0)
            {
                msg = $"Write Success - {func}";
            }
            else
            {
                msg = $"Write Fail ({func}): {result}";
            }

            toolStripStatusLabel1.Text = msg;
        }


        #endregion Init


        #region Control


        private void BtnGetDataNow_Click(object sender, EventArgs e)
        {
            //After sending this command a general incoming data report will be given with
            //the 3rd byte (Data Type) 2nd bit set.  If program switch is up byte 3 will be 2
            //and if it is pressed byte 3 will be 3.  This is useful for getting the initial state
            //or unit id of the device before it sends any data.

            int? result = CurrentDevice?.GetDataNow();

            ShowResult(result, "Generate Data");
        }


        private void ChkGreenLED_CheckStateChanged(object sender, EventArgs e)
        {
            int? result = CurrentDevice?.SetGreenLED((int)ChkGreenLED.CheckState);

            ShowResult(result, "Green LED");
        }


        private void ChkRedLED_CheckStateChanged(object sender, EventArgs e)
        {
            int? result = CurrentDevice?.SetRedLED((int)ChkRedLED.CheckState);

            ShowResult(result, "Red LED");
        }


        private void BtnBLToggle_Click(object sender, EventArgs e)
        {
            int? result = CurrentDevice?.ToggleBacklights();

            ShowResult(result, "Toggle BL");
        }


        /// <summary>
        /// Key Index for XK-24 (in decimal)
        /// Columns-->
        ///   0   8   16  24
        ///   1   9   17  25
        ///   2   10  18  26
        ///   3   11  19  27
        ///   4   12  20  28
        ///   5   13  21  29 
        /// </summary>
        /// <returns></returns>
        int GetSelectedBank()
        {
            string sindex = CboBL.Text;
            int iindex;
            if (sindex.IndexOf("b1") != -1) //bank 1 backlight
            {
                sindex = sindex.Remove(sindex.IndexOf("-b1"), 3);
                iindex = Convert.ToInt16(sindex);
            }
            else //bank 2 backlight
            {
                sindex = sindex.Remove(sindex.IndexOf("-b2"), 3);
                iindex = Convert.ToInt16(sindex) + 32;  //Add 32 to get corresponding bank 2 backlight
            }

            return iindex;
        }


        private void ChkFlash_CheckedChanged(object sender, EventArgs e)
        {
            //Use the Set Flash Freq to control frequency of blink
            //Key Index for XK-24 (in decimal)
            //Columns-->
            //  0   8   16  24
            //  1   9   17  25
            //  2   10  18  26
            //  3   11  19  27
            //  4   12  20  28
            //  5   13  21  29
            //first get selected index
            int iindex = GetSelectedBank();

            //now get state 0=off, 1=on, 2=flash
            int state = ChkFlash.Checked ? 2 : ChkBLOnOff.Checked ? 1 : 0;

            int? result = CurrentDevice?.SetBtnLED(state, (byte)iindex);

            ShowResult(result, "Flash BL");
        }


        /// <summary>
        /// Same as above
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkBLOnOff_CheckedChanged(object sender, EventArgs e) => ChkFlash_CheckedChanged(sender, e);



        private void BtnSetFlash_Click(object sender, EventArgs e)
        {
            //Sets the frequency of flashing for both the LEDs and backlighting
            byte freq = (byte)(Convert.ToInt16(TxtFlashFreq.Text));
            int? result = CurrentDevice?.SetFlashFrequency(freq);

            ShowResult(result, "Set Flash Frequency");
        }


        /// <summary>
        /// Turns on or off, depending on value of ChkGreenOnOff, ALL green BLs using current intensity
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkBlueOnOff_CheckedChanged(object sender, EventArgs e)
        {
            // OR turn individual rows on or off using bits.  1st bit=row 1, 2nd bit=row 2, 3rd bit =row 3, etc
            int sl = ChkBlueOnOff.Checked ? 255 : 0;
            // 0 for Blue, 1 for Red
            int? result = CurrentDevice?.SetBankOnOff(0, (byte)sl);

            ShowResult(result, "All Green BL on/off");
        }


        /// <summary>
        /// Turns on or off, depending on value of ChkRedOnOff, ALL red BLs using current intensity
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkRedOnOff_CheckedChanged(object sender, EventArgs e)
        {
            // OR turn individual rows on or off using bits.  1st bit=row 1, 2nd bit=row 2, 3rd bit =row 3, etc
            int sl = ChkRedOnOff.Checked ? 255 : 0;
            // 0 for Blue, 1 for Red
            int? result = CurrentDevice?.SetBankOnOff(1, (byte)sl);

            ShowResult(result, "All Red BL on/off");
        }


        private void BtnBL_Click(object sender, EventArgs e)
        {
            byte bank1 = (byte)(Convert.ToInt16(TxtIntensity.Text)); ; //0-255 for brightness of bank 1 bl leds
            byte bank2 = (byte)(Convert.ToInt16(TxtIntensity2.Text)); ; //0-255 for brightness of bank 2 bl leds
            int? result = CurrentDevice?.SetBankBrightness(bank1, bank2);

            ShowResult(result, "Backlighting Intensity");
        }


        /// <summary>
        /// Write current state of backlighting to EEPROM.  
        /// NOTE: Is it not recommended to do this frequently as there are a finite number of writes to the EEPROM allowed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSaveBL_Click(object sender, EventArgs e)
        {
            int? result = CurrentDevice?.SaveBacklighting();

            ShowResult(result, "Save Backlight to EEPROM");
        }


        #endregion Control
    }
}
