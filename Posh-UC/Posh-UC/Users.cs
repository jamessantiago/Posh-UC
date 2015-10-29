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

    [Cmdlet(VerbsCommon.Set, "UcUser")]
    public class SetUcUser : PSCmdlet
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
            var result = CurrentAxlClient.Instance.Client.Execute(client =>
            {
                var updateRequest = new UpdateUserReq
                {
                    ItemElementName = ItemChoiceType6.userid,
                    Item = Username
                };
                bool hasChange = false;
                if (AssociatedDevices != null)
                {
                    hasChange = true;
                    updateRequest.associatedDevices = AssociatedDevices;
                }

                if (hasChange)
                {
                    var res = client.updateUser(updateRequest);
                    return res.@return;
                }
                else
                    throw new Exception("No change has been specified");
            });
                      
                        
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
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 0,
            HelpMessage = "Username of UcUser to modify")]
        public string Username;

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 1,
            HelpMessage = "Array of device(s) to associate to the user")]
        public string[] AssociatedDevices;
    }
}
