using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using System.IO;
using UcNetClient;
using RosterNetClient;

namespace Posh_UC
{
    public sealed class CurrentImpConnection
    {
        private static volatile CurrentImpConnection instance;
        private static object syncRoot = new object();

        private CurrentImpConnection()
        {
            Loaded = false;
        }

        public static CurrentImpConnection Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new CurrentImpConnection();
                    }
                }
                return instance;
            }
        }

        public bool Loaded = false;
        public RosterClient RosterClient { get; set; }

        public void Connect(string Server, string Username, string password, bool Verify = true)
        {
            Loaded = false;
            var settings = new UcClientSettings
            {
                Server = Server,
                User = Username,
                Password = password
            };
            if (Verify)
            {
                var tempclient = new RosterClient(settings);
                var testResults = tempclient.Execute(client =>
                {
                    return client.getClusterInfo(new GetClusterInfoReq { });
                });

                testResults.Exception.ThrowIfNotNull();

                RosterClient = tempclient;
                Loaded = true;
            }
            else
            {
                RosterClient = new RosterClient(settings);
                Loaded = true;
            }
        }

        public void Disconnect()
        {
            RosterClient = null;
            Loaded = false;
        }
    }

    [Cmdlet(VerbsCommunications.Connect, "ImpServer")]
    public class ConnectImpServer : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            if (!CurrentImpConnection.Instance.Loaded || Force)
            {
                Exception failure = null;

                try
                {
                    CurrentImpConnection.Instance.Connect(Server, Credential.UserName,
                    Credential.Password.ConvertToUnsecureString(), !DoNotVerify.IsPresent);
                }
                catch (Exception ex) { failure = ex; }

                WriteObject(CurrentImpConnection.Instance.Loaded);
                if (failure != null)
                {
                    Console.WriteLine(string.Format("Failed to connect: {0}", failure.Message));
                }
            }
            else
            {
                WriteObject(CurrentImpConnection.Instance.Loaded);
                Console.WriteLine("The IMP client is already loaded.  Use the -Force switch to reconnect");
            }
        }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 0,
            HelpMessage = "IMP server")]
        public string Server;

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 1,
            HelpMessage = "IMP credentials to connect with")]
        public PSCredential Credential;

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            Position = 2,
            HelpMessage = "Force to connect even if the client is already loaded")]
        public SwitchParameter Force;

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            Position = 3,
            HelpMessage = "Skip connection verification")]
        public SwitchParameter DoNotVerify;
    }

    [Cmdlet(VerbsCommunications.Disconnect, "ImpServer")]
    public class DisconnectImpServer : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            CurrentImpConnection.Instance.Disconnect();
            WriteObject("Successfully disconnected the client");
        }
    }
}
