using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;


namespace PIEDeviceLib
{
    public struct Button
    {
        public bool down;

        /// <summary>
        /// 0=was up, now up, 
        /// 1=was up, now down, 
        /// 2= was down, still down, 
        /// 3= was down, now up
        /// </summary>
        public int state;

        public int keynum;
        public int c;
        public int r;

        public override string ToString()
        {
            return $"Button {keynum}: {(down ? "down" : "up")}, state {state}";
        }
    }


    /// <summary>
    /// ensures that the fields are laid out in memory in the order they are defined.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DataStruct
    {
        /// <summary>
        /// number of columns of Xkeys digital button data,
        /// labeled "Keys" in P.I. Engineering SDK - General Incoming Data Input Report
        /// </summary>
        const int maxcols = 4;

        /// <summary>
        /// constant, 8 bits per byte
        /// </summary>
        const int maxrows = 8;


        /// <summary>
        /// consult P.I. Engineering SDK documentation for the key numbers
        /// </summary>
        const int numbuttons = 30;


        byte b0;
        public byte uid;
        // "switch up"/"switch down"
        public byte SwitchPos;

        public long absolutetime;

        // Calculated in Changed()
        public long? deltatime;

        /// <summary>
        /// Array of buttons states (down/up)
        /// </summary>
        bool[] buttons;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data"></param>
        public DataStruct(byte[] data)
        {
            Debug.Assert(data != null);

            b0 = data[0];
            // read the unit ID
            uid = data[1];
            // check the switch byte 
            SwitchPos = (byte)(data[2] & 1);

            // time stamp info 4 bytes //ms
            // This code is just mental
            //absolutetime = 16777216 * data[7] + 65536 * data[8] + 256 * data[9] + data[10]; 

            byte[] tb = data.Skip(7).Take(4).Reverse().ToArray();
            absolutetime = BitConverter.ToUInt32(tb, 0);
            deltatime = null;

            // Overwrite original 30"
            int numbuttons = maxcols * maxrows;
            buttons = new bool[numbuttons];

            // Original code calculates keynum,
            // but should be the same as n++
            int n = 0;

            // loop through digital button bytes
            for (int c = 0; c < maxcols; c++)
            {
                // loop through each bit in the button byte
                for (int r = 0; r < maxrows; r++)
                {
                    bool b = get(c, r, data, out int keynum);

                    // Original code calculates keynum,
                    // but should be the same as n++
                    Debug.Assert(n == keynum);
                    n++;

                    //if (keynum < numbuttons)
                    {
                        buttons[keynum] = b;
                    }
                }
            }
        }


        /// <summary>
        /// Get button state from data
        /// </summary>
        /// <param name="c"></param>
        /// <param name="r"></param>
        /// <param name="data"></param>
        /// <param name="keynum">
        /// Original code calculates keynum,
        /// but should be the same as n++</param>
        /// <returns>True if 'down'</returns>
        bool get(int c, int r, byte[] data, out int keynum)
        {
            keynum = -1;
            try
            {
                // 1, 2, 4, 8, 16, 32, 64, 128
                int pow = (int)Math.Pow(2, r);

                // check using bitwise AND the current value of this bit.
                // The + 3 is because the 1st button byte starts 3 bytes in at data[3]

                int ind = c + 3;
                Debug.Assert(ind != 7 && ind != 8 && ind != 9 && ind != 10 );

                byte val = (byte)(data[ind] & pow);

                // using key numbering in sdk;
                // column 1 = 0,1,2... column 2 = 8,9,10... column 3 = 16,17,18... column 4 = 24,25,26... etc
                keynum = maxrows * c + r;

                return val != 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message} (i {c}, j {r}, keynum {keynum})");
            }
        }


        public bool btn(int c, int r)
        {
            Debug.Assert(c < maxcols);
            Debug.Assert(r < maxrows);

            int keynum = maxrows * c + r;

            Debug.Assert(buttons != null);
            Debug.Assert(keynum < buttons.Length);

            return buttons[keynum];
        }


        /// <summary>
        /// Find which button state changed
        /// </summary>
        /// <param name="prev"></param>
        /// <returns></returns>
        public Button? Changed(DataStruct? prev)
        {
            if (prev != null)
            {
                deltatime = absolutetime - ((DataStruct)prev).absolutetime;
            }

            Debug.Assert(buttons != null);

            for (int i = 0; i < buttons.Length; i++)
            {
                bool b2 = buttons[i];
                bool b3 = prev?.buttons[i] ?? false;
                if (b2 == b3)
                {
                    continue;
                }

                //0=was up, now up, 1=was up, now down, 2= was down, still down, 3= was down, now up
                int state = 0; 
                if (b2 && !b3) state = 1; //press
                else if (b2 && b3) state = 2; //held down
                else if (!b2 && b3) state = 3; //release

                return new Button()
                {
                    down = b2,
                    state = state,
                    keynum = i,
                    // r = ,
                    // c = ,
                };
            }

            return null;
        }
    }
}
