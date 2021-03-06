﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.IO;
using AxlNetClient;

namespace Posh_UC
{
    [Cmdlet(VerbsCommon.Get, "UcPhone")]
    public class GetUcPhone : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            if (!CurrentUcClient.Instance.Loaded)
            {
                throw new ClientNotLoadedException();
            }
        }

        protected override void ProcessRecord()
        {
            var phone = CurrentUcClient.Instance.Client.Execute(client =>
            {
                var res = client.getPhone(new GetPhoneReq
                {
                    ItemElementName = ItemChoiceType140.name,
                    Item = DeviceName
                });
                return res.@return;
            });
            if (phone.Exception != null)
                throw phone.Exception;
            else
                WriteObject(phone.Value.phone);
        }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 0,
            HelpMessage = "DeviceName to retrieve")]
        public string DeviceName;
    }

    [Cmdlet(VerbsCommon.Remove, "UcPhone")]
    public class RemoveUcPhone : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            if (!CurrentUcClient.Instance.Loaded)
            {
                throw new ClientNotLoadedException();
            }
        }

        protected override void ProcessRecord()
        {
            if (!KeepLines.IsPresent)
            { 
                var phone = CurrentUcClient.Instance.Client.Execute(client =>
                {
                    var res = client.getPhone(new GetPhoneReq
                    {
                        ItemElementName = ItemChoiceType140.name,
                        Item = DeviceName
                    });
                    return res.@return;
                });
                if (phone.Exception != null)
                    throw phone.Exception;

                
                var removedLineResult = CurrentUcClient.Instance.Client.Execute(client =>
                {
                    var res = client.removeLine(new RemoveLineReq
                    {
                        ItemsElementName = Enumerable.Repeat(ItemsChoiceType56.pattern, phone.Value.phone.lines.Items.Count()).ToArray(),
                        Items = phone.Value.phone.lines.Items.Select(d => ((RPhoneLine)d).dirn.pattern).ToArray()
                    });
                    return res.@return;
                });

                if (removedLineResult.Exception != null)
                    throw removedLineResult.Exception;
            }

            var removedResult = CurrentUcClient.Instance.Client.Execute(client =>
            {
                var res = client.removePhone(new NameAndGUIDRequest
                {
                    ItemElementName = ItemChoiceType36.name,
                    Item = DeviceName
                });
                return res.@return;
            });
            if (removedResult.Exception != null)
                throw removedResult.Exception;

                      
        }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 0,
            HelpMessage = "DeviceName to remove")]
        public string DeviceName;

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            HelpMessage = "Specify this option to keep any associated directory numbers")]
        public SwitchParameter KeepLines;
    }

    [Cmdlet(VerbsCommon.Get, "UcLine")]
    public class GetUcLine : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            if (!CurrentUcClient.Instance.Loaded)
            {
                throw new ClientNotLoadedException();
            }
        }

        protected override void ProcessRecord()
        {
            var line = CurrentUcClient.Instance.Client.Execute(client =>
            {
                var res = client.getLine(new GetLineReq
                {
                    ItemsElementName = new ItemsChoiceType57[] { ItemsChoiceType57.pattern },
                    Items = new object[] { DirectoryNumber }
                });
                return res.@return;
            });
            if (line.Exception != null)
                throw line.Exception;
            else
                WriteObject(line.Value.line);
        }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 0,
            HelpMessage = "DirectoryNumber to retrieve")]
        public string DirectoryNumber;
    }

    [Cmdlet(VerbsCommon.Set, "UcLine")]
    public class SetUcLine : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            if (!CurrentUcClient.Instance.Loaded)
            {
                throw new ClientNotLoadedException();
            }
        }

        protected override void ProcessRecord()
        {
            var line = CurrentUcClient.Instance.Client.Execute(client =>
            {
                var req = new UpdateLineReq();

                req.ItemsElementName = new ItemsChoiceType14[] { ItemsChoiceType14.pattern };
                req.Items = new object[] { DirectoryNumber };
                if (PrimaryDirectoryUri.HasValue())
                {
                    req.directoryURIs = new XDirectoryUri[]
                    {
                        new XDirectoryUri
                        {
                            advertiseGloballyViaIls = "t",
                            isPrimary = "t",
                            uri = PrimaryDirectoryUri,
                            partition = new XFkType()
                        }
                    };
                }
                if (AlertingName.HasValue()) req.alertingName = AlertingName;
                return client.updateLine(req);
            });

            if (line.Exception != null) throw line.Exception;            
        }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 0,
            HelpMessage = "DirectoryNumber to retrieve")]
        public string DirectoryNumber;

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 1,
            HelpMessage = "Directory URI")]
        public string PrimaryDirectoryUri;

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = false,
            Position = 2,
            HelpMessage = "Alerting Name")]
        public string AlertingName;
    }        
}
