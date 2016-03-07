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
    [Cmdlet(VerbsCommon.Get, "UcPerfMonCounters")]
    public class GetPerfMonCounters : PSCmdlet
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
            var counters = CurrentUcClient.Instance.PerfClient.Execute(client =>
            {
                return client.perfmonListCounter(Hostname);
            });

            WriteObject(counters.Value);
        }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 0,
            HelpMessage = "Host to list counters for")]
        public string Hostname;
    }

    [Cmdlet(VerbsCommon.Get, "UcPerfMonCounterData")]
    public class GetPerfMonCounterData : PSCmdlet
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
            var counter = CurrentUcClient.Instance.PerfClient.Execute(client =>
            {
                return client.perfmonCollectCounterData(Hostname, new PerfNetClient.ObjectNameType
                {
                    Value = Counter
                });
            });

            WriteObject(counter.Value);
        }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 0,
            HelpMessage = "Host to list counter data for")]
        public string Hostname;

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 1,
            HelpMessage = "Counter to retrieve")]
        public string Counter;
    }
}
