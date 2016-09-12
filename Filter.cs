using System.Windows.Controls;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Media;
using System;

namespace TQCollector
{
    partial class Filterer
    {
        private struct rList
        {
            public WrapPanel w;
            public int h;
        }

        public static int ItemsTotal = 0;
        public static int ItemsCount = 0;
        private static List<rList> resizeList = new List<rList>();

        public static void resizeLists()
        {
            foreach (rList w in resizeList)
            {
                w.w.Height = (w.h / ((((int)Application.Current.MainWindow.Width - 10) / 200) - 1)) + 100;
            }
        }

        private static string generateText(string title, int sub, int tot)
        {
            if (Files.Configuration.UseItemCount)
            {
                return title + ": " + sub + "/" + tot + " (" + ((double)sub / tot * 100).ToString("N2") + "%)";
            }
            else
            {
                return title;
            }
        }

        // Removes any {.*} formatting on the item name
        private static string generateContent(string content)
        {
            int start = content.IndexOf('{');
            if (start == -1) start = 0;
            int end = content.LastIndexOf('}');
            if (end == -1) end = 0;
            else end++;

            return content.Remove(start, (end - start));
        }

        public static string generateSummary()
        {
            return "Total: " + ItemsCount + "/" + ItemsTotal + " (" + ((double)ItemsCount / ItemsTotal * 100).ToString("N2") + "%)";
        }

        public static TabControl Display()
        {
            //Reset item counts
            ItemsTotal = 0;
            ItemsCount = 0;
            resizeList = new List<rList>();

            //Create tabs
            TabControl MasterControl = new TabControl();

            if (Files.Configuration.Filters.MonsterInfrequent.Amount != Amount.None)
            {
                MasterControl.Items.Add(DisplayMonsterInfrequent());
            }
            if (Files.Configuration.Filters.Uniques.Amount != Amount.None)
            {
                MasterControl.Items.Add(DisplayUniques());
            }
            if (Files.Configuration.Filters.Artifacts.Amount != Amount.None && (Files.Configuration.UseIT || Files.Configuration.UseAE))
            {
                MasterControl.Items.Add(DisplayArtifacts());
            }
            if (Files.Configuration.Filters.Charms.Amount != Amount.None)
            {
                MasterControl.Items.Add(DisplayCharms());
            }
            if (Files.Configuration.Filters.Parchments.Amount != Amount.None && (Files.Configuration.UseIT || Files.Configuration.UseAE))
            {
                MasterControl.Items.Add(DisplayParchments());
            }
            if (Files.Configuration.Filters.Formulae.Amount != Amount.None && (Files.Configuration.UseIT || Files.Configuration.UseAE))
            {
                MasterControl.Items.Add(DisplayFormulae());
            }
            if (Files.Configuration.Filters.Relics.Amount != Amount.None)
            {
                MasterControl.Items.Add(DisplayRelics());
            }
            if (Files.Configuration.Filters.Sets.Amount != Amount.None)
            {
                MasterControl.Items.Add(DisplaySets());
            }

            return MasterControl;
        }

        private static TabItem _createTab(string header, Panel content)
        {
            TabItem ti = new TabItem();
            ti.Header = header;
            ti.VerticalContentAlignment = VerticalAlignment.Top;
            ti.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            ScrollViewer sv1 = new ScrollViewer();
            sv1.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            sv1.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            sv1.VerticalContentAlignment = System.Windows.VerticalAlignment.Stretch;
            sv1.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            sv1.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch;
            ti.Content = sv1;
            sv1.Content = content;

            return ti;
        }

        private static string _createTooltip(Item item)
        {
            string tooltip = "...";

            if (item.count > 0)
            {
                tooltip = string.Format(Files.Language["mouseover01"], item.count) + "\n";
                //List locations where the player has this item
                tooltip += System.String.Join("\n", item.locations.ToArray());
                tooltip += FormatFind(item.find);
            }
            else
            {
                tooltip = Files.Language["mouseover03"] + FormatFind(item.find);
            }

            return tooltip;
        }

