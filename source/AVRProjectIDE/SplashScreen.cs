﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AVRProjectIDE
{
    public partial class SplashScreen : Form
    {
        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref Message m)
        {
            try
            {
                base.WndProc(ref m);
            }
            catch (Exception ex)
            {
                ErrorReportWindow.Show(ex, "Error In Splash Screen");
                
            }
        }

        public SplashScreen()
        {
            InitializeComponent();
            
            this.Width = this.BackgroundImage.Width;
            this.Height = this.BackgroundImage.Height;
        }
    }
}
