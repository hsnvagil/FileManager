using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FileManager.Annotations;

namespace FileManager {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged {
        private readonly MyFileInfo _leftItemList;
        private readonly MyFileInfo _rightItemList;
        private string _leftItemPath;
        private string _rightItemPath;
        private int _selectedIndex;
        private int _selectedIndex2;
        private bool _showHiddenFiles;
        private bool _showReadOnlyFiles;

        public MainWindow() {
            InitializeComponent();
            DataContext = this;
            _leftItemList = new MyFileInfo();
            _rightItemList = new MyFileInfo();
            Initialize();
        }

        public string LeftItemPath {
            get => _leftItemPath;
            set {
                if (value == _leftItemPath) return;
                _leftItemPath = value;
                OnPropertyChanged();
            }
        }

        public string RightItemPath {
            get => _rightItemPath;
            set {
                if (value == _rightItemPath) return;
                _rightItemPath = value;
                OnPropertyChanged();
            }
        }

        public int SelectedIndex {
            get => _selectedIndex;
            set {
                if (value == _selectedIndex) return;
                _selectedIndex = value;
                OnPropertyChanged();
            }
        }

        public int SelectedIndex2 {
            get => _selectedIndex2;
            set {
                if (value == _selectedIndex2) return;
                _selectedIndex2 = value;
                OnPropertyChanged();
            }
        }

        public bool ShowHiddenFiles {
            get => _showHiddenFiles;
            set {
                if (value == _showHiddenFiles) return;
                _showHiddenFiles = value;
                OnPropertyChanged();
            }
        }

        public bool ShowReadOnlyFiles {
            get => _showReadOnlyFiles;
            set {
                if (value == _showReadOnlyFiles) return;
                _showReadOnlyFiles = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void Initialize() {
            var allDrives = await AsyncIO.GetDrives();
            foreach (var driveInfo in allDrives) {
                ComboBox.Items.Add(driveInfo.Name);
                ComboBox2.Items.Add(driveInfo.Name);
            }

            ComboBox.SelectedIndex = 0;
            ComboBox2.SelectedIndex = 0;
            LeftItemPath = ComboBox.SelectedItem.ToString();
            RightItemPath = ComboBox2.SelectedItem.ToString();

            ShowHiddenFiles = false;
            ShowReadOnlyFiles = true;
        }

        private List<string> GetSelectedItems() {
            var selectedItems = ListView.SelectedItems;
            var itemsIndex = new List<int>();
            foreach (var o in selectedItems)
                for (var index = 0; index < ListView.Items.Count; index++) {
                    var listViewItem = ListView.Items[index];
                    if (listViewItem.ToString() == o.ToString()) itemsIndex.Add(index);
                }

            itemsIndex.Sort();

            return itemsIndex.Select(i => _leftItemList.Files[i]).ToList();
        }

        private async void ViewLeftItemList() {
            try {
                var directory = new DirectoryInfo(_leftItemList.Root);
                var directoryInfos = await AsyncIO.GetDirectories(_leftItemList.Root);
                var fileInfos = await AsyncIO.GetFiles(directory);

                _leftItemList.Directories = directoryInfos.Where(
                        d => !d.Attributes.HasFlag(FileAttributes.Hidden) &&
                             !d.Attributes.HasFlag(FileAttributes.ReadOnly))
                    .Select(d => d.FullName).ToList();
                _leftItemList.Files = fileInfos
                    .Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden) &&
                                !f.Attributes.HasFlag(FileAttributes.ReadOnly))
                    .Select(f => f.FullName).ToList();

                if (ShowHiddenFiles) {
                    foreach (var directoryInfo in directoryInfos.Where(
                        directoryInfo => directoryInfo.Attributes.HasFlag(FileAttributes.Hidden)))
                        _leftItemList.Directories.Add(directoryInfo.FullName);

                    foreach (var fileInfo in fileInfos.Where(
                        fileInfo => fileInfo.Attributes.HasFlag(FileAttributes.Hidden)))
                        _leftItemList.Files.Add(fileInfo.FullName);
                }

                if (ShowReadOnlyFiles) {
                    foreach (var directoryInfo in directoryInfos.Where(
                        directoryInfo => directoryInfo.Attributes.HasFlag(FileAttributes.ReadOnly)))
                        _leftItemList.Directories.Add(directoryInfo.FullName);

                    foreach (var fileInfo in fileInfos.Where(
                        fileInfo => fileInfo.Attributes.HasFlag(FileAttributes.ReadOnly)))
                        _leftItemList.Files.Add(fileInfo.FullName);
                }

                foreach (var fileCollectionDirectory in _leftItemList.Directories)
                    _leftItemList.Files.Add(fileCollectionDirectory);
                _leftItemList.Files.Sort();
                _leftItemList.Files.Insert(0, "..");
                Dispatcher?.Invoke(() => {
                    ListView.Items.Clear();
                    foreach (var file in _leftItemList.Files) ListView.Items.Add(Path.GetFileName(file));
                });
                SelectedIndex = 1;
                LeftItemPath = _leftItemList.Root;
            } catch (Exception e) {
                MessageBox.Show(e.Message);
            }
        }

