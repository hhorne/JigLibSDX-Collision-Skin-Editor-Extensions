using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace JigLibSDX_CSE
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();

            this.TopMost = true;
            this.Focus();
        }

        private void fadeInTimer_Tick(object sender, EventArgs e)
        {
            this.Opacity += 0.04;

            if (this.Opacity == 1)
            {
                fadeInTimer.Stop();
                this.TopMost = false;
            }
        }

        private void fadeOutTimer_Tick(object sender, EventArgs e)
        {
            this.Opacity -= 0.04;

            if (this.Opacity < 0.04)
            {
                fadeOutTimer.Stop();
                this.Close();
            }
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
            EndSplash();
        }

        private void EndSplash()
        {
            fadeInTimer.Stop();
            fadeOutTimer.Start();
        }

        private void About_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyData)
            { 
                case Keys.Escape:
                case Keys.Space:
                case Keys.Return:
                    EndSplash();
                    break;
            }
        }
    }
}
