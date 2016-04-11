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
                var query = new lstUser();
                if (Username.HasValue()) query.webExId = Username;
                if (Active.IsPresent) query.active = activeType.ACTIVATED;
                if (Inactive.IsPresent) query.active = activeType.DEACTIVATED;
                if (Inactive.IsPresent || Active.IsPresent) query.activeSpecified = true;                                
                return client.lstUser(query);
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

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 1,
            HelpMessage = "User is active")]
        public SwitchParameter Active;

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 2,
            HelpMessage = "User is inactive")]
        public SwitchParameter Inactive;
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
                if (Active) action.active = activeType.ACTIVATED;
                else action.active = activeType.DEACTIVATED;
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
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 1,
            HelpMessage = "Set whether user is active or not")]
        public bool Active;
    }
}