        private async void ViewRightItemList() {
            try {
                var directory = new DirectoryInfo(_rightItemList.Root);
                var directoryInfos = await AsyncIO.GetDirectories(_rightItemList.Root);
                var fileInfos = await AsyncIO.GetFiles(directory);

                _rightItemList.Directories = directoryInfos.Where(
                        d => !d.Attributes.HasFlag(FileAttributes.Hidden) &&
                             !d.Attributes.HasFlag(FileAttributes.ReadOnly))
                    .Select(d => d.FullName).ToList();
                _rightItemList.Files = fileInfos
                    .Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden) &&
                                !f.Attributes.HasFlag(FileAttributes.ReadOnly))
                    .Select(f => f.FullName).ToList();

                if (ShowHiddenFiles) {
                    foreach (var directoryInfo in directoryInfos.Where(
                        directoryInfo => directoryInfo.Attributes.HasFlag(FileAttributes.Hidden)))
                        _rightItemList.Directories.Add(directoryInfo.FullName);

                    foreach (var fileInfo in fileInfos.Where(
                        fileInfo => fileInfo.Attributes.HasFlag(FileAttributes.Hidden)))
                        _rightItemList.Files.Add(fileInfo.FullName);
                }

                if (ShowReadOnlyFiles) {
                    foreach (var directoryInfo in directoryInfos.Where(
                        directoryInfo => directoryInfo.Attributes.HasFlag(FileAttributes.ReadOnly)))
                        _rightItemList.Directories.Add(directoryInfo.FullName);

                    foreach (var fileInfo in fileInfos.Where(
                        fileInfo => fileInfo.Attributes.HasFlag(FileAttributes.ReadOnly)))
                        _rightItemList.Files.Add(fileInfo.FullName);
                }

                foreach (var fileCollection2Directory in _rightItemList.Directories)
                    _rightItemList.Files.Add(fileCollection2Directory);

