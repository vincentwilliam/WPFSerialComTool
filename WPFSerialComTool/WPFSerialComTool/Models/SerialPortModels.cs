using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.ComponentModel;

namespace WPFSerialComTool
{
    public class SerialPortModel : SerialPort, INotifyPropertyChanged
    {
        //public SerialPort Port { get; set; }

        //DispatcherTimer

        private string[] portnames;
        /// <summary>
        /// 所有端口号
        /// </summary>
        public string[] PortNames
        {
            get { return portnames; }
            set
            {
                portnames = value;
                RaisePropertyChanged("PortNames");
            }
        }

        private bool issend;
        /// <summary>
        /// 是否发送数据
        /// </summary>
        public bool IsSend
        {
            get { return issend; }
            set
            {
                issend = value;
                RaisePropertyChanged("IsSend");
            }
        }


        private int senddatacounter;

        public int SendDataCounter
        {
            get { return senddatacounter; }
            set
            {
                senddatacounter = value;
                RaisePropertyChanged("SendDataCounter");
            }
        }

        private int sendbytescounter;

        public int SendBytesCounter
        {
            get { return sendbytescounter; }
            set
            {
                sendbytescounter = value;
                RaisePropertyChanged("SendBytesCounter");
            }
        }


        private int receivedatacounter;

        public int ReceiveDataCounter
        {
            get { return receivedatacounter; }
            set
            {
                receivedatacounter = value;
                RaisePropertyChanged("ReceiveDataCounter");
            }
        }

        private int receivebytescounter;

        public int ReceiveBytesCounter
        {
            get { return receivebytescounter; }
            set
            {
                receivebytescounter = value;
                RaisePropertyChanged("ReceiveBytesCounter");
            }
        }


        private int errordatacounter;

        public int ErrorDataCounter
        {
            get { return errordatacounter; }
            set
            {
                errordatacounter = value;
                RaisePropertyChanged("ErrorDataCounter");
            }
        }

        private bool ishexdata;

        public bool IsHexData
        {
            get { return ishexdata; }
            set
            {
                ishexdata = value;
                RaisePropertyChanged("IsHexData");
            }
        }

        private bool isshowdate;

        public bool IsShowDate
        {
            get { return isshowdate; }
            set
            {
                isshowdate = value;
                RaisePropertyChanged("IsShowDate");
            }
        }

        private bool isshowsend;

        public bool IsShowSend
        {
            get { return isshowsend; }
            set
            {
                isshowsend = value;
                RaisePropertyChanged("IsShowSend");
            }
        }



        private string BytesToHexString(byte[] bytes)
        {
            string str = bytes[0].ToString("X2");
            for (int i = 1; i < bytes.Length; i++)
            {
                str = string.Format("{0} {1:X2}", str, bytes[i]);
            }
            return str;
        }

        public string SendDataUpdate(string str)
        {

            if (IsOpen == false)
            {
                throw new Exception("请打开串口！");
            }

            if (str == "")
            {
                throw new Exception("请输入十六进制数！");
            }

            try
            {
                if (IsHexData)//判断是否为十六进制的数
                {
                    str = str.Replace(" ", "");
                    if ((str.Length % 2) != 0)
                    {
                        str = str.Insert(str.Length - 1, "0");
                    }
                    byte[] txData = new byte[str.Length / 2];

                    for (int i = 0; i < txData.Length; i++)
                    {
                        txData[i] = Convert.ToByte(str.Substring(i * 2, 2), 16);
                    }

                    return SendDataUpdate(txData);
                }
                else
                {
                    WriteLine(str);

                    SendDataCounter++;
                    SendBytesCounter += str.Length;

                    if (IsShowSend) str = "[发送] " + str;

                    if (IsShowDate) str = String.Format("[{0:HH:mm:ss.fff}]{1}", DateTime.Now, str);

                    return str;

                }
            }
            catch (Exception)
            {
                throw new Exception("请输入标准hex，必须是标准Hex啊.例如：1F8A 或者: 1E 2F ");
            }


        }

        public string SendDataUpdate(byte[] data)
        {
            if (!IsOpen)
            {
                throw new Exception("端口未打开！");
            }

            Write(data, 0, data.Length);

            IsSend = true;

            SendDataCounter++;
            SendBytesCounter += data.Length;

            string str = BytesToHexString(data);

            if (IsShowSend) str = "[发送] " + str;

            if (IsShowDate) str = String.Format("[{0:HH:mm:ss.fff}]{1}", DateTime.Now, str);

            return str;
        }

        public string ReceiveDataUpdate(byte[] data)
        {
            ReceiveDataCounter++;
            ReceiveBytesCounter += data.Length;

            string str = null;

            if (IsHexData)
            {
                str = BytesToHexString(data);
            }
            else
            {
                str = Encoding.ASCII.GetString(data);
            }

            if (IsShowSend) str = "[接收] " + str;

            if (IsShowDate) str = String.Format("[{0:HH:mm:ss.fff}]{1}", DateTime.Now, str);

            return str;
        }

        public string ReceiveDataUpdate(string str)
        {
            ReceiveDataCounter++;
            ReceiveBytesCounter += str.Length;

            if (IsShowSend) str = "[接收] " + str;

            if (IsShowDate) str = String.Format("[{0:HH:mm:ss.fff}]{1}", DateTime.Now, str);

            return str;
        }

        public string ErrorTextUpdate(byte[] data)
        {
            ErrorDataCounter++;

            string str = BytesToHexString(data);

            if (IsShowSend) str = "[接收] " + str;

            if (IsShowDate) str = String.Format("[{0:HH:mm:ss.fff}]{1}", DateTime.Now, str);

            return str;
        }

        public SerialPortModel()
        {
            PortNames = SerialPort.GetPortNames();

            IsHexData = true;

            IsSend = false;

            IsShowDate = false;

            IsShowSend = false;
        }

        public SerialPortModel(bool ishex = true, bool isshowdate = false, bool isshowsend = false)
        {
            PortNames = SerialPort.GetPortNames();

            IsHexData = ishex;

            IsSend = false;

            IsShowDate = isshowdate;

            IsShowSend = isshowsend;
        }

        public void ClearCounter()
        {
            SendBytesCounter = 0;
            SendDataCounter = 0;
            ReceiveBytesCounter = 0;
            ReceiveDataCounter = 0;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
