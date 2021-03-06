﻿using System;
using System.Drawing;
using System.Runtime.InteropServices;
using EightBitSystem;
using static Simulator.IDisplayComponent;

namespace Simulator
{

    public class ProgramCounter : ICounter, IBusConnectedComponent
    {
        public bool CountEnabled { get { return countEnableLine.State; } }

        public byte MaxValue { get { return 255; } }

        public byte Value { get; private set; }
        public string BinaryValue { get { return Convert.ToString(Value, 2).PadLeft(8, '0'); } }

        public IBus Bus { get; private set; }
        public string Name { get { return "PC"; } }

        public Point ConsoleXY { get; set; }

        ControlLine busOutputLine;
        ControlLine countEnableLine;
        ControlLine busInputLine;

        public ProgramCounter(IClock clock, IBus bus, IControlUnit controlUnit)
        {
            this.Bus = bus;
            busOutputLine = controlUnit.GetControlLine(ControlLineId.PC_OUT);
            countEnableLine = controlUnit.GetControlLine(ControlLineId.PC_ENABLE);
            busInputLine = controlUnit.GetControlLine(ControlLineId.PC_IN);
            clock.AddConnectedComponent(this);

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

        public void Reset()
        {
            Value = 0;

            if (Bus.Driver == this)
            {
                Bus.Driver = null;
            }
        }

        public void OnRisingEdge()
        {
            if(busInputLine.State == true)
            {
                Value = Bus.Value;
                return;
            }

            if (busOutputLine.State == true)
            {
                Bus.Driver = this;
            }

            if(CountEnabled)
            {
                Value++;

                if(Value > MaxValue)
                {
                    Value = 0;
                }
            }
        }


        public void OnFallingEdge()
        {
        }


        public void OutputState(ValueFormat format)
        {
            Console.ForegroundColor = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? ConsoleColor.Black : ConsoleColor.White;
            if (Bus.Driver == this)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            if (busInputLine.State)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
            }

            Console.SetCursorPosition(ConsoleXY.X, ConsoleXY.Y);
            Console.Write("|-----------------------|");
            Console.SetCursorPosition(ConsoleXY.X, ConsoleXY.Y + 1);
            Console.Write("|                       |");
            Console.SetCursorPosition(ConsoleXY.X, ConsoleXY.Y + 1);

            switch(format)
            {
                case ValueFormat.Hex:
                    Console.Write(String.Format("|PC: 0x{0:X2}", Value));
                break;

                case ValueFormat.Decimal:
                    Console.Write(String.Format("|PC: {0}", Value));
                break;

                case ValueFormat.Binary:
                    Console.Write(String.Format("|PC: {0}", BinaryValue));
                    break;
            }

            Console.SetCursorPosition(ConsoleXY.X, ConsoleXY.Y + 2);
            Console.Write("|-----------------------|");
        }
    }

}
