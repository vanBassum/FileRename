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
using System.Windows.Forms.VisualStyles;

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

                    e.Graphics.DrawString(item.Source,
                        font, Brushes.Black, e.Bounds.X + 30, e.Bounds.Y);

                    e.Graphics.DrawString("DST",
                        font, Brushes.Black, e.Bounds.X, e.Bounds.Y + 15);

                    if (item.Destination != null)
                    {
                        e.Graphics.DrawString(item.Destination,
                            font, Brushes.Black, e.Bounds.X + 30, e.Bounds.Y + 15);
                    }

                    CheckBoxRenderer.DrawCheckBox(e.Graphics,
                                                    new Point(e.Bounds.X + e.Bounds.Width - 50, e.Bounds.Y + 7),
                                                    item.Moved ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal);


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
           
            if (IsValidRegexPattern(textBox1.Text))
            {
                textBox1.BackColor = Color.White;
                work.Enqueue(new FilterAction(textBox1.Text, richTextBox1.Text));
            }
            else
                textBox1.BackColor = Color.IndianRed;
            
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1_TextChanged(null, null);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (sender is TextBox tb)
            {
                work.Enqueue(new RenameAction(tb.Text));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            listBox1.Enabled = false;
            button1.Enabled = false;

            work.Enqueue(new MoveAction(()=> {
                textBox1.Enabled = true;
                textBox2.Enabled = true;
                listBox1.Enabled = true;
                button1.Enabled =  true;
            }));
        }


        void Work()
        {
            IEnumerator<string> files = null;
            IEnumerator<ChangeItem> rename = null;
            IEnumerator<ChangeItem> move = null;
            Action renameDoneCallback = null;
            string filter = "*";
            string format = "";
            bool delay = true;

            while (true)
            {
                if (work.TryDequeue(out IEvent ev))
                {

                    switch (ev)
                    {
                        case FilterAction filterEvent:
                            this.InvokeIfRequired(() => items.Clear());
                            filter = filterEvent.Value;
                            files = Directory.EnumerateFiles(@"C:\Users\Bas\Desktop\TEST", "*", SearchOption.AllDirectories).GetEnumerator();
                            if (rename != null)
                                rename = items.GetEnumerator();
                            break;

                        case RenameAction destinationChangedEvent:
                            format = destinationChangedEvent.Value;
                            rename = items.GetEnumerator();
                            break;

                        case MoveAction startRenameEvent:
                            renameDoneCallback = startRenameEvent.FinishedCallback;
                            move = items.GetEnumerator();
                            break;

                    }
                    delay = false;
                }



                if (files != null)
                {
                    try
                    {
                        if (files.MoveNext())
                        {
                            Match m = Regex.Match(files.Current, filter);
                            if (m.Success)
                            {
                                var ci = new ChangeItem(files.Current, m);
                                FileAttributes attributes = File.GetAttributes(ci.Source);
                                FileInfo fi = new FileInfo(ci.Source);
                                this.InvokeIfRequired(() => items.Add(ci));
                            }

                            delay = false;
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        
                    }
                    catch { }
                }

                if (rename != null)
                {
                    try
                    {
                        if (rename.MoveNext())
                        {
                            this.InvokeIfRequired(() =>
                            {
                                Rename(rename.Current, format);
                                listBox1.Refresh();
                            });
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        rename = items.GetEnumerator();
                    }
                    catch { }
                }

                if (move != null)
                {
                    try
                    {
                        
                        if (move.MoveNext())
                        {
                            MoveFile(move.Current);
                            this.InvokeIfRequired(() =>
                            {
                                listBox1.Refresh();
                            });
                        }
                        else
                        {
                            if(renameDoneCallback != null)
                            {
                                this.InvokeIfRequired(()=>renameDoneCallback());
                                renameDoneCallback = null;
                            }
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        
                    }
                    catch { }
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

        


        void MoveFile(ChangeItem changeItem)
        {
            string dir = Path.GetDirectoryName(changeItem.Destination);
            Directory.CreateDirectory(dir);
            File.Move(changeItem.Source, changeItem.Destination);
            changeItem.Moved = true;
        }

        void Rename(ChangeItem changeItem, string format)
        {
            string result = format;
            Match m;

            while((m = Regex.Match(result, @"{(\d+)}")).Success)
            {
                int groupNo = int.Parse(m.Groups[1].Value);
                string buf = result.Substring(0, m.Index);
                buf += changeItem.Match.Groups[groupNo+1].Value;
                buf += result.Substring(m.Index + m.Length);
                result = buf;
            }

            changeItem.Destination = result;
        }



        void RenameOld(ChangeItem changeItem, string format)
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
                        else if ((m = Regex.Match(format, "\\d+")).Success)
                        {
                            if (int.TryParse(m.Value, out int groupNo))
                            {
                                if (groupNo < changeItem.Match.Groups.Count)
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
                    if (ind < 0)
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

            changeItem.Destination = result;
        }

        
    }

}

