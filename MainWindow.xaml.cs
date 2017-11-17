using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
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

        public MainWindow()
        {
            InitializeComponent();
            btnGenerateTestData_Click(this, null);
        }
        
        private void btnGenerateTestData_Click(object sender, RoutedEventArgs e)
        {
            //byte[] randomBytes = new byte[Convert.ToInt32(txtTestDataSize.Text)];
            //Random rnd = new Random();
            //rnd.NextBytes(randomBytes);

            //payload = randomBytes;

            payload = Encoding.ASCII.GetBytes("tester");
        }

        private void btnEncryptData_Click(object sender, RoutedEventArgs e)
        {
            ProcessRequest(Operation.Encode);
        }

        private void btnDecryptData_Click(object sender, RoutedEventArgs e)
        {
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
                Int32 port = 4544;
                TcpClient client = new TcpClient("codingchallenge.identityone.net", port);

                // Open the network stream and send the request
                NetworkStream stream = client.GetStream();
                stream.Write(requestByteArray, 0, requestByteArray.Length);

                // Receive the response
                ProtocolResponse response = ProtocolResponse.Receive(stream);
                payload = response.payload;

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
        }      
    }
}