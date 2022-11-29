﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
	public partial class Form1 : Form
	{
		private bool m_Connected = false;
		private Socket m_ClientSocket;
		private Thread recieveThread;

		public Form1()
		{
			Control.CheckForIllegalCrossThreadCalls = false;
			this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
			InitializeComponent();
		}

		private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Environment.Exit(0);
		}
		private void connectButton_Click(object sender, EventArgs e)
		{
			m_ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			string IpAddress = IPValue.Text;
			string client_name;
			int portNum;
			if(Int32.TryParse(portValue.Text, out portNum))
			{
				try
				{
					m_ClientSocket.Connect(IpAddress, portNum);
					client_name = textBox_name.Text;
					connectButton.Enabled = false;
					m_Connected = true;
					button_disconnect.Enabled = true;

					if(client_name != "" && client_name.Length <= 64) 
					{
						Byte[] buffer = Encoding.Default.GetBytes(client_name);
						m_ClientSocket.Send(buffer);
					}

					Byte[] buffer2 = new Byte[64];
					m_ClientSocket.Receive(buffer2);
					logs_debug.AppendText("whyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy");
					string incomingMessage = Encoding.Default.GetString(buffer2);
					incomingMessage = incomingMessage.Substring(0, incomingMessage.IndexOf("\0"));
					//debug
					logs_debug.AppendText("\n\n" + incomingMessage);

					if (incomingMessage == "this name is taken!")
					{
						logs.AppendText("Name is already taken can't connect to the server with this name!\n");
						connectButton.Enabled = true;
						m_Connected = false;
						button_disconnect.Enabled = false;
					}
					else
					{
						
						recieveThread = new Thread(Recieve);
						recieveThread.Start();
						logs.AppendText("Connected to the server!\n");
					}
					//logs.AppendText("Server: " + incomingMessage + "\n");
				
					//logs.AppendText("Name is: " + client_name+ "\n");
				}
				catch
				{
					logs.AppendText("Failed to connect to the server due to non name related issues!\n");
				}
			}
		}

		private void Recieve()
		{
			while(m_Connected)
			{
				try
				{
					Byte[] buffer = new Byte[1024];
					m_ClientSocket.Receive(buffer);

					String incoming_message = Encoding.Default.GetString(buffer);
					//debug                        incoming_message
					logs_debug.AppendText("\n\n" + incoming_message);

					String type = incoming_message.Substring(0, 5);
					incoming_message = incoming_message.Substring(5, incoming_message.IndexOf("\0")-5);
					//logs.AppendText("CHECKING QUEST ERROR  " + incoming_message + "\n");
					logs_debug.AppendText("\n type :::: " + type + "\n");

					if (type == "QUEST")
					{
						button_send_answer.Enabled = true;
						String question_line = incoming_message;
						logs.AppendText("Server Asks: " + question_line + "\n");
					}
					if (type == "SCORE")
					{
						button_send_answer.Enabled = true;
						String score_table = incoming_message;
						logs.AppendText("\n"+score_table +"\n");
					}
					if (type == "ANSWE")
					{
						button_send_answer.Enabled = true;
						String answer_table = incoming_message;
						logs.AppendText("\n" + answer_table + "\n");
					}
					if (type == "GMOVR")
					{
						button_send_answer.Enabled = false;
						String winner = incoming_message;
						logs.AppendText("\n" + winner + "\n");
					}
				}
				catch
				{
					button_disconnect.Enabled = false;
					try
					{
						m_ClientSocket.Close();
					}
					catch
					{}
					m_Connected = false;
					logs.AppendText("Should be disconnecting now \n");
					
					connectButton.Enabled = true;
				}
			}
						
		}

		private void IPValue_TextChanged(object sender, EventArgs e)
		{

		}

		//button_disconnect
		private void button1_Click(object sender, EventArgs e)
		{
			m_Connected = false;
			button_disconnect.Enabled = false;
			connectButton.Enabled = true;
			
			m_ClientSocket.Close();
			recieveThread.Abort();

			logs.AppendText("Disconnecting from server...\n");
		}

		private void textBox_answer_TextChanged(object sender, EventArgs e)
		{

		}

		private void button_send_answer_Click(object sender, EventArgs e)
		{
			string answer = textBox_answer.Text;
			Byte[] buffer = Encoding.Default.GetBytes(answer);
			m_ClientSocket.Send(buffer);
			button_send_answer.Enabled = false;
		}
	}
}
