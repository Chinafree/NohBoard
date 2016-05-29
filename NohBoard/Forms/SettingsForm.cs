﻿/*
Copyright (C) 2016 by Eric Bataille <e.c.p.bataille@gmail.com>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 2 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/


namespace ThoNohT.NohBoard.Forms
{
    using System;
    using System.Windows.Forms;
    using Extra;
    using ThoNohT.NohBoard.Hooking.Interop;

    public partial class SettingsForm : Form
    {
        /// <summary>
        /// Indicates whether the form is currently capturing the trap toggle key.
        /// </summary>
        private bool capturingKey = false;

        /// <summary>
        /// The keycode of the key to use for toggling traps.
        /// </summary>
        private int trapToggleKey;

        public SettingsForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_Load(object sender, System.EventArgs e)
        {
            this.udMouseSensitivity.Value = GlobalSettings.Settings.MouseSensitivity;
            this.udScrollHold.Value = GlobalSettings.Settings.ScrollHold;

            this.chkTrapKeyboard.Checked = GlobalSettings.Settings.TrapKeyboard;
            this.chkTrapMouse.Checked = GlobalSettings.Settings.TrapMouse;

            this.chkShowKeypresses.Checked = GlobalSettings.Settings.ShowKeyPresses;

            this.trapToggleKey = GlobalSettings.Settings.TrapToggleKeyCode;
            this.txtToggleKey.Text = ((Keys)this.trapToggleKey).ToString();

            switch (GlobalSettings.Settings.Capitalization)
            {
                case CapitalizationMethod.Capitalize:
                    this.rdbAlwaysCaps.Checked = true;
                    break;
                case CapitalizationMethod.Lowercase:
                    this.rdbAlwaysLower.Checked = true;
                    break;
                case CapitalizationMethod.FollowKeys:
                    this.rdbFollowKeystate.Checked = true;
                    break;
            }

            this.SetToolTips();
        }

        /// <summary>
        /// Sets the tooltips for the settings controls.
        /// </summary>
        private void SetToolTips()
        {
            var tooltip = new ToolTip();
            tooltip.SetToolTip(this.txtToggleKey, "Double click to change the toggle key for the mouse/keyboard trap.");

            tooltip.SetToolTip(
                this.udMouseSensitivity,
                "The sensitivity with which the mouse speed indicator reacts to mouse movement.");

            tooltip.SetToolTip(this.udScrollHold, "The time (in milliseconds) before a mouse scroll is released.");

            tooltip.SetToolTip(
                this.chkTrapKeyboard,
                "Check to let the keyboard be trapped by pressing the trap toggle key.");
            tooltip.SetToolTip(this.chkTrapMouse, "Check to let the mouse be trapped by pressing the trap toggle key.");

            tooltip.SetToolTip(
                this.chkShowKeypresses,
                "When checked, the last pressed numeric key will be displayed in (TODO). "
                + "This can be used to determine key codes when manually creating keyboard files.");

            tooltip.SetToolTip(
                this.rdbFollowKeystate,
                "Show capitalized letter when shift or caps is pressed, " +
                "but normal letters when both or neither are pressed.");
            tooltip.SetToolTip(this.rdbAlwaysCaps, "Always show capitalized letters.");
            tooltip.SetToolTip(this.rdbAlwaysLower, "Always show lower-case letters.");
        }

        private void OkButton_Click(object sender, System.EventArgs e)
        {
            // Apply the new settings.
            GlobalSettings.Settings.MouseSensitivity = (int)this.udMouseSensitivity.Value;
            GlobalSettings.Settings.ScrollHold = (int)this.udScrollHold.Value;

            GlobalSettings.Settings.TrapKeyboard = this.chkTrapKeyboard.Checked;
            GlobalSettings.Settings.TrapMouse = this.chkTrapMouse.Checked;
            GlobalSettings.Settings.TrapToggleKeyCode = this.trapToggleKey;

            GlobalSettings.Settings.ShowKeyPresses = this.chkShowKeypresses.Checked;

            GlobalSettings.Settings.Capitalization = this.rdbFollowKeystate.Checked
                                                         ? CapitalizationMethod.FollowKeys
                                                         : this.rdbAlwaysLower.Checked
                                                               ? CapitalizationMethod.Lowercase
                                                               : CapitalizationMethod.Capitalize;

            GlobalSettings.Save();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void txtToggleKey_KeyUp(object sender, KeyEventArgs e)
        {
            if (!this.capturingKey) return;

            this.txtToggleKey.Text = e.KeyCode.ToString();
            this.trapToggleKey = (int)e.KeyCode;

            e.SuppressKeyPress = true;
            this.capturingKey = false;
            HookManager.EnableMouseHook();
            HookManager.EnableKeyboardHook();
        }

        private void txtToggleKey_DoubleClick(object sender, System.EventArgs e)
        {
            HookManager.DisableKeyboardHook();
            HookManager.DisableMouseHook();
            this.capturingKey = true;
            this.txtToggleKey.Text = "Press a key...";
        }
    }
}
