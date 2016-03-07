using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using System.IO;
using AxlNetClient;
using RisNetClient;
using PerfNetClient;
using UcNetClient;

namespace Posh_UC
{

    public sealed class CurrentUcClient
    {
        private static volatile CurrentUcClient instance;
        private static object syncRoot = new object();

        private CurrentUcClient()
        {
            Loaded = false;
        }

        public static CurrentUcClient Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new CurrentUcClient();
                    }
                }
                return instance;
            }
        }

        public bool Loaded { get; private set; }
        public AxlClient Client { get; private set; }
        public RisClient RisClient { get; private set; }  
        public PerfClient PerfClient { get; private set; }    

        public void Connect(string Server, string Username, string Password, bool verify = true)
        {
            Loaded = false;
            var settings = new UcClientSettings
            {
                Server = Server,
                User = Username,
                Password = Password
            };
            if (verify)
            {
                var tempClient = new AxlClient(settings);
                var testResults = tempClient.Execute(client =>
                {
                    var res = client.getAppUser(new GetAppUserReq
                    {
                        ItemElementName = ItemChoiceType102.userid,
                        Item = Username
                    });
                });

                if (testResults.Exception != null)
                    throw testResults.Exception;
                else
                {
                    Client = tempClient;
                    RisClient = new RisClient(settings);
                    PerfClient = new PerfClient(settings);
                    Loaded = true;
                }
            }            
            else
            {
                Client = new AxlClient(settings);
                RisClient = new RisClient(settings);
                PerfClient = new PerfClient(settings);
                Loaded = true;
            }
            
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
            if (!CurrentUcClient.Instance.Loaded || Force)
            {
                Exception failure = null;
                
                try { CurrentUcClient.Instance.Connect(Server, Credential.UserName, 
                    Credential.Password.ConvertToUnsecureString(), !DoNotVerify.IsPresent); }
                catch (Exception ex) { failure = ex; }

                WriteObject(CurrentUcClient.Instance.Loaded);
                if (failure != null)
                {
                    Console.WriteLine(string.Format("Failed to connect: {0}", failure.Message));
                }
            } else
            {
                WriteObject(CurrentUcClient.Instance.Loaded);
                Console.WriteLine("The AXL client is already loaded.  Use the -Force switch to reconnect");
            }
        }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 0,
            HelpMessage = "UC server")]
        public string Server;
        
        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 1,
            HelpMessage = "UC credentials to connect with")]
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

    [Cmdlet(VerbsCommunications.Disconnect, "UcServer")]
    public class DisconnectUcServer : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            CurrentUcClient.Instance.Disconnect();
            WriteObject("Successfully disconnected the client");
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
