using System.Windows;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Controls;

namespace TQCollector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        //To stop Files.reloadFiles() and refreshDisplay() being called when initially setting the toggles.
        private bool loaded = false;

        public MainWindow()
        {
            InitializeComponent();

            //Load config.xml, itemdb.xml, vaults, characters...
            if (Files.LoadFiles())
            {
                this.Title = "TQ Collector v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

                //Set toolbar toggle buttons
                LoadToggles();
                LoadLanguageList();
            }
            else
            {
                this.Close();
                return;
            }

            loaded = true;
            refreshDisplay();
        }

        private void LoadToggles()
        {
            Button_ToggleSP.IsChecked = Files.Configuration.UseSP;
            Button_ToggleSP.IsEnabled = Files.Configuration.UseIT || Files.Configuration.UseAE;
            Button_ToggleSP.ToolTip = Files.Language["tooltip05"];
            Button_ToggleCaravan.IsChecked = Files.Configuration.UseCaravan;
            Button_ToggleCaravan.ToolTip = Files.Language["tooltip01"];
            Button_ToggleCaravan.IsEnabled = Files.Configuration.UseIT || Files.Configuration.UseAE;
            Button_ToggleInventory.IsChecked = Files.Configuration.UseInventories;
            Button_ToggleInventory.ToolTip = Files.Language["tooltip03"];
            Button_ToggleItemCount.IsChecked = Files.Configuration.UseItemCount;
            Button_ToggleItemCount.ToolTip = Files.Language["tooltip06"];
            Button_About.ToolTip = Files.Language["tooltip07"];
            Button_CustomDirectory.ToolTip = Files.Language["tooltip08"];
            Button_ExportDataAs.ToolTip = Files.Language["tooltip09"];
            Button_Refresh.ToolTip = Files.Language["tooltip10"];
            Button_Filters.ToolTip = Files.Language["tooltip11"];
        }

        private void Button_Export_Click(object sender, RoutedEventArgs e)
        {
            SummaryExport f = new SummaryExport(Filterer.generateSummary());
            f.ShowDialog();
        }

        private void Button_Filters_Click(object sender, RoutedEventArgs e)
        {
            FilterOptions Form_FilterOptions = new FilterOptions();
            Form_FilterOptions.ShowDialog();
            refreshDisplay();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (loaded)
            {
                Files.SaveConfig();
            }
        }

        private void Button_ToggleItemCount_Checked(object sender, RoutedEventArgs e)
        {
            if (loaded)
            {
                Files.Configuration.UseItemCount = true;
                refreshDisplay();
            }
        }

        private void Button_ToggleItemCount_Unchecked(object sender, RoutedEventArgs e)
        {
            if (loaded)
            {
                Files.Configuration.UseItemCount = false;
                refreshDisplay();
            }
        }

        private void refreshDisplay()
        {
            myGrid.Children.Clear();
            myGrid.Children.Add(Filterer.Display());
            Filterer.resizeLists();
            countLabel.Content = Files.Language["count01"] + Filterer.ItemsCount + "/" + Filterer.ItemsTotal + " (" + (((double)Filterer.ItemsCount) / Filterer.ItemsTotal * 100).ToString("N2") + "%)";
        }

        private void Button_About_Click(object sender, RoutedEventArgs e)
        {
            About ab = new About();
            ab.ShowDialog();
        }

        private void Button_CustomDirectory_Click(object sender, RoutedEventArgs e)
        {
            Directories d = new Directories();
            if ((bool)d.ShowDialog())
            {
                Files.reloadFiles();
                refreshDisplay();
            }
        }

        private void Button_ToggleCaravan_Checked(object sender, RoutedEventArgs e)
        {
            if (loaded)
            {
                Files.Configuration.UseCaravan = true;
                Files.reloadFiles();
                refreshDisplay();
            }
        }

        private void Button_ToggleCaravan_Unchecked(object sender, RoutedEventArgs e)
        {
            if (loaded)
            {
                Files.Configuration.UseCaravan = false;
                Files.reloadFiles();
                refreshDisplay();
            }
        }

        private void Button_ToggleInventory_Checked(object sender, RoutedEventArgs e)
        {
            if (loaded)
            {
                Files.Configuration.UseInventories = true;
                Files.reloadFiles();
                refreshDisplay();
            }
        }

        private void Button_ToggleInventory_Unchecked(object sender, RoutedEventArgs e)
        {
            if (loaded)
            {
                Files.Configuration.UseInventories = false;
                Files.reloadFiles();
                refreshDisplay();
            }
        }

        private void Button_Refresh_Click(object sender, RoutedEventArgs e)
        {
            Files.reloadFiles();
            refreshDisplay();
        }

        private void Button_ToggleSP_Checked(object sender, RoutedEventArgs e)
        {
            if (loaded)
            {
                Files.Configuration.UseSP = true;
                Files.reloadFiles();
                refreshDisplay();
            }
        }

        private void Button_ToggleSP_Unchecked(object sender, RoutedEventArgs e)
        {
            if (loaded)
            {
                Files.Configuration.UseSP = false;
                Files.reloadFiles();
                refreshDisplay();
            }
        }

        private void LoadLanguageList()
        {
            //Get list of languages in TQC folder.
            DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
            FileInfo[] rgFiles = di.GetFiles("text_*.xml");
            List<string> sl = new List<string>();

            foreach (FileInfo f in rgFiles)
            {
                string s = f.Name.Substring(5, 2);
                //Make sure we compare all lowercase
                s = s.ToLowerInvariant();
                sl.Add(s);
            }

            //Compare to available IT ones
            if (Files.Configuration.UseIT)
            {
                DirectoryInfo di2 = new DirectoryInfo(Files.Configuration.Directories.IT + "\\Resources\\");
                FileInfo[] rgFiles2 = di2.GetFiles("text_*.arc");
                List<string> sl2 = new List<string>();

                foreach (FileInfo f in rgFiles2)
                {
                    string s = f.Name.Substring(5, 2);
                    //Make sure we compare all lowercase
                    s = s.ToLowerInvariant();
                    sl2.Add(s);
                }

                foreach (string st in sl)
                {
                    if (sl2.Contains(st))
                    {
                        //Add to Language list
                        ComboBoxItem cb = new ComboBoxItem();
                        cb.Content = st;
                        cb.Selected += Language_Combo_Change;
                        Language_Combo.Items.Add(cb);
                        if(st.Equals(Files.Configuration.Language)) Language_Combo.SelectedItem = cb;
                    }
                }
            }
            // Anniversary Edition
            else if(Files.Configuration.UseAE)
            {
                DirectoryInfo di2 = new DirectoryInfo(Files.Configuration.Directories.AE + "\\Text\\");
                FileInfo[] rgFiles2 = di2.GetFiles("text_*.arc");
                List<string> sl2 = new List<string>();

                foreach (FileInfo f in rgFiles2)
                {
                    string s = f.Name.Substring(5, 2);
                    //Make sure we compare all lowercase
                    s = s.ToLowerInvariant();
                    sl2.Add(s);
                }

                foreach (string st in sl)
                {
                    if (sl2.Contains(st))
                    {
                        //Add to Language list
                        ComboBoxItem cb = new ComboBoxItem();
                        cb.Content = st;
                        cb.Selected += Language_Combo_Change;
                        Language_Combo.Items.Add(cb);
                        if (st.Equals(Files.Configuration.Language)) Language_Combo.SelectedItem = cb;
                    }
                }
            }
            //TQ instead of IT.
            else
            {
                DirectoryInfo di2 = new DirectoryInfo(Files.Configuration.Directories.TQ + "\\Text\\");
                FileInfo[] rgFiles2 = di.GetFiles("text_*.arc");
                List<string> sl2 = new List<string>();

                foreach (FileInfo f in rgFiles2)
                {
                    string s = f.Name.Substring(5, 2);
                    //Make sure we compare all lowercase
                    s = s.ToLowerInvariant();
                    sl2.Add(s);
                }

                foreach (string st in sl)
                {
                    if (sl2.Contains(st))
                    {
                        //Add to Language list
                        ComboBoxItem cb = new ComboBoxItem();
                        cb.Content = st;
                        Language_Combo.Items.Add(cb);
                        if (st.Equals(Files.Configuration.Language)) Language_Combo.SelectedItem = cb;
                    }
                }
            }
        }

        private void Language_Combo_Change(object sender, RoutedEventArgs e)
        {
            ComboBoxItem c = sender as ComboBoxItem;
            string s = c.Content.ToString();
            Files.Configuration.Language = s;
            Files.SaveConfig();
            if (loaded)
            {
                Files.reloadFiles();
                refreshDisplay();
            }
        }

        private void MWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (loaded) Filterer.resizeLists();
        }
    }
}