        private static Label _createLabel(Item item, Thickness margin, int width, Thickness padding)
        {
            Label ct = new Label();

            ct.ToolTip = _createTooltip(item);
            ct.Margin = margin;
            ct.Content = generateContent(item.name);
            ct.FontWeight = (item.count > 0) ? FontWeights.Bold : FontWeights.Normal;
            ct.Width = width;
            ct.Padding = padding;

            return ct;
        }

        private static CheckBox _createCheckBox(Item item)
        {
            CheckBox ct = new CheckBox();
            ct.IsEnabled = false;
            ct.ToolTip = _createTooltip(item);
            ct.Margin = new Thickness(5, 0, 0, 0);
            ct.Content = generateContent(item.name);
            ct.IsChecked = (item.count > 0);
            ct.Width = 200;
            ct.Foreground = Brushes.Black;

            return ct;
        }

        private static TabItem CreateTab(string header, Set[] set)
        {
            return _createTab(generateText(header, Files.Count(set), Files.Total(set)), CreatePanel(set));
        }

        private static TabItem CreateCompletionTab(string header, Set[] set)
        {
            return _createTab(generateText(header, Files.Count(set), Files.Total(set)), CreateCompletionPanel(set));
        }

        private static TabItem CreateListTab(string header, Set[] set)
        {
            return _createTab(generateText(header, Files.Count(set), Files.Total(set)), CreateListPanel(set));
        }

        private static StackPanel CreatePanel(Set[] set)
        {
            StackPanel sp = new StackPanel();

            foreach (Set s in set)
            {
                StackPanel sp2 = new StackPanel();
                sp2.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                sp2.Orientation = Orientation.Vertical;
                WrapPanel w = new WrapPanel();
                w.Orientation = Orientation.Horizontal;
                w.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                double percentage = 0;

                percentage = (double)Files.Count(s) / s.Item.Length * 100;
                //If empty, don't add a header
                TextBlock t = new TextBlock();
                if (!s.name.Equals(""))
                {
                    t.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    t.FontSize = 18;
                    t.FontStyle = FontStyles.Italic;
                    t.Text = Files.Database.exists(s.name) ? Files.Database.getName(s.name) : Files.Language[s.name] + ": " + Files.Count(s) + "/" + s.Item.Length + " (" + percentage.ToString("N2") + "%)";
                    sp2.Children.Add(t);
                }

                sp2.Children.Add(w);
                w.HorizontalAlignment = HorizontalAlignment.Stretch;

                for (int i = 0; i < s.Item.Length; i++)
                {
                    if ((Files.Configuration.UseIT || Files.Configuration.UseAE || !s.Item[i].isIT) && (Files.Configuration.UseItemOwned || (!Files.Configuration.UseItemOwned && s.Item[i].count == 0)))
                    {
                        w.Children.Add(_createLabel(s.Item[i], new Thickness(5, 0, 0, 3), Int32.Parse(Files.Language["width01"]), new Thickness(0)));
                    }
                }

                if(w.Children.Count==0)
                {
                    sp2.Children.Remove(w);
                    sp2.Children.Remove(t);
                }
                sp.Children.Add(sp2);
            }

            return sp;
        }

        private static WrapPanel CreateListPanel(Set[] set)
        {
            WrapPanel sp = new WrapPanel();
            sp.Orientation = Orientation.Vertical;
            rList r = new rList();
            r.w = sp;
            r.h = (Files.Total(set) * 15) + (set.Length * 15);
            resizeList.Add(r);

            foreach (Set s in set)
            {
                StackPanel sp2 = new StackPanel();
                sp2.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                sp2.Orientation = Orientation.Vertical;
                WrapPanel w = new WrapPanel();
                w.Orientation = Orientation.Vertical;
                w.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                double percentage = 0;

                percentage = (double)Files.Count(s) / s.Item.Length * 100;
                //If empty, don't add a header
                TextBlock t = new TextBlock();
                if (!s.name.Equals(""))
                {
                    t.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    t.FontSize = 15;
                    t.Margin = new Thickness(0, 10, 0, 0);
                    t.FontStyle = FontStyles.Italic;
                    t.Text = Files.Database.exists(s.name) ? Files.Database.getName(s.name) : Files.Language[s.name];
                    sp2.Children.Add(t);
                }

                sp2.Children.Add(w);
                w.HorizontalAlignment = HorizontalAlignment.Stretch;

                for (int i = 0; i < s.Item.Length; i++)
                {
                    if (Files.Configuration.UseIT || Files.Configuration.UseAE || !s.Item[i].isIT)
                    {
                        if (Files.Configuration.UseCheckBox)
                        {
                            w.Children.Add(_createCheckBox(s.Item[i]));
                        }
                        else
                        {
                            if (Files.Configuration.UseItemOwned || (!Files.Configuration.UseItemOwned && s.Item[i].count == 0))
                            {
                                w.Children.Add(_createLabel(s.Item[i], new Thickness(5, 0, 0, 0), Int32.Parse(Files.Language["width02"]), new Thickness(0)));
                            }
                        }
                        
                    }
                }
                if (w.Children.Count == 0)
                {
                    sp2.Children.Remove(w);
                    sp2.Children.Remove(t);
                }
                sp.Children.Add(sp2);
            }

            return sp;
        }

