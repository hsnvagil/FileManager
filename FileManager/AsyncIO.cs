using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace FileManager {
    public static class AsyncIO {
        public static Task<List<FileInfo>> GetFiles(DirectoryInfo directory) {
            return Task.Factory.StartNew(() => _getFiles(directory));
        }

        public static Task<List<DirectoryInfo>> GetDirectories(string path) {
            return Task.Factory.StartNew(() => _getDirectories(path));
        }

        public static Task<bool> DeleteDirectory(string path) {
            return Task.Factory.StartNew(() => _deleteDirectory(path));
        }

        public static Task<bool> RenameDirectory(string path, string path2) {
            return Task.Factory.StartNew(() => _renameDirectory(path, path2));
        }

        public static Task<bool> RenameFile(string path, string path2) {
            return Task.Factory.StartNew(() => _renameFile(path, path2));
        }

        public static Task<bool> DeleteFile(string path) {
            return Task.Factory.StartNew(() => _deleteFile(path));
        }

        public static async Task<List<string>> MoveFiles(Dictionary<string, string> files) {
            var completedTaskList = new List<string>();
            var error = false;
            var pr = new ProgressWindow();
            var source = new CancellationTokenSource();
            pr.Closed += (o, args) => { source.CancelAfter(TimeSpan.FromMilliseconds(1)); };
            void ProgressCallback(double prof) => pr.ProgressBar.Value = prof;

            var totalSize = files.Where(file => !File.GetAttributes(file.Key).HasFlag(FileAttributes.Directory))
                .Sum(file => new FileInfo(file.Key).Length);
            pr.Show();

            long totalRead = 0;

            const double progressSize = 100.0;
            if (totalSize <= 0) {
                pr.ProgressBar.Value = 100;
            }

            foreach (var item in files) {
                long totalReadForFile = 0;

                var from = item.Key;
                var to = item.Value;

                if (File.GetAttributes(from).HasFlag(FileAttributes.Directory)) {
                    Directory.CreateDirectory(to);
                    continue;
                }


                using (var outStream = new FileStream(to, FileMode.Create, FileAccess.Write, FileShare.Read)) {
                    using (var inStream = new FileStream(from, FileMode.Open, FileAccess.ReadWrite, FileShare.Read)) {
                        try {
                            var read = totalRead;
                            await CopyStream(inStream, outStream, x => {
                                totalReadForFile = x;
                                ProgressCallback(
                                    (read + totalReadForFile) /
                                    (double) totalSize * progressSize);
                            }, source.Token).ContinueWith((task) => {
                                inStream.Dispose();
                                inStream.Close();
                                File.Delete(inStream.Name);
                                completedTaskList.Add($"{from} moved {to} {DateTime.Now}");
                            }, source.Token);
                        } catch (OperationCanceledException) {
                            outStream.Dispose();
                            outStream.Close();
                            if (File.Exists(to)) File.Delete(to);
                            error = true;
                            pr.Close();
                            break;
                        } catch (Exception e) {
                            MessageBox.Show(e.Message);
                            error = true;
                        }
                    }
                }

                totalRead += totalReadForFile;
            }

            if (!error) {
                foreach (var file in files) {
                    if (Directory.Exists(file.Key) && File.GetAttributes(file.Key).HasFlag(FileAttributes.Directory)) {
                        Directory.Delete(file.Key, true);
                    }

                    break;
                }
            }

            return completedTaskList;
        }

        public static async Task<List<string>> EncryptFile(List<string> list) {
            var completedTaskList = new List<string>();
            var pr = new ProgressWindow();
            var source = new CancellationTokenSource();
            pr.Closed += (sender, args) => { source.CancelAfter(TimeSpan.FromMilliseconds(1)); };
            void ProgressCallback(double prof) => pr.ProgressBar.Value = prof;

            var totalSize = list.Select(x => new FileInfo(x).Length).Sum();
            pr.Show();
            long totalRead = 0;
            const double progressSize = 100.00;
            if (totalSize <= 0) {
                pr.ProgressBar.Value = 100;
            }


            foreach (var item in list) {
                long totalReadForFile = 0;
                var path = item;
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)) {
                    var length = (int) stream.Length;
                    var tempBytes = new byte[length];
                    await stream.ReadAsync(tempBytes, 0, length, source.Token);
                    try {
                        var read = totalRead;
                        await EncryptStream(stream, x => {
                                totalReadForFile = x;
                                ProgressCallback((read + totalReadForFile) / (double) totalSize * progressSize);
                            }, source.Token)
                            .ContinueWith((task) => completedTaskList.Add($"{path} encrypted {DateTime.Now}"),
                                          source.Token);
                    } catch (OperationCanceledException) {
                        stream.Seek(0, 0);
                        await stream.WriteAsync(tempBytes, 0, length);
                        pr.Close();
                        break;
                    } catch (Exception e) {
                        MessageBox.Show(e.Message);
                    }
                }

                totalRead += totalReadForFile;
            }

            return completedTaskList;
        }

        public static async Task<List<string>> DecryptFile(List<string> list) {
            var completedTaskList = new List<string>();
            var pr = new ProgressWindow();
            var source = new CancellationTokenSource();
            pr.Closed += (sender, args) => { source.CancelAfter(TimeSpan.FromMilliseconds(1)); };
            void ProgressCallback(double prof) => pr.ProgressBar.Value = prof;

            var totalSize = list.Select(x => new FileInfo(x).Length).Sum();
            pr.Show();
            long totalRead = 0;
            const double progressSize = 100.00;
            if (totalSize <= 0) {
                pr.ProgressBar.Value = 100;
            }


            foreach (var item in list) {
                long totalReadForFile = 0;
                var path = item;
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)) {
                    var length = (int) stream.Length;
                    var tempBytes = new byte[length];
                    await stream.ReadAsync(tempBytes, 0, length, source.Token);
                    try {
                        var read = totalRead;
                        await DecryptStream(stream, x => {
                                totalReadForFile = x;
                                ProgressCallback((read + totalReadForFile) / (double) totalSize * progressSize);
                            }, source.Token)
                            .ContinueWith((task) => completedTaskList.Add($"{path} decrypted {DateTime.Now}"),
                                          source.Token);
                    } catch (OperationCanceledException) {
                        stream.Seek(0, 0);
                        await stream.WriteAsync(tempBytes, 0, length);
                        pr.Close();
                        break;
                    } catch (Exception e) {
                        MessageBox.Show(e.Message);
                    }
                }

                totalRead += totalReadForFile;
            }

            return completedTaskList;
        }

        public static async Task<List<string>> CopyFiles(Dictionary<string, string> files) {
            var completedTaskList = new List<string>();
            var pr = new ProgressWindow();
            var source = new CancellationTokenSource();
            pr.Closed += (o, args) => { source.CancelAfter(TimeSpan.FromMilliseconds(1)); };
            void ProgressCallback(double prof) => pr.ProgressBar.Value = prof;

            var totalSize = files.Where(file => !File.GetAttributes(file.Key).HasFlag(FileAttributes.Directory))
                .Sum(file => new FileInfo(file.Key).Length);
            pr.Show();

            long totalRead = 0;

            const double progressSize = 100.0;
            if (totalSize <= 0) {
                pr.ProgressBar.Value = 100;
            }

            foreach (var item in files) {
                long totalReadForFile = 0;

                var from = item.Key;
                var to = item.Value;

                if (File.GetAttributes(from).HasFlag(FileAttributes.Directory)) {
                    Directory.CreateDirectory(to);
                    continue;
                }


                using (var outStream = new FileStream(to, FileMode.Create, FileAccess.Write, FileShare.Read)) {
                    using (var inStream = new FileStream(from, FileMode.Open, FileAccess.ReadWrite, FileShare.Read)) {
                        try {
                            var read = totalRead;
                            await CopyStream(inStream, outStream, x => {
                                    totalReadForFile = x;
                                    ProgressCallback(
                                        (read + totalReadForFile) /
                                        (double) totalSize * progressSize);
                                }, source.Token)
                                .ContinueWith(task => completedTaskList.Add($"{from} copied to {to} {DateTime.Now}"),
                                              source.Token);
                        } catch (OperationCanceledException) {
                            outStream.Dispose();
                            outStream.Close();
                            if (File.Exists(to)) File.Delete(to);
                            pr.Close();
                            break;
                        } catch (Exception e) {
                            MessageBox.Show(e.Message);
                        }
                    }
                }

                totalRead += totalReadForFile;
            }

            return completedTaskList;
        }

        private static async Task CopyStream(Stream from, Stream to, Action<long> progress, CancellationToken source) {
            const int bufferSize = 10240;

            var buffer = new byte[bufferSize];

            long totalRead = 0;

            while (totalRead < from.Length) {
                var read = await from.ReadAsync(buffer, 0, bufferSize, source);

                await to.WriteAsync(buffer, 0, read, source);

                totalRead += read;

                progress(totalRead);
            }
        }

        private static async Task EncryptStream(Stream stream, Action<long> progress, CancellationToken source) {
            const int bufferSize = 10240;


            long totalRead = 0;

            while (totalRead < stream.Length) {
                var buffer = new byte[bufferSize];
                var read = await stream.ReadAsync(buffer, 0, bufferSize, source);

                for (var i = totalRead; i < buffer.Length; i++) {
                    buffer[i] += 0x11;
                }

                stream.Seek(totalRead, 0);
                await stream.WriteAsync(buffer, 0, read, source);

                totalRead += read;
                progress(totalRead);
            }
        }

        private static async Task DecryptStream(Stream stream, Action<long> progress, CancellationToken source) {
            const int bufferSize = 10240;


            long totalRead = 0;

            while (totalRead < stream.Length) {
                var buffer = new byte[bufferSize];
                var read = await stream.ReadAsync(buffer, 0, bufferSize, source);

                for (var i = totalRead; i < buffer.Length; i++) {
                    buffer[i] -= 0x11;
                }

                stream.Seek(totalRead, 0);
                await stream.WriteAsync(buffer, 0, read, source);

                totalRead += read;
                progress(totalRead);
            }
        }

        public static Task<List<DriveInfo>> GetDrives() {
            return Task.Factory.StartNew(_getDrives);
        }

        private static List<DriveInfo> _getDrives() {
            try {
                return DriveInfo.GetDrives().ToList();
            } catch (Exception e) {
                MessageBox.Show(e.Message);
            }

            return null;
        }

        private static List<DirectoryInfo> _getDirectories(string path) {
            try {
                var directory = new DirectoryInfo(path);
                return directory.GetDirectories().ToList();
            } catch (Exception e) {
                MessageBox.Show(e.Message);
            }

            return null;
        }

        private static List<FileInfo> _getFiles(DirectoryInfo directory) {
            try {
                return directory.GetFiles().ToList();
            } catch (Exception e) {
                MessageBox.Show(e.Message);
            }

            return null;
        }

        public static Dictionary<string, string> CreateDirectories(string sourceDirName, string destDirName,
                                                                   Dictionary<string, string> dictionary) {
            if (dictionary == null) {
                dictionary = new Dictionary<string, string>();
            }

            var dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);

            var dirs = dir.GetDirectories();
            if (!Directory.Exists(destDirName)) dictionary.Add(sourceDirName, destDirName);

            var files = dir.GetFiles();
            foreach (var file in files) {
                var tempPath = Path.Combine(destDirName, file.Name);
                dictionary.Add(file.FullName, tempPath);
            }

            foreach (var subDir in dirs) {
                var tempPath = Path.Combine(destDirName, subDir.Name);
                CreateDirectories(subDir.FullName, tempPath, dictionary);
            }

            return dictionary;
        }

        private static bool _deleteFile(string path) {
            try {
                File.Delete(path);
                return true;
            } catch (Exception e) {
                MessageBox.Show(e.Message);
            }

            return false;
        }

        private static bool _deleteDirectory(string path) {
            try {
                Directory.Delete(path, true);
                return true;
            } catch (Exception e) {
                MessageBox.Show(e.Message);
            }

            return false;
        }

        private static bool _renameFile(string sourceFileName, string destFileName) {
            try {
                File.Move(sourceFileName, destFileName);
                return true;
            } catch (Exception e) {
                MessageBox.Show(e.Message);
            }

            return false;
        }

        private static bool _renameDirectory(string sourceDirName, string destDirName) {
            try {
                Directory.Move(sourceDirName, destDirName);
                return true;
            } catch (Exception e) {
                MessageBox.Show(e.Message);
            }

            return false;
        }
    }
}