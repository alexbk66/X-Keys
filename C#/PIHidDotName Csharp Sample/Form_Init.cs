using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using PIEHid32Net;


namespace PIHidDotName_Csharp_Sample
{
    public partial class Form1
    {
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
            CurrentDevice = devicesex[CboDevices.SelectedIndex];

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

    }
}