        private static WrapPanel CreateCompletionPanel(Set[] set)
        {
            WrapPanel sp = new WrapPanel();
            sp.Orientation = Orientation.Vertical;
            rList r = new rList();
            r.w = sp;
            r.h = (Files.Total(set) * 15) + (set.Length * 15);
            resizeList.Add(r);

            foreach (Set s in set)
            {
                StackPanel sp2 = new StackPanel();
                sp2.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                sp2.Orientation = Orientation.Vertical;
                WrapPanel w = new WrapPanel();
                w.Orientation = Orientation.Vertical;
                w.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                double percentage = 0;

                percentage = (double)Files.Count(s) / s.Item.Length * 100;
                //If empty, don't add a header
                TextBlock t = new TextBlock();
                if (!s.name.Equals(""))
                {
                    t.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    t.FontSize = 15;
                    t.Margin = new Thickness(0, 10, 0, 0);
                    t.FontStyle = FontStyles.Italic;
                    t.Text = Files.Database.exists(s.name) ? Files.Database.getName(s.name) : Files.Language[s.name];
                    sp2.Children.Add(t);
                }

                sp2.Children.Add(w);
                w.HorizontalAlignment = HorizontalAlignment.Stretch;

                for (int i = 0; i < s.Item.Length; i++)
                {
                    if (Files.Configuration.UseCheckBox)
                    {
                        w.Children.Add(_createCheckBox(s.Item[i]));
                    }
                    else
                    {
                        if (Files.Configuration.UseItemOwned || (!Files.Configuration.UseItemOwned && s.Item[i].count == 0))
                        {
                            w.Children.Add(_createLabel(s.Item[i], new Thickness(5, 0, 0, 0), Int32.Parse(Files.Language["width02"]), new Thickness(0)));
                        }
                    }
                }
                if (w.Children.Count == 0)
                {
                    sp2.Children.Remove(w);
                    sp2.Children.Remove(t);
                }
                sp.Children.Add(sp2);
            }

            return sp;
        }

        private static string FormatFind(string f)
        {
            if (f.Length < 1) return null;

            string s1 = "\n" + Files.Language["mouseover02"] + "\n";
            string s = f;

            if (f.Contains("@"))
            {
                do
                {
                    int i = s.IndexOf('@');
                    int j = s.IndexOf('$');
                    string lookup = s.Substring(i + 1, j - i - 1);
                    s = s.Replace("@" + lookup + "$", Files.Language[lookup]);
                } while (s.Contains("@"));
            }

            return s1 + s;
        }

