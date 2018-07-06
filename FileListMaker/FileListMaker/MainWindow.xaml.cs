using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Collections;
using System.Windows.Forms;
using System.IO;

namespace FileListMaker
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 메인
        bool isExistOriginalFile;
        Dictionary<string, FileItem> fileItemArray = null;

        string folderPath = string.Empty;
        ArrayList fileListArray = null;

        public MainWindow()
        {
            InitializeComponent();

            if (fileItemArray == null)
            {
                fileItemArray = new Dictionary<string, FileItem>();
            }

            if (fileListArray == null)
            {
                fileListArray = new ArrayList();
            }
            BtnRun.IsEnabled = false;
            BtnSave.IsEnabled = false;
        }

        private void BtnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                DialogResult result = folderDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    folderPath = folderDialog.SelectedPath;
                    textBoxFolderPath.Text = folderPath;
                    BtnRun.IsEnabled = true;
                }
            }

            FileInfo fi = new FileInfo(Path.Combine(folderPath, "FileList_Original.txt"));
            if (fi.Exists)
            {
                using (StreamReader file = new StreamReader(Path.Combine(folderPath, "FileList_Original.txt")))
                {
                    string ver = file.ReadLine();
                    string fileCount = file.ReadLine();

                    while (!file.EndOfStream)
                    {
                        string readStr = file.ReadLine();
                        string[] splitArray = readStr.Split(',');
                        FileItem item = new FileItem(splitArray[0], splitArray[1], splitArray[2]);
                        fileItemArray.Add(item.FilePath, item);
                    }
                }

                isExistOriginalFile = true;
                if(fileItemArray == null || fileItemArray.Count <= 0)
                    isExistOriginalFile = false;
            }
            else
            {
                isExistOriginalFile = false;
            }
        }

        private void BtnResetList_Click(object sender, RoutedEventArgs e)
        {
            BtnRun.IsEnabled = false;
            BtnSave.IsEnabled = false;
            
            listBoxFileList.ItemsSource = null;

            folderPath = string.Empty;
            textBoxFolderPath.Text = folderPath;

            fileItemArray = null;
            fileItemArray = new Dictionary<string, FileItem>();

            fileListArray = null;
            fileListArray = new ArrayList();
        }

        private void BtnRun_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(folderPath))
            {
                DirectoryInfo info = new DirectoryInfo(folderPath);
                foreach (DirectoryInfo di in info.GetDirectories())
                {
                    foreach (FileInfo fi in di.GetFiles())
                    {
                        FileItem fileItem = getFileItem(fi, folderPath);
                        if (fileItem != null)
                            fileListArray.Add(fileItem);
                    }
                }
                foreach (FileInfo fi in info.GetFiles())
                {
                    FileItem fileItem = getFileItem(fi, folderPath);
                    if (fileItem != null)
                        fileListArray.Add(fileItem);
                }
                                
                listBoxFileList.ItemsSource = fileListArray;
                BtnSave.IsEnabled = true;
            }
        }

        FileItem getFileItem(FileInfo fi, string rootPath)
        {
            string filePath = fi.FullName.Replace(rootPath, "");
            string fileName = Path.GetFileName(filePath);
            if (fileName.Equals("FileList.txt") || fileName.Equals("FileList_Original.txt"))
                return null;

            string fileDate = fi.LastWriteTime.Ticks.ToString();
            string fileSize = fi.Length.ToString();

            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(fileDate) || string.IsNullOrEmpty(fileSize))
            {
                return null;
            }
            return new FileItem(filePath, fileDate, fileSize);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (fileListArray == null || fileListArray.Count <= 0)
                return;

            if (isExistOriginalFile)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(Path.Combine(folderPath, "FileList.txt")))
                {
                    foreach (FileItem item in fileListArray)
                    {                        
                        if (fileItemArray.ContainsKey(item.FilePath))
                        {
                            FileItem fi = fileItemArray[item.FilePath];
                            if (!fi.FileDate.Equals(item.FileDate) || !fi.FileSize.Equals(item.FileSize))
                            {
                                string line = string.Format("{0},{1},{2}", item.FilePath, item.FileDate, item.FileSize);
                                file.WriteLine(line);
                            }
                        }
                        else
                        {
                            string line = string.Format("{0},{1},{2}", item.FilePath, item.FileDate, item.FileSize);
                            file.WriteLine(line);
                        }
                    }
                }
            }
            else
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(Path.Combine(folderPath, "FileList_Original.txt")))
                {
                    foreach (FileItem item in fileListArray)
                    {
                        string line = string.Format("{0},{1},{2}", item.FilePath, item.FileDate, item.FileSize);
                        file.WriteLine(line);
                    }
                }

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(Path.Combine(folderPath, "FileList.txt")))
                {
                    foreach (FileItem item in fileListArray)
                    {
                        string line = string.Format("{0},{1},{2}", item.FilePath, item.FileDate, item.FileSize);
                        file.WriteLine(line);
                    }
                }
            }
            System.Windows.MessageBox.Show("Save FileList.txt Complete!");

            BtnResetList_Click(sender, e);
        }
        #endregion

        #region 파일아이템
        public class FileItem
        {
            private string filePath;

            public string FilePath
            {
                get { return filePath; }
                set { filePath = value; }
            }

            private string fileDate;

            public string FileDate
            {
                get { return fileDate; }
                set { fileDate = value; }
            }

            private string fileSize;

            public string FileSize
            {
                get { return fileSize; }
                set { fileSize = value; }
            }

            public FileItem(string path, string date, string size)
            {
                filePath = path;
                fileDate = date;
                fileSize = size;
            }
        }
        #endregion
    }
}
