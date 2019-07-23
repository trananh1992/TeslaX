﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using TheLeftExit.TeslaX.Properties;
using TheLeftExit.TeslaX.Static;
using Message = TheLeftExit.TeslaX.Static.Message;

namespace TheLeftExit.TeslaX.Interface
{
    public partial class NewMainForm : Form
    {
        // ToolStripDropDownButton doesn't share ancestors with Controls that have Enabled property.
        // Therefore we can't enable/disable all of them in a single foreach.
        private void EnableSettings(bool value)
        {
            propertyGrid.Enabled = value;
            topMenuStrip.Enabled = value;
            blockSelector.Enabled = value;
        }

        public NewMainForm()
        {
            InitializeComponent();

            // Setting icon.
            Icon = Resources.pickaxe;

            // Loading Discord.
            Discord.Update(DiscordStatus.Idle);

            // Linking form to app logic.
            propertyGrid.SelectedObject = UserSettings.Current;
            App.StatusLabel = statusLabel;

            // Enabling block selector.
            blockSelector.Text = App.Sprites[0].Name;
            foreach (var item in App.Sprites)
            {
                if (item.Name != "Custom")
                    blockSelector.DropDownItems.Add(item.Name, null, (EventHandler)delegate
                    {
                        UserSettings.Current.SelectedBlock = App.Sprites.FindIndex(x => x.Name == item.Name);
                        blockSelector.Text = item.Name;
                    });
                else
                    blockSelector.DropDownItems.Add(item.Name, null, (EventHandler)delegate
                    {
                        using (var dlg = new OpenFileDialog())
                        {
                            dlg.Filter = "PNG files|*.png";
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                Bitmap newcustom = new Bitmap(dlg.FileName);
                                if (newcustom.Height == 32 && newcustom.Width % 32 == 0)
                                {
                                    App.CustomSprite.Dispose();
                                    App.CustomSprite = newcustom;
                                    UserSettings.Current.SelectedBlock = App.Sprites.Count - 1;
                                    blockSelector.Text = item.Name;
                                    return;
                                }
                                else
                                {
                                    newcustom.Dispose();
                                    Message.NoCustomSpritesheet();
                                }
                            }

                            UserSettings.Current.SelectedBlock = 0;
                            blockSelector.Text = App.Sprites[0].Name;
                        }
                    });
            }
        }

        private void ToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            new ScriptForm().ShowDialog();
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (!Workflow.Active)
            {
                startButton.Text = "Stop";
                EnableSettings(false);
                new Thread(() =>
                {
                    while (Workflow.Start()) ; // This'll loop workflow/script if Continue is set.
                    Workflow.Active = false;
                    Discord.Update(DiscordStatus.Idle);
                    Invoke((MethodInvoker)delegate
                    {
                        startButton.Enabled = true;
                        startButton.Text = "Start";
                        EnableSettings(true);
                    });
                }).Start();
            }
            else
            {
                startButton.Enabled = false;
                Workflow.Active = false;
            }
        }

        private void ResetSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var res = MessageBox.Show("All settings will be set to defaults. Are you absolutely sure?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (res == DialogResult.Yes)
            {
                UserSettings.Current = new UserSettings();
                blockSelector.Text = App.Sprites[0].Name;
            }
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Texture.Delete())
                Message.TextureDeleted();
            else
                Message.TextureAlreadyDeleted();
        }

        private void ReplaceIncachegameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Texture.Replace();
            Message.TextureSwapped();
        }

        private void RestoreIncachegameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Texture.Restore();
            Message.TextureRestored();
        }

        private void CheckToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Texture.Replaced())
                Message.TexturesAreCustom();
            else
                Message.TexturesAreOriginal();
        }

        private void HelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var res = MessageBox.Show("Would you like to visit project's wiki on GitHub?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (res == DialogResult.Yes)
                Process.Start("https://github.com/TheLeftExit/TeslaX/wiki");
        }
    }
}