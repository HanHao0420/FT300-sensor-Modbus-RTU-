using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EasyModbus;

namespace FT300_sensor
{
    public partial class FT300_Sensor : Form
    {
     //Modbus method:
        ModbusClient modbusClient;

        float Fx = 0.0F;
        float Fy = 0.0F;
        float Fz = 0.0F;
        float Mx = 0.0F;
        float My = 0.0F;
        float Mz = 0.0F;

        float tempFx = 0.0F;
        float tempFy = 0.0F;
        float tempFz = 0.0F;
        float tempMx = 0.0F;
        float tempMy = 0.0F;
        float tempMz = 0.0F;

        //SerialPort method:
        //Request = "09;03;00;B4;00;01;C5;64" for getting Fx
        //Request = "09;03;00;B4;00;06;84;A6" for getting all quantity
        //byte[] request = new byte[] {0x09, 0x03, 0x00, 0xB4, 0x00, 0x06, 0x84, 0xA6};
        //byte[] receive;
        //int NumberofByte = 0;

        public FT300_Sensor()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      //COMport Setting Region:
       //SerialPort:
        private void FT300Port_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {

        }

      //COM setting:
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }
        void getavailableports() // get available COM
        {
            comboBox1.Items.Clear();
            string[] ports = SerialPort.GetPortNames();
            comboBox1.Items.AddRange(ports);
        }
        private void comboBox1_MouseClick(object sender, MouseEventArgs e) //let user choose COM
        {
            getavailableports();
        }

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      //This region Contains all the functions of Buttons:
        //Start Button:
        private void Start_Click(object sender, EventArgs e) // Start button being pressed
        {
            try
            {
                Invoke(new EventHandler(ChangeColor));

              //SerialPort Method: (CTRL+K C; CTRL+K U)
                //FT300Port.PortName = comboBox1.Text;
                //FT300Port.BaudRate = Convert.ToInt32(BaudRate.Text);
                //FT300Port.Parity = Parity.None;
                //FT300Port.DataBits = 8;
                //FT300Port.StopBits = StopBits.One;
                //FT300Port.Open();
                //System.Threading.Thread.Sleep(500);

              //EasyModbus.dll:
                modbusClient = new ModbusClient(comboBox1.Text); // COM3
                modbusClient.UnitIdentifier = 9; // slave ID = 9 (default slaveID = 1)
                modbusClient.Baudrate = Convert.ToInt32(BaudRate.Text); // Baud Rate = 19200 (default baudrate = 9600)
                modbusClient.Parity = Parity.None; // Parity = None
                modbusClient.StopBits = StopBits.One; // StopBits = 1
                modbusClient.ConnectionTimeout = 500;
                modbusClient.Connect();

                lb_status.Text = "Connected";

              //Timer for data reading:
                timer_Modbus.Enabled = true;


            }
            catch(Exception ex) //catch error message
            {
                lb_status.Text = ex.ToString();
                throw;
            }
        }
        private void ChangeColor(object sender, EventArgs e)
        {
            Start.Text = "Streaming";
            Start.BackColor = Color.Red;
        }

      //Disconnect Button:
        private void Disconnect_Click(object sender, EventArgs e)
        {
          //Modbus Part:
            modbusClient.Disconnect();

          //SerialPort Part:
            //FT300Port.Close();

          //Change the prefix of Start button:
            Start.Text = "Start";
            Start.BackColor = Color.DarkGray;
            lb_status.Text = "Disconnected";

            //Change the prefix of Tamper button:
            Tamper_To_Zero.Text = "Tamper To Zero";

          //Timer disable:
            timer_Modbus.Enabled = false;
        }
       
       //Tamper the Value of FT300 to Zero:
        private void Tamper_To_Zero_Click(object sender, EventArgs e)
        {
            Tamper_To_Zero.Text = "Tampering";
            tempFx = Fx;
            tempFy = Fy;
            tempFz = Fz;
            tempMx = Mx;
            tempMy = My;
            tempMz = Mz;
        }

