using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using PIEHid32Net;
using PIEDeviceLib;
using static PIEDeviceLib.PIEDeviceEx;

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


        #region Callback


        //error callback
        public void HandlePIEHidError(PIEDevice sourceDevice, Int32 error)
        {
            this.SetToolStrip("Error: " + error.ToString());
        }


        private void CurrentDevice_OnError(object sender, MessageEventArgs e)
        {
            this.SetToolStrip(e.msg);
        }


        private void CurrentDevice_OnMessage(object sender, MessageEventArgs e)
        {
            //HandlePIEHidData(e.data, sender as PIEDevice, e.error ?? 0);

            if (e.button != null)
            {
                c = this.LblButtons;
                SetText(e.button.ToString());
            }

            if (e.data_struct != null)
            {
                DataStruct ds = (DataStruct)e.data_struct;
                //check the switch byte 
                c = this.LblSwitchPos;
                this.SetText($"switch {(ds.SwitchPos == 0 ? "up" : "down")}");

                //read the unit ID
                c = this.LblUnitID;
                this.SetText(ds.uid.ToString());

                ShowAbsolutetime(ds.absolutetime, ds.deltatime);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="absolutetime"></param>
        /// <param name="deltatime">Passed from new code, but old code uses saveabsolutetime</param>
        void ShowAbsolutetime(long absolutetime, long? deltatime = null)
        {
            //time stamp info 4 bytes
            long absolutetime2 = absolutetime / 1000; //seconds
            c = this.label19;
            this.SetText("absolute time: " + absolutetime2.ToString() + " s");

            deltatime = deltatime ?? absolutetime - saveabsolutetime;
            c = this.label20;
            this.SetText("delta time: " + deltatime + " ms");
            saveabsolutetime = absolutetime;
        }


        //data callback
        // NOT USED
        public void HandlePIEHidData(Byte[] data, PIEDevice sourceDevice, int error)
        {
            //check the sourceDevice and make sure it is the same device as selected in CboDevice   
            if (sourceDevice == devices[selecteddevice])
            {
                //check the switch byte 
                byte val2 = (byte)(data[2] & 1);
                c = this.LblSwitchPos;
                this.SetText($"switch {(val2 == 0 ? "up" : "down")}");

                //read the unit ID
                c = this.LblUnitID;
                this.SetText(data[1].ToString());

                //write raw data to listbox1 in HEX
                String output = "Callback: " + sourceDevice.Pid + ", ID: " + selecteddevice.ToString() + ", data=";
                for (int i = 0; i < sourceDevice.ReadLength; i++)
                {
                    output = output + BinToHex(data[i]) + " ";
                }
                this.SetListBox(output);

                //buttons
                //this routine is for separating out the individual button presses/releases from the data byte array.
                int maxcols = 4; //number of columns of Xkeys digital button data, labeled "Keys" in P.I. Engineering SDK - General Incoming Data Input Report
                int maxrows = 8; //constant, 8 bits per byte
                c = this.LblButtons;
                string buttonsdown = "Buttons: "; //for demonstration, reset this every time a new input report received
                this.SetText(buttonsdown);

                for (int i = 0; i < maxcols; i++) //loop through digital button bytes 
                {
                    for (int j = 0; j < maxrows; j++) //loop through each bit in the button byte
                    {
                        int temp1 = (int)Math.Pow(2, j); //1, 2, 4, 8, 16, 32, 64, 128
                        int keynum = 8 * i + j; //using key numbering in sdk; column 1 = 0,1,2... column 2 = 8,9,10... column 3 = 16,17,18... column 4 = 24,25,26... etc
                        byte temp2 = (byte)(data[i + 3] & temp1); //check using bitwise AND the current value of this bit. The + 3 is because the 1st button byte starts 3 bytes in at data[3]
                        //byte temp3 = (byte)(lastdata[i + 3] & temp1); //check using bitwise AND the previous value of this bit
                        //
                        int state = 0; //0=was up, now up, 1=was up, now down, 2= was down, still down, 3= was down, now up
                        //if (temp2 != 0 && temp3 == 0) state = 1; //press
                        //else if (temp2 != 0 && temp3 != 0) state = 2; //held down
                        //else if (temp2 == 0 && temp3 != 0) state = 3; //release
                        //
                        //switch (state)
                        //{
                        //    case 1: //key was up and now is pressed
                        //        buttonsdown = buttonsdown + keynum.ToString() + " ";
                        //        c = this.LblButtons;
                        //        SetText(buttonsdown);
                        //        break;
                        //    case 2: //key was pressed and still is pressed
                        //        buttonsdown = buttonsdown + keynum.ToString() + " ";
                        //        c = this.LblButtons;
                        //        SetText(buttonsdown);
                        //        break;
                        //    case 3: //key was pressed and now released
                        //        break;
                        //}

                        // Perform action based on key number,
                        // consult P.I. Engineering SDK documentation for the key numbers
                        switch (keynum)
                        {
                            case 0: //button 0 (top left)
                                if (state == 1) //key was pressed
                                {
                                    //do press actions
                                }
                                else if (state == 3) //key was released
                                {
                                    //do release action
                                }
                                break;
                            case 1: //button 1
                                if (state == 1) //key was pressed
                                {
                                    //do press actions
                                }
                                else if (state == 3) //key was released
                                {
                                    //do release action
                                }
                                break;
                            case 2: //button 2
                                if (state == 1) //key was pressed
                                {
                                    //do press actions
                                }
                                else if (state == 3) //key was released
                                {
                                    //do release action
                                }
                                break;
                            case 3: //button 3
                                if (state == 1) //key was pressed
                                {
                                    //do press actions
                                }
                                else if (state == 3) //key was released
                                {
                                    //do release action
                                }
                                break;
                            case 4: //button 4
                                if (state == 1) //key was pressed
                                {
                                    //do press actions
                                }
                                else if (state == 3) //key was released
                                {
                                    //do release action
                                }
                                break;
                            case 5: //button 5
                                if (state == 1) //key was pressed
                                {
                                    //do press actions
                                }
                                else if (state == 3) //key was released
                                {
                                    //do release action
                                }
                                break;

                            //Next column of buttons
                            case 8: //button 8
                                if (state == 1) //key was pressed
                                {
                                    //do press actions
                                }
                                else if (state == 3) //key was released
                                {
                                    //do release action
                                }
                                break;
                            case 9: //button 9
                                if (state == 1) //key was pressed
                                {
                                    //do press actions
                                }
                                else if (state == 3) //key was released
                                {
                                    //do release action
                                }
                                break;
                            case 10: //button 10
                                if (state == 1) //key was pressed
                                {
                                    //do press actions
                                }
                                else if (state == 3) //key was released
                                {
                                    //do release action
                                }
                                break;
                            case 11: //button 11
                                if (state == 1) //key was pressed
                                {
                                    //do press actions
                                }
                                else if (state == 3) //key was released
                                {
                                    //do release action
                                }
                                break;
                            case 12: //button 12
                                if (state == 1) //key was pressed
                                {
                                    //do press actions
                                }
                                else if (state == 3) //key was released
                                {
                                    //do release action
                                }
                                break;
                            case 13: //button 13
                                if (state == 1) //key was pressed
                                {
                                    //do press actions
                                }
                                else if (state == 3) //key was released
                                {
                                    //do release action
                                }
                                break;

                            //Next column of buttons
                            case 16: //button 16
                                if (state == 1) //key was pressed
                                {
                                    //do press actions
                                }
                                else if (state == 3) //key was released
                                {
                                    //do release action
                                }
                                break;
                            case 17: //button 17
                                if (state == 1) //key was pressed
                                {
                                    //do press actions
                                }
                                else if (state == 3) //key was released
                                {
                                    //do release action
                                }
                                break;
                            case 18: //button 18
                                if (state == 1) //key was pressed
                                {
                                    //do press actions
                                }
                                else if (state == 3) //key was released
                                {
                                    //do release action
                                }
                                break;
                            case 19: //button 19
                                if (state == 1) //key was pressed
                                {
                                    //do press actions
                                }
                                else if (state == 3) //key was released
                                {
                                    //do release action
                                }
                                break;
                            case 20: //button 20
                                if (state == 1) //key was pressed
                                {
                                    //do press actions
                                }
                                else if (state == 3) //key was released
                                {
                                    //do release action
                                }
                                break;
                            case 21: //button 21
                                if (state == 1) //key was pressed
                                {
                                    //do press actions
                                }
                                else if (state == 3) //key was released
                                {
                                    //do release action
                                }
                                break;

                            //Next column of buttons
                            case 24: //button 24
                                if (state == 1) //key was pressed
                                {
                                    //do press actions
                                }
                                else if (state == 3) //key was released
                                {
                                    //do release action
                                }
                                break;
                            case 25: //button 25
                                if (state == 1) //key was pressed
                                {
                                    //do press actions
                                }
                                else if (state == 3) //key was released
                                {
                                    //do release action
                                }
                                break;
                            case 26: //button 26
                                if (state == 1) //key was pressed
                                {
                                    //do press actions
                                }
                                else if (state == 3) //key was released
                                {
                                    //do release action
                                }
                                break;
                            case 27: //button 27
                                if (state == 1) //key was pressed
                                {
                                    //do press actions
                                }
                                else if (state == 3) //key was released
                                {
                                    //do release action
                                }
                                break;
                            case 28: //button 28
                                if (state == 1) //key was pressed
                                {
                                    //do press actions
                                }
                                else if (state == 3) //key was released
                                {
                                    //do release action
                                }
                                break;
                            case 29: //button 29
                                if (state == 1) //key was pressed
                                {
                                    //do press actions
                                }
                                else if (state == 3) //key was released
                                {
                                    //do release action
                                }
                                break;

                        }
                    }
                }

                //for (int i = 0; i < sourceDevice.ReadLength; i++)
                //{
                //    lastdata[i] = data[i];
                //}
                //end buttons

                //time stamp info 4 bytes
                long absolutetime = 16777216 * data[7] + 65536 * data[8] + 256 * data[9] + data[10];  //ms
                ShowAbsolutetime(absolutetime);
            }
        }


        #endregion Callback


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
