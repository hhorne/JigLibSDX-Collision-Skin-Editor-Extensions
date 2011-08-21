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
    public partial class SplashScreen : Form
    {
        public SplashScreen()
        {
            InitializeComponent();

            //this.TopMost = true;
            this.Focus();
        }

        private void fadeInTimer_Tick(object sender, EventArgs e)
        {
            this.Opacity += 0.04;

            if (this.Opacity == 1)
            {
                fadeInTimer.Stop();
                waitTimer.Start();
                //this.TopMost = false;
            }
        }

        private void waitTimer_Tick(object sender, EventArgs e)
        {
            waitTimer.Stop();
            fadeOutTimer.Start();
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

        private void SplashScreen_Leave(object sender, EventArgs e)
        {
            EndSplash();
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
            EndSplash();
        }

        private void EndSplash()
        {
            fadeInTimer.Stop();
            waitTimer.Stop();
            fadeOutTimer.Start();
        }
    }
}
