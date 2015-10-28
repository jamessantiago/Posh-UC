using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using System.IO;
using AxlNetClient;

namespace Posh_UC
{
    [Cmdlet(VerbsCommon.Get, "UcPhone")]
    public class GetUcPhone : PSCmdlet
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
                var res = client.getPhone(new GetPhoneReq
                {
                    ItemElementName = ItemChoiceType140.name,
                    Item = DeviceName
                });
                return res.@return;
            });
            if (user.Exception != null)
                throw user.Exception;
            else
                WriteObject(user.Value.phone);
        }

        [Parameter(
            ParameterSetName = "String",
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 0,
            HelpMessage = "DeviceName to retrieve")]
        public string DeviceName;
    }
}
