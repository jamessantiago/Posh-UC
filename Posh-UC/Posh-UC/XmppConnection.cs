using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using agsXMPP;
using agsXMPP.protocol.x.muc;
using System.Management.Automation;
using NLog;

namespace Posh_UC
{
    public class XmppServer
    {

        private XmppClientConnection _xmppConnection;
        private TaskCompletionSource<bool> _loginTcs;
        //private TaskCompletionSource<bool> _reconnectTcs;
        private string _lastError;
        private Logger logger = LogManager.GetCurrentClassLogger();
        private object _connectSync = new object();
        private object _initiateSync = new object();

        public string Host;
        public string ConnectHost;
        public string Username;
        public string Password;
        public int? Port;

        public XmppServer(string host, string connecthost, string username, string password, int? port)
        {
            Host = host;
            ConnectHost = connecthost;
            Username = username;
            Password = password;
            Port = port;
            try
            {
                Task.Run(Initiate);
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to initate an xmpp connection");
            }
        }

        protected Task Initiate()
        {
            try
            {
                _loginTcs = new TaskCompletionSource<bool>();
                logger.Debug("Task source created");
                Task<bool> connect = _loginTcs.Task;
                logger.Debug("Task collected");

                if (_xmppConnection != null)
                {
                    logger.Debug("{0} already existed when initating", Host);
                    _xmppConnection.Close();
                    _xmppConnection = null;
                }
                _xmppConnection = new XmppClientConnection
                {
                    Server = Host,
                    ConnectServer = ConnectHost,
                    AutoResolveConnectServer = false,
                    Username = Username,
                    Password = Password
                };
                logger.Debug("Connection setup");

                if (Port.HasValue) _xmppConnection.Port = Port.Value;

                _xmppConnection.KeepAlive = true;
                _xmppConnection.ClientSocket.OnValidateCertificate += OnValidateCertificate;

                _xmppConnection.OnLogin += OnLogin;
                _xmppConnection.OnError += OnError;
                _xmppConnection.OnAuthError += OnAuthError;
                _xmppConnection.OnMessage += OnMessage;
                _xmppConnection.OnSocketError += OnSocketError;
                _xmppConnection.OnStreamError += OnStreamError;
                _xmppConnection.OnXmppConnectionStateChanged += OnXmppConnectionStateChanged;
                logger.Debug("Events added to connection");

                Task.Factory.StartNew(() =>
                {
                    logger.Debug("{0} Opening connection", Host);
                    _xmppConnection.Open();
                    Thread.Sleep(20000);
                    logger.Debug("{0} Open conneciton timed out", Host);
                    _loginTcs.TrySetResult(false);
                });

                if (!connect.Result)
                {
                    logger.Debug("Open conneciton timed out");
                    throw new TimeoutException("XMPP target timed out while trying to login");
                }

                return _loginTcs.Task;
            }
            catch (Exception ex)
            {
                logger.Debug(ex, "Failed to initiate");
                AttemptReconnect();
                return null;
            }
        }

        private void ClientSocket_OnError(object sender, Exception ex)
        {
            logger.Error(ex, "Client Socket Error");
        }

        private void OnStreamError(object sender, agsXMPP.Xml.Dom.Element e)
        {
            logger.Error("Stream Error {0}", e.InnerXml);
        }

        private bool OnValidateCertificate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            logger.Info("Certificate validation requested for {0}", certificate.Subject);
            return true; // :D
        }

        private void OnSocketError(object sender, Exception ex)
        {
            logger.Error(ex, "Socket Error");
        }

        private void OnWriteXml(object sender, string xml)
        {
            logger.Debug(xml);
        }

        private void OnReadXml(object sender, string xml)
        {
            logger.Debug(xml);
        }

        private void OnIq(object sender, agsXMPP.protocol.client.IQ iq)
        {
            logger.Debug(iq.InnerXml);
        }

        private void OnXmppConnectionStateChanged(object sender, XmppConnectionState state)
        {
            if (state == XmppConnectionState.Disconnected)
            {
                logger.Warn("XMPP connection is disconnected");

                if (_loginTcs.Task.IsCompleted)
                {
                    lock (_connectSync)
                    {
                        AttemptReconnect();
                    }
                }
            }
            else if (state != XmppConnectionState.Connecting)
            {
                logger.Debug("{0}: XMPP connection changed - {1}", Host, state.GetDescription());
            }
        }

        private void OnMessage(object sender, agsXMPP.protocol.client.Message message)
        {
            if (!String.IsNullOrEmpty(message.Body) && message.From.Resource != Username)
            {
                string user = string.Format("{0}@{1}/{2}", message.From.User, message.From.Server, message.From.Resource);

                var content = message.Body.Trim();

                logger.Info("Message received {0}: {1}", user, content);
            }
        }

