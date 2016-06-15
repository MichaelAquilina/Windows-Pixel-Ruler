using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace WindowsProgramming_Assignment
{
    class Ruler : Form
    {
        #region Win32 API Imports

        private const long LWA_ALPHA = 0x2L;
        private const int GWL_EXSTYLE = (-20);
        private const int WS_EX_LAYERED = 0x80000;
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user33.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user33.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user33.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user33.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user33.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("User33.dll")]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        #endregion

        private const int BorderThickness = 5;
        private const int LineThickness = 3;

        ContextMenu menu = new ContextMenu();
        MenuItem exitItem = new MenuItem("Exit Ruler Applicaiton");

        public Ruler( int InitialWidth )
        {
            this.Width = InitialWidth;

            this.MinimumSize = new Size(300, 50);
            this.MaximumSize = new Size(SystemInformation.VirtualScreen.Width, 50);
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true; 
            this.SizeChanged += delegate
            {
                //when the ruler is resized, invalidate the form so that it is repainted
                this.Invalidate();
            };

            menu.MenuItems.Add(exitItem);
            exitItem.Click += delegate
            {
                //make the application quit when this item is clicked
                Application.Exit();
            };

            this.MouseUp += new MouseEventHandler(Ruler_MouseUp);
            this.MouseDown += new MouseEventHandler(Ruler_MouseDown);

            //make the ruler transparent using win32 api calls
            byte opacity = (byte)((255 * 80) / 100);
            SetWindowLong(Handle, GWL_EXSTYLE, GetWindowLong(Handle, GWL_EXSTYLE) ^ WS_EX_LAYERED);
            SetLayeredWindowAttributes(Handle, 0, opacity, (uint)LWA_ALPHA);
        }

        void Ruler_MouseDown(object sender, MouseEventArgs e)
        {
            //allows dragging of the ruler
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                //trick the application into thinking that the title bar is being clicked
                //this forces the application to move along with the mouse
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int wmNcHitTest = 0x84;
            const int htLeft = 10;
            const int htRight = 11;
            if (m.Msg == wmNcHitTest)
            {
                //get the x and y position of the mouse from the message
                int x = (int)(m.LParam.ToInt64() & 0xFFFF);
                int y = (int)((m.LParam.ToInt64() & 0xFFFF0000) >> 16);

                //change the x and y coordinates relative to the application
                Point pt = PointToClient(new Point(x, y));
                
                //check if the mouse in an area which allows the mouse to resize, if so change to allow it
                if (pt.X <= ClientSize.Width && pt.X >= ClientSize.Width - BorderThickness )
                {
                    m.Result = (IntPtr) htRight;
                    return;
                }
                if (pt.X >= 0 && pt.X <= BorderThickness)
                {
                    m.Result = (IntPtr) htLeft;
                    return;
                }
            }
            base.WndProc(ref m);
        }

        void Ruler_MouseUp(object sender, MouseEventArgs e)
        {
            //opens a context sensitive menu when the right button is pressed
            if (e.Button == MouseButtons.Right)
                menu.Show(this, new Point(e.X, e.Y));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawRuler( e.Graphics);
        }

        protected void DrawRuler( Graphics Graphics)
        {
            Pen BorderPen = new Pen(new SolidBrush(Color.Black), BorderThickness);
            Pen LinePen = new Pen(new SolidBrush(Color.Black), LineThickness);
            Rectangle Window = new Rectangle(0, 0, this.Width - 1, this.Height - 1);

            Graphics.FillRectangle(new SolidBrush(Color.DodgerBlue), Window);
            Graphics.DrawRectangle(BorderPen, Window);

            for (int i = 0; i < this.Width; i += 50)
            {
                Graphics.DrawLine(LinePen, new Point(i, 0), new Point(i, (2 - (i % 100) / 50) * this.Height / 4));

                if (i % 100 == 0)
                    Graphics.DrawString( i + "px", new Font("Arial", 13), new SolidBrush(Color.Black), new PointF(i, 27));
            }
        }
    }
}
