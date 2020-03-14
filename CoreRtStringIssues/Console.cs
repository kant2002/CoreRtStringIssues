﻿using System.Runtime.InteropServices;

namespace System
{
    public static class Console
    {
        private enum BOOL : int
        {
            FALSE = 0,
            TRUE = 1,
        }

        [DllImport("api-ms-win-core-processenvironment-l1-1-0")]
        private static unsafe extern IntPtr GetStdHandle(int c);

        private readonly static IntPtr s_outputHandle = GetStdHandle(-11);

        private readonly static IntPtr s_inputHandle = GetStdHandle(-10);

        [DllImport("api-ms-win-core-console-l2-1-0.dll", EntryPoint = "SetConsoleTitleW")]
        private static unsafe extern BOOL SetConsoleTitle(char* c);

        public static unsafe string Title
        {
            set
            {
                fixed (char* c = value)
                    SetConsoleTitle(c);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CONSOLE_CURSOR_INFO
        {
            public uint Size;
            public BOOL Visible;
        }

        [DllImport("api-ms-win-core-console-l2-1-0")]
        private static unsafe extern BOOL SetConsoleCursorInfo(IntPtr handle, CONSOLE_CURSOR_INFO* cursorInfo);

        public static unsafe bool CursorVisible
        {
            set
            {
                CONSOLE_CURSOR_INFO cursorInfo = new CONSOLE_CURSOR_INFO
                {
                    Size = 1,
                    Visible = value ? BOOL.TRUE : BOOL.FALSE
                };
                SetConsoleCursorInfo(s_outputHandle, &cursorInfo);
            }
        }

        [DllImport("api-ms-win-core-console-l2-1-0")]
        private static unsafe extern BOOL SetConsoleTextAttribute(IntPtr handle, ushort attribute);

        public static ConsoleColor ForegroundColor
        {
            set
            {
                SetConsoleTextAttribute(s_outputHandle, (ushort)value);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEY_EVENT_RECORD
        {
            public BOOL KeyDown;
            public short RepeatCount;
            public short VirtualKeyCode;
            public short VirtualScanCode;
            public short UChar;
            public int ControlKeyState;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT_RECORD
        {
            public short EventType;
            public KEY_EVENT_RECORD KeyEvent;
        }

        [DllImport("api-ms-win-core-console-l1-2-0", EntryPoint = "PeekConsoleInputW", CharSet = CharSet.Unicode)]
        private static unsafe extern BOOL PeekConsoleInput(IntPtr hConsoleInput, INPUT_RECORD* lpBuffer, uint nLength, uint* lpNumberOfEventsRead);

        public static unsafe bool KeyAvailable
        {
            get
            {
                uint nRead;
                INPUT_RECORD buffer;
                while (true)
                {
                    PeekConsoleInput(s_inputHandle, &buffer, 1, &nRead);

                    if (nRead == 0)
                        return false;

                    if (buffer.EventType == 1 && buffer.KeyEvent.KeyDown != BOOL.FALSE)
                        return true;

                    ReadConsoleInput(s_inputHandle, &buffer, 1, &nRead);
                }
            }
        }

        public enum ConsoleColor
        {
            Black, DarkBlue, DarkGreen, DarkCyan, DarkRed, DarkMagenta, DarkYellow,
            Gray, DarkGray, Blue, Green, Cyan, Red, Magenta, Yellow, White
        }

        public enum ConsoleKey
        {
            Escape = 27,
            LeftArrow = 37,
            UpArrow = 38,
            RightArrow = 39,
            DownArrow = 40,
        }

        public readonly struct ConsoleKeyInfo
        {
            public ConsoleKeyInfo(char keyChar, ConsoleKey key, bool shift, bool alt, bool control)
            {
                Key = key;
            }

            public readonly ConsoleKey Key;
        }
        [DllImport("api-ms-win-core-console-l1-2-0", EntryPoint = "ReadConsoleInputW", CharSet = CharSet.Unicode)]
        private static unsafe extern BOOL ReadConsoleInput(IntPtr hConsoleInput, INPUT_RECORD* lpBuffer, uint nLength, uint* lpNumberOfEventsRead);

        public static unsafe ConsoleKeyInfo ReadKey(bool intercept)
        {
            uint nRead;
            INPUT_RECORD buffer;
            do
            {
                ReadConsoleInput(s_inputHandle, &buffer, 1, &nRead);
            }
            while (buffer.EventType != 1 || buffer.KeyEvent.KeyDown == BOOL.FALSE);

            return new ConsoleKeyInfo((char)buffer.KeyEvent.UChar, (ConsoleKey)buffer.KeyEvent.VirtualKeyCode, false, false, false);
        }

        struct SMALL_RECT
        {
            public short Left, Top, Right, Bottom;
        }

        [DllImport("api-ms-win-core-console-l2-1-0")]
        private static unsafe extern BOOL SetConsoleWindowInfo(IntPtr handle, BOOL absolute, SMALL_RECT* consoleWindow);

        public static unsafe void SetWindowSize(int x, int y)
        {
            SMALL_RECT rect = new SMALL_RECT
            {
                Left = 0,
                Top = 0,
                Right = (short)(x - 1),
                Bottom = (short)(y - 1),
            };
            SetConsoleWindowInfo(s_outputHandle, BOOL.TRUE, &rect);
        }

        [StructLayout(LayoutKind.Sequential)]
        struct COORD
        {
            public short X, Y;
        }

        [DllImport("api-ms-win-core-console-l2-1-0")]
        private static unsafe extern BOOL SetConsoleScreenBufferSize(IntPtr handle, COORD size);

        public static void SetBufferSize(int x, int y)
        {
            SetConsoleScreenBufferSize(s_outputHandle, new COORD { X = (short)x, Y = (short)y });
        }

        [DllImport("api-ms-win-core-console-l2-1-0")]
        private static unsafe extern BOOL SetConsoleCursorPosition(IntPtr handle, COORD position);

        public static void SetCursorPosition(int x, int y)
        {
            SetConsoleCursorPosition(s_outputHandle, new COORD { X = (short)x, Y = (short)y });
        }

        [DllImport("api-ms-win-core-console-l1-2-0", EntryPoint = "WriteConsoleW")]
        private static unsafe extern BOOL WriteConsole(IntPtr handle, void* buffer, int numChars, int* charsWritten, void* reserved);

        public static unsafe void Write(char c)
        {
            int dummy;
            WriteConsole(s_outputHandle, &c, 1, &dummy, null);
        }
    }
}