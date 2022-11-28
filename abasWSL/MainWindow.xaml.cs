using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;


namespace abasWSL
{
    public static class ExtensionMethods
    {
        private static Action EmptyDelegate = delegate () { };


        public static void Refresh(this UIElement uiElement)

        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }
    }

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       
        public MainWindow()
        {
            
            InitializeComponent();
            String output = callwsl("--list --verbose", 1000);
            int i = output.IndexOf("erp");
            if (i > 0)
            {
                output = output.Substring(i, output.Length - i);
                i = output.IndexOf("1");
                output = output.Substring(0, i);
                if (output.Contains("Stopped"))
                {
                    startbtn.Content = "Start abas";
                    //inststatus.Text = "Installed";
                    startbtn.IsEnabled = true;
                    importbtn.IsEnabled = false;
                    targetbtn.IsEnabled = false;
                    sourcebtn.IsEnabled = false;
                    

                }
                if (output.Contains("Running"))
                {
                    startbtn.Content= "Stop abas";
                    //inststatus.Text = "Installed";
                    startbtn.IsEnabled = true;
                    importbtn.IsEnabled = false;
                    targetbtn.IsEnabled = false;
                    sourcebtn.IsEnabled = false;

                }
                if (output.Contains("Installing"))
                {
                    startbtn.Content = "Installing";
                    importbtn.Content = "Installing";
                    startbtn.IsEnabled = false;
                    importbtn.IsEnabled = false;
                    targetbtn.IsEnabled = false;
                    sourcebtn.IsEnabled = false;

                }

            }
            else
            {
                startbtn.Content = "Not installed";
                //inststatus.Text = "Not installed";
                startbtn.IsEnabled = false;
                importbtn.IsEnabled = true;
                targetbtn.IsEnabled= true;
                sourcebtn.IsEnabled= true;

            }
        }
       
        void OnClicksourcebtn(object sender, RoutedEventArgs e)
        {

            /*OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                tardir.Text = openFileDialog.FileName;*/


            var fileSelectorDialog = new CommonOpenFileDialog();
            fileSelectorDialog.EnsureReadOnly = true;
            fileSelectorDialog.IsFolderPicker = false;
            fileSelectorDialog.AllowNonFileSystemItems = false;
            fileSelectorDialog.Filters.Add(new CommonFileDialogFilter("Tar Files","*.tar"));
            fileSelectorDialog.Multiselect = false;
            fileSelectorDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            fileSelectorDialog.Title = "Select abas tar-Image";
            if (fileSelectorDialog.ShowDialog()==CommonFileDialogResult.Ok)
            { 
                string SelectedFolderPath = fileSelectorDialog.FileName;
                sourcebtn.Content = SelectedFolderPath;
                
            }
        }
            void OnClicktargetbtn(object sender, RoutedEventArgs e)
        {
            

            var folderSelectorDialog = new CommonOpenFileDialog();
            folderSelectorDialog.EnsureReadOnly = true;
            folderSelectorDialog.IsFolderPicker = true;
            folderSelectorDialog.AllowNonFileSystemItems = false;
            folderSelectorDialog.Multiselect = false;
            folderSelectorDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            folderSelectorDialog.Title = "Select Destination Folder";
            if (folderSelectorDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string SelectedFolderPath = folderSelectorDialog.FileName;
                targetbtn.Content = SelectedFolderPath;
            }

        }
        async void OnClick_Install(object sender, RoutedEventArgs e)
        {
            importbtn.Content = "Installing...";
            importbtn.IsEnabled = false;
            targetbtn.IsEnabled = false;
            sourcebtn.IsEnabled = false;
            object args= new object[2] {targetbtn.Content, sourcebtn.Content};
            Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle);
            //inststatus.Refresh();
            //installwslAsync("--import erp " + targetdir.Text + " " + tardir.Text + " --version 1", 0);
            
            await callwsl2("--import erp " + targetbtn.Content + " " + sourcebtn.Content + " --version 1", 0);

            importbtn.Content= "Install ready !!!";
            startbtn.IsEnabled = true;
            startbtn.Content = "Start abas";

        }
        
        public async void installwslAsync(string command, int v)
        {
                        Task x = callwsl2(command, v);
                        await x;
        }
        
        void OnClick_togglewsl(object sender, RoutedEventArgs e)
        {
            String output= callwsl("--list --verbose",0);
            int i = output.IndexOf("erp");
            output = output.Substring(i, output.Length - i);
            i = output.IndexOf("1");
            output = output.Substring(0, i);
            if (output.Contains("Stopped"))
            {//Wsl aktuell gestoppt, kann gestartet werden 
                callwsl("-d erp -u root sh /abas/bin/starteVersion.sh run",5000);
            }
            else
            {
                callwsl("--terminate erp",0);
            }
            // Status abholen
           output = callwsl("--list --verbose", 0);
            i = output.IndexOf("erp");
            output = output.Substring(i, output.Length - i);
            i = output.IndexOf("1");
            output = output.Substring(0, i);
            if (output.Contains("Stopped"))
            {
                startbtn.Content = "Start abas";
            }
            if (output.Contains("Installing"))
            {
                startbtn.Content = "Installing";
            }
            if (output.Contains("Running"))
            {
                startbtn.Content = "Stop abas";
            }
            Console.WriteLine(output);
         
        }

         string callwsl(string command, int wait)
        {

            using (var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"wsl.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                    Arguments = command
                }
            })
            { proc.Start();
              System.Threading.Thread.Sleep(500);
              proc.StandardInput.Flush();
              proc.StandardInput.Close();
                

                proc.WaitForExit(wait);
              string output = proc.StandardOutput.ReadToEnd();
              output = output.Replace("\x00", "");
                return output;

            }
        }
        async Task callwsl2(string command, int wait)
        {

            using (var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"wsl.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    //RedirectStandardInput = true,
                    CreateNoWindow = true,
                    Arguments = command
                }
            })
            {
                proc.Start();
               // System.Threading.Thread.Sleep(500);
                //proc.StandardInput.Flush();
                //proc.StandardInput.Close();
                // proc.WaitForExit(wait);
                while (!proc.HasExited)
                {
                    await Task.Delay(100);
                    Console.WriteLine("Läuft");
                }
                
                string output = proc.StandardOutput.ReadToEnd();
                output = output.Replace("\x00", "");
                

            }
           // return Task.FromResult(0);
        }










    }
}
