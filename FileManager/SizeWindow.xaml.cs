using System.Windows;

namespace FileManager {
    /// <summary>
    /// Interaction logic for SizeWindow.xaml
    /// </summary>
    public partial class SizeWindow : Window {
        public SizeWindow() {
            InitializeComponent();
        }

        public void SetSize(long size) {
            if (size > 1073741824) {
                size /= 1073741824;
                TextBlock.Text = $"{size:0.00} GB";
            } else if (size > 1048576) {
                size /= 1048576;
                TextBlock.Text = $"{size:0.00} MB";
            } else if (size > 1024) {
                size /= 1024;
                TextBlock.Text = $"{size:0.00} KB";
            } else {
                TextBlock.Text = $"{size:0.00} bytes";
            }
        }
    }
}