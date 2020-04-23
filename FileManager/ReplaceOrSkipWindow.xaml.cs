using System.Windows;

namespace FileManager {
    /// <summary>
    ///     Interaction logic for ReplaceOrSkipWindow.xaml
    /// </summary>
    public partial class ReplaceOrSkipWindow : Window {
        private bool _check;

        public ReplaceOrSkipWindow() {
            InitializeComponent();
        }

        public void SetName(string name) => TextBlock.Text += name;

        public bool Return() => _check;

        private void SkipBtn_OnClick(object sender, RoutedEventArgs e) {
            _check = false;
            Close();
        }

        private void ReplaceBtn_OnClick(object sender, RoutedEventArgs e) {
            _check = true;
            Close();
        }
    }
}