        private static TabItem DisplayMonsterInfrequent()
        {
            TabItem tb = new TabItem();
            int sub = 0;
            int tot = 0;

            if (Files.Configuration.Filters.MonsterInfrequent.Amount == Amount.Some)
            {
                TabControl MonsterInfrequentTabControl = new TabControl();
                if (Files.Configuration.Filters.MonsterInfrequent.Normal)
                {
                    sub += Files.Count(Files.ItemDatabase.MonsterInfrequent.Normal);
                    tot += Files.Total(Files.ItemDatabase.MonsterInfrequent.Normal);
                    MonsterInfrequentTabControl.Items.Add(CreateListTab(Files.Language["level04"], Files.ItemDatabase.MonsterInfrequent.Normal));
                }
                if (Files.Configuration.Filters.MonsterInfrequent.Epic)
                {
                    sub += Files.Count(Files.ItemDatabase.MonsterInfrequent.Epic);
                    tot += Files.Total(Files.ItemDatabase.MonsterInfrequent.Epic);
                    MonsterInfrequentTabControl.Items.Add(CreateListTab(Files.Language["level05"], Files.ItemDatabase.MonsterInfrequent.Epic));
                }
                if (Files.Configuration.Filters.MonsterInfrequent.Legendary)
                {
                    sub += Files.Count(Files.ItemDatabase.MonsterInfrequent.Legendary);
                    tot += Files.Total(Files.ItemDatabase.MonsterInfrequent.Legendary);
                    MonsterInfrequentTabControl.Items.Add(CreateListTab(Files.Language["level06"], Files.ItemDatabase.MonsterInfrequent.Legendary));
                }

                tb.Header = generateText(Files.Language["category01"], sub, tot);
                tb.Content = MonsterInfrequentTabControl;
            }
            else if (Files.Configuration.Filters.MonsterInfrequent.Amount == Amount.All)
            {
                sub = Files.Count(Files.ItemDatabase.MonsterInfrequent.Normal) + Files.Count(Files.ItemDatabase.MonsterInfrequent.Epic) + Files.Count(Files.ItemDatabase.MonsterInfrequent.Legendary);
                tot = Files.Total(Files.ItemDatabase.MonsterInfrequent.Normal) + Files.Total(Files.ItemDatabase.MonsterInfrequent.Epic) + Files.Total(Files.ItemDatabase.MonsterInfrequent.Legendary);
                tb.Header = generateText(Files.Language["category01"], sub, tot);
                TabControl MonsterInfrequentTabControl = new TabControl();
                MonsterInfrequentTabControl.Items.Add(CreateListTab(Files.Language["level04"], Files.ItemDatabase.MonsterInfrequent.Normal));
                MonsterInfrequentTabControl.Items.Add(CreateListTab(Files.Language["level05"], Files.ItemDatabase.MonsterInfrequent.Epic));
                MonsterInfrequentTabControl.Items.Add(CreateListTab(Files.Language["level06"], Files.ItemDatabase.MonsterInfrequent.Legendary));
                tb.Content = MonsterInfrequentTabControl;
            }
            else if (Files.Configuration.Filters.MonsterInfrequent.Amount == Amount.Any)
            {
                //Don't care what difficulty, as long as they have it.
                Set[] combined = Files.Combine(Files.ItemDatabase.MonsterInfrequent.Normal, Files.ItemDatabase.MonsterInfrequent.Legendary);
                combined = Files.Combine(combined, Files.ItemDatabase.MonsterInfrequent.Epic);
                sub = Files.Count(combined);
                tot = Files.Total(combined);
                tb = CreateListTab(Files.Language["category01"], combined);
            }

            ItemsCount += sub;
            ItemsTotal += tot;

            return tb;
        }

        private static TabItem _createUniqueTabItem(Set[] uniques, Set[] sets, string heading, ref int sub, ref int tot)
        {
            TabItem ti = null;
            Set[] temp = null;

            if (Files.Configuration.Filters.Sets.Count)
            {
                //Make Set[] according to what we need (Secret Passage or not)
                temp = Files.Configuration.UseSP ? Files.Add(uniques, sets) : Files.Add(Files.removeSP(sets), Files.removeSP(uniques));
            }
            else
            {
                temp = Files.Configuration.UseSP ? uniques : Files.removeSP(uniques);
            }

            sub += Files.Count(temp);
            tot += Files.Total(temp);
            ti = CreateTab(heading, temp);

            return ti;
        }

