using System;
using System.Net.Sockets;
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
        private byte[] currentData;
        private int cycleCount = 0;
        private int currentByteCount = 0;

        public MainWindow()
        {
            InitializeComponent();
            btnGenerateTestData_Click(this, null);            
        }
        
        // Reset button click
        private void btnGenerateTestData_Click(object sender, RoutedEventArgs e)
        {            
            // Generate random bytes test data
            currentByteCount = int.Parse(txtTestDataSize.Text);            
            currentData = GenerateRandomBytes(currentByteCount);
            
            // Reset application state
            ResetCycleCount();
            UpdateStatusList(Operation.Clear);
        }

        // Encode button click
        private async void btnEncryptData_Click(object sender, RoutedEventArgs e)
        {
            // Try to execute the encrypt operation
            try
            {
                int result = await ExecuteOperation(Operation.Encode);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());

                UpdateStatusList(Operation.Update, GetStatusMessage("App Error", exception.ToString()));
            }           
        }

        // Decode button click
        private async void btnDecryptData_Click(object sender, RoutedEventArgs e)
        {
            // Try to execute the decrypt operation
            try
            {
                int result = await ExecuteOperation(Operation.Decode);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());

                UpdateStatusList(Operation.Update, GetStatusMessage("App Error", exception.ToString()));
            }            
        }

        // Execute the specified type of operation
        private async Task<int> ExecuteOperation (Operation operation)
        {
            // Disable the UI
            this.IsEnabled = false;

            // Report the attempt status
            UpdateStatusList(Operation.Update, GetStatusMessage("Information", "Requesting to " + operation.ToString() + " " + currentByteCount + " byte(s) of data."));
            
            // Process the request
            int result = await ProcessRequest(operation, int.Parse(txtTimeout.Text));

            // Report the result status
            UpdateStatusList(Operation.Update, GetReportStatus(result, operation));

            // Re-enable the UI
            this.IsEnabled = true;

            UpdateCycleCount(result, operation);            

            return result;
        }

        // Send a request and receive a response
        private async Task<int> ProcessRequest(Operation operation, int timeout)
        {
            await Task.Delay(timeout);
            int result = 0;

            // Create a new request
            ProtocolRequest request = new ProtocolRequest(currentData, operation);

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
                currentData = response.data;
                result = response.Status;

                // Close the stream and TCP client
                stream.Close();
                client.Close();         
            }
            catch (ArgumentNullException exception)
            {
                Console.WriteLine("ArgumentNullException: {0}", exception.ToString());

                UpdateStatusList(Operation.Update, GetStatusMessage("Error", "ArgumentNullException: " + exception.ToString()));
            }
            catch (SocketException exception)
            {
                Console.WriteLine("SocketException: {0}", exception);

                UpdateStatusList(Operation.Update, GetStatusMessage("Error", "SocketException: " + exception.ToString()));
            }
            catch (Exception exception)
            {
                Console.WriteLine("General Exception: {0}", exception);

                UpdateStatusList(Operation.Update, GetStatusMessage("Error", "App error - see output for additional detail."));
            }
            
            return result;
        }

        // Get the appropriate status message from the passed status code and operation
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
                    resultMessage = GetStatusMessage("Error", "Unknown response error");
                    break;
            }           

            return resultMessage;
        }

        // Format a status message based on the message 
        private string GetStatusMessage(string type, string message)
        {
            return DateTime.Now + "\t" + type + "\t" + message;
        }

        // Update the cycle count and cycle count label
        private void UpdateCycleCount(int result, Operation operation)
        {
            
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

            lblEncodeCycleCount.Content = cycleCount + " (Cycle Count)";
        }

        // Reset the cycle count and cycle count label to zero
        private void ResetCycleCount()
        {
            cycleCount = 0;

            lblEncodeCycleCount.Content = cycleCount + " (Cycle Count)";
        }

        // Update or clear content of the status list box
        private void UpdateStatusList(Operation operation, string content = "")
        {
            if (operation == Operation.Update)
            {
                lstStatus.Items.Add(content);
            }
            else if (operation == Operation.Clear)
            {
                lstStatus.Items.Clear();
            }            
        }

        // Generate randomized byte array of the specified size
        private byte[] GenerateRandomBytes(int size)
        {
            Random random = new Random();
            byte[] bytes = new byte[size];
            random.NextBytes(bytes);

            return bytes;
        }
    }
}