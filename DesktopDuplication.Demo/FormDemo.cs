using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesktopDuplication.Demo
{
    public partial class FormDemo : Form
    {
        private DesktopDuplicator desktopDuplicator;

        public FormDemo()
        {
            InitializeComponent();

            try
            {
                desktopDuplicator = new DesktopDuplicator(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void FormDemo_Shown(object sender, EventArgs e)
        {
            while (true)
            {
                Application.DoEvents();
                desktopDuplicator.UpdateFrame();
                this.BackgroundImage = desktopDuplicator.CurrentFrame.DesktopImage;
            }
        }
    }
}