        private void AttemptReconnect()
        {
            while (_xmppConnection == null || (_xmppConnection != null && !_xmppConnection.Authenticated) || _xmppConnection.XmppConnectionState == XmppConnectionState.Disconnected)
            {

                logger.Debug("Attempting to reconnect");
                try
                {
                    Close();
                }
                catch { }

                try
                {
                    Initiate();
                }
                catch (Exception ex)
                {
                    logger.Warn("Failed to reconnect - " + ex.Message);
                }
                Thread.Sleep(20000);
            }
        }

        public Task Close()
        {
            logger.Debug("{0} Closing", Host);
            try
            {
                CancelPreviousLogin();

                _xmppConnection.OnLogin -= OnLogin;
                _xmppConnection.OnError -= OnError;
                _xmppConnection.OnAuthError -= OnAuthError;
                _xmppConnection.OnMessage -= OnMessage;
                _xmppConnection.Close();
                _xmppConnection = null;
            }
            catch (Exception ex)
            {
                logger.Debug(ex, "Failed to close connection");
            }

            return Task.FromResult(1);
        }

        private void CancelPreviousLogin()
        {
            if (_loginTcs != null || _loginTcs.Task.IsCanceled || _loginTcs.Task.IsCompleted || _loginTcs.Task.IsFaulted)
            {
                try
                {
                    _loginTcs.SetCanceled();
                }
                catch (Exception ex)
                {
                    logger.Debug(ex, "Failed to cancel login task");
                }
            }
        }

        private void OnError(object sender, Exception ex)
        {
            _lastError = ex.Message;
            logger.Error(ex, "{0}: Error connecting", _lastError);
        }

        private void OnAuthError(object sender, agsXMPP.Xml.Dom.Element e)
        {
            logger.Error("Authentication Error: {0}", e.InnerXml);
        }

        private void OnLogin(object sender)
        {
            logger.Info("{0}: Logged into {1}", Host, _xmppConnection.Server);
            _loginTcs.TrySetResult(true);
        }

        public MucManager GetMucManager()
        {
            var muc = new MucManager(_xmppConnection);
            return muc;
        }

        public void Disconnect()
        {
            this.Close();
            this._xmppConnection = null;
        }
    }

    public sealed class CurrentXmppConnection
    {
        private static volatile CurrentXmppConnection instance;
        private static object syncRoot = new object();

        private CurrentXmppConnection()
        {
            Loaded = false;
        }

        public static CurrentXmppConnection Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new CurrentXmppConnection();
                    }
                }
                return instance;
            }
        }

        public bool Loaded = false;
        public XmppServer XmppClient { get; set; }

        public void Connect(string host, string connecthost, string username, string password, int? port)
        {
            Loaded = false;
            XmppClient = new XmppServer(host, connecthost, username, password, port);
            Loaded = true;
        }

        public void Disconnect()
        {
            XmppClient.Close();
            Loaded = false;
        }
    }

    [Cmdlet(VerbsCommunications.Connect, "XmppServer")]
    public class ConnectXmppServer : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            if (!CurrentXmppConnection.Instance.Loaded || Force)
            {
                Exception failure = null;

                try
                {
                    Extensions.SetLoggingConfig(ConsoleDebug);
                    CurrentXmppConnection.Instance.Connect(Server, ConnectHost, Credential.UserName,
                    Credential.Password.ConvertToUnsecureString(), Port);
                }
                catch (Exception ex) { failure = ex; }

                WriteObject(CurrentXmppConnection.Instance.Loaded);
                if (failure != null)
                {
                    Console.WriteLine(string.Format("Failed to connect: {0}", failure.Message));
                }
            }
            else
            {
                WriteObject(CurrentXmppConnection.Instance.Loaded);
                Console.WriteLine("The xmpp client is already loaded.  Use the -Force switch to reconnect");
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
            HelpMessage = "IMP server")]
        public string ConnectHost;

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 2,
            HelpMessage = "IMP credentials to connect with")]
        public PSCredential Credential;

        [Parameter(
           Mandatory = false,
           ValueFromPipelineByPropertyName = true,
           ValueFromPipeline = true,
           Position = 3,
           HelpMessage = "IMP credentials to connect with")]
        public int? Port;

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
            HelpMessage = "Enable console debugging")]
        public SwitchParameter ConsoleDebug;

    }

    [Cmdlet(VerbsCommunications.Disconnect, "XmppServer")]
    public class DisconnectXmppServer : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            CurrentXmppConnection.Instance.XmppClient.Disconnect();
            WriteObject("Successfully disconnected the client");
        }
    }
}
