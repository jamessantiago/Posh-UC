using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Management.Automation;
using System.IO;
using AxlNetClient;

namespace Posh_UC
{
    [Cmdlet(VerbsCommon.Get, "UcUser")]
    public class GetUcUser : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            if (!CurrentAxlClient.Instance.Loaded)
            {
                throw new ClientNotLoadedException();
            }
        }

        protected override void ProcessRecord()
        {
            var user = CurrentAxlClient.Instance.Client.Execute(client =>
            {
                var res = client.getUser(new GetUserReq
                {
                    ItemElementName = ItemChoiceType100.userid,
                    Item = Username
                });
                return res.@return;
            });
            if (user.Exception != null)
                throw user.Exception;
            else
                WriteObject(user.Value.user);
        }

        [Parameter(
            ParameterSetName = "String",
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 0,
            HelpMessage = "Username to retrieve")]
        public string Username;
    }
}
