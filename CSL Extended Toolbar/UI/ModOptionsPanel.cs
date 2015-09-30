using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using ColossalFramework.UI;
using CommonShared.UI;
using CommonShared.UI.Extensions;
using UnityEngine;

namespace ExtendedToolbar.UI
{
    public class ModOptionsPanel : ConfigPanelBase
    {
        public ModOptionsPanel(UIHelper helper) : base(helper) { }

        private UIHelper modSettingsGroup;
        private UICheckBox debugLoggingCheckBox;
        private UILabel versionInfoLabel;

        protected override void PopulateUI()
        {
            this.RootPanel.eventVisibilityChanged += this.RootPanel_eventVisibilityChanged;

            // Create global options
            this.modSettingsGroup = this.RootHelper.AddGroup2("Mod settings");
            this.debugLoggingCheckBox = (UICheckBox)this.modSettingsGroup.AddCheckbox("Enable debug logging (don't use this during normal gameplay)", Mod.Instance.Settings.ExtraDebugLogging, v =>
            {
                Mod.Instance.Settings.ExtraDebugLogging = v;
                Mod.Instance.Log.EnableDebugLogging = v;
            });

            // Add mod information
            this.versionInfoLabel = this.RootPanel.AddUIComponent<UILabel>();
            this.versionInfoLabel.isVisible = false;
            this.versionInfoLabel.autoSize = true;
            this.versionInfoLabel.textScale = 0.8f;
            this.versionInfoLabel.text = Mod.Instance.BuildVersion;
        }

        private void RootPanel_eventVisibilityChanged(UIComponent component, bool value)
        {
            if (value)
            {
                // The panel is visible now, here we change the layout of our UI components to work around issue Archomeda/csl-ambient-sounds-tuner#35
                this.RootPanel.eventVisibilityChanged -= this.RootPanel_eventVisibilityChanged;

                // Start a delayed timer because the values of certain sizes/positions on UIComponents are not reliable, see
                // https://forum.paradoxplaza.com/forum/index.php?threads/variable-inconsistencies-in-custom-mod-option-panels.884268/
                // We have to disable auto layout, but if we disable it too early, we mess up the whole panel,
                // so after auto layout has settled, we proceed to disable it and relocate our mod information label
                Timer timer = new Timer(10);
                timer.Elapsed += (s, e) =>
                {
                    // We have to relocate our mod information label to the top right where it makes more sense
                    this.RootPanel.autoLayout = false;
                    this.versionInfoLabel.relativePosition = new Vector3(this.RootPanel.width - this.versionInfoLabel.size.x - 10, 10);
                    this.versionInfoLabel.Show();
                    timer.Dispose();
                };
                timer.AutoReset = false;
                timer.Start();
            }
        }

        protected override void OnClose()
        {
            Mod.Instance.Log.Debug("Options panel closed, saving config");
            Mod.Instance.Settings.SaveConfig(Mod.Instance.SettingsFilename);
        }
    }
}
