using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Management.Automation;
using System.IO;
using WebexNetClient;

namespace Posh_UC
{
    [Cmdlet(VerbsCommon.Get, "WebexUser")]
    public class GetWebexUser : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            if (!CurrentWebexClient.Instance.Loaded)
            {
                throw new WebexClientNotLoadedException();
            }
        }

        protected override void ProcessRecord()
        {
            var user = CurrentWebexClient.Instance.Client.Execute(client =>
            {
                return client.getUser(new getUser
                {
                    webExId = Username
                });
            });
            if (user.Exception != null)
                throw user.Exception;
            else
                WriteObject(user.Value);
        }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 0,
            HelpMessage = "Username to retrieve")]
        public string Username;
        
    }

    [Cmdlet(VerbsCommon.Remove, "WebexUser")]
    public class RemoveWebexuser : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            if (!CurrentWebexClient.Instance.Loaded)
            {
                throw new WebexClientNotLoadedException();
            }
        }

        protected override void ProcessRecord()
        {
            var result = CurrentWebexClient.Instance.Client.Execute(client =>
            {
                var res = client.delUser(new delUser
                {
                    webExId = WebExId
                });                
                return res;
            });

            if (result.Exception != null)
                throw result.Exception;
            else
                WriteObject(result.Value.user);
        }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 0,
            HelpMessage = "Username(s) to remove")]
        public string[] WebExId;
    }

    [Cmdlet(VerbsCommon.Set, "WebexUser")]
    public class SetWebexuser : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            if (!CurrentWebexClient.Instance.Loaded)
            {
                throw new WebexClientNotLoadedException();
            }
        }

        protected override void ProcessRecord()
        {
            var result = CurrentWebexClient.Instance.Client.Execute(client =>
            {
                var action = new setUser();
                action.webExId = WebExId;
                action.activeSpecified = true;
                if (Active.HasValue && Active.Value)                    
                    action.active = activeType.ACTIVATED;
                else if (Active.HasValue)
                    action.active = activeType.DEACTIVATED;
                if (Email.HasValue())
                    action.email = Email;
                return client.setUser(action);
            });
            if (result.Exception != null)
                throw result.Exception;
            //response is empty class
        }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 0,
            HelpMessage = "Username to modify")]
        public string WebExId;

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 1,
            HelpMessage = "Set whether user is active or not")]
        public bool? Active;

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 1,
            HelpMessage = "Set whether user is active or not")]
        public string Email;
    }
}
