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

        private async void btnEncryptData_Click(object sender, RoutedEventArgs e)
        {
            var result = await ExecuteOperation(Operation.Encode);       
        }

        private async void btnDecryptData_Click(object sender, RoutedEventArgs e)
        {
            var result = await ExecuteOperation(Operation.Decode);
        }

        private async Task<int> ExecuteOperation (Operation operation)
        {
            // Disable the UI
            this.IsEnabled = false;

            // Report the attempt status
            lstStatus.Items.Add(GetStatusMessage("Information", "Requesting to " + operation.ToString() + txtTestDataSize.Text + " byte(s) of data."));
            
            // Process the request
            var result = await ProcessRequest(operation, Int32.Parse(txtTimeout.Text));

            // Report the result status
            lstStatus.Items.Add(GetReportStatus(result, operation));

            // Re-enable the UI
            this.IsEnabled = true;

            if (result == 0)
            {
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
            }

            return result;
        }

        private async Task<int> ProcessRequest(Operation operation, int timeout)
        {
            await Task.Delay(timeout);
            int result = 0;

            // Create a new request
            ProtocolRequest request = new ProtocolRequest(payload, operation);

            // Get verified (calculated checksum) request bytes
            byte[] requestByteArray = request.ToVerifiedBytes().ToArray();
            
            Console.WriteLine(request.ToString());

            try
            {                
                // Create the TCP client
                Int32 port = Int32.Parse(txtServerPort.Text);
                TcpClient client = new TcpClient("codingchallenge.identityone.net", port);

                // Open the network stream and send the request
                NetworkStream stream = client.GetStream();
                stream.Write(requestByteArray, 0, requestByteArray.Length);

                // Receive the response
                ProtocolResponse response = ProtocolResponse.Receive(stream);

                // Update the current payload & response status
                payload = response.payload;
                result = response.Status;

                // Close the stream and TCP client
                stream.Close();
                client.Close();         
            }
            catch (ArgumentNullException exception)
            {
                Console.WriteLine("ArgumentNullException: {0}", exception);
            }
            catch (SocketException exception)
            {
                Console.WriteLine("SocketException: {0}", exception);
            }
            
            return result;
        }

        private string GetReportStatus(int status, Operation operation)
        {
            string resultMessage = "";

            switch (status)
            {
                case 0:
                    if (operation == Operation.Encode || operation == Operation.Decode)
                    {
                        resultMessage = GetStatusMessage("Information", "Request completed successfully");
                    }
                    else
                    {
                        resultMessage = GetStatusMessage("Error", "Unknown operation");
                    }                    
                    break;
                case 1:
                    resultMessage = GetStatusMessage("Error", "Invalid header was received");
                    break;
                case 2:
                    resultMessage = GetStatusMessage("Error", "Unsupported protocol version was received");
                    break;
                case 3:
                    resultMessage = GetStatusMessage("Error", "Unsupported protocol operation was received");
                    break;
                case 4:
                    resultMessage = GetStatusMessage("Error", "Timed out waiting for more data / Incomplete data length received");
                    break;
                case 5:
                    resultMessage = GetStatusMessage("Error", "Maximum request length has been exceeded");
                    break;
                case 6:
                    resultMessage = GetStatusMessage("Error", "Invalid checksum was received");
                    break;
                case 7:
                    resultMessage = GetStatusMessage("Error", "Encode operation failed");
                    break;
                case 8:
                    resultMessage = GetStatusMessage("Error", "Decode operation failed");
                    break;
                case 9:
                    resultMessage = GetStatusMessage("Error", "Maximum response length after operation exceeds maximum allowed response length");
                    break;
                default:
                    resultMessage = GetStatusMessage("Error", "Unknown Error (Response)");
                    break;
            }

            lblEncodeCycleCount.Content = cycleCount + " (Cycle Count)";

            return resultMessage;
        }

        private string GetStatusMessage(string type, string message)
        {
            return DateTime.Now + "\t" + type + "\t" + message;
        }
    }
}