        private static TabItem DisplayUniques()
        {
            TabItem tb = new TabItem();
            TabControl UniqueTabControl = new TabControl();
            int sub = 0;
            int tot = 0;

            if (Files.Configuration.Filters.Uniques.Amount == Amount.Some)
            {
                if (Files.Configuration.Filters.Uniques.Epic)
                {
                    UniqueTabControl.Items.Add(_createUniqueTabItem(Files.ItemDatabase.Uniques.Epic, Files.ItemDatabase.Sets.Epic, Files.Language["level05"], ref sub, ref tot));
                }
                if (Files.Configuration.Filters.Uniques.Legendary)
                {
                    UniqueTabControl.Items.Add(_createUniqueTabItem(Files.ItemDatabase.Uniques.Legendary, Files.ItemDatabase.Sets.Legendary, Files.Language["level06"], ref sub, ref tot));
                }
                tb.Header = generateText(Files.Language["category06"], sub, tot);
                tb.Content = UniqueTabControl;
            }
            else if (Files.Configuration.Filters.Uniques.Amount == Amount.All)
            {
                UniqueTabControl.Items.Add(_createUniqueTabItem(Files.ItemDatabase.Uniques.Epic, Files.ItemDatabase.Sets.Epic, Files.Language["level05"], ref sub, ref tot));
                UniqueTabControl.Items.Add(_createUniqueTabItem(Files.ItemDatabase.Uniques.Legendary, Files.ItemDatabase.Sets.Legendary, Files.Language["level06"], ref sub, ref tot));

                tb.Header = generateText(Files.Language["category06"], sub, tot);
                tb.Content = UniqueTabControl;
            }

            ItemsCount += sub;
            ItemsTotal += tot;

            return tb;
        }

        private static TabItem DisplayArtifacts()
        {
            TabItem tb = new TabItem();
            int sub = 0;
            int tot = 0;

            if (Files.Configuration.Filters.Artifacts.Amount == Amount.Some)
            {
                TabControl ArtifactTabControl = new TabControl();
                if (Files.Configuration.Filters.Artifacts.Normal)
                {
                    sub += Files.Count(Files.ItemDatabase.Artifacts.Normal);
                    tot += Files.Total(Files.ItemDatabase.Artifacts.Normal);
                    ArtifactTabControl.Items.Add(CreateTab(Files.Language["level01"], Files.ItemDatabase.Artifacts.Normal));
                }
                if (Files.Configuration.Filters.Artifacts.Epic)
                {
                    sub += Files.Count(Files.ItemDatabase.Artifacts.Epic);
                    tot += Files.Total(Files.ItemDatabase.Artifacts.Epic);
                    ArtifactTabControl.Items.Add(CreateTab(Files.Language["level02"], Files.ItemDatabase.Artifacts.Epic));
                }
                if (Files.Configuration.Filters.Artifacts.Legendary)
                {
                    sub += Files.Count(Files.ItemDatabase.Artifacts.Legendary);
                    tot += Files.Total(Files.ItemDatabase.Artifacts.Legendary);
                    ArtifactTabControl.Items.Add(CreateTab(Files.Language["level03"], Files.ItemDatabase.Artifacts.Legendary));
                }
                tb.Header = generateText(Files.Language["category04"], sub, tot);
                tb.Content = ArtifactTabControl;
            }
            else if (Files.Configuration.Filters.Artifacts.Amount == Amount.All)
            {
                sub = Files.Count(Files.ItemDatabase.Artifacts.Normal) + Files.Count(Files.ItemDatabase.Artifacts.Epic) + Files.Count(Files.ItemDatabase.Artifacts.Legendary);
                tot = Files.Total(Files.ItemDatabase.Artifacts.Normal) + Files.Total(Files.ItemDatabase.Artifacts.Epic) + Files.Total(Files.ItemDatabase.Artifacts.Legendary);
                TabControl ArtifactTabControl = new TabControl();
                ArtifactTabControl.Items.Add(CreateTab(Files.Language["level01"], Files.ItemDatabase.Artifacts.Normal));
                ArtifactTabControl.Items.Add(CreateTab(Files.Language["level02"], Files.ItemDatabase.Artifacts.Epic));
                ArtifactTabControl.Items.Add(CreateTab(Files.Language["level03"], Files.ItemDatabase.Artifacts.Legendary));
                tb.Header = generateText(Files.Language["category04"], sub, tot);
                tb.Content = ArtifactTabControl;
            }

            ItemsCount += sub;
            ItemsTotal += tot;

            return tb;
        }

