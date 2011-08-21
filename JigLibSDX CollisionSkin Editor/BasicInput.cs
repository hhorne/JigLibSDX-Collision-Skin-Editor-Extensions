/* Copyright (c) David Reschke
 * 
 * Microsoft Public License (Ms-PL)
 * This license governs use of the accompanying software. If you use the software, 
 * you accept this license. If you do not accept the license, do not use the software.
 * 
 * 1. Definitions
 * The terms "reproduce," "reproduction," "derivative works," and "distribution" have
 * the same meaning here as under U.S. copyright law. A "contribution" is the original
 * software, or any additions or changes to the software.
 * 
 * A "contributor" is any person that distributes its contribution under this license.
 * 
 * "Licensed patents" are a contributor's patent claims that read directly on its 
 * contribution.
 * 
 * 2. Grant of Rights
 * (A) Copyright Grant- Subject to the terms of this license, including the license 
 * conditions and limitations in section 3, each contributor grants you a non-exclusive,
 * worldwide, royalty-free copyright license to reproduce its contribution, prepare 
 * derivative works of its contribution, and distribute its contribution or any 
 * derivative works that you create.
 * 
 * (B) Patent Grant- Subject to the terms of this license, including the license 
 * conditions and limitations in section 3, each contributor grants you a non-exclusive,
 * worldwide, royalty-free license under its licensed patents to make, have made, use,
 * sell, offer for sale, import, and/or otherwise dispose of its contribution in the
 * software or derivative works of the contribution in the software.
 * 
 * 3. Conditions and Limitations
 * (A) No Trademark License- This license does not grant you rights to use any 
 * contributors' name, logo, or trademarks.
 * 
 * (B) If you bring a patent claim against any contributor over patents that you claim
 * are infringed by the software, your patent license from such contributor to the
 * software ends automatically.
 * 
 * (C) If you distribute any portion of the software, you must retain all copyright,
 * patent, trademark, and attribution notices that are present in the software.
 * 
 * (D) If you distribute any portion of the software in source code form, you may do so
 * only under this license by including a complete copy of this license with your 
 * distribution. If you distribute any portion of the software in compiled or object 
 * code form, you may only do so under a license that complies with this license.
 * 
 * (E) The software is licensed "as-is." You bear the risk of using it. 
 * The contributors give no express warranties, guarantees or conditions. You may have
 * additional consumer rights under your local laws which this license cannot change.
 * To the extent permitted under your local laws, the contributors exclude the implied
 * warranties of merchantability, fitness for a particular purpose and
 * non-infringement.
 */

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using SlimDX;
using SlimDX.DirectInput;

namespace JigLibSDX_CSE
{
    public class BasicInput : IDisposable
    {
        #region Attributes
        Device<KeyboardState> _keyboardDevice;
        Device<MouseState> _mouseDevice;

        private KeyboardState _keyboardState;
        private MouseState _mouseState;

        private IEnumerable<BufferedData<KeyboardState>> _keyboardBufferedData;
        private IEnumerable<BufferedData<MouseState>> _mouseBufferedData;

        private bool _keyboardAvailable;
        private bool _mouseAvailable;
        #endregion

        #region Properties
        public KeyboardState KeyboardState
        {
            get { return _keyboardState; }
            private set { _keyboardState = value; }
        }

        public IEnumerable<BufferedData<KeyboardState>> KeyboardBufferedData
        {
            get { return _keyboardBufferedData; }
            private set { _keyboardBufferedData = value; }
        }

        public MouseState MouseState
        {
            get { return _mouseState; }
            private set { _mouseState = value; }
        }

        public IEnumerable<BufferedData<MouseState>> MouseBufferedData
        {
            get { return _mouseBufferedData; }
            private set { _mouseBufferedData = value; }
        }

        public bool KeyboardAvailable
        {
            get { return _keyboardAvailable; }
            private set { _keyboardAvailable = value; }
        }

        public bool MouseAvailable
        {
            get { return _mouseAvailable; }
            private set { _mouseAvailable = value; }
        }
        #endregion

        public BasicInput(IntPtr controlHandle)
        {
            DirectInput directInput = new DirectInput();
            CooperativeLevel cooperativeLevel = CooperativeLevel.Nonexclusive | CooperativeLevel.Foreground | CooperativeLevel.NoWinKey;

            try
            {
                _keyboardDevice = new Device<KeyboardState>(directInput, SystemGuid.Keyboard);
                _keyboardDevice.SetCooperativeLevel(controlHandle, cooperativeLevel);
                _keyboardDevice.Properties.BufferSize = 16;
            }
            catch (DirectInputException e)
            {
                MessageBox.Show(e.Message);
                return;
            }

            try
            {
                _mouseDevice = new Device<MouseState>(directInput, SystemGuid.Mouse);
                _mouseDevice.SetCooperativeLevel(controlHandle, cooperativeLevel);
                _mouseDevice.Properties.BufferSize = 16;
            }
            catch (DirectInputException e)
            {
                MessageBox.Show(e.Message);
                return;
            }
        }

        public void Update()
        {
            UpdateKeyboard();
            UpdateMouse();
        }

        private void UpdateKeyboard()
        {
            if (_keyboardDevice.Acquire().IsFailure)
            {
                //This is normal in CooperativeLevel.Foreground
                //Console.WriteLine("Can't acquire Keybord.");
                return;
            }
            else
            {
                if (_keyboardDevice.Poll().IsFailure)
                {
                    Console.WriteLine("Can't poll Keybord.");
                    _keyboardAvailable = false;
                    return;
                }
                else
                {
                    _keyboardBufferedData = _keyboardDevice.GetBufferedData();
                    _keyboardState = _keyboardDevice.GetCurrentState();

                    _keyboardAvailable = true;
                }
            }
        }

        private void UpdateMouse()
        {
            if (_mouseDevice.Acquire().IsFailure)
            {
                //This is normal in CooperativeLevel.Foreground
                //Console.WriteLine("Can't acquire Mouse.");
                return;
            }
            else
            {
                if (_mouseDevice.Poll().IsFailure)
                {
                    Console.WriteLine("Can't poll Mouse.");
                    _mouseAvailable = false;
                    return;
                }
                else
                {
                    _mouseBufferedData = _mouseDevice.GetBufferedData();
                    _mouseState = _mouseDevice.GetCurrentState();

                    _mouseAvailable = true;
                }
            }
        }

        #region IDisposable Member
        public void Dispose()
        {
            if (_keyboardDevice != null)
            {
                _keyboardDevice.Unacquire();
                _keyboardDevice.Dispose();
                _keyboardDevice = null;
            }

            if (_keyboardState != null)
            {
                _keyboardState = null;
            }

            if (_mouseDevice != null)
            {
                _mouseDevice.Unacquire();
                _mouseDevice.Dispose();
                _mouseDevice = null;
            }

            if (_mouseState != null)
            {
                _mouseState = null;
            }
        }
        #endregion
    }
}
