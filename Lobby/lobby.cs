﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ConnectionClasses;
using System.Net;
using System.Threading;

namespace GameConfig
{
    #region Lobby Class

    public partial class Lobby : Form
    {
        #region Fields
        private LobbyClient LobbyClient;
        private Thread ReadFromServerThread;
        private string[] StoredRooms = null;
        #endregion

        #region Constructor
        public Lobby(string SetClientName)
        {
            InitializeComponent();
            LobbyClient = new LobbyClient(IPAddress.Parse("192.168.0.107"), 5500, SetClientName);
            label2.Text = LobbyClient.LobbyClientName;
            ReadFromServerThread = new Thread(ReadHostList);
            ReadFromServerThread.Start();
        }
        #endregion

        #region EventHandlers
        private void CreateButton_Click(object sender, EventArgs e)
        {
            Config GameConfig = new Config();
            DialogResult result;
            result = GameConfig.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.Hide();
            }
        }
        #endregion

        #region Methods

        #region Read From Server for Hosts 
        private void ReadHostList()
        {
            string[] LoadedRooms;
            while (true)
            {
                if(LobbyClient.HostStream != null)
                {
                    byte[] EncodedRooms = new byte[1000];

                    LobbyClient.HostStream.Read(EncodedRooms, 0, EncodedRooms.Length);
                    LoadedRooms = Encoding.ASCII.GetString(EncodedRooms).Trim((char)0).Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                    if((StoredRooms == null) || StoredRooms.Length != LoadedRooms.Length)
                    {
                        StoredRooms = LoadedRooms;
                        AddRoomControls();
                    }
                    else
                    {
                        int flag;

                        for (flag = 0; (flag < LoadedRooms.Length) && (LoadedRooms[flag] == StoredRooms[flag]); flag++);

                        if(flag != LoadedRooms.Length)
                        {
                            StoredRooms = LoadedRooms;
                            AddRoomControls();
                        }
                    }
                    Thread.Sleep(1000);
                }
            }
        }
        #endregion

        #region Create Room Controls
        private void AddRoomControls()
        {
            int StartX = 15; //X position of Room info W.R.T panel
            int StartY = 0; //Y position of Room info W.R.T panel

            Room[] Rooms = null;

            if (StoredRooms!= null && StoredRooms[0] != "Empty")
            {
                Rooms = new Room[StoredRooms.Length];
                for(int i=0; i<StoredRooms.Length; i++)
                {
                    Room SpecificRoom = new Room();
                    SpecificRoom.RoomName.Location = new Point(StartX, StartY + i * 50 + 5); //location of label1 
                    SpecificRoom.RoomName.Text = StoredRooms[i].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)[1];  //from server
                    SpecificRoom.RoomName.ForeColor = Color.White;

                    SpecificRoom.PlayersNumber.Location = new Point(StartX+120, StartY + i * 50 + 5); //location of label 2
                    SpecificRoom.PlayersNumber.Text = StoredRooms[i].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)[3] + " Players"; //from server

                    SpecificRoom.RoomButton.Location = new Point(StartX + 235, StartY + i * 50);
                    SpecificRoom.RoomButton.BackColor = StoredRooms[i].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)[2] != "started" ? Color.LightSeaGreen : Color.Black;
                    //Delegate of button
                    Rooms[i] = SpecificRoom;
                }
            }

            Availablerooms.Invoke((MethodInvoker)delegate { UpdatePanel(Rooms); });
        }
        #endregion

        #region Update Panel Controls (Cross Threading)

        private void UpdatePanel(Room[] Rooms)
        {
            Availablerooms.Controls.Clear();
            if (Rooms != null)
            {
                foreach (Room SpecificRoom in Rooms)
                {
                    Availablerooms.Controls.Add(SpecificRoom.RoomName);
                    Availablerooms.Controls.Add(SpecificRoom.PlayersNumber);
                    Availablerooms.Controls.Add(SpecificRoom.RoomButton);
                }
            }
        }

        #endregion

        #endregion
    }

    #endregion
}