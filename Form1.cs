using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

// This is the code for your desktop app.
// Press Ctrl+F5 (or go to Debug > Start Without Debugging) to run your app.

namespace SerialCommunication
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();


            // We use these three SQLite objects:

            SQLiteConnection sqlite_conn;
            SQLiteCommand sqlite_cmd;
            SQLiteDataReader sqlite_datareader;
            string filePath = ConfigurationManager.AppSettings["FilePath"];
            // create a new database connection:

            //sqlite_conn = new SQLiteConnection($@"Data Source={filePath}database.db;Version=3;New=True;Compress=True;");

            //if (!File.Exists("database.db"))
            //    // open the connection:
            //    sqlite_conn.Open();
            //// create a new SQL command:
            //sqlite_cmd = sqlite_conn.CreateCommand();
            //// Let the SQLiteCommand object know our SQL-Query:
            ////sqlite_cmd.CommandText = "CREATE TABLE test (id integer primary key, text varchar(100));";
            //// Now lets execute the SQL ;D
            ////sqlite_cmd.ExecuteNonQuery();
            //// Lets insert something into our new table:
            //sqlite_cmd.CommandText = "INSERT INTO test (id, text) VALUES (7, 'Test Text 1');";
            //// And execute this again ;D
            //sqlite_cmd.ExecuteNonQuery();
            //// ...and inserting another line:
            //sqlite_cmd.CommandText = "INSERT INTO test (id, text) VALUES (8, 'Test Text 2');";
            //// And execute this again ;D
            //sqlite_cmd.ExecuteNonQuery();
            //// But how do we read something out of our table ?
            //// First lets build a SQL-Query again:
            //sqlite_cmd.CommandText = "SELECT * FROM test";
            //// Now the SQLiteCommand object can give us a DataReader-Object:
            //sqlite_datareader = sqlite_cmd.ExecuteReader();
            //// The SQLiteDataReader allows us to run through the result lines:
            //while (sqlite_datareader.Read()) // Read() returns true if there is still a result line to read
            //{
            //    // Print out the content of the text field:
            //    //System.Console.WriteLine( sqlite_datareader["text"] );
            //    string myreader = sqlite_datareader.GetString(0);
            //    MessageBox.Show(myreader);
            //}
            //// We are ready, now lets cleanup and close our connection:
            //sqlite_conn.Close();



            

            try
            {
                using (SerialComEntities db = new SerialComEntities())
                {

                }
            }
            catch (Exception)
            {
                MessageBox.Show(this, "Could not open the connection with database. Most likely it is not installed, or is unavailable.", "SQL Express unavailable", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }


            //CreateDatabase(conn, dbName);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cmbBaudRate.SelectedIndex = 3;
            cmbParity.SelectedIndex = 0;
            cmbDataBits.SelectedIndex = 2;
            cmbStopBits.SelectedIndex = 0;
            updatePorts();           //Call this function everytime the page load 
                                     //to update port names
        }
        private void updatePorts()
        {
            // Retrieve the list of all COM ports on your Computer
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                cmbPortName.Items.Add(port);
            }
        }
        private SerialPort ComPort = new SerialPort();  //Initialise ComPort Variable as SerialPort
        private void connect()
        {
            bool error = false;

            // Check if all settings have been selected

            if (cmbPortName.SelectedIndex != -1 & cmbBaudRate.SelectedIndex != -1 & cmbParity.SelectedIndex != -1 & cmbDataBits.SelectedIndex != -1 & cmbStopBits.SelectedIndex != -1)
            {
                //if yes than Set The Port's settings
                ComPort.PortName = cmbPortName.Text;
                ComPort.BaudRate = int.Parse(cmbBaudRate.Text);      //convert Text to Integer
                ComPort.Parity = (Parity)Enum.Parse(typeof(Parity), cmbParity.Text); //convert Text to Parity
                ComPort.DataBits = int.Parse(cmbDataBits.Text);        //convert Text to Integer
                ComPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cmbStopBits.Text);  //convert Text to stop bits

                try  //always try to use this try and catch method to open your port. 
                     //if there is an error your program will not display a message instead of freezing.
                {
                    //Open Port
                    ComPort.Open();
                }
                catch (UnauthorizedAccessException) { error = true; }
                catch (System.IO.IOException) { error = true; }
                catch (ArgumentException) { error = true; }

                if (error) MessageBox.Show(this, "Could not open the COM port. Most likely it is already in use, has been removed, or is unavailable.", "COM Port unavailable", MessageBoxButtons.OK, MessageBoxIcon.Stop);

            }
            else
            {
                MessageBox.Show("Please select all the COM Serial Port Settings", "Serial Port Interface", MessageBoxButtons.OK, MessageBoxIcon.Stop);

            }
            //if the port is open, Change the Connect button to disconnect, enable the send button.
            //and disable the groupBox to prevent changing configuration of an open port.
            if (ComPort.IsOpen)
            {
                btnConnect.Text = "Disconnect";
                btnSend.Enabled = true;
                if (!rdText.Checked & !rdHex.Checked)  //if no data mode is selected, then select text mode by default
                {
                    rdText.Checked = true;
                }
                groupBox1.Enabled = false;


            }
        }
        // Call this function to close the port.
        private void disconnect()
        {
            ComPort.Close();
            btnConnect.Text = "Connect";
            btnSend.Enabled = false;
            groupBox1.Enabled = true;

        }
        //whenever the connect button is clicked, it will check if the port is already open, call the disconnect function.
        // if the port is closed, call the connect function.
        private void btnConnect_Click(object sender, EventArgs e)

        {
            if (ComPort.IsOpen)
            {
                disconnect();
            }
            else
            {
                connect();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            //Clear the screen
            rtxtDataArea.Clear();
            txtSend.Clear();
        }
        // a function to send data to the serial port
        private void sendData()
        {
            bool error = false;
            if (rdText.Checked == true)        //if text mode is selected, send data as tex
            {
                // Send the user's text straight out the port
                ComPort.Write(txtSend.Text);

                // Show in the terminal window 
                rtxtDataArea.ForeColor = Color.Green;    //write text data in Green
                rtxtDataArea.AppendText(txtSend.Text + "\n");
                txtSend.Clear();                       //clear screen after sending data

            }
            else                    //if Hex mode is selected, send data in hexadecimal
            {
                try
                {
                    // Convert the user's string of hex digits (example: E1 FF 1B) to a byte array
                    byte[] data = HexStringToByteArray(txtSend.Text);

                    // Send the binary data out the port
                    ComPort.Write(data, 0, data.Length);

                    // Show the hex digits on in the terminal window
                    rtxtDataArea.ForeColor = Color.Blue;   //write Hex data in Blue
                    rtxtDataArea.AppendText(txtSend.Text.ToUpper() + "\n");
                    txtSend.Clear();                       //clear screen after sending data
                }
                catch (FormatException) { error = true; }

                // Inform the user if the hex string was not properly formatted
                catch (ArgumentException) { error = true; }

                if (error) MessageBox.Show(this, "Not properly formatted hex string: " + txtSend.Text + "\n", "Format Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);

            }
        }
        //Convert a string of hex digits (example: E1 FF 1B) to a byte array. 
        //The string containing the hex digits (with or without spaces)
        //Returns an array of bytes. </returns>
        private byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }

        //Converts an array of bytes into a formatted string of hex digits (example: E1 FF 1B)
        //The array of bytes to be translated into a string of hex digits. 
        //Returns a well formatted string of hex digits with spacing. 
        private string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
            return sb.ToString().ToUpper();
        }
        private void btnSend_Click(object sender, EventArgs e)
        {
            sendData();
        }
        //This event will be raised when the form is closing.
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ComPort.IsOpen) ComPort.Close();  //close the port if open when exiting the application.
        }
        // when data is received on the port, it will raise this event 
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string recievedData = serialPort1.ReadExisting(); //read all available data in the receiving buffer.

            // Show in the terminal window 
            rtxtDataArea.ForeColor = Color.Green;    //write text data in Green
            rtxtDataArea.AppendText(recievedData + "\n");




            //string[] arr = recievedData.Split(',');

            //decimal length = decimal.TryParse(arr[0].Split('=')[1], out decimal length_dec) ? length_dec : 0;
            //decimal width = decimal.TryParse(arr[1].Split('=')[1], out decimal width_dec) ? width_dec : 0;
            //decimal height = decimal.TryParse(arr[2].Split('=')[1], out decimal height_dec) ? height_dec : 0;
            //decimal weight = decimal.TryParse(arr[3].Split('=')[1], out decimal weight_dec) ? weight_dec : 0;
            //string barcode = arr[4].Split('=')[1].ToString();
            //string machineId = arr[5].Split('=')[1].ToString();

            //lblLengthVal.Text = length.ToString();
            //lblWidthVal.Text = width.ToString();
            //lblHeightVal.Text = height.ToString();
            //lblWeightVal.Text = weight.ToString();





            //string tempString = "Length=25.2,Width=24.8,Hieght=25.7,Weigth=12.8,BarCode=1234561234561,MachineId=001,Flag=1";

            //if (tempString.Contains("Flag=1"))
            //{
            //    string[] arr = tempString.Split(',');

            //    using (SqlConnection conn = new SqlConnection(str))
            //    {
            //        String query = "INSERT INTO CubeData (dbo.table.ID, dbo.table.secondvar) VALUES (@ID, @secondvar)";
            //        using (SqlCommand cmd = new SqlCommand(query, conn))
            //        {
            //            cmd.Parameters.AddWithValue("@Length", arr[0].Split('=')[1]);
            //            cmd.Parameters.AddWithValue("@Width", arr[1].Split('=')[1]);
            //            cmd.Parameters.AddWithValue("@Hieght", arr[2].Split('=')[1]);
            //            cmd.Parameters.AddWithValue("@Weigth", arr[3].Split('=')[1]);
            //            cmd.Parameters.AddWithValue("@BarCode", arr[4].Split('=')[1]);
            //            cmd.Parameters.AddWithValue("@MachineId", arr[5].Split('=')[1]);
            //            conn.Open();
            //            int result = cmd.ExecuteNonQuery();

            //        }
            //    }
            //}





            //string tempString = "Length=25.2,Width=24.8,Height=25.7,Weight=12.8,BarCode=1234561234561,MachineId=001,Flag=1";

            if (recievedData.Contains("Flag=1"))
            {
                string[] arr = recievedData.Split(',');

                using (SerialComEntities entities = new SerialComEntities())
                {
                    decimal length = decimal.TryParse(arr[0].Split('=')[1], out decimal length_dec) ? length_dec : 0;
                    decimal width = decimal.TryParse(arr[1].Split('=')[1], out decimal width_dec) ? width_dec : 0;
                    decimal height = decimal.TryParse(arr[2].Split('=')[1], out decimal height_dec) ? height_dec : 0;
                    decimal weight = decimal.TryParse(arr[3].Split('=')[1], out decimal weight_dec) ? weight_dec : 0;
                    string barcode = arr[4].Split('=')[1].ToString();
                    string machineId = arr[5].Split('=')[1].ToString();

                    lblLengthVal.Text = length.ToString();
                    lblWidthVal.Text = width.ToString();
                    lblHeightVal.Text = height.ToString();
                    lblWeightVal.Text = weight.ToString();


                    CubeData new_data = new CubeData()
                    {
                        Guid = new Guid().ToString(),
                        RawValue = recievedData,
                        Length = length,
                        Width = width,
                        Height = height,
                        Weight = weight,
                        BarCode = barcode,
                        MachineId = machineId,
                        DateTime = DateTime.UtcNow
                    };
                    entities.CubeDatas.Add(new_data);
                    entities.SaveChanges();



                }
            }
        }


        public bool CreateDatabase(SqlConnection connection, string txtDatabase)
        {
            if (MSSQLServerCheck())
            {
                String CreateDatabase;
                string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                GrantAccess(appPath); //Need to assign the permission for current application to allow create database on server (if you are in domain).
                bool IsExits = CheckDatabaseExists(connection, txtDatabase); //Check database exists in sql server.
                if (!IsExits)
                {
                    //CreateDatabase = "CREATE DATABASE " + txtDatabase + " ; CREATE TABLE SerialPortData(GuidId uniqueidentifier NOT NULL DEFAULT NEWID(), ReceivedData varchar(MAX), CreateDate datetime DEFAULT GETUTCDATE());";
                    CreateDatabase = "CREATE DATABASE " + txtDatabase + " CREATE TABLE SerialPortData123123123(First_Name char(50),Last_Name char(50),Address char(50),City char(50),Country char(25),Birth_Date datetime);";
                    SqlCommand command = new SqlCommand(CreateDatabase, connection);
                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("Please Check Server and Database name.Server and Database name are incorrect .", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;
                    }
                    finally
                    {
                        if (connection.State == ConnectionState.Open)
                        {
                            connection.Close();
                        }
                    }
                    return true;
                }
                return true;
            }
            else
            {
                MessageBox.Show("SQL Server is not installed on this system. Please install SQL Server to store data.");
                return false;
            }
        }

        private bool MSSQLServerCheck()
        {
            RegistryView registryView = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
            using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
            {
                RegistryKey instanceKey = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL", false);
                if (instanceKey != null)
                {
                    foreach (var instanceName in instanceKey.GetValueNames())
                    {
                        Console.WriteLine(Environment.MachineName + @"\" + instanceName);
                    }
                }
            }

            RegistryKey RK = Registry.CurrentUser.OpenSubKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server");
            if (RK != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool GrantAccess(string fullPath)
        {
            DirectoryInfo info = new DirectoryInfo(fullPath);
            WindowsIdentity self = System.Security.Principal.WindowsIdentity.GetCurrent();
            DirectorySecurity ds = info.GetAccessControl();
            ds.AddAccessRule(new FileSystemAccessRule(self.Name,
            FileSystemRights.FullControl,
            InheritanceFlags.ObjectInherit |
            InheritanceFlags.ContainerInherit,
            PropagationFlags.None,
            AccessControlType.Allow));
            info.SetAccessControl(ds);
            return true;
        }

        public static bool CheckDatabaseExists(SqlConnection tmpConn, string databaseName)
        {
            string sqlCreateDBQuery;
            bool result = false;

            try
            {
                sqlCreateDBQuery = string.Format("SELECT database_id FROM sys.databases WHERE Name = '{0}'", databaseName);
                using (SqlCommand sqlCmd = new SqlCommand(sqlCreateDBQuery, tmpConn))
                {
                    tmpConn.Open();
                    object resultObj = sqlCmd.ExecuteScalar();
                    int databaseID = 0;
                    if (resultObj != null)
                    {
                        int.TryParse(resultObj.ToString(), out databaseID);
                    }
                    tmpConn.Close();
                    result = (databaseID > 0);
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblCurrentDateTime.Text = DateTime.UtcNow.ToString("dddd , MMM dd yyyy,hh:mm:ss");

        }


    }
}
