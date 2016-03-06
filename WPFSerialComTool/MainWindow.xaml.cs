using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Windows.Threading;
using Microsoft.Win32;
using System.IO;

namespace WPFSerialComTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public SerialPortModel Port { get; set; }

        public bool IsRecSend1 { get; set; }

        public bool IsRecSend2 { get; set; }

        public bool IsStop { get; set; }

        private DispatcherTimer sendTimer1 = new DispatcherTimer();

        private DispatcherTimer sendTimer2 = new DispatcherTimer();


        public MainWindow()
        {

            InitializeComponent();

            Port = new SerialPortModel();

            Port.DataReceived += new SerialDataReceivedEventHandler(Port_DataReceived);

            string[] DelayTimes = { "0", "500", "1000", "1500", "2000", "3000", "4000", "5000", "6000", "7000", "8000", "9000", "10000" };

            cmbSend1.ItemsSource = DelayTimes;
            cmbSend2.ItemsSource = DelayTimes;

            this.DataContext = this;

            cmbPortName.SelectedIndex = Properties.Settings.Default.PortName;
            cmbBaudRate.SelectedIndex = Properties.Settings.Default.BaudRate;
            txtSend1.Text = Properties.Settings.Default.Send1;
            txtSend2.Text = Properties.Settings.Default.Send2;

            btnOpenPort.Click += delegate (object sender, RoutedEventArgs e)
            {
                if ((bool)btnOpenPort.IsChecked)
                {
                    Port.PortName = (string)cmbPortName.SelectedItem;
                    Port.BaudRate = Convert.ToInt32(cmbBaudRate.SelectedItem);

                    try
                    {
                        Port.Open();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        btnOpenPort.IsChecked = false;
                        return;
                    }

                    cmbBaudRate.IsEnabled = false;
                    cmbDataBit.IsEnabled = false;
                    cmbPortName.IsEnabled = false;
                    cmbParity.IsEnabled = false;
                    cmbStopBit.IsEnabled = false;


                }
                else
                {
                    Port.Close();

                    cmbBaudRate.IsEnabled = true;
                    cmbDataBit.IsEnabled = true;
                    cmbPortName.IsEnabled = true;
                    cmbParity.IsEnabled = true;
                    cmbStopBit.IsEnabled = true;
                }
            };


            btnClearText.Click += delegate (object sender, RoutedEventArgs e)
            {
                //txtRecData.Clear();
                txtRecData.Document.Blocks.Clear();
            };

            btnClearZero.Click += delegate (object sender, RoutedEventArgs e)
            {
                Port.ClearCounter();
            };

            btnSave.Click += delegate (object sender, RoutedEventArgs e)
            {
                SaveFileDialog sfd = new SaveFileDialog()
                {
                    Filter = "文本|*.log"
                };
                if (sfd.ShowDialog() == true)
                {

                    //File.WriteAllText(sfd.FileName, txtRecData.Text);       
                    //using (FileStream stream = File.OpenWrite(sfd.FileName))
                    //{
                    //    TextRange documentTextRange = new TextRange(txtRecData.Document.ContentStart, txtRecData.Document.ContentEnd);
                    //    string dataFormat = DataFormats.Text;
                    //    string ext = System.IO.Path.GetExtension(sfd.FileName);
                    //    if (String.Compare(ext, ".xaml", true) == 0)
                    //    {
                    //        dataFormat = DataFormats.Xaml;
                    //    }
                    //    else if (String.Compare(ext, ".rtf", true) == 0)
                    //    {
                    //        dataFormat = DataFormats.Rtf;
                    //    }
                    //    documentTextRange.Save(stream, dataFormat);
                    //}

                    TextRange textRange = new TextRange(txtRecData.Document.ContentStart, txtRecData.Document.ContentEnd);
                    File.WriteAllText(sfd.FileName, textRange.Text);
                }
            };


            btnSend1.Click += delegate (object sender, RoutedEventArgs e)
            {
                SendData(txtSend1.Text);
            };

            cmbSend1.SelectionChanged += delegate (object sender, SelectionChangedEventArgs e)
            {
                int time = Convert.ToInt32(cmbSend1.SelectedValue);
                if (time > 0)
                {
                    sendTimer1.Interval = new TimeSpan(time * 10000);
                    sendTimer1.Start();
                }
                else
                {
                    sendTimer1.Stop();
                }
            };

            sendTimer1.Tick += delegate (object sender, EventArgs e)
            {
                SendData(txtSend1.Text);
            };


            btnSend2.Click += delegate (object sender, RoutedEventArgs e)
            {
                SendData(txtSend2.Text);
            };

            cmbSend2.SelectionChanged += delegate (object sender, SelectionChangedEventArgs e)
            {
                int time = Convert.ToInt32(cmbSend2.SelectedValue);
                if (time > 0)
                {
                    sendTimer2.Interval = new TimeSpan(time * 10000);
                    sendTimer2.Start();
                }
                else
                {
                    sendTimer2.Stop();
                }
            };

            sendTimer2.Tick += delegate (object sender, EventArgs e)
            {
                SendData(txtSend2.Text);
            };


            txtRecData.TextChanged += delegate (object sender, TextChangedEventArgs e)
            {
                txtRecData.ScrollToEnd();
            };

            this.Closing += delegate (object sender, System.ComponentModel.CancelEventArgs e)
            {
                Properties.Settings.Default.PortName = cmbPortName.SelectedIndex;
                Properties.Settings.Default.BaudRate = cmbBaudRate.SelectedIndex;
                Properties.Settings.Default.Send1 = txtSend1.Text;
                Properties.Settings.Default.Send2 = txtSend2.Text;
                Properties.Settings.Default.Save();
            };

            this.Activated += delegate (object sender, EventArgs e)
            {
                string str = Clipboard.GetText();
                Clipboard.SetText(str.Replace("\r\n", ""));
            };
        }




        private void SendData(string str)
        {
            if (IsStop || !Port.IsShowSend)
            {
                Port.SendDataUpdate(str);
            }
            else
            {
                //txtRecData.AppendText(Port.SendDataUpdate(str) + Environment.NewLine);

                //Paragraph p = new Paragraph();
                //p.Foreground = Brushes.Blue;
                //Run r = new Run(Port.SendDataUpdate(str));
                //r.Foreground = Brushes.Blue;
                //p.Inlines.Add(r);
                //txtRecData.Document.Blocks.Add(p);  

                Paragraph p = (Paragraph)txtRecData.Document.Blocks.FirstBlock;
                Run r = new Run(Port.SendDataUpdate(str) + Environment.NewLine);
                r.Foreground = Brushes.DarkBlue;
                p.Inlines.Add(r);
            }
        }



        public delegate void UpdateBytesDelegate(byte[] data);
        void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int length = Port.BytesToRead;
            byte[] data = new byte[length];
            for (int i = 0; i < length; i++)
            {
                data[i] = (byte)Port.ReadByte();
            }

            Dispatcher.Invoke(new UpdateBytesDelegate(UpdateBytesbox), data);


        }

        private void UpdateBytesbox(byte[] data)
        {
            if (IsStop)
            {
                Port.ReceiveDataUpdate(data);
            }
            else
            {
                //txtRecData.AppendText(Port.ReceiveDataUpdate(data) + Environment.NewLine);
                Paragraph p = (Paragraph)txtRecData.Document.Blocks.FirstBlock;
                Run r = new Run(Port.ReceiveDataUpdate(data) + Environment.NewLine);
                r.Foreground = Brushes.DarkRed;
                p.Inlines.Add(r);
            }

            if (IsRecSend1)
            {
                SendData(txtSend1.Text);
            }
            else if (IsRecSend2)
            {
                SendData(txtSend2.Text);
            }

        }



    }
}
