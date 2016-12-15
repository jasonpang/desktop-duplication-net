using System;
using System.Drawing;
using System.Windows.Forms;

namespace DesktopDuplication.Demo
{
    public partial class FormDemo : Form
    {
        private Bitmap _screen;
        private DesktopDuplicator _desktopDuplicator;
        private Bitmap _cursorImage;
        private Point _cursorLocation;

        public FormDemo()
        {
            InitializeComponent();

            Paint += CursorPaint;

            try
            {
                _desktopDuplicator = new DesktopDuplicator(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void CursorPaint(object sender, PaintEventArgs e)
        {
            if (_screen != null)
            {
                e.Graphics.DrawImage(_screen, e.ClipRectangle, e.ClipRectangle, GraphicsUnit.Pixel);
                //e.Graphics.DrawImageUnscaledAndClipped(screen, ClientRectangle);
            }
            if (_cursorImage != null)
            {
                e.Graphics.DrawImageUnscaled(_cursorImage, _cursorLocation);
            }
        }

        private void FormDemo_Shown(object sender, EventArgs e)
        {
            while (Visible)
            {
                Application.DoEvents();
                FormBorderStyle = WindowState == FormWindowState.Maximized ? FormBorderStyle.None : FormBorderStyle.Sizable;

                DesktopFrame frame;
                try
                {
                    frame = _desktopDuplicator.GetLatestFrame();
                }
                catch
                {
                    _desktopDuplicator = new DesktopDuplicator(0);
                    continue;
                }

                if (frame != null)
                {
                    var update = new Region();
                    if (frame.CursorBitmap != null) {
                        _cursorImage?.Dispose();
                        _cursorImage = frame.CursorBitmap;
                        update.Union(new Rectangle(_cursorLocation, _cursorImage.Size));
                    }
                    if (frame.CursorLocation != new Point() && _cursorImage != null)
                    {
                        update.Union(new Rectangle(_cursorLocation, _cursorImage.Size));
                        _cursorLocation = frame.CursorLocation;
                        update.Union(new Rectangle(_cursorLocation, _cursorImage.Size));
                    }

                    if (frame.DesktopImage != null) {
                        _screen?.Dispose();
                        _screen = frame.DesktopImage;
                        //update.Union(ClientRectangle);
                        foreach (var region in frame.UpdatedRegions) update.Union(region);
                        foreach (var region in frame.MovedRegions) update.Union(region.Destination);
                    }

                    Invalidate(update, false);
                }
            }
        }
    }
}
