using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Management.Automation;
using System.Threading;
using System.IO;
using System.Data.SqlClient;
using agsXMPP.protocol.x.muc;

namespace Posh_UC
{
    public class RoomMember : IEquatable<RoomMember>
    {
        public string Role = string.Empty;
        public string Affiliation = string.Empty;
        public string Nickname = string.Empty;
        public string Jid = string.Empty;
        public agsXMPP.Jid FullJid = string.Empty;
        
        public bool Equals(RoomMember member)
        {
            if (Object.ReferenceEquals(member, null)) return false;
            if (object.ReferenceEquals(this, member)) return true;
            return Role.Equals(member.Role) &&
                   Affiliation.Equals(member.Affiliation) &&
                   Nickname.Equals(member.Nickname) &&
                   Jid.Equals(member.Jid);
        }

        public override int GetHashCode()
        {
            int hashRole = Role.GetHashCode();
            int hashAffiliation = Affiliation.GetHashCode();
            int hashNickname = Nickname.GetHashCode();
            int hashJid = Jid.GetHashCode();
            return hashRole ^ hashAffiliation ^ hashNickname ^ hashJid;
        }
    }

    [Cmdlet(VerbsCommon.Get, "XmppRoomMembers")]
    public class GetXmppRoomMembers : PSCmdlet
    {
        ManualResetEvent messageReceived = new ManualResetEvent(false);
        bool messageComplete = false;
        List<RoomMember> members = new List<RoomMember>();

        protected override void BeginProcessing()
        {
            if (!CurrentXmppConnection.Instance.Loaded)
                throw new ClientNotLoadedException();
        }

        protected override void ProcessRecord()
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            var muc = CurrentXmppConnection.Instance.XmppClient.GetMucManager();
            muc.JoinRoom(Room, "Posh-UC Support");
            muc.RequestList(Role.participant, Room, new agsXMPP.IqCB(OnMembershipResult), null);
            messageReceived.WaitOne(100);
            messageReceived.Reset();
            muc.RequestMemberList(Room, new agsXMPP.IqCB(OnMembershipResult), null);
            messageReceived.WaitOne(100);
            messageReceived.Reset();
            muc.RequestAdminList(Room, new agsXMPP.IqCB(OnMembershipResult), null);
            messageReceived.WaitOne(100);
            messageReceived.Reset();
            muc.RequestModeratorList(Room, new agsXMPP.IqCB(OnMembershipResult), null);
            messageReceived.WaitOne(100);
            messageReceived.Reset();
            muc.RequestOwnerList(Room, new agsXMPP.IqCB(OnMembershipResult), null);
            messageReceived.WaitOne(5000);
            muc.LeaveRoom(Room, "Posh-UC Support");
            if (!messageComplete)
                logger.Error("Timeout while waiting for list");
            else
                WriteObject(members.Distinct(), true);
        }

        [Parameter(
        ParameterSetName = "String",
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        ValueFromPipeline = true,
        Position = 0,
        HelpMessage = "xmpp room to retrieve")]
        public string Room;

        private void OnMembershipResult(object sender, agsXMPP.protocol.client.IQ iq, object data)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            if (iq.Type == agsXMPP.protocol.client.IqType.result)
            {
                var item = iq.Query.FirstChild as agsXMPP.protocol.x.muc.Item;                
                if (item != null && item.Nickname != "Posh-UC Support")
                {
                    var tem = new RoomMember();
                    tem.Affiliation = item.Affiliation.ToString();
                    tem.Jid = item.Jid.Bare;
                    tem.Role = item.Role.ToString();
                    tem.Nickname = item.Nickname;
                    tem.FullJid = item.Jid;
                    members.Add(tem);
                }
            } else
            {
                logger.Error("Failed to get result: {0}", iq.Error.ToString());
            }
            messageComplete = true;
            messageReceived.Set();
        }
    }

    [Cmdlet(VerbsCommon.Add, "XmppRoomMember")]
    public class AddXmppRoomMember : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            if (!CurrentXmppConnection.Instance.Loaded)
                throw new ClientNotLoadedException();
        }

        protected override void ProcessRecord()
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            var muc = CurrentXmppConnection.Instance.XmppClient.GetMucManager();
            muc.JoinRoom(Room, "Posh-UC Support");
            muc.Invite(MemberJid, Room);
            muc.LeaveRoom(Room, "Posh-UC Support");
        }

        [Parameter(
        ParameterSetName = "String",
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        ValueFromPipeline = true,
        Position = 0,
        HelpMessage = "xmpp room to retrieve")]
        public string Room;

        [Parameter(
        ParameterSetName = "String",
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        ValueFromPipeline = true,
        Position = 1,
        HelpMessage = "jid of user to add")]
        public string MemberJid;
    }

    [Cmdlet(VerbsCommon.Remove, "XmppRoomMember")]
    public class RemoveXmppRoomMember : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            if (!CurrentXmppConnection.Instance.Loaded)
                throw new ClientNotLoadedException();
        }

        protected override void ProcessRecord()
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            var muc = CurrentXmppConnection.Instance.XmppClient.GetMucManager();
            muc.JoinRoom(Room, "Posh-UC Support");
            muc.KickOccupant(Room, Nick);
            muc.LeaveRoom(Room, "Posh-UC Support");
        }

        [Parameter(
        ParameterSetName = "String",
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        ValueFromPipeline = true,
        Position = 0,
        HelpMessage = "xmpp room to retrieve")]
        public string Room;

        [Parameter(
        ParameterSetName = "String",
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        ValueFromPipeline = true,
        Position = 1,
        HelpMessage = "nick of user to remove")]
        public string Nick;
    }
}
