﻿using System;
using System.IO;
using EightBitSystem;

namespace Simulator
{

    public class Ram : IMemoryController
    {
        // We use a 1K RAM chip
        private MemoryStream mem = new MemoryStream(1024);
        private IRegister mar;

        ControlLine busOutputLine;

        public IBus Bus { get; private set; }

        public byte Value { get { return Read(); } }


        public Ram(IBus bus, IControlUnit controlUnit, IRegister mar)
        {
            this.Bus = bus;
            busOutputLine = controlUnit.GetControlLine(ControlLineId.RAM_OUT);
            this.mar = mar;

            // Setup the callback for when the bus output line goes high or low. Depending on which, we either start or stop driving the bus
            busOutputLine.onTransition = () =>
            {
                if (busOutputLine.State == true)
                {
                    Bus.Driver = this;
                }
                else
                {
                    if (Bus.Driver == this)
                    {
                        Bus.Driver = null;
                    }
                }
                return true;
            };
        }


        public byte Read()
        {
            byte address = (byte) mar.Value;
            return mem.GetBuffer()[address];
        }


        public void Write(byte value)
        {
            byte address = (byte)mar.Value;
            mem.GetBuffer()[address] = value;
        }

        

    }

}