       //This Button will send about 50 0xff to the sensor which will disable the Data Stream Mode:
        private void Convert_Mode_Click(object sender, EventArgs e)
        {
            if (FT300Port.BaseStream != null)
            {
                try
                {
                    byte[] off = new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };
                    FT300Port.Write(off, 0, off.Length);
                }
                catch (Exception ex)
                {
                    lb_status.Text = ex.ToString();
                    throw;
                }
            }
            else
            {
                lb_status.Text = "The Mode of FT300 sensor is ready for Modbus RTU!!!";
            }
        }

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
       //Timer for data reading:
        private void timer_Modbus_Tick(object sender, EventArgs e)
        {
            timer_Modbus.Enabled = false;

          //Modbus Timer Part:
            int[] Read = modbusClient.ReadHoldingRegisters(180, 6); // ReadHoldingRegister from 180, Total registers read = 6

            Fx = Convert.ToSingle(Read[0]) / 100;
            Fy = Convert.ToSingle(Read[1]) / 100;
            Fz = Convert.ToSingle(Read[2]) / 100;
            Mx = Convert.ToSingle(Read[3]) / 1000;
            My = Convert.ToSingle(Read[4]) / 1000;
            Mz = Convert.ToSingle(Read[5]) / 1000;

            if (Tamper_To_Zero.Text == "Tampering")
            {
                Fx -= tempFx;
                Fy -= tempFy;
                Fz -= tempFz;
                Mx -= tempMx;
                My -= tempMy;
                Mz -= tempMz;

                textBox1.Text = Convert.ToString(Math.Round(Fx,2)); 
                textBox2.Text = Convert.ToString(Math.Round(Fy,2));
                textBox3.Text = Convert.ToString(Math.Round(Fz,2));
                textBox4.Text = Convert.ToString(Math.Round(Mx,3));
                textBox5.Text = Convert.ToString(Math.Round(My,3));
                textBox6.Text = Convert.ToString(Math.Round(Mz,3));
            }
            else
            {
                textBox1.Text = Convert.ToString(Fx);
                textBox2.Text = Convert.ToString(Fy);
                textBox3.Text = Convert.ToString(Fz);
                textBox4.Text = Convert.ToString(Mx);
                textBox5.Text = Convert.ToString(My);
                textBox6.Text = Convert.ToString(Mz);
            }

          //Serial Timer Part:
            //FT300Port.Write(readHoldingRegister(0x09, 0x00B4, 6), 0, 8);
            //FT300Port.Write(request, 0, request.Length);
            //byte[] response = ReadExact(FT300Port.BaseStream, 1);
            //textBox2.Text = Convert.ToString(response[0]);
            //for (int i = 0; i < response.Length; i++)
            //{
            //    //float FT = Convert.ToSingle(response[i]) / 100;
            //    textBox1.Text = Convert.ToString(response[i]);
            //}

            timer_Modbus.Enabled = true;
        }

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
     //The Following Part is the Demonstration of functions used to reach the data of Modbus without utilizing any Library
        public static byte[] ReadExact(System.IO.Stream s, int nbytes)
        {
            byte[] buf = new byte[nbytes];
            var readpos = 0;

            while (readpos < nbytes)
            {
                readpos += s.Read(buf, readpos, nbytes - readpos);
            }
            return buf;
        }

      //FT300 data port is, in the case of Robotiq products, based on the Little Endian byte order
        public static byte[] readHoldingRegister(int id, int startAddress, int Length)
        {
            byte[] data = new byte[8];
            byte High, Low;

            data[0] = Convert.ToByte(id);
            data[1] = Convert.ToByte(3);

            byte[] _adr = BitConverter.GetBytes(startAddress);

            data[2] = _adr[1];
            data[3] = _adr[0];

            byte[] _length = BitConverter.GetBytes(Length);

            data[4] = _length[1];
            data[5] = _length[0];

            CRCgenerate(data, 6, out High, out Low);

            data[6] = Low;
            data[7] = High;

            //Array.Reverse(data);
            return data;
        }

        public static void CRCgenerate(byte[] message, int length, out byte CRCHigh, out byte CRCLow)
        {
            ushort CRCFull = 0xFFFF;
            for (int i = 0; i < length; i++)
            {
                CRCFull = (ushort)(CRCFull ^ message[i]);
                for (int j = 0; j < 8; j++)
                {
                    if ((CRCFull & 0x0001) == 0)
                        CRCFull = (ushort)(CRCFull >> 1);
                    else
                    {
                        CRCFull = (ushort)((CRCFull >> 1) ^ 0xA001);
                    }
                }
            }
            CRCHigh = (byte)((CRCFull >> 8) & 0xFF);
            CRCLow = (byte)(CRCFull & 0xFF);
        }

      //TextBoxes Region:
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }
       
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      // Other form parts:
        private void FT300_Value_Enter(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

    }
}
