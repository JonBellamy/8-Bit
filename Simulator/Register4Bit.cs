﻿using System;
using System.Drawing;
using EightBitSystem;

namespace Simulator
{
    public class Register4Bit : IRegister
    {
        SystemRegister id;

        public string Name { get { return id.ToString(); } }

        public byte Value { get; private set; }

        public Point consoleXY { get; set; }
        
        public string BinarytValue { get { return Convert.ToString(Value, 2).PadLeft(4, '0'); } }

        ControlLine updateFlagsLine;

        IAlu alu;

        public Register4Bit(SystemRegister id, IClock clock, IControlUnit controlUnit, IAlu alu)
        {
            this.id = id;
            Value = 0;

            this.alu = alu;

            clock.clockConnectedComponents.Add(this);

            updateFlagsLine = controlUnit.GetControlLine(ControlLineId.UPDATE_FLAGS);
        }


        public void SetBit(int bit, bool value)
        {
            if (bit < 0 || bit > 3)
            {
                throw new ArgumentException("Bit must be 0 - 3");
            }

            byte mask = (byte) (1 << bit);
            Value |= mask;
        }


        public bool GetBit(int bit)
        {
            if(bit < 0 ||  bit > 3)
            {
                throw new ArgumentException("Bit must be 0 - 3");
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
            if (updateFlagsLine.State == true)
            {
                Byte val = 0;
                if (alu.Carry) val |= (byte) (AluFlags.Carry);
                if (alu.Zero) val |= (byte)(AluFlags.Zero);
                if (val > 15)
                {
                    throw new ArgumentException("Flags cannot exceed 4 bits");
                }
                Value = val;
            }
        }


        public void OnFallingEdge()
        {
        }


        public void OutputState()
        {
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
                Console.Write(String.Format(" {0} {1}",opCode.ToString(), reg.ToString()));
            }
            Console.SetCursorPosition(consoleXY.X, consoleXY.Y + 2);
            Console.Write("|-----------------------|");
        }

    }
}
