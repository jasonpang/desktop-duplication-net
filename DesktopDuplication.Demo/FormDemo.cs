using System;
using System.Drawing;
using System.Windows.Forms;

namespace DesktopDuplication.Demo
{
    public partial class FormDemo : Form
    {
        private Bitmap _screen;
        private DesktopDuplicator _desktopDuplicator;
        private Cursor _cursorIcon;
        private Point _cursorLocation;
        private Size _cursorSize;

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
                //e.Graphics.DrawImage(_screen, e.ClipRectangle, e.ClipRectangle, GraphicsUnit.Pixel);
                e.Graphics.DrawImageUnscaledAndClipped(_screen, e.ClipRectangle);
            }
            if (_cursorIcon != null)
            {
                //e.Graphics.DrawIconUnstretched(_cursorIcon, new Rectangle(_cursorLocation, _cursorIcon.Size));
                _cursorIcon.DrawStretched(e.Graphics, new Rectangle(_cursorLocation, _cursorSize));
                //_cursorIcon.DrawStretched(e.Graphics, new Rectangle(_cursorLocation, _cursorIcon.Size));
            }
        }

        private void FormDemo_Shown(object sender, EventArgs e)
        {
            while (Visible)
            {
                Application.DoEvents();
                FormBorderStyle = WindowState == FormWindowState.Maximized
                    ? FormBorderStyle.None
                    : FormBorderStyle.Sizable;

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
                    if ((frame.CursorIcon != null || frame.CursorLocation != new Point())
                        && (_cursorLocation != new Point() && _cursorSize != new Size()))
                    {
                        update.Union(new Rectangle(_cursorLocation, _cursorSize));
                    }
                    if (frame.CursorIcon != null)
                    {
                        _cursorIcon?.Dispose();
                        _cursorIcon = frame.CursorIcon;
                        _cursorSize = frame.CursorSize;
                    }
                    if (frame.CursorLocation != new Point() && _cursorIcon != null)
                    {
                        _cursorLocation = frame.CursorLocation;
                    }
                    if ((frame.CursorIcon != null || frame.CursorLocation != new Point())
                        && (_cursorLocation != new Point() && _cursorSize != new Size()))
                    {
                        update.Union(new Rectangle(_cursorLocation, _cursorSize));
                    }
                    if (frame.DesktopImage != null)
                    {
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