        private static TabItem DisplayCharms()
        {
            TabItem tb = new TabItem();
            int sub = 0;
            int tot = 0;

            if (Files.Configuration.Filters.Charms.Amount == Amount.All)
            {
                sub = Files.Count(Files.ItemDatabase.Charms.Normal) + Files.Count(Files.ItemDatabase.Charms.Epic) + Files.Count(Files.ItemDatabase.Charms.Legendary);
                tot = Files.Total(Files.ItemDatabase.Charms.Normal) + Files.Total(Files.ItemDatabase.Charms.Epic) + Files.Total(Files.ItemDatabase.Charms.Legendary);
                tb.Header = generateText(Files.Language["category03"], sub, tot);
                TabControl CharmsTabControl = new TabControl();
                CharmsTabControl.Items.Add(CreateTab(Files.Language["level04"], Files.ItemDatabase.Charms.Normal));
                CharmsTabControl.Items.Add(CreateTab(Files.Language["level05"], Files.ItemDatabase.Charms.Epic));
                CharmsTabControl.Items.Add(CreateTab(Files.Language["level06"], Files.ItemDatabase.Charms.Legendary));
                tb.Content = CharmsTabControl;
            }
            if (Files.Configuration.Filters.Charms.Amount == Amount.Some)
            {
                TabControl CharmsTabControl = new TabControl();
                if (Files.Configuration.Filters.Charms.Normal)
                {
                    sub += Files.Count(Files.ItemDatabase.Charms.Normal);
                    tot += Files.Total(Files.ItemDatabase.Charms.Normal);
                    CharmsTabControl.Items.Add(CreateTab(Files.Language["level04"], Files.ItemDatabase.Charms.Normal));
                }
                if (Files.Configuration.Filters.Charms.Epic)
                {
                    sub += Files.Count(Files.ItemDatabase.Charms.Epic);
                    tot += Files.Total(Files.ItemDatabase.Charms.Epic);
                    CharmsTabControl.Items.Add(CreateTab(Files.Language["level05"], Files.ItemDatabase.Charms.Epic));
                }
                if (Files.Configuration.Filters.Charms.Legendary)
                {
                    sub += Files.Count(Files.ItemDatabase.Charms.Legendary);
                    tot += Files.Total(Files.ItemDatabase.Charms.Legendary);
                    CharmsTabControl.Items.Add(CreateTab(Files.Language["level06"], Files.ItemDatabase.Charms.Legendary));
                }
                tb.Header = generateText(Files.Language["category03"], sub, tot);
                tb.Content = CharmsTabControl;
            }

            ItemsCount += sub;
            ItemsTotal += tot;

            return tb;
        }

        private static TabItem DisplayParchments()
        {
            TabItem tb = CreateTab(Files.Language["category07"], Files.ItemDatabase.Parchments.Normal);
            int sub = Files.Count(Files.ItemDatabase.Parchments.Normal);
            int tot = Files.Total(Files.ItemDatabase.Parchments.Normal);

            ItemsCount += sub;
            ItemsTotal += tot;

            return tb;
        }