                _rightItemList.Files.Sort();
                _rightItemList.Files.Insert(0, "..");
                Dispatcher?.Invoke(() => {
                    ListView2.Items.Clear();
                    foreach (var file in _rightItemList.Files) ListView2.Items.Add(Path.GetFileName(file));
                });
                SelectedIndex2 = 1;
                RightItemPath = _rightItemList.Root;
            } catch (Exception e) {
                MessageBox.Show(e.Message);
            }
        }

        private void ComboBox2_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            _rightItemList.Root = ComboBox2.SelectedItem.ToString();
            ViewRightItemList();
        }

        private void ComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            _leftItemList.Root = ComboBox.SelectedItem.ToString();
            ViewLeftItemList();
        }

        private void MenuItem_OnChecked(object sender, RoutedEventArgs e) {
            ShowHiddenFiles = true;
            ViewLeftItemList();
            ViewRightItemList();
        }

        private void MenuItem_OnUnchecked(object sender, RoutedEventArgs e) {
            ShowHiddenFiles = false;
            ViewLeftItemList();
            ViewRightItemList();
        }

        private void MenuItem_OnUnchecked1(object sender, RoutedEventArgs e) {
            ShowReadOnlyFiles = false;
            ViewLeftItemList();
            ViewRightItemList();
        }

        private void MenuItem_OnChecked1(object sender, RoutedEventArgs e) {
            ShowReadOnlyFiles = true;
            ViewLeftItemList();
            ViewRightItemList();
        }

        private void EventSetter_OnHandler(object sender, MouseButtonEventArgs e) {
            var item = _leftItemList.Files[SelectedIndex];
            if (SelectedIndex == 0) {
                if (!_leftItemList.Root.Substring(_leftItemList.Root.Length - 2).Equals(":\\")) {
                    _leftItemList.Root = Directory.GetParent(_leftItemList.Root).ToString();
                    ViewLeftItemList();
                }
            } else if (File.GetAttributes(item).HasFlag(FileAttributes.Directory)) {
                _leftItemList.Root = _leftItemList.Files[SelectedIndex];
                ViewLeftItemList();
            } else {
                try {
                    Process.Start(_leftItemList.Files[SelectedIndex]);
                } catch (Exception exception) {
                    MessageBox.Show(exception.Message);
                }
            }
        }

        private void EventSetter_OnHandler2(object sender, MouseButtonEventArgs e) {
            var item = _rightItemList.Files[SelectedIndex2];
            if (SelectedIndex2 == 0) {
                if (!_rightItemList.Root.Substring(_rightItemList.Root.Length - 2).Equals(":\\")) {
                    _rightItemList.Root = Directory.GetParent(_rightItemList.Root).ToString();
                    ViewRightItemList();
                }
            } else if (File.GetAttributes(item).HasFlag(FileAttributes.Directory)) {
                _rightItemList.Root = _rightItemList.Files[SelectedIndex2];
                ViewRightItemList();
            } else {
                try {
                    Process.Start(_rightItemList.Files[SelectedIndex2]);
                } catch (Exception exception) {
                    MessageBox.Show(exception.Message);
                }
            }
        }

        private async void DeleteBtn_OnClick(object sender, RoutedEventArgs e) {
            var r =
                MessageBox.Show("Do you want to delete this file?", "Confirmation", MessageBoxButton.OKCancel,
                                MessageBoxImage.Question);
            if (r == MessageBoxResult.Cancel) return;

            foreach (var deletedFile in GetSelectedItems()) {
                var tmpPath = LeftItemPath;
                bool result;

                if (File.GetAttributes(deletedFile).HasFlag(FileAttributes.Directory))
                    result = await AsyncIO.DeleteDirectory(deletedFile);
                else
                    result = await AsyncIO.DeleteFile(deletedFile);

                if (result) {
                    HistoryCollection.Items.Add($"{deletedFile} deleted {DateTime.Now}");
                    if (LeftItemPath == tmpPath) ViewLeftItemList();

                    if (RightItemPath == tmpPath) ViewRightItemList();
                }
            }
        }

        private async void CopyBtn_OnClick(object sender, RoutedEventArgs e) {
            var dictionary = new Dictionary<string, string>();
            foreach (var copiedFile in GetSelectedItems()) {
                var path2 = Path.Combine(RightItemPath, Path.GetFileName(copiedFile));
                if (File.GetAttributes(copiedFile).HasFlag(FileAttributes.Directory)) {
                    if (path2 == copiedFile) {
                        MessageBox.Show("The destination folder is the same as the source folder",
                                        "Interrupted Action");
                        continue;
                    }

                    if (Directory.Exists(path2)) {
                        var w = new ReplaceOrSkipWindow();
                        w.SetName(Path.GetFileName(path2));
                        if (w.ShowDialog() == false) {
                            if (!w.Return()) continue;
                            try {
                                Directory.Delete(path2, true);
                            } catch (Exception exception) {
                                MessageBox.Show(exception.Message);
                            }
                        }
                    }

                    var tempDic = AsyncIO.CreateDirectories(copiedFile, path2, null);
                    foreach (var z in tempDic) {
                        dictionary.Add(z.Key, z.Value);
                    }
                } else {
                    if (path2 == copiedFile) {
                        MessageBox.Show("The source and destination file names are the same",
                                        "Interrupted Action");
                        continue;
                    }

                    if (File.Exists(path2)) {
                        var w = new ReplaceOrSkipWindow();
                        w.SetName(Path.GetFileName(path2));
                        if (w.ShowDialog() == false) {
                            if (!w.Return()) continue;
                            try {
                                File.Delete(path2);
                            } catch (Exception exception) {
                                MessageBox.Show(exception.Message);
                            }
                        }
                    }

                    dictionary.Add(copiedFile, path2);
                }
            }

            dictionary
                = dictionary.OrderByDescending(d => File.GetAttributes(d.Key).HasFlag(FileAttributes.Directory))
                    .ToDictionary(v => v.Key, v => v.Value);


            if (dictionary.Count > 0) {
                var task = AsyncIO.CopyFiles(dictionary);
                await task.ContinueWith((t) => {
                    ViewLeftItemList();
                    ViewRightItemList();
                });
                var list = task.Result;
                foreach (var variable in list) {
                    HistoryCollection.Items.Add(variable);
                }
            }
        }

        private async void MoveBtn_OnClick(object sender, RoutedEventArgs e) {
            var dictionary = new Dictionary<string, string>();
            foreach (var movedFile in GetSelectedItems()) {
                var path2 = Path.Combine(RightItemPath, Path.GetFileName(movedFile));
                if (File.GetAttributes(movedFile).HasFlag(FileAttributes.Directory)) {
                    if (path2 == movedFile) {
                        MessageBox.Show("The destination folder is the same as the source folder",
                                        "Interrupted Action");
                        continue;
                    }

                    if (movedFile == Path.GetDirectoryName(path2)) {
                        MessageBox.Show("The destination folder is a sub folder of the source folder",
                                        "Interrupted Action");
                        continue;
                    }

                    if (Directory.Exists(path2)) {
                        var w = new ReplaceOrSkipWindow();
                        w.SetName(Path.GetFileName(path2));
                        if (w.ShowDialog() == false) {
                            if (!w.Return()) continue;
                            try {
                                Directory.Delete(path2, true);
                            } catch (Exception exception) {
                                MessageBox.Show(exception.Message);
                            }
                        }
                    }

                    var tempDic = AsyncIO.CreateDirectories(movedFile, path2, null);
                    foreach (var z in tempDic) {
                        dictionary.Add(z.Key, z.Value);
                    }
                } else {
                    if (path2 == movedFile) {
                        MessageBox.Show("The source and destination file names are the same",
                                        "Interrupted Action");
                        continue;
                    }

                    if (File.Exists(path2)) {
                        var w = new ReplaceOrSkipWindow();
                        w.SetName(Path.GetFileName(path2));
                        if (w.ShowDialog() == false) {
                            if (!w.Return()) continue;
                            try {
                                File.Delete(path2);
                            } catch (Exception exception) {
                                MessageBox.Show(exception.Message);
                            }
                        }
                    }

                    dictionary.Add(movedFile, path2);
                }
            }

            dictionary
                = dictionary.OrderByDescending(d => File.GetAttributes(d.Key).HasFlag(FileAttributes.Directory))
                    .ToDictionary(v => v.Key, v => v.Value);


            if (dictionary.Count > 0) {
                var task = AsyncIO.MoveFiles(dictionary);
                await task.ContinueWith((t) => {
                    ViewLeftItemList();
                    ViewRightItemList();
                });
                var list = task.Result;
                foreach (var variable in list) {
                    HistoryCollection.Items.Add(variable);
                }
            }
        }

        private async void RenameBtn_OnClick(object sender, RoutedEventArgs e) {
            var window = new RenameWindow(Path.GetFileNameWithoutExtension(_leftItemList.Files[SelectedIndex]));
            var tmpPath = LeftItemPath;
            if (window.ShowDialog() == false) {
                var str = window.ReturnNewName();
                if (str != null) {
                    var newPath = Path.Combine(LeftItemPath, str);
                    var list = GetSelectedItems();
                    var count = list.Count;
                    var index = 0;
                    if (count > 1) {
                        for (var j = 0; j < count; j++) {
                            var i = j + 1;
                            if (Directory.Exists(newPath + i)) {
                                count++;
                                continue;
                            }

                            if (File.Exists(newPath + i + Path.GetExtension(list[index]))) {
                                count++;
                                continue;
                            }

                            var item = list[index];
                            index++;
                            bool result;
                            if (File.GetAttributes(item).HasFlag(FileAttributes.Directory)) {
                                result = await AsyncIO.RenameDirectory(item, newPath + i);
                            } else {
                                var ext = Path.GetExtension(item);
                                result = await AsyncIO.RenameFile(item, newPath + i + ext);
                            }

                            if (result) {
                                HistoryCollection.Items.Add($"{item} rename {newPath} {DateTime.Now}");
                                if (LeftItemPath == tmpPath) ViewLeftItemList();

                                if (RightItemPath == tmpPath) ViewRightItemList();
                            }
                        }
                    } else {
                        bool result;
                        var path = _leftItemList.Files[SelectedIndex];
                        if (File.GetAttributes(path).HasFlag(FileAttributes.Directory)) {
                            result = await AsyncIO.RenameDirectory(path, newPath);
                        } else {
                            var ext = Path.GetExtension(path);
                            result = await AsyncIO.RenameFile(path, newPath + ext);
                        }


                        if (result) {
                            HistoryCollection.Items.Add($"{path} rename {newPath} {DateTime.Now}");
                            if (LeftItemPath == tmpPath) ViewLeftItemList();
                            if (RightItemPath == tmpPath) ViewRightItemList();
                        }
                    }
                }
            }
        }

        private async void EncryptBtn_OnClick(object sender, RoutedEventArgs e) {
            var list = new List<string>();
            foreach (var selectedItem in GetSelectedItems()) {
                if (File.GetAttributes(selectedItem).HasFlag(FileAttributes.Directory)) {
                    list.AddRange(Directory.EnumerateFiles(selectedItem, "*.*", SearchOption.AllDirectories));
                } else {
                    list.Add(selectedItem);
                }
            }

            if (list.Count > 0) {
                var collection = await AsyncIO.EncryptFile(list);
                foreach (var variable in collection) {
                    HistoryCollection.Items.Add(variable);
                }
            }
        }

        private async void DecryptBtn_OnClick(object sender, RoutedEventArgs e) {
            var list = new List<string>();
            foreach (var selectedItem in GetSelectedItems()) {
                if (File.GetAttributes(selectedItem).HasFlag(FileAttributes.Directory)) {
                    list.AddRange(Directory.EnumerateFiles(selectedItem, "*.*", SearchOption.AllDirectories));
                } else {
                    list.Add(selectedItem);
                }
            }

            if (list.Count > 0) {
                var collection = await AsyncIO.DecryptFile(list);
                foreach (var variable in collection) {
                    HistoryCollection.Items.Add(variable);
                }
            }
        }

        private void OpenFileExp_OnClick(object sender, RoutedEventArgs e) {
            try {
                Process.Start("explorer.exe", LeftItemPath);
            } catch (Exception exception) {
                MessageBox.Show(exception.Message);
            }
        }

        private async void SizeBtn_OnClick(object sender, RoutedEventArgs e) {
            var sizeWindow = new SizeWindow();
            long size = 0;
            sizeWindow.Show();
            sizeWindow.TextBlock.Text = "Loading";
            try {
                foreach (var selectedItem in GetSelectedItems())
                    if (File.GetAttributes(selectedItem).HasFlag(FileAttributes.Directory)) {
                        var directory = new DirectoryInfo(selectedItem);
                        size += await Task.Run(() => directory.EnumerateFiles("*", SearchOption.AllDirectories)
                                                   .Sum(f => f.Length));
                    } else {
                        var fileInfo = new FileInfo(selectedItem);
                        size += await Task.Run(() => fileInfo.Length);
                    }

                sizeWindow.SetSize(size);
            } catch (Exception exception) {
                MessageBox.Show(exception.Message);
                sizeWindow.Close();
            }
        }

        private void Refresh_OnClick(object sender, RoutedEventArgs e) {
            ViewLeftItemList();
        }

        private void ExitBtn_OnClick(object sender, RoutedEventArgs e) {
            Close();
        }

        private void AboutBtn_OnClick(object sender, RoutedEventArgs e) {
            var w = new AboutWindow();
            w.ShowDialog();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}