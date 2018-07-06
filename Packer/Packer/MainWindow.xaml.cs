using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.IO;

namespace Packer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public string SelectedFolderPath { get; set; }
        public string SelectedFilePath { get; set; }
        public string Log { get; set; }

        public MainWindow()
        {
            InitializeComponent();                        
            Log = "Selected Menu...";
            setLog(null);
        }

        private void setLog(string addString)
        {
            if (addString != null)
            {
                Log += addString;
            }

            tb_log_box.Text = Log;
        }

        private void btn_file_pack_Click(object sender, RoutedEventArgs e)
        {
            setLog("\nOpen File..");
            using (System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog1.Filter = "Binary Files|*.bin";
                openFileDialog1.Title = "Select a Target File";

                if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SelectedFilePath = openFileDialog1.FileName;
                    setLog(string.Format("\nSelected File Path : {0}", SelectedFilePath));

                    YamEncrpty.Yam yam = new YamEncrpty.Yam("ABCDEFGHIJKLMNOPQR");

                    setLog("\nFile Packing...");

                    byte[] data = yam.Encode(YamEncrpty.PackUtility.readFile(SelectedFilePath));

                    string uname = SelectedFilePath.Replace(".bin", ".bz");
                    using (FileStream fs = new FileStream(uname, FileMode.Create))
                    {
                        fs.Write(data, 0, data.Length);
                    }

                    setLog("\nFile Packing Complete..Check it");
                }
            }
        }

        private void btn_folder_pack_Click(object sender, RoutedEventArgs e)
        {
            setLog("\nOpen Folder..");

            using (System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    SelectedFolderPath = folderDialog.SelectedPath;
                    setLog(string.Format("\nSelected Folder Path : {0}",SelectedFolderPath));

                    YamEncrpty.Yam yam = new YamEncrpty.Yam("ABCDEFGHIJKLMNOPQR");
                    DirectoryInfo dir = new DirectoryInfo(SelectedFolderPath);
                    FileInfo[] dirFiles = dir.GetFiles("*.bin");

                    setLog("\nFolder Files Packing...");

                    foreach (FileInfo file in dirFiles)
                    {
                        string filePath = file.FullName;

                        setLog(string.Format("\nPacking.. : {0}", filePath));

                        byte[] data = yam.Encode(YamEncrpty.PackUtility.readFile(filePath));

                        string uname = SelectedFilePath.Replace(".bin", ".bz");
                        using (FileStream fs = new FileStream(uname, FileMode.Create))
                        {
                            fs.Write(data, 0, data.Length);
                        }
                    }

                    setLog("\nFile Packing Complete..Check it");
                }
            }
        }

        private void btn_md5_number_Click(object sender, RoutedEventArgs e)
        {
            setLog("\nOpen target .bz File..");
            using (System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog1.Filter = "Pack Files|*.bz";
                openFileDialog1.Title = "Select a Target File";

                if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {                    
                    setLog(string.Format("\nSelected Pack File Path : {0}", openFileDialog1.FileName));

                    CMd5 md5 = new CMd5();
                    setLog(string.Format("\nMD5 Value : {0}", md5.GetMd5Hash(openFileDialog1.FileName)));
                }
            }
        }


    }
}