        private static TabItem DisplayFormulae()
        {
            TabItem tb = new TabItem();
            int sub = 0;
            int tot = 0;

            if (Files.Configuration.Filters.Formulae.Amount == Amount.All)
            {
                sub = Files.Count(Files.ItemDatabase.Formulae.Normal) + Files.Count(Files.ItemDatabase.Formulae.Epic) + Files.Count(Files.ItemDatabase.Formulae.Legendary);
                tot = Files.Total(Files.ItemDatabase.Formulae.Normal) + Files.Total(Files.ItemDatabase.Formulae.Epic) + Files.Total(Files.ItemDatabase.Formulae.Legendary);
                TabControl FormulaeTabControl = new TabControl();
                FormulaeTabControl.Items.Add(CreateTab(Files.Language["level01"], Files.ItemDatabase.Formulae.Normal));
                FormulaeTabControl.Items.Add(CreateTab(Files.Language["level02"], Files.ItemDatabase.Formulae.Epic));
                FormulaeTabControl.Items.Add(CreateTab(Files.Language["level03"], Files.ItemDatabase.Formulae.Legendary));
                tb.Header = generateText(Files.Language["category08"], sub, tot);
                tb.Content = FormulaeTabControl;
            }
            else if (Files.Configuration.Filters.Formulae.Amount == Amount.Some)
            {
                TabControl FormulaeTabControl = new TabControl();
                if (Files.Configuration.Filters.Formulae.Normal)
                {
                    sub += Files.Count(Files.ItemDatabase.Formulae.Normal);
                    tot += Files.Total(Files.ItemDatabase.Formulae.Normal);
                    FormulaeTabControl.Items.Add(CreateTab(Files.Language["level01"], Files.ItemDatabase.Formulae.Normal));
                }
                if (Files.Configuration.Filters.Formulae.Epic)
                {
                    sub += Files.Count(Files.ItemDatabase.Formulae.Epic);
                    tot += Files.Total(Files.ItemDatabase.Formulae.Epic);
                    FormulaeTabControl.Items.Add(CreateTab(Files.Language["level02"], Files.ItemDatabase.Formulae.Epic));
                }
                if (Files.Configuration.Filters.Formulae.Legendary)
                {
                    sub += Files.Count(Files.ItemDatabase.Formulae.Legendary);
                    tot += Files.Total(Files.ItemDatabase.Formulae.Legendary);
                    FormulaeTabControl.Items.Add(CreateTab(Files.Language["level03"], Files.ItemDatabase.Formulae.Legendary));
                }
                tb.Header = generateText(Files.Language["category08"], sub, tot);
                tb.Content = FormulaeTabControl;
            }

            ItemsCount += sub;
            ItemsTotal += tot;

            return tb;
        }

        private static TabItem DisplayRelics()
        {
            TabItem tb = new TabItem();
            int sub = 0;
            int tot = 0;

            if (Files.Configuration.Filters.Relics.Amount == Amount.All)
            {
                sub = Files.Count(Files.ItemDatabase.Relics.Normal) + Files.Count(Files.ItemDatabase.Relics.Epic) + Files.Count(Files.ItemDatabase.Relics.Legendary);
                tot = Files.Total(Files.ItemDatabase.Relics.Normal) + Files.Total(Files.ItemDatabase.Relics.Epic) + Files.Total(Files.ItemDatabase.Relics.Legendary);
                TabControl RelicsTabControl = new TabControl();
                if (Files.Configuration.UseIT || Files.Configuration.UseAE)
                {
                    RelicsTabControl.Items.Add(CreateTab(Files.Language["level07"], Files.ItemDatabase.Relics.Normal));
                    RelicsTabControl.Items.Add(CreateTab(Files.Language["level08"], Files.ItemDatabase.Relics.Epic));
                    RelicsTabControl.Items.Add(CreateTab(Files.Language["level09"], Files.ItemDatabase.Relics.Legendary));
                }
                else
                {
                    RelicsTabControl.Items.Add(CreateTab(Files.Language["level07"], Files.removeHades(Files.ItemDatabase.Relics.Normal)));
                    RelicsTabControl.Items.Add(CreateTab(Files.Language["level08"], Files.removeHades(Files.ItemDatabase.Relics.Epic)));
                    RelicsTabControl.Items.Add(CreateTab(Files.Language["level09"], Files.removeHades(Files.ItemDatabase.Relics.Legendary)));
                }
                tb.Header = generateText(Files.Language["category02"], sub, tot);
                tb.Content = RelicsTabControl;
            }
            else if (Files.Configuration.Filters.Relics.Amount == Amount.Some)
            {
                TabControl RelicsTabControl = new TabControl();
                if (Files.Configuration.Filters.Relics.Normal)
                {
                    sub += Files.Count(Files.ItemDatabase.Relics.Normal);
                    tot += Files.Total(Files.ItemDatabase.Relics.Normal);
                    RelicsTabControl.Items.Add(CreateTab(Files.Language["level07"], Files.ItemDatabase.Relics.Normal));
                }
                if (Files.Configuration.Filters.Relics.Epic)
                {
                    sub += Files.Count(Files.ItemDatabase.Relics.Epic);
                    tot += Files.Total(Files.ItemDatabase.Relics.Epic);
                    RelicsTabControl.Items.Add(CreateTab(Files.Language["level08"], Files.ItemDatabase.Relics.Epic));
                }
                if (Files.Configuration.Filters.Relics.Legendary)
                {
                    sub += Files.Count(Files.ItemDatabase.Relics.Legendary);
                    tot += Files.Total(Files.ItemDatabase.Relics.Legendary);
                    RelicsTabControl.Items.Add(CreateTab(Files.Language["level09"], Files.ItemDatabase.Relics.Legendary));
                }
                tb.Header = generateText(Files.Language["category02"], sub, tot);
                tb.Content = RelicsTabControl;
            }

            ItemsCount += sub;
            ItemsTotal += tot;

            return tb;
        }

