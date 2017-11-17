using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CodingChallengeV2Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private byte[] payload;
        private int cycleCount = 0;

        public MainWindow()
        {
            InitializeComponent();
            btnGenerateTestData_Click(this, null);
            
        }
        
        private void btnGenerateTestData_Click(object sender, RoutedEventArgs e)
        {
            payload = Encoding.ASCII.GetBytes("tester");
        }

        private void btnEncryptData_Click(object sender, RoutedEventArgs e)
        {
            lstStatus.Items.Add(DateTime.Now + "\t" + "Information" + "\t" + "Requesting to Encode " + txtTestDataSize.Text + " byte(s) of data.");
            ProcessRequest(Operation.Encode);            
        }

        private void btnDecryptData_Click(object sender, RoutedEventArgs e)
        {
            lstStatus.Items.Add(DateTime.Now + "\t" + "Information" + "\t" + "Requesting to Decode " + txtTestDataSize.Text + " byte(s) of data.");
            ProcessRequest(Operation.Decode);
        }

        private void ProcessRequest(Operation operation)
        {
            ProtocolRequest request = new ProtocolRequest(payload, operation);

            byte[] requestByteArray = request.ToVerifiedBytes().ToArray();
            
            Console.WriteLine(request.ToString());

            try
            {
                
                // Create the TCP client
                Int32 port = Int32.Parse(txtServerPort.Text);
                TcpClient client = new TcpClient("codingchallenge.identityone.net", port);

                // Open the network stream and send the request

                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;

                    NetworkStream stream = client.GetStream();
                    stream.Write(requestByteArray, 0, requestByteArray.Length);

                    // Receive the response
                    ProtocolResponse response = ProtocolResponse.Receive(stream);
                    payload = response.payload;

                    ReportStatus(response.Status, operation);

                    stream.Close();
                    client.Close();
                }).Start();              
            }
            catch (ArgumentNullException exception)
            {
                Console.WriteLine("ArgumentNullException: {0}", exception);
            }
            catch (SocketException exception)
            {
                Console.WriteLine("SocketException: {0}", exception);
            }
        }
        
        private string GetStatusInfo(string type, string message)
        {
            return DateTime.Now + "\t" + type + "\t" + message;
        }

        private void ReportStatus(int status, Operation)
        {
            switch (status)
            {
                case 0:
                    if (operation == Operation.Encode)
                    {
                        cycleCount++;
                    }
                    else if (operation == Operation.Decode)
                    {
                        if (cycleCount > 0)
                        {
                            cycleCount--;
                        }
                    }
                    else
                    {
                        lstStatus.Items.Add(GetStatusInfo("Error", "Unknown operation"));
                    }
                    break;
                case 1:
                    lstStatus.Items.Add(GetStatusInfo("Error", "Invalid header was received"));
                    break;
                case 2:
                    lstStatus.Items.Add(GetStatusInfo("Error", "Unsupported protocol version was received"));
                    break;
                case 3:
                    lstStatus.Items.Add(GetStatusInfo("Error", "Unsupported protocol operation was received"));
                    break;
                case 4:
                    lstStatus.Items.Add(GetStatusInfo("Error", "Timed out waiting for more data / Incomplete data length received"));
                    break;
                case 5:
                    lstStatus.Items.Add(GetStatusInfo("Error", "Maximum request length has been exceeded"));
                    break;
                case 6:
                    lstStatus.Items.Add(GetStatusInfo("Error", "Invalid checksum was received"));
                    break;
                case 7:
                    lstStatus.Items.Add(GetStatusInfo("Error", "Encode operation failed"));
                    break;
                case 8:
                    lstStatus.Items.Add(GetStatusInfo("Error", "Decode operation failed"));
                    break;
                case 9:
                    lstStatus.Items.Add(GetStatusInfo("Error", "Maximum response length after operation exceeds maximum allowed response length"));
                    break;
                default:
                    lstStatus.Items.Add(GetStatusInfo("Error", "Unknown Error (Response)"));
                    break;
            }

            lblEncodeCycleCount.Content = cycleCount + " (Cycle Count)";
        }
    }
}