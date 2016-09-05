using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using System.IO;
using WebexNetClient;
using UcNetClient;

namespace Posh_UC
{

    public sealed class CurrentWebexClient
    {
        private static volatile CurrentWebexClient instance;
        private static object syncRoot = new object();

        private CurrentWebexClient()
        {
            Loaded = false;
        }

        public static CurrentWebexClient Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new CurrentWebexClient();
                    }
                }
                return instance;
            }
        }

        public bool Loaded { get; private set; }
        public WebexClient Client { get; private set; }  

        public void Connect(string Server, string Username, string Password, string Email = null, string SiteName = null,  bool verify = true)
        {
            Loaded = false;
            var settings = new WebexClientSettings
            {
                SiteName = SiteName,       
                Server = Server,
                User = Username,
                Password = Password,
                Email = Email
            };
            if (verify)
            {
                var tempClient = new WebexClient(settings);
                var testResults = tempClient.Execute(client =>
                {
                    var res = client.getUser(new getUser
                    {
                        webExId = Username
                    });
                });

                if (testResults.Exception != null)
                    throw testResults.Exception;
                else
                {
                    Client = tempClient;
                    Loaded = true;
                }
            }            
            else
            {
                Client = new WebexClient(settings);
                Loaded = true;
            }
            
        }

        public void Disconnect()
        {
            Client = null;
            Loaded = false;
        }

    }

    

    [Cmdlet(VerbsCommunications.Connect, "WebexServer")]
    public class ConnectWebexServer : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            if (!CurrentUcClient.Instance.Loaded || Force)
            {
                Exception failure = null;
                string email = Email ?? Credential.UserName;
                try { CurrentWebexClient.Instance.Connect(Server, Credential.UserName, 
                    Credential.Password.ConvertToUnsecureString(), email, SiteName, !DoNotVerify.IsPresent); }
                catch (Exception ex) { failure = ex; }

                WriteObject(CurrentWebexClient.Instance.Loaded);
                if (failure != null)
                {
                    Console.WriteLine(string.Format("Failed to connect: {0}", failure.Message));
                }
            } else
            {
                WriteObject(CurrentWebexClient.Instance.Loaded);
                Console.WriteLine("The webex client is already loaded.  Use the -Force switch to reconnect");
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
            HelpMessage = "Email address, will use username if not specified")]
        public string Email;
        
        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            Position = 3,
            HelpMessage = "Site name")]
        public string SiteName;

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            Position = 4,
            HelpMessage = "Force to connect even if the client is already loaded")]
        public SwitchParameter Force;

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            Position = 5,
            HelpMessage = "Skip connection verification")]
        public SwitchParameter DoNotVerify;
    }

    public class WebexClientNotLoadedException : Exception
    {
        public override string Message
        {
            get
            {
                return "Client has not been loaded.  Use Connect-WebexServer before executing any webex Posh-UC commands";
            }
        }
    }
}