        private static TabItem DisplaySets()
        {
            TabItem tb = new TabItem();
            int sub = 0;
            int tot = 0;

            if (Files.Configuration.Filters.Sets.Amount == Amount.All)
            {
                TabControl SetsTabControl = new TabControl();
                if (Files.Configuration.UseSP)
                {
                    sub = Files.Count(Files.ItemDatabase.Sets.Epic) + Files.Count(Files.ItemDatabase.Sets.Legendary);
                    tot = Files.Total(Files.ItemDatabase.Sets.Epic) + Files.Total(Files.ItemDatabase.Sets.Legendary);
                    SetsTabControl.Items.Add(CreateListTab(Files.Language["level05"], Files.ItemDatabase.Sets.Epic));
                    SetsTabControl.Items.Add(CreateListTab(Files.Language["level06"], Files.ItemDatabase.Sets.Legendary));
                }
                else
                {
                    Set[] es = Files.removeSP(Files.ItemDatabase.Sets.Epic);
                    Set[] ls = Files.removeSP(Files.ItemDatabase.Sets.Legendary);
                    sub = Files.Count(es) + Files.Count(ls);
                    tot = Files.Total(es) + Files.Total(ls);
                    SetsTabControl.Items.Add(CreateListTab(Files.Language["level05"], es));
                    SetsTabControl.Items.Add(CreateListTab(Files.Language["level06"], ls));
                }
                
                tb.Header = generateText(Files.Language["category05"], sub, tot);
                tb.Content = SetsTabControl;
            }
            else if (Files.Configuration.Filters.Sets.Amount == Amount.Some)
            {
                TabControl SetsTabControl = new TabControl();
                if (Files.Configuration.Filters.Sets.Epic)
                {
                    if (Files.Configuration.UseSP)
                    {
                        sub += Files.Count(Files.ItemDatabase.Sets.Epic);
                        tot += Files.Total(Files.ItemDatabase.Sets.Epic);
                        SetsTabControl.Items.Add(CreateListTab(Files.Language["level05"], Files.ItemDatabase.Sets.Epic));
                    }
                    else
                    {
                        Set[] es = Files.removeSP(Files.ItemDatabase.Sets.Epic);
                        sub += Files.Count(es);
                        tot += Files.Total(es);
                        SetsTabControl.Items.Add(CreateListTab(Files.Language["level05"], es));
                    }
                }
                if (Files.Configuration.Filters.Sets.Legendary)
                {
                    if (Files.Configuration.UseSP)
                    {
                        sub += Files.Count(Files.ItemDatabase.Sets.Legendary);
                        tot += Files.Total(Files.ItemDatabase.Sets.Legendary);
                        SetsTabControl.Items.Add(CreateListTab(Files.Language["level06"], Files.ItemDatabase.Sets.Legendary));
                    }
                    else
                    {
                        Set[] ls = Files.removeSP(Files.ItemDatabase.Sets.Legendary);
                        sub += Files.Count(ls);
                        tot += Files.Total(ls);
                        SetsTabControl.Items.Add(CreateListTab(Files.Language["level06"], ls));
                    }
                }
                tb.Header = generateText(Files.Language["category05"], sub, tot);
                tb.Content = SetsTabControl;
            }

            ItemsCount += sub;
            ItemsTotal += tot;

            return tb;
        }
    }
}
