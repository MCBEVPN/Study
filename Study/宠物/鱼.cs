using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Study.宠物
{
    public partial class 鱼 : Form
    {
        public 鱼()
        {
            InitializeComponent();
            this.TopMost = true;
            toRight = true;
            frame = 20;
            frame = 0;
            frameWidth = FullImage.Width / 20;
            frameHeight = FullImage.Height;
            left = -frameWidth;
            top = Screen.PrimaryScreen.WorkingArea.Height / 2f;
            timerSpeed.Interval = 50;
            timerSpeed.Enabled = true;
            timerSpeed.Tick += new EventHandler(timerSpeed_Tick);
            this.DoubleClick += new EventHandler(Form2_DoubleClick);
            this.MouseDown += new MouseEventHandler(Form2_MouseDown);
            this.MouseUp += new MouseEventHandler(Form2_MouseUp);
            this.MouseMove += new MouseEventHandler(Form2_MouseMove);
        }
        Point oldPoint = new Point(0, 0);
        bool mouseDown = false;
        bool haveHandle = false;
        Timer timerSpeed = new Timer();
        int MaxCount = 50;
        float stepX = 2f;
        float stepY = 0f;
        int count = 0;
        bool speedMode = false;
        float left = 0f, top = 0f;
        bool toRight = true;
        int frameCount = 20;
        int frame = 0;
        int frameWidth = 100;
        int frameHeight = 100;

        void Form2_MouseUp(object sender, MouseEventArgs e)
        {
            count = 0;
            MaxCount = new Random().Next(70) + 40;
            timerSpeed.Interval = new Random().Next(20) + 2;
            speedMode = true;
            mouseDown = false;
        }

        private void InitializeStyles()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            UpdateStyles();
        }

        void timerSpeed_Tick(object sender, EventArgs e)
        {
            if (!mouseDown)
            {
                count++;
                if (count > MaxCount)
                {
                    MaxCount = new Random().Next(70) + 30;
                    if (speedMode) timerSpeed.Interval = 50;

                    count = 0;
                    stepX = (float)new Random().NextDouble() * 3f + 0.5f;
                    stepY = ((float)new Random().NextDouble() - 0.5f) * 0.5f;
                }

                left = (left + (toRight ? 1 : -1) * stepX);
                top = (top + stepY);
                FixLeftTop();
                this.Left = (int)left;
                this.Top = (int)top;
            }
            frame++;
            if (frame >= frameCount) frame = 0;

            SetBits(FrameImage);
        }

        private void FixLeftTop()
        {
            if (toRight && left > Screen.PrimaryScreen.WorkingArea.Width)
            {
                toRight = false;
                frame = 0;
                count = 0;
            }
            else if (!toRight && left < -frameWidth)
            {
                toRight = true;
                frame = 0;
                count = 0;
            }
            if (top < -frameHeight)
            {
                stepY = 1f;
                count = 0;
            }
            else if (top > Screen.PrimaryScreen.WorkingArea.Height)
            {
                stepY = -1f;
                count = 0;
            }
        }

        private Image FullImage
        {
            get
            {
                if (toRight)
                    return Properties.Resources.Pet_FishRight;
                else
                    return Properties.Resources.Pet_FishLeft;
            }
        }

        public Bitmap FrameImage
        {
            get
            {
                Bitmap bitmap = new Bitmap(frameWidth, frameHeight);
                Graphics g = Graphics.FromImage(bitmap);
                g.DrawImage(FullImage, new Rectangle(0, 0, bitmap.Width, bitmap.Height), new Rectangle(frameWidth * frame, 0, frameWidth, frameHeight), GraphicsUnit.Pixel);
                return bitmap;
            }
        }

        void Form2_DoubleClick(object sender, EventArgs e)
        {

        }

        void Form2_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Left += (e.X - oldPoint.X);
                this.Top += (e.Y - oldPoint.Y);
                left = this.Left;
                top = this.Top;
                FixLeftTop();
            }
        }

        void Form2_MouseDown(object sender, MouseEventArgs e)
        {
            oldPoint = e.Location;
            mouseDown = true;
        }

        public void SetBits(Bitmap bitmap)
        {
            if (!haveHandle) return;
            if (!Image.IsCanonicalPixelFormat(bitmap.PixelFormat) || !Image.IsAlphaPixelFormat(bitmap.PixelFormat))
                throw new ApplicationException("The picture must be 32bit picture with alpha channel.");
            IntPtr oldBits = IntPtr.Zero;
            IntPtr screenDC = PetApi.GetDC(IntPtr.Zero);
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr memDc = PetApi.CreateCompatibleDC(screenDC);
            try
            {
                PetApi.Point topLoc = new PetApi.Point(Left, Top);
                PetApi.Size bitMapSize = new PetApi.Size(bitmap.Width, bitmap.Height);
                PetApi.BLENDFUNCTION blendFunc = new PetApi.BLENDFUNCTION();
                PetApi.Point srcLoc = new PetApi.Point(0, 0);
                hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
                oldBits = PetApi.SelectObject(memDc, hBitmap);
                blendFunc.BlendOp = PetApi.AC_SRC_OVER;
                blendFunc.SourceConstantAlpha = 255;
                blendFunc.AlphaFormat = PetApi.AC_SRC_ALPHA;
                blendFunc.BlendFlags = 0;
                PetApi.UpdateLayeredWindow(Handle, screenDC, ref topLoc, ref bitMapSize, memDc, ref srcLoc, 0, ref blendFunc,PetApi.ULW_ALPHA);
            }
            finally
            {
                if (hBitmap != IntPtr.Zero)
                {
                    PetApi.SelectObject(memDc, oldBits);
                    PetApi.DeleteObject(hBitmap);
                }
                PetApi.ReleaseDC(IntPtr.Zero, screenDC);
                PetApi.DeleteDC(memDc);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            base.OnClosing(e);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            InitializeStyles();
            base.OnHandleCreated(e);
            haveHandle = true;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cParms = base.CreateParams;
                cParms.ExStyle |= 0x00080000;
                return cParms;
            }
        }
    }
}
