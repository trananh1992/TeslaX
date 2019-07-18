﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using TeslaX.Properties;

namespace TeslaX
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void OnStartButtonClick(object sender, EventArgs e)
        {
            if (!Workflow.Active)
            {
                // On click, change it to Stop.
                StartButton.Text = "Stop";
                Task.Factory.StartNew(() => {
                    Workflow.Start(Settings.Default.Continue && Settings.Default.SimulateInput);
                    Invoke((MethodInvoker)delegate
                    {
                        StartButton.Text = "Start";
                        StartButton.Enabled = true;
                    });
                });
            }
            else
            {
                // On click, disable. Shortly, in asynchronous thread, we'll enable and change to Start.
                StartButton.Enabled = false;
                Workflow.Active = false;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Workflow.Active = false;
            Settings.Default.Save();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            foreach (var b in App.Sprites)
                BlockSelector.Items.Add(b.Name);
            BlockSelector.SelectedIndex = 0;

            SkinColor.BackColor = Game.SkinColors[(int)Settings.Default.SkinColor];
        }

        private void OnSkinColorChange(object sender, EventArgs e)
        {
            Color newbg = Game.SkinColors[Convert.ToInt32(SkinColor.Value)];
            SkinColor.BackColor = newbg;
        }

        private void OnTextureClick(object sender, EventArgs e)
        {
            (new TextureForm()).ShowDialog();
        }

        private void OnBlockSelectorChange(object sender, EventArgs e)
        {
            if (BlockSelector.SelectedIndex == BlockSelector.Items.Count - 1)
            {
                Bitmap src;
                using (OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Title = "Select spritesheet image";
                    dlg.Filter = "PNG files (*.png)|*.png";

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        // No safety check. If you've gone out of your way to load a non-image, deal with it.
                        src = new Bitmap(dlg.FileName);
                        Settings.Default.SelectedBlock = BlockSelector.SelectedIndex;
                        App.CustomSprite = src;
                        return;
                    }
                    BlockSelector.SelectedIndex = 0;
                }
            }

            Settings.Default.SelectedBlock = BlockSelector.SelectedIndex;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            (new ScriptForm()).ShowDialog();
        }
    }
}
