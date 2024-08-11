using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PIEHid32Net;

namespace PIHidDotName_Csharp_Sample
{
    public partial class Form1
    {
        //error callback
        public void HandlePIEHidError(PIEDevice sourceDevice, Int32 error)
        {
            this.SetToolStrip("Error: " + error.ToString());
        }



        //data callback    
        public void HandlePIEHidData(Byte[] data, PIEDevice sourceDevice, int error)
        {

            //check the sourceDevice and make sure it is the same device as selected in CboDevice   
            if (sourceDevice == devices[selecteddevice])
            {
                //check the switch byte 
                byte val2 = (byte)(data[2] & 1);
                if (val2 == 0)
                {
                    c = this.LblSwitchPos;
                    this.SetText("switch up");

                }
                else
                {
                    c = this.LblSwitchPos;
                    this.SetText("switch down");

                }
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
                        byte temp3 = (byte)(lastdata[i + 3] & temp1); //check using bitwise AND the previous value of this bit
                        int state = 0; //0=was up, now up, 1=was up, now down, 2= was down, still down, 3= was down, now up
                        if (temp2 != 0 && temp3 == 0) state = 1; //press
                        else if (temp2 != 0 && temp3 != 0) state = 2; //held down
                        else if (temp2 == 0 && temp3 != 0) state = 3; //release
                        switch (state)
                        {
                            case 1: //key was up and now is pressed
                                buttonsdown = buttonsdown + keynum.ToString() + " ";
                                c = this.LblButtons;
                                SetText(buttonsdown);
                                break;
                            case 2: //key was pressed and still is pressed
                                buttonsdown = buttonsdown + keynum.ToString() + " ";
                                c = this.LblButtons;
                                SetText(buttonsdown);
                                break;
                            case 3: //key was pressed and now released
                                break;
                        }
                        //Perform action based on key number, consult P.I. Engineering SDK documentation for the key numbers
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
                for (int i = 0; i < sourceDevice.ReadLength; i++)
                {
                    lastdata[i] = data[i];
                }
                //end buttons

                //time stamp info 4 bytes
                long absolutetime = 16777216 * data[7] + 65536 * data[8] + 256 * data[9] + data[10];  //ms
                long absolutetime2 = absolutetime / 1000; //seconds
                c = this.label19;
                this.SetText("absolute time: " + absolutetime2.ToString() + " s");
                long deltatime = absolutetime - saveabsolutetime;
                c = this.label20;
                this.SetText("delta time: " + deltatime + " ms");
                saveabsolutetime = absolutetime;
            }
        }

        private void BtnGetDataNow_Click(object sender, EventArgs e)
        {
            //After sending this command a general incoming data report will be given with
            //the 3rd byte (Data Type) 2nd bit set.  If program switch is up byte 3 will be 2
            //and if it is pressed byte 3 will be 3.  This is useful for getting the initial state
            //or unit id of the device before it sends any data.
            if (selecteddevice != -1) //do nothing if not enumerated
            {
                for (int j = 0; j < devices[selecteddevice].WriteLength; j++)
                {
                    wData[j] = 0;
                }

                wData[0] = 0;
                wData[1] = 177; //0xb1

                int result = 404;
                while (result == 404) { result = devices[selecteddevice].WriteData(wData); }

                if (result != 0)
                {
                    toolStripStatusLabel1.Text = "Write Fail: " + result;
                }
                else
                {
                    toolStripStatusLabel1.Text = "Write Success - Generate Data";
                }
            }
        }
    }
}
