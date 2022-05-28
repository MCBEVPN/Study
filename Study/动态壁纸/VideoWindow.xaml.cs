using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Study.动态壁纸
{
    public partial class VideoWindow : Window
    {
        public VideoWindow()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
        }

        public Uri Source
        {
            get => mediaElement.Source;
            set => mediaElement.Source = value;
        }

        public double Volume
        {
            get => mediaElement.Volume;
            set => mediaElement.Volume = value;
        }

        public bool IsMuted
        {
            get => mediaElement.IsMuted;
            set => mediaElement.IsMuted = value;
        }

        public void Play() => mediaElement.Play();

        public void Pause() => mediaElement.Pause();

        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaElement.Position = TimeSpan.Zero;
            mediaElement.Play();
        }

        protected override void OnClosed(EventArgs e)
        {
            mediaElement.Close();
            mediaElement = null;
            base.OnClosed(e);
        }
    }
}
