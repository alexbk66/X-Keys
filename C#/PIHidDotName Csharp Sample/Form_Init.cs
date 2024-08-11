using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PIHidDotName_Csharp_Sample
{
    public partial class Form1
    {
        private void BtnEnumerate_Click(object sender, EventArgs e)
        {
            CboDevices.Items.Clear();
            cbotodevice = new int[128]; //128=max # of devices
            //enumerate and setupinterfaces for all devices
            devices = PIEHid32Net.PIEDevice.EnumeratePIE();
            if (devices.Length == 0)
            {
                toolStripStatusLabel1.Text = "No Devices Found";
            }
            else
            {
                //System.Media.SystemSounds.Beep.Play(); 

                int cbocount = 0; //keeps track of how many valid devices were added to the CboDevice box

                for (int i = 0; i < devices.Length; i++)
                {
                    //information about device
                    //PID = devices[i].Pid);
                    //HID Usage = devices[i].HidUsage);
                    //HID Usage Page = devices[i].HidUsagePage);
                    //HID Version = devices[i].Version); //NOTE: this is NOT the firmware version which is given in the descriptor
                    int hidusagepg = devices[i].HidUsagePage;
                    int hidusage = devices[i].HidUsage;

                    if (devices[i].HidUsagePage != 0xc || devices[i].WriteLength != 36)
                    {
                        continue;
                    }

                    switch (devices[i].Pid)
                    {
                        case 1027:
                            //Device 2 Keyboard, Joystick, Input and Output endpoints, PID #3
                            CboDevices.Items.Add(devices[i].ProductString + " (" + devices[i].Pid + "=PID #1)");
                            cbotodevice[cbocount] = i;
                            cbocount++;
                            break;
                        case 1028:
                            //Device 1 Keyboard, Joystick, Mouse and Output endpoints,. PID #2
                            CboDevices.Items.Add(devices[i].ProductString + " (" + devices[i].Pid + "=PID #2)");
                            cbotodevice[cbocount] = i;
                            cbocount++;
                            break;
                        case 1029:
                            //Device 0 Keyboard, Mouse, Input and Output endpoints (factory default), PID #1
                            CboDevices.Items.Add(devices[i].ProductString + " (" + devices[i].Pid + "=PID #3)");
                            cbotodevice[cbocount] = i;
                            cbocount++;
                            break;
                        case 1249:
                            //Device 3 Keyboard, Multimedia, Mouse and Output endpoints, PID #4
                            CboDevices.Items.Add(devices[i].ProductString + " (" + devices[i].Pid + "=PID #4)");
                            cbotodevice[cbocount] = i;
                            cbocount++;
                            break;
                        default:
                            CboDevices.Items.Add("Unknown Device: " + devices[i].ProductString + " (" + devices[i].Pid + ")");
                            cbotodevice[cbocount] = i;
                            cbocount++;
                            break;
                    }

                    devices[i].SetupInterface();
                    devices[i].suppressDuplicateReports = false;
                }
            }

            if (CboDevices.Items.Count > 0)
            {
                // This causes CboDevices_SelectedIndexChanged
                CboDevices.SelectedIndex = 0;
                //selecteddevice = cbotodevice[CboDevices.SelectedIndex];
                //wData = new byte[devices[selecteddevice].WriteLength];//go ahead and setup for write
                //lastdata = new byte[devices[selecteddevice].ReadLength];
                //LblVersion.Text = devices[selecteddevice].Version.ToString();
            }
        }



        private void CboDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            selecteddevice = cbotodevice[CboDevices.SelectedIndex];
            wData = new byte[devices[selecteddevice].WriteLength];//size write array 
            lastdata = new byte[devices[selecteddevice].ReadLength];
            LblVersion.Text = devices[selecteddevice].Version.ToString();
        }


        private void BtnCallback_Click(object sender, EventArgs e)
        {
            //setup callback if there are devices found for each device found

            if (selecteddevice != -1)
            {
                for (int i = 0; i < CboDevices.Items.Count; i++)
                {
                    //use the cbotodevice array which contains the mapping of the devices in the CboDevices to the actual device IDs
                    devices[cbotodevice[i]].SetErrorCallback(this);
                    devices[cbotodevice[i]].SetDataCallback(this);
                    devices[cbotodevice[i]].callNever = false;
                }
            }
        }
    }
}
