using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using System.Windows.Media.Imaging;


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
            
            Readwsl();
            importbtn.IsEnabled= false;
            sourcebtn.IsEnabled = false;


           
        }
       void Readwsl()
        {
            String output = callwsl("--list --verbose", 1000);
            //1. zeile weg
            int lineend = output.IndexOf("\n");
            output = output.Substring(lineend + 1);
            imagecombo.Items.Clear();
            while (output.Contains("\n"))
            {
                lineend = output.IndexOf("\n");
                string line = output.Substring(0, lineend);
                string imagename = line.Substring(2, line.Length - 2);
                imagename = imagename.Substring(0, imagename.IndexOf(" "));
                imagecombo.Items.Add(imagename);
                if (imagecombo.Items.Count == 1)
                {
                    if (line.Contains("Stopped"))
                    {
                        startbtn.Content = "Start Image"; ;
                        startbtn.IsEnabled = true;
                    }
                    if (line.Contains("Running"))
                    {
                        startbtn.Content = "Stop Image"; ;
                        startbtn.IsEnabled = true;
                    }
                    if (line.Contains("Installing"))
                    {
                        startbtn.Content = "Installing"; ;
                        startbtn.IsEnabled = true;
                    }
                }
                lineend = output.IndexOf("\n");
                output = output.Substring(lineend + 1);
            }
            if (imagecombo.Items != null)
            {
                imagecombo.SelectedIndex = 0;
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
                importbtn.IsEnabled = true;
                importbtn.Content = "Install image";
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
                sourcebtn.IsEnabled = true;
            }


        }
        void OnSelectionChanged_imagecombo(object sender, RoutedEventArgs e)
        {
            if (imagecombo.Items !=null && imagecombo.SelectedValue!=null)
            {
                uninstallbtn.Content = "Uninstall " + imagecombo.SelectedValue.ToString();   
            String output = callwsl("--list --verbose", 1000);
            //1. zeile weg
            int lineend = output.IndexOf("\n");
            output = output.Substring(lineend + 1);
            
            while (output.Contains("\n"))
            {
                lineend = output.IndexOf("\n");
                string line = output.Substring(0, lineend);
                string imagename = line.Substring(2, line.Length - 2);
                imagename = imagename.Substring(0, imagename.IndexOf(" "));
                if (imagecombo.SelectedValue.ToString()==imagename)
                { 
                  if (line.Contains("Stopped"))
                    {
                        startbtn.Content = "Start Image"; ;
                        startbtn.IsEnabled = true;
                    }
                    if (line.Contains("Running"))
                    {
                        startbtn.Content = "Stop Image"; ;
                        startbtn.IsEnabled = true;
                    }
                    if (line.Contains("Installing"))
                    {
                        startbtn.Content = "Installing"; ;
                        startbtn.IsEnabled = true;
                    }
                }
                lineend = output.IndexOf("\n");
                output = output.Substring(lineend + 1);
            }
            uninstallbtn.IsEnabled= true;
            }

        }
        async void OnClick_Install(object sender, RoutedEventArgs e)
        {
            string imagestr;
            importbtn.Content = "Installing...";
            importbtn.IsEnabled = false;
            targetbtn.IsEnabled = false;
            sourcebtn.IsEnabled = false;
            uninstallbtn.IsEnabled = false;
            object args= new object[2] {targetbtn.Content, sourcebtn.Content};
            Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle);
            //inststatus.Refresh();
            //installwslAsync("--import erp " + targetdir.Text + " " + tardir.Text + " --version 1", 0);
            imagestr = sourcebtn.Content.ToString();
            int backslash = imagestr.LastIndexOf("\\")+1;
            imagestr = imagestr.Substring(backslash, imagestr.Length - backslash);
            imagestr =imagestr.Substring(0,imagestr.IndexOf("."));
            await callwsl2("--import "+imagestr + " " + targetbtn.Content + "  " + sourcebtn.Content , 0);

            importbtn.Content= "Install ready !!!";
            //startbtn.Content = "Start abas";
            Readwsl();
            targetbtn.IsEnabled = true;
            targetbtn.Content = "Choose Target Directory";
            sourcebtn.Content = "Choose Tar Image";





        }
        async void OnClick_Uninstall(object sender, RoutedEventArgs e)
        {
            uninstallbtn.Content = "Uninstalling...";
            uninstallbtn.IsEnabled = false;
            startbtn.IsEnabled = false;
            importbtn.IsEnabled = false;
            targetbtn.IsEnabled = false;
            sourcebtn.IsEnabled = false;
            object args = new object[2] { targetbtn.Content, sourcebtn.Content };
            Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle);
            //inststatus.Refresh();
            //installwslAsync("--import erp " + targetdir.Text + " " + tardir.Text + " --version 1", 0);

            await callwsl2("--unregister "+imagecombo.SelectedValue.ToString(), 0);

            uninstallbtn.Content = "Uninstalled!!!";
            
            
            Readwsl();
            targetbtn.IsEnabled = true;


        }

        public async void installwslAsync(string command, int v)
        {
                        Task x = callwsl2(command, v);
                        await x;
        }
        
        void OnClick_togglewsl(object sender, RoutedEventArgs e)
        {
            String output= callwsl("--list --verbose",0);
            int i = output.IndexOf(" "+imagecombo.SelectedValue.ToString()+" ");   
            output = output.Substring(i, output.Length - i);
            i = output.IndexOf("2\r\n");
            output = output.Substring(0, i);
            if (output.Contains("Stopped"))
            {//Wsl aktuell gestoppt, kann gestartet werden 
                callwsl("-d " +imagecombo.SelectedValue.ToString()+ " -u root sh /abas/bin/starteVersion.sh run",5000);
            }
            else
            {
                callwsl("--terminate "+imagecombo.SelectedValue.ToString(),0);
            }
            // Status abholen
           output = callwsl("--list --verbose", 0);
            i = output.IndexOf(" "+imagecombo.SelectedValue.ToString()+" ");
            output = output.Substring(i, output.Length - i);
            i = output.IndexOf("2\r\n");
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
                Console.WriteLine(output);
                if (output.Length > 0)
                {
                    MessageBox.Show(output, "Error", MessageBoxButton.OK);
                }

            }
           // return Task.FromResult(0);
        }










    }
}
