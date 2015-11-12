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
    [Cmdlet(VerbsLifecycle.Invoke, "UcSqlCmd")]
    public class UcSqlCmd : PSCmdlet
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
            var data = CurrentUcClient.Instance.Client.Execute(client =>
            {
                var res = client.executeSQLQuery(new ExecuteSQLQueryReq
                {
                    sql = Command
                });
                return res.@return;
            });
            if (data.Exception != null)
                throw data.Exception;
            else
            {
                WriteObject(data.Value, true);
            }
        }

        [Parameter(
            ParameterSetName = "String",
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 0,
            HelpMessage = "Informix SQL command to run against the UC server")]
        public string Command;
    }
}
