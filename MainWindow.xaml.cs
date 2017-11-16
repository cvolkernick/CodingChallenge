using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CodingChallengeV2Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private string payload = "";

        public MainWindow()
        {
            InitializeComponent();
            btnGenerateTestData_Click(this, null);
        }
        
        private void btnGenerateTestData_Click(object sender, RoutedEventArgs e)
        {
            payload = "tester";            
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
            
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~");
            Console.WriteLine("REQUEST");
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~");
            Console.WriteLine(request.ToString());

            //for (int i = 0; i < requestByteArray.Length; i++)
            //{
            //    Console.WriteLine(requestByteArray[i]);
            //}

            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~");
            Console.WriteLine("RESPONSE");
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~");

            try
            {
                // Create the TCP client
                Int32 port = 4544;
                TcpClient client = new TcpClient("codingchallenge.identityone.net", port);

                // Open the network stream and send the request
                NetworkStream stream = client.GetStream();
                stream.Write(requestByteArray, 0, requestByteArray.Length);
                Console.WriteLine("Sent: {0}", payload);

                // Receive the response
                ProtocolResponse response = ProtocolResponse.Receive(stream);



                requestByteArray = new Byte[7];

                int length = requestByteArray[3] + requestByteArray[4] + requestByteArray[5];

                requestByteArray = new Byte[length];

                String responseData = String.Empty;

                Int32 bytes = stream.Read(requestByteArray, 0, requestByteArray.Length);

                for (int i = 0; i < requestByteArray.Length; i++)
                {
                    Console.WriteLine(requestByteArray[i]);
                }

                Console.WriteLine(bytes);
                responseData = System.Text.Encoding.ASCII.GetString(requestByteArray, 0, bytes);
                Console.WriteLine("Received: {0}", responseData);

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

        //private byte[] CreatePacket(string s, Operation op)
        //{
        //    var packet = new List<byte>();
        //    packet.AddRange(BitConverter.GetBytes((UInt16)0x1092));
        //    packet.Add(1);
        //    var data = Encoding.ASCII.GetBytes(s);
        //    packet.AddRange(BitConverter.GetBytes((UInt32)(9 + data.Length)));
        //    packet.Add((byte)(op == Operation.Encode ? 1 : 2));
        //    packet.AddRange(data);
        //    packet.Add(CheckSum(packet));
        //    return packet.ToArray();
        //}

        //private byte[] Encode()
        //{
        //    return new byte[1];
        //}

        //private byte[] Decode()
        //{
        //    return new byte[1];
        //}        
    }
}