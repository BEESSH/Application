using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BeeSSH.Core.API;
using BeeSSH.Interface.CustomMessageBox;
using Renci.SshNet;
using static BeeSSH.Core.API.Cache;
using static BeeSSH.Core.API.Request;
using static BeeSSH.Utils.DiscordRPC.DiscordRPCManager;

namespace BeeSSH.Interface.UserControlls
{
    /// <summary>
    /// Interaction logic for TerminalUsercControl.xaml
    /// </summary>
    public partial class TerminalUsercControl : UserControl
    {
       
        private SshClient client;
        private ShellStream _shellStream;
        private ServerListModel _ServerListModel;

        public TerminalUsercControl()
        {
            InitializeComponent();
            var ServerUID = _ServerUID;
            PandleServer(ServerUID);
            Terminal_MainView();
            _ServerListModel = ServerList.Find(x => x.ServerUID.Contains(ServerUID));
            ConnectToServer($"{_ServerListModel.ServerIP}:{_ServerListModel.ServerPort}", _ServerListModel.ServerUserName, _ServerListModel.ServerPassword);
        }

       
        private SshClient sshClient;
        
        public void ConnectToServer(string hostname, string username, string password)
        {
            try
            {
                if (_ServerListModel.RSAKEY)
                {
                    // Connect with RSA Key
                    sshClient = new SshClient(hostname, username, new PrivateKeyFile(_ServerListModel.RSAKeyText));
                }
                else
                {
                    // Erstellen einer SSH-Verbindung zum Server
                    sshClient = new SshClient(hostname, username, password);
                }
                
                

                // Überprüfen des Host-Keys, wenn ein neuer Host-Key empfangen wird
                sshClient.HostKeyReceived += (sender, e) =>
                {
                    var fingerprint = new BeeFingerprint($"The Server {hostname} has a different fingerprint", hostname);
                    if (Convert.ToBoolean(fingerprint.ShowDialog()))
                    {
                        e.CanTrust = true;
                    }
                    else
                    {
                        e.CanTrust = false;
                    }
                };

                // Verbindung zum Server herstellen
                sshClient.Connect();

                // Warten auf Benutzereingabe von Befehlen
                inputBox.KeyUp += (sender, e) =>
                {
                    if (e.Key == System.Windows.Input.Key.Enter)
                    {
                        var command = inputBox.Text;
                        inputBox.Text = "";
                        ExecuteCommand(command);
                    }
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to server: {ex.Message}");
            }
        }

        private void ExecuteCommand(string command)
        {
            try
            {
                // Ausführen des Befehls auf dem Server und Anzeigen der Antwort im RichTextBox
                var output = sshClient.RunCommand(command);
                outputBox.AppendText(output.Result);
                outputBox.ScrollToEnd();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing command: {ex.Message}");
            }
        }
        
    }
}