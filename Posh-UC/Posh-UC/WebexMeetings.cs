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
    [Cmdlet(VerbsCommon.Remove, "WebexMeeting")]
    public class RemoveWebexMeeting : PSCmdlet
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
                var res = client.DelMeeting(new DelMeeting
                {
                    meetingKey = MeetingNumber
                });
            });
            if (result.Exception != null)
                throw result.Exception;            
        }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 0,
            HelpMessage = "Meeting to remove")]
        public long MeetingNumber;        
    }

    [Cmdlet(VerbsCommon.Get, "WebexMeeting")]
    public class GetWebexMeeting : PSCmdlet
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
            if (MeetingNumber.HasValue)
            {
                var result = CurrentWebexClient.Instance.Client.Execute(client =>
                {
                    return client.GetMeeting(new GetMeeting
                    {
                        meetingKey = MeetingNumber.Value
                    });
                });
                if (result.Exception != null)
                    throw result.Exception;
                WriteObject(result.Value);
            }
            else
            {
                var result = CurrentWebexClient.Instance.Client.Execute(client =>
                {
                    return client.LstsummaryMeeting(new LstsummaryMeeting
                    {
                        listControl = new lstControlType
                        {
                            startFrom = "1",
                            maximumNum = "10",
                            listMethod = lstMethodType.OR,
                            listMethodSpecified = true
                        },
                        order = new orderType3
                        {
                            orderBy = new orderByType3[] { orderByType3.CONFNAME, orderByType3.STARTTIME }
                        }
                    });
                });

                if (result.Exception != null) throw result.Exception;

                WriteObject(result.Value.meeting);
            }
        }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 0,
            HelpMessage = "Meeting to get")]
        public long? MeetingNumber;
    }
}
