using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using FileManager.Annotations;

namespace FileManager {
    /// <summary>
    ///     Interaction logic for RenameWindow.xaml
    /// </summary>
    public partial class RenameWindow : INotifyPropertyChanged {
        private string _fileName;
        private bool _check;

        public RenameWindow(string str) {
            InitializeComponent();
            DataContext = this;
            _check = false;
            FileName = str;
        }

        public string FileName {
            get => _fileName;
            set {
                if (value == _fileName) return;
                _fileName = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OKBtn_OnClick(object sender, RoutedEventArgs e) {
            _check = true;
            Close();
        }

        private void CancelBtn_OnClick(object sender, RoutedEventArgs e) => Close();


        public string ReturnNewName() => FileName.Length == 0 ? null : FileName;

        private void RenameWindow_OnClosed(object sender, EventArgs e) {
            if (!_check) FileName = "";
        }


        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e) {
            if (TextBox.Text == FileName || TextBox.Text.Length == 0)
                Button.IsEnabled = false;
            else
                Button.IsEnabled = true;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}