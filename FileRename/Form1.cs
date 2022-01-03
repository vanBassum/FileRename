using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace FileRename
{
    public partial class Form1 : Form
    {
        Font font = new Font("Consolas", 10);
        Task worker;
        BindingList<ChangeItem> items = new BindingList<ChangeItem>();
        ConcurrentQueue<IEvent> work = new ConcurrentQueue<IEvent>();

        public Form1()
        {
            InitializeComponent();
            listBox1.DataSource = items;
            listBox1.DrawMode = DrawMode.OwnerDrawVariable;
            listBox1.DrawItem += ListBox1_DrawItem;
            listBox1.MeasureItem += ListBox1_MeasureItem;
            worker = Task.Run(() => Work());
        }

        

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void ListBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if(sender is ListBox listbox)
            {
                e.DrawBackground();

                if(listbox.Items[e.Index] is ChangeItem item)
                {
                    e.Graphics.DrawString("SRC",
                        font, Brushes.Black, e.Bounds.X, e.Bounds.Y);

                    e.Graphics.DrawString(item.Source.FullName,
                        font, Brushes.Black, e.Bounds.X + 30, e.Bounds.Y);

                    e.Graphics.DrawString("DST",
                        font, Brushes.Black, e.Bounds.X, e.Bounds.Y + 15);

                    if (item.Destination != null)
                    {
                        e.Graphics.DrawString(item.Destination.FullName,
                            font, Brushes.Black, e.Bounds.X + 30, e.Bounds.Y + 15);
                    }
                }
                
                // Draw the focus rectangle around the selected item.
                e.DrawFocusRectangle();
            }
        }

        private void ListBox1_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            if (sender is ListBox listbox)
            {
                e.ItemHeight = 32;
            }
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (sender is TextBox tb)
            {
                if (IsValidRegexPattern(tb.Text))
                {
                    tb.BackColor = Color.White;
                    work.Enqueue(new FilterChangedEvent(tb.Text));
                }
                else
                    tb.BackColor = Color.IndianRed;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (sender is TextBox tb)
            {
                work.Enqueue(new DestinationChangedEvent(tb.Text));
            }
        }


        void Rename(ChangeItem changeItem, string format)
        {
            string result = "";
            do
            {
                if (format[0] == '\\')
                {
                    format = format.Substring(1);
                    Match m;

                    if (format.Length > 0)
                    {
                        if (format[0] == '\\')
                        {
                            result += "\\";
                            format = format.Substring(1);
                        }
                        else if((m = Regex.Match(format, "\\d+")).Success)
                        {
                            if(int.TryParse(m.Value, out int groupNo))
                            {
                                if(groupNo < changeItem.Match.Groups.Count)
                                {
                                    result += changeItem.Match.Groups[groupNo].Value;
                                }
                            }
                            format = format.Substring(m.Length);
                        }
                        else
                        {
                            //invalid escape character
                        }
                    }
                }
                else
                {
                    int ind = format.IndexOf('\\');
                    if(ind < 0)
                    {
                        result += format;
                        format = "";
                    }
                    else
                    {
                        result += format.Substring(0, ind + 1);
                        format = format.Substring(ind);
                    }
                }

            }
            while (format.Length > 0);

            changeItem.Destination = new FileInfo(result);
        }


        void Work()
        {
            IEnumerator<string> files = null;
            IEnumerator<ChangeItem> change = null;
            string filter = "*";
            string format = "";
            bool delay = true;

            while (true)
            {
                if (work.TryDequeue(out IEvent ev))
                {
                    switch (ev)
                    {
                        case FilterChangedEvent filterEvent:
                            this.InvokeIfRequired(() => items.Clear());
                            filter = filterEvent.Value;
                            files = Directory.EnumerateFiles(@"C:\Users\Bas\Desktop\TEST", "*", SearchOption.AllDirectories).GetEnumerator();
                            break;

                        case DestinationChangedEvent destinationChangedEvent:
                            change = items.GetEnumerator();
                            format = destinationChangedEvent.Value;
                            break;

                    }
                    delay = false;
                }

                if (files != null)
                {
                    if(files.MoveNext())
                    {
                        Match m = Regex.Match(files.Current, filter);
                        if(m.Success)
                            this.InvokeIfRequired(() => items.Add(new ChangeItem(files.Current, m)));

                        delay = false;
                    }
                }

                if (change != null)
                {
                    if (change.MoveNext())
                    {
                        try
                        {
                            this.InvokeIfRequired(() => {
                                Rename(change.Current, format);
                                listBox1.Refresh();
                            });
                        }
                        catch
                        {

                        }
                    }
                }

                if (delay)
                    Thread.Sleep(100);
            }
        }

        //https://stackoverflow.com/questions/218680/can-i-test-if-a-regex-is-valid-in-c-sharp-without-throwing-exception
        public static bool IsValidRegexPattern(string pattern, string testText = "", int maxSecondTimeOut = 20)
        {
            if (string.IsNullOrEmpty(pattern)) return false;
            
            try {
                Regex re = new Regex(pattern, RegexOptions.None, new TimeSpan(0, 0, maxSecondTimeOut));
                re.IsMatch(testText); 
            }
            catch { return false; } //ArgumentException or RegexMatchTimeoutException
            return true;
        }

        
    }

}

