using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.IO;
using RisNetClient;
using UcNetClient;

namespace Posh_UC
{
    [Cmdlet(VerbsCommon.Get, "UcCmDevice")]
    public class GetUcCmDevice : PSCmdlet
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
            var device = CurrentUcClient.Instance.RisClient.Execute(client =>
            {
                var res = client.selectCmDevice(string.Empty, new CmSelectionCriteria
                {
                    MaxReturnedDevices = 1000,
                    DeviceClass = "Any",
                    Model = 255, //refers to any device, full listing here: https://developer.cisco.com/site/sxml/documents/api-reference/risport/#ModelTable
                    Status = "Any",
                    NodeName = string.Empty, //null for all nodes
                    SelectBy = CmSelectBy.Name,
                    SelectItems = DeviceName != null ? new ArrayOfSelectItem() { new SelectItem() { Item = DeviceName } } :
                    new ArrayOfSelectItem() { new SelectItem() { Item = "*" } },
                    Protocol = ProtocolType.Any,
                    DownloadStatus = DeviceDownloadStatus.Any,
                });

                return res;
            });

            if (device.Exception != null)
                throw device.Exception;

            WriteObject(device.Value.SelectCmDeviceResult);
        }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 0,
            HelpMessage = "DeviceName to retrieve")]
        public string DeviceName;
    }

    [Cmdlet(VerbsCommon.Get, "UcCtiItem")]
    public class GetUcCtiItem : PSCmdlet
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
            var device = CurrentUcClient.Instance.RisClient.Execute(client =>
            {
                var res = client.selectCtiItem(string.Empty, new CtiSelectionCriteria
                {
                    MaxReturnedItems = 1000,
                    CtiMgrClass = CtiMgrClass.Line,
                    Status = CtiStatus.Any,
                    NodeName = string.Empty,
                    SelectAppBy = CtiSelectAppBy.UserId,
                    AppItems = new ArrayOfSelectAppItem() { new SelectAppItem() { AppItem = "*" } },
                    DevNames = DeviceName != null ? new ArrayOfSelectDevName() { new SelectDevName() { DevName = DeviceName } } :
                    new ArrayOfSelectDevName(),
                    DirNumbers = DirectoryNumber != null ? new ArrayOfSelectDirNumber() { new SelectDirNumber() { DirNumber = DirectoryNumber} } :
                    new ArrayOfSelectDirNumber(),
                });

                return res;
            });

            if (device.Exception != null)
                throw device.Exception;

            WriteObject(device.Value.SelectCtiItemResult);
        }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 0,
            HelpMessage = "DeviceName to retrieve")]
        public string DeviceName;

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 0,
            HelpMessage = "Directory number to retrieve")]
        public string DirectoryNumber;
    }
}
