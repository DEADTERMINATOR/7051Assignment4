using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Pong
{
    class KeyboardHandler
    {
        private Keys[] lastPressedKeys;
        public List<Command> previousCommands;
        public String currentCommand;
        public Boolean firstKey;

        public KeyboardHandler()
        {
            lastPressedKeys = new Keys[0];
            previousCommands = new List<Command>();
            currentCommand = "";
        }

        public void Update()
        {
            KeyboardState kbState = Keyboard.GetState();
            Keys[] pressedKeys = kbState.GetPressedKeys();

            // Check if the currently pressed keys were already pressed
            foreach (Keys key in pressedKeys)
            {
                if (!lastPressedKeys.Contains(key))
                {
                    OnKeyDown(key);
                }
                if(firstKey)
                {
                    firstKey = false;
                }
            }

            // Save the currently pressed keys so we can compare on the next update
            lastPressedKeys = pressedKeys;
        }

        /// <summary>
        /// Adds the keystoke to the command string if it is a letter, ignores it if it 
        /// is not. All possible keys are handled so that keystrokes that should not appear
        /// in the console, do not appear.
        /// </summary>
        /// <param name="key">The key that was pressed.</param>
        private void OnKeyDown(Keys key)
        {
            switch (key)
            {
                case Keys.A:
                    currentCommand += key.ToString();
                    break;
                case Keys.Add:
                    break;
                case Keys.Apps:
                    break;
                case Keys.Attn:
                    break;
                case Keys.B:
                    currentCommand += key.ToString();
                    break;
                case Keys.Back:
                    if(currentCommand.Length > 0)
                    {
                        currentCommand = currentCommand.Remove(currentCommand.Length - 1);
                    }
                    break;
                case Keys.BrowserBack:
                    break;
                case Keys.BrowserFavorites:
                    break;
                case Keys.BrowserForward:
                    break;
                case Keys.BrowserHome:
                    break;
                case Keys.BrowserRefresh:
                    break;
                case Keys.BrowserSearch:
                    break;
                case Keys.BrowserStop:
                    break;
                case Keys.C:
                    if(!firstKey)
                    {
                        currentCommand += key.ToString();
                    }                    
                    break;
                case Keys.CapsLock:
                    break;
                case Keys.ChatPadGreen:
                    break;
                case Keys.ChatPadOrange:
                    break;
                case Keys.Crsel:
                    break;
                case Keys.D:
                    currentCommand += key.ToString();
                    break;
                case Keys.D0:
                    break;
                case Keys.D1:
                    break;
                case Keys.D2:
                    break;
                case Keys.D3:
                    break;
                case Keys.D4:
                    break;
                case Keys.D5:
                    break;
                case Keys.D6:
                    break;
                case Keys.D7:
                    break;
                case Keys.D8:
                    break;
                case Keys.D9:
                    break;
                case Keys.Decimal:
                    break;
                case Keys.Delete:
                    break;
                case Keys.Divide:
                    break;
                case Keys.Down:
                    break;
                case Keys.E:
                    currentCommand += key.ToString();
                    break;
                case Keys.End:
                    break;
                case Keys.Enter:
                    SubmitCommand();
                    break;
                case Keys.EraseEof:
                    break;
                case Keys.Escape:
                    break;
                case Keys.Execute:
                    break;
                case Keys.Exsel:
                    break;
                case Keys.F:
                    currentCommand += key.ToString();
                    break;
                case Keys.F1:
                    break;
                case Keys.F10:
                    break;
                case Keys.F11:
                    break;
                case Keys.F12:
                    break;
                case Keys.F13:
                    break;
                case Keys.F14:
                    break;
                case Keys.F15:
                    break;
                case Keys.F16:
                    break;
                case Keys.F17:
                    break;
                case Keys.F18:
                    break;
                case Keys.F19:
                    break;
                case Keys.F2:
                    break;
                case Keys.F20:
                    break;
                case Keys.F21:
                    break;
                case Keys.F22:
                    break;
                case Keys.F23:
                    break;
                case Keys.F24:
                    break;
                case Keys.F3:
                    break;
                case Keys.F4:
                    break;
                case Keys.F5:
                    break;
                case Keys.F6:
                    break;
                case Keys.F7:
                    break;
                case Keys.F8:
                    break;
                case Keys.F9:
                    break;
                case Keys.G:
                    currentCommand += key.ToString();
                    break;
                case Keys.H:
                    currentCommand += key.ToString();
                    break;
                case Keys.Help:
                    break;
                case Keys.Home:
                    break;
                case Keys.I:
                    currentCommand += key.ToString();
                    break;
                case Keys.ImeConvert:
                    break;
                case Keys.ImeNoConvert:
                    break;
                case Keys.Insert:
                    break;
                case Keys.J:
                    currentCommand += key.ToString();
                    break;
                case Keys.K:
                    currentCommand += key.ToString();
                    break;
                case Keys.Kana:
                    break;
                case Keys.Kanji:
                    break;
                case Keys.L:
                    currentCommand += key.ToString();
                    break;
                case Keys.LaunchApplication1:
                    break;
                case Keys.LaunchApplication2:
                    break;
                case Keys.LaunchMail:
                    break;
                case Keys.Left:
                    break;
                case Keys.LeftAlt:
                    break;
                case Keys.LeftControl:
                    break;
                case Keys.LeftShift:
                    break;
                case Keys.LeftWindows:
                    break;
                case Keys.M:
                    currentCommand += key.ToString();
                    break;
                case Keys.MediaNextTrack:
                    break;
                case Keys.MediaPlayPause:
                    break;
                case Keys.MediaPreviousTrack:
                    break;
                case Keys.MediaStop:
                    break;
                case Keys.Multiply:
                    break;
                case Keys.N:
                    currentCommand += key.ToString();
                    break;
                case Keys.None:
                    break;
                case Keys.NumLock:
                    break;
                case Keys.NumPad0:
                    break;
                case Keys.NumPad1:
                    break;
                case Keys.NumPad2:
                    break;
                case Keys.NumPad3:
                    break;
                case Keys.NumPad4:
                    break;
                case Keys.NumPad5:
                    break;
                case Keys.NumPad6:
                    break;
                case Keys.NumPad7:
                    break;
                case Keys.NumPad8:
                    break;
                case Keys.NumPad9:
                    break;
                case Keys.O:
                    currentCommand += key.ToString();
                    break;
                case Keys.Oem8:
                    break;
                case Keys.OemAuto:
                    break;
                case Keys.OemBackslash:
                    break;
                case Keys.OemClear:
                    break;
                case Keys.OemCloseBrackets:
                    break;
                case Keys.OemComma:
                    break;
                case Keys.OemCopy:
                    break;
                case Keys.OemEnlW:
                    break;
                case Keys.OemMinus:
                    break;
                case Keys.OemOpenBrackets:
                    break;
                case Keys.OemPeriod:
                    break;
                case Keys.OemPipe:
                    break;
                case Keys.OemPlus:
                    break;
                case Keys.OemQuestion:
                    break;
                case Keys.OemQuotes:
                    break;
                case Keys.OemSemicolon:
                    break;
                case Keys.OemTilde:
                    break;
                case Keys.P:
                    currentCommand += key.ToString();
                    break;
                case Keys.Pa1:
                    break;
                case Keys.PageDown:
                    break;
                case Keys.PageUp:
                    break;
                case Keys.Pause:
                    break;
                case Keys.Play:
                    break;
                case Keys.Print:
                    break;
                case Keys.PrintScreen:
                    break;
                case Keys.ProcessKey:
                    break;
                case Keys.Q:
                    currentCommand += key.ToString();
                    break;
                case Keys.R:
                    currentCommand += key.ToString();
                    break;
                case Keys.Right:
                    break;
                case Keys.RightAlt:
                    break;
                case Keys.RightControl:
                    break;
                case Keys.RightShift:
                    break;
                case Keys.RightWindows:
                    break;
                case Keys.S:
                    currentCommand += key.ToString();
                    break;
                case Keys.Scroll:
                    break;
                case Keys.Select:
                    break;
                case Keys.SelectMedia:
                    break;
                case Keys.Separator:
                    break;
                case Keys.Sleep:
                    break;
                case Keys.Space:
                    currentCommand += " ";
                    break;
                case Keys.Subtract:
                    break;
                case Keys.T:
                    currentCommand += key.ToString();
                    break;
                case Keys.Tab:
                    break;
                case Keys.U:
                    currentCommand += key.ToString();
                    break;
                case Keys.Up:
                    break;
                case Keys.V:
                    currentCommand += key.ToString();
                    break;
                case Keys.VolumeDown:
                    break;
                case Keys.VolumeMute:
                    break;
                case Keys.VolumeUp:
                    break;
                case Keys.W:
                    currentCommand += key.ToString();
                    break;
                case Keys.X:
                    currentCommand += key.ToString();
                    break;
                case Keys.Y:
                    currentCommand += key.ToString();
                    break;
                case Keys.Z:
                    currentCommand += key.ToString();
                    break;
                case Keys.Zoom:
                    break;
                default:
                    break;
            }
        }

        private void SubmitCommand()
        {
            previousCommands.Add(new Command(currentCommand));
            currentCommand = "";
        }

        public void ClearPressedKeys()
        {
            Array.Clear(lastPressedKeys, 0, lastPressedKeys.Length);
        }
    }
}
