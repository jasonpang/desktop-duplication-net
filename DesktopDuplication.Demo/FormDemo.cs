using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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

                DesktopFrame frame = null;
                try
                {
                    frame = desktopDuplicator.GetLatestFrame();
                }
                catch
                {
                    desktopDuplicator = new DesktopDuplicator(0);
                    continue;
                }

                if (frame != null)
                {
                    LabelCursor.Location = frame.CursorLocation;
                    LabelCursor.Visible = frame.CursorVisible;
                    //Debug.WriteLine("--------------------------------------------------------");
                    //foreach (var moved in frame.MovedRegions)
                    //{
                    //    Debug.WriteLine(String.Format("Moved: {0} -> {1}", moved.Source, moved.Destination));
                    //    MovedRegion.Location = moved.Destination.Location;
                    //    MovedRegion.Size = moved.Destination.Size;
                    //}
                    //foreach (var updated in frame.UpdatedRegions)
                    //{
                    //    Debug.WriteLine(String.Format("Updated: {0}", updated.ToString()));
                    //    UpdatedRegion.Location = updated.Location;
                    //    UpdatedRegion.Size = updated.Size;
                    //}
                    this.BackgroundImage = frame.DesktopImage;
                }
            }
        }
    }
}
