using System;
using System.Windows;

namespace FileManager {
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window {
        public ProgressWindow() {
            InitializeComponent();
            ProgressBar.Maximum = 100.0;
        }

        private void CancelBtn_OnClicked(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}