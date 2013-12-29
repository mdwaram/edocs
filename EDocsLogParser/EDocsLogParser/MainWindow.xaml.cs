﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
using System.Windows.Shapes;

namespace EDocsLog {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        string InputPath { get; set; }

        string OutputPath {
            get {
                return InputPath;
            }
        }

        private string[] LoadLogFiles() {
            return Directory.EnumerateFiles(InputPath, "*.log", SearchOption.TopDirectoryOnly).ToArray();
        }

        public void ParseLog(string fileName) {
            var log = new LogFile { FileName = fileName };
            var parser = new LogParser { Log = log };
            parser.Parse();

            string file = System.IO.Path.GetFileNameWithoutExtension(fileName);
            string xmlFile = System.IO.Path.Combine(OutputPath, file + ".xml");
            //var toSave = parser.Events.Where(EventFilters.IsSqlSlow).ToList();
            var toSave = parser.Events; // save all
            EventScanner.SerializeToXml<List<BaseEvent>>(toSave, xmlFile);
        }

        string[] files;

        private void btnStart_Click(object sender, RoutedEventArgs e) {
            files = LoadLogFiles();
            btnStart.IsEnabled = false;
            progressMain.Maximum = files.Length;
            progressMain.Value = 0;

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += ProcessAllFiles;
            worker.ProgressChanged += (s, args) => progressMain.Value = args.ProgressPercentage;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        private void ProcessAllFiles(object s, DoWorkEventArgs args) {
            int fileNo = 0;
            foreach(var file in files) {
                ParseLog(file);
                ((BackgroundWorker)s).ReportProgress(++fileNo);
            }
        }

        private void worker_RunWorkerCompleted(object s, RunWorkerCompletedEventArgs args) {
            btnStart.IsEnabled = true;
            MessageBox.Show("Done.");
        }

        private void btnFolder_Click(object sender, RoutedEventArgs e) {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                InputPath = dlg.SelectedPath;
                lblFolder.Content = InputPath;
                btnStart.IsEnabled = true;
            }
        }
    }
}
