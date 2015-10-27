using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using System.IO;
using AxlNetClient;


namespace Posh_UC
{

    public sealed class CurrentAxlClient
    {
        private static volatile CurrentAxlClient instance;
        private static object syncRoot = new object();

        private CurrentAxlClient()
        {
            Loaded = false;
        }

        public static CurrentAxlClient Instance
        {
            get
            {
                if (instance != null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new CurrentAxlClient();
                    }
                }
                return instance;
            }
        }

        public bool Loaded { get; private set; }
        public AxlClient Client { get; private set; }

        public void Connect(string Server, string Username, string Password)
        {
            Loaded = false;
            var tempClient = new AxlClient(new AxlClientSettings
            {
                Server = Server,
                User = Username,
                Password = Password
            });
            var testResults = tempClient.Execute(client =>
            {
                var res = client.getUser(new GetUserReq
                {
                    ItemElementName = ItemChoiceType100.userid,
                    Item = Username
                });
            });
            if (testResults.Exception != null)
                throw testResults.Exception;
            else
                Loaded = true;
        }

        public void Disconnect()
        {
            Client = null;
            Loaded = false;
        }

    }

    

    [Cmdlet(VerbsCommunications.Connect, "UcServer")]
    public class ConnectUcServer : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            CurrentAxlClient.Instance.Connect(Server, Username, Password);
            if (CurrentAxlClient.Instance.Loaded)
                WriteObject("Successfully connected the AXL client");
        }

        [Parameter(
            ParameterSetName = "String",
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 0,
            HelpMessage = "AXL UC server to run the sql command against")]
        public string Server;

        [Parameter(
            ParameterSetName = "String",
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 1,
            HelpMessage = "AXL username to connect with")]
        public string Username;

        [Parameter(
            ParameterSetName = "String",
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 2,
            HelpMessage = "AXL password to connect with")]
        public string Password;
    }

    [Cmdlet(VerbsCommunications.Disconnect, "UcServer")]
    public class DisconnectUcServer : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            CurrentAxlClient.Instance.Disconnect();
            WriteObject("Successfully disconnected the AXL client");
        }
    }

    public class ClientNotLoadedException : Exception
    {
        public override string Message
        {
            get
            {
                return "Client has not been loaded.  Use Connect-UcServer before executing any Posh-UC commands";
            }
        }
    }
}
