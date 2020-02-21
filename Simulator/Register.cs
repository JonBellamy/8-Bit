﻿using System;
using System.Drawing;
using EightBitSystem;

namespace Simulator
{
    public class Register : IRegister, IBusConnectedComponent
    {
        SystemRegister id;

        public IBus Bus { get; private set; }
        public string Name { get { return id.ToString(); } }

        public byte Value { get; private set; }

        public Point consoleXY { get; set; }
        
        public string BinarytValue { get { return Convert.ToString(Value, 2).PadLeft(8, '0'); } }

        ControlLine busOutputLine;
        ControlLine busInputLine;

        public Register(SystemRegister id, IClock clock, IBus bus, IControlUnit controlUnit)
        {
            this.id = id;
            Bus = bus;
            Value = 0;

            clock.clockConnectedComponents.Add(this);

            switch (id)
            {
                case SystemRegister.A:
                    busOutputLine = controlUnit.GetControlLine(ControlLineId.A_REG_OUT);
                    busInputLine = controlUnit.GetControlLine(ControlLineId.A_REG_IN);
                    break;

                case SystemRegister.B:
                    busOutputLine = controlUnit.GetControlLine(ControlLineId.B_REG_OUT);
                    busInputLine = controlUnit.GetControlLine(ControlLineId.B_REG_IN);
                    break;

                case SystemRegister.MAR:
                    busOutputLine = null;
                    busInputLine = controlUnit.GetControlLine(ControlLineId.MAR_IN);
                    break;

                case SystemRegister.IR:
                    busOutputLine = null;
                    busInputLine = controlUnit.GetControlLine(ControlLineId.IR_IN);
                    break;

                case SystemRegister.IR_PARAM:
                    busOutputLine = controlUnit.GetControlLine(ControlLineId.IR_PARAM_OUT);
                    busInputLine = controlUnit.GetControlLine(ControlLineId.IR_PARAM_IN);
                    break;

                case SystemRegister.OUT:
                    busOutputLine = null;
                    busInputLine = controlUnit.GetControlLine(ControlLineId.OUT_REG_IN);
                    break;

                default:
                    throw new ArgumentException("missing reg type");
            }

            // Setup the callback for when the bus output line goes high or low. Depending on which, we either start or stop driving the bus
            if(busOutputLine != null)
            {
                busOutputLine.onTransition = () =>
                {
                    if (busOutputLine.State == true)
                    {
                        Bus.Driver = this;
                    }
                    else
                    {
                        if(Bus.Driver == this)
                        {
                            Bus.Driver = null;
                        }
                    }
                    return true;
                };
            }
        }


        public void SetBit(int bit, bool value)
        {
            if (bit < 0 || bit > 7)
            {
                throw new ArgumentException("Bit must be 0 - 7");
            }

            byte mask = (byte) (1 << bit);
            Value |= mask;
        }


        public bool GetBit(int bit)
        {
            if(bit < 0 ||  bit > 7)
            {
                throw new ArgumentException("Bit must be 0 - 7");
            }

            int mask = (byte) (1 << bit);
            return (Value & mask) != 0;
        }


        public void Reset()
        {
            Value = 0;
        }


        public void OnRisingEdge()
        {
            if(busInputLine != null && busInputLine.State == true)
            {
                Value = Bus.Value;
                return;
            }

            if(busOutputLine != null && busOutputLine.State == true)
            {
                Bus.Driver = this;
            }
        }


        public void OnFallingEdge()
        {
        }


        public void OutputState()
        {
            Console.ForegroundColor = ConsoleColor.Black;
            if (Bus.Driver == this)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            if(busInputLine != null && busInputLine.State)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
            }
            Console.SetCursorPosition(consoleXY.X, consoleXY.Y);
            Console.Write("|-----------------------|");
            Console.SetCursorPosition(consoleXY.X, consoleXY.Y + 1);
            Console.Write("|                       |");
            Console.SetCursorPosition(consoleXY.X, consoleXY.Y + 1);
            Console.Write(String.Format("|{0} - 0x{1:X2}", id.ToString(), Value));

            // Yes this should be done with inheritence...
            if (id == SystemRegister.IR)
            {
                OpCode opCode = (OpCode) (Value >> 3);
                GeneralPurposeRegisterId reg = (GeneralPurposeRegisterId)(Value & 0x07);            
                Console.Write(String.Format(" {0}",opCode.ToString()));

                if(Enum.IsDefined(reg.GetType(), reg))
                {
                    Console.Write(String.Format(" {0}", reg.ToString()));
                }
            }
            Console.SetCursorPosition(consoleXY.X, consoleXY.Y + 2);
            Console.Write("|-----------------------|");
        }

    }
}
