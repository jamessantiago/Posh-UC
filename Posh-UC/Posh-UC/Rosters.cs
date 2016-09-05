using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Management.Automation;
using System.IO;
using System.Data.SqlClient;
using RosterNetClient;

namespace Posh_UC
{
    public static class RosterManagement
    {
        public enum RosterAction
        {
            CONTACT_ADD,
            CONTACT_MODIFY,
            DELETE_CONTACT,
            GROUP_ADD,
            GROUP_MODIFY,
            GROUP_DELETE,
            CONTACT_SUBSCRIBE
        }
    }


    [Cmdlet(VerbsCommon.Get, "ImpUser")]
    public class GetImpUser : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            if (!CurrentImpConnection.Instance.Loaded)
            {
                throw new ClientNotLoadedException();
            }
        }

        protected override void ProcessRecord()
        {
            var user = CurrentImpConnection.Instance.RosterClient.Execute(client =>
            {
                var res = client.getUser(new GetUserReq
                {
                    userid = Username
                });
                return res.@return;
            });
            user.Exception.ThrowIfNotNull();
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

    [Cmdlet(VerbsCommon.Get, "ImpContacts")]
    public class GetImpContacts : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            if (!CurrentImpConnection.Instance.Loaded)
                throw new ClientNotLoadedException();
            Username = Username.EscapeSql();
        }

        protected override void ProcessRecord()
        {
            var contacts = CurrentImpConnection.Instance.RosterClient.Execute(client =>
            {
                return client.executeSQLQuery(new ExecuteSQLQueryReq
                {
                    sql = string.Format(@"
select
 u.userid Owner,
 r.contact_jid Buddy,
 r.nickname Nickname,
 r.state State,
 g.group_name Group
from 
 rosters r 
 join enduser u on u.xcp_user_id = r.user_id 
 left outer join groups g on g.roster_id = r.roster_id 
where
 lower(u.userid) like lower('{0}') 
order by
 u.userid,r.contact_jid", Username.ToLower())
                });
            });

            contacts.Exception.ThrowIfNotNull();

            WriteObject(contacts.Value.@return.ToNodeList(), true);

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

    [Cmdlet(VerbsCommon.Remove, "ImpContact2")]
    public class RemoveImpContact2 : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            if (!CurrentImpConnection.Instance.Loaded)
                throw new ClientNotLoadedException();

            Owner = Owner.EscapeSql();
            Contact = Contact.EscapeSql();
            Group = Group.EscapeSql();
        }

        protected override void ProcessRecord()
        {
            var sqlresult = CurrentImpConnection.Instance.RosterClient.Execute(client =>
            {
                return client.executeSQLUpdate(new ExecuteSQLUpdateReq
                {
                    sql = string.Format(@"
insert into RosterSyncQueue
  (userid, buddyjid, buddynickname, groupname, action)
values
  ('{0}', '{1}', '', '{2}', 3)
", Owner, Contact, Group)
                });
            });

            sqlresult.Exception.ThrowIfNotNull();
        }

        [Parameter(
        ParameterSetName = "String",
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        ValueFromPipeline = true,
        Position = 0,
        HelpMessage = "Username to retrieve")]
        public string Owner;

        [Parameter(
        ParameterSetName = "String",
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        ValueFromPipeline = true,
        Position = 1,
        HelpMessage = "Contact to remove")]
        public string Contact;

        [Parameter(
        ParameterSetName = "String",
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        ValueFromPipeline = true,
        Position = 2,
        HelpMessage = "Contact group0")]
        [AllowEmptyString]
        public string Group;
    }

    [Cmdlet(VerbsCommon.Remove, "ImpContact")]
    public class RemoveImpContact : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            if (!CurrentImpConnection.Instance.Loaded)
                throw new ClientNotLoadedException();

            Owner = Owner.EscapeSql();
            Contact = Contact.EscapeSql();
            Group = Group.EscapeSql();
        }

        protected override void ProcessRecord()
        {
            var impresult = CurrentImpConnection.Instance.RosterClient.Execute(client =>
            {
                return client.removeContact(new removeContactReq
                {
                    contact = Contact,
                    groupName = Group,
                    userJid = Owner
                });
            });

            impresult.Exception.ThrowIfNotNull();

            WriteObject(impresult.Value);
        }

        [Parameter(
        ParameterSetName = "String",
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        ValueFromPipeline = true,
        Position = 0,
        HelpMessage = "Username to retrieve")]
        public string Owner;

        [Parameter(
        ParameterSetName = "String",
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        ValueFromPipeline = true,
        Position = 1,
        HelpMessage = "Contact to remove")]
        public string Contact;

        [Parameter(
        ParameterSetName = "String",
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        ValueFromPipeline = true,
        Position = 2,
        HelpMessage = "Contact group0")]
        [AllowEmptyString]
        public string Group;
    }

    [Cmdlet(VerbsCommon.Add, "ImpContact2")]
    public class AddImpContact2 : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            if (!CurrentImpConnection.Instance.Loaded)
                throw new ClientNotLoadedException();
            Owner = Owner.EscapeSql();
            Contact = Contact.EscapeSql();
            Nickname = Nickname.EscapeSql();
            Group = Group.EscapeSql();           
        }

        protected override void ProcessRecord()
        {
            var sqlresult = CurrentImpConnection.Instance.RosterClient.Execute(client =>
            {
                return client.executeSQLUpdate(new ExecuteSQLUpdateReq
                {
                    sql = string.Format(@"
insert into RosterSyncQueue
  (userid, buddyjid, buddynickname, groupname, action)
values
  ('{0}', '{1}', '{2}', '{3}', 1)
", Owner, Contact, Nickname, Group)
                });
            });

            sqlresult.Exception.ThrowIfNotNull();
        }

        [Parameter(
        ParameterSetName = "String",
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        ValueFromPipeline = true,
        Position = 0,
        HelpMessage = "Username to retrieve")]
        public string Owner;

        [Parameter(
        ParameterSetName = "String",
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        ValueFromPipeline = true,
        Position = 1,
        HelpMessage = "Contact to remove")]
        [ValidatePattern(@"^(?!sip:)\S+@\S+\.\S+")]
        public string Contact;

        [Parameter(
        ParameterSetName = "String",
        Mandatory = false,
        ValueFromPipelineByPropertyName = true,
        ValueFromPipeline = true,
        Position = 2,
        HelpMessage = "Contact to remove")]
        [AllowEmptyString]
        public string Nickname;

        [Parameter(
        ParameterSetName = "String",
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        ValueFromPipeline = true,
        Position = 3,
        HelpMessage = "Contact group0")]
        public string Group;
    }

    [Cmdlet(VerbsCommon.Add, "ImpContact")]
    public class AddImpContact : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            if (!CurrentImpConnection.Instance.Loaded)
                throw new ClientNotLoadedException();
            Owner = Owner.EscapeSql();
            Contact = Contact.EscapeSql();
            Nickname = Nickname.EscapeSql();
            Group = Group.EscapeSql();
        }

        protected override void ProcessRecord()
        {
            var impresult = CurrentImpConnection.Instance.RosterClient.Execute(client =>
            {
                var req = new addContactReq
                {
                    contact = Contact,
                    groupName = Group,
                    userJid = Owner
                };
                if (Nickname.HasValue())
                    req.nickName = Nickname;

                return client.addContact(req);
            });

            impresult.Exception.ThrowIfNotNull();

            WriteObject(impresult.Value);
        }

        [Parameter(
        ParameterSetName = "String",
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        ValueFromPipeline = true,
        Position = 0,
        HelpMessage = "Username to retrieve")]
        public string Owner;

        [Parameter(
        ParameterSetName = "String",
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        ValueFromPipeline = true,
        Position = 1,
        HelpMessage = "Contact to remove")]
        [ValidatePattern(@"^(?!sip:)\S+@\S+\.\S+")]
        public string Contact;

        [Parameter(
        ParameterSetName = "String",
        Mandatory = false,
        ValueFromPipelineByPropertyName = true,
        ValueFromPipeline = true,
        Position = 2,
        HelpMessage = "Contact to remove")]
        [AllowEmptyString]
        public string Nickname;

        [Parameter(
        ParameterSetName = "String",
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        ValueFromPipeline = true,
        Position = 3,
        HelpMessage = "Contact group0")]
        public string Group;
    }

    [Cmdlet(VerbsData.Update, "ImpContact")]
    public class UpdateImpContact : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            if (!CurrentImpConnection.Instance.Loaded)
                throw new ClientNotLoadedException();
        }

        protected override void ProcessRecord()
        {
            var impresult = CurrentImpConnection.Instance.RosterClient.Execute(client =>
            {
                return client.updateContact(new updateContactReq
                {
                    newContact = NewContact,
                    oldContact = OldContact
                });
            });

            impresult.Exception.ThrowIfNotNull();

            WriteObject(impresult.Value);
        }

        [Parameter(
        ParameterSetName = "String",
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        ValueFromPipeline = true,
        Position = 0,
        HelpMessage = "Username to retrieve")]
        public string NewContact;

        [Parameter(
        ParameterSetName = "String",
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        ValueFromPipeline = true,
        Position = 1,
        HelpMessage = "Contact to remove")]
        public string OldContact;
    }

    [Cmdlet(VerbsLifecycle.Invoke, "ImpSqlCommand")]
    public class InvokeImpSqlCommand : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            if (!CurrentImpConnection.Instance.Loaded)
                throw new ClientNotLoadedException();            
        }

        protected override void ProcessRecord()
        {
            var sqlresults = CurrentImpConnection.Instance.RosterClient.Execute(client =>
            {
                return client.executeSQLQuery(new ExecuteSQLQueryReq
                {
                    sql = Sql
                });
            });

            sqlresults.Exception.ThrowIfNotNull();

            WriteObject(sqlresults.Value.@return.ToNodeList(), true);
        }

        [Parameter(
        ParameterSetName = "String",
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        ValueFromPipeline = true,
        Position = 0,
        HelpMessage = "Sql command to run")]
        public string Sql;
    }
}
