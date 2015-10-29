using System;
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
            if (!CurrentAxlClient.Instance.Loaded)
            {
                throw new ClientNotLoadedException();
            }
        }

        protected override void ProcessRecord()
        {
            var phone = CurrentAxlClient.Instance.Client.Execute(client =>
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

    [Cmdlet(VerbsCommon.Get, "UcLine")]
    public class GetUcLine : PSCmdlet
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
            var line = CurrentAxlClient.Instance.Client.Execute(client =>
            {
                var res = client.getLine(new GetLineReq
                {
                    ItemsElementName = new ItemsChoiceType56[] { ItemsChoiceType56.pattern },
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
            HelpMessage = "DeviceName to retrieve")]
        public string DirectoryNumber;
    }

    [Cmdlet(VerbsCommon.Add, "UcPhone")]
    public class AddUcPhone : PSCmdlet
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
            Console.WriteLine("Retrieving phone template");

            var phone = CurrentAxlClient.Instance.Client.Execute(client =>
            {
                var res = client.getPhone(new GetPhoneReq
                {
                    ItemElementName = ItemChoiceType140.name,
                    Item = TemplateDevice
                });
                return res.@return;
            });
            if (phone.Exception != null)
                throw phone.Exception;

            var template = phone.Value.phone;

            Console.WriteLine("Retrieving line template");

            var lineresult = CurrentAxlClient.Instance.Client.Execute(client =>
            {
                var res = client.getLine(new GetLineReq
                {
                    ItemsElementName = new ItemsChoiceType56[] { ItemsChoiceType56.pattern },
                    Items = new object[] { ((RPhoneLine)template.lines.Items[0]).dirn.pattern }
                });

                return res.@return;
            });

            if (lineresult.Exception != null)
                throw lineresult.Exception;

            var linetemplate = lineresult.Value.line;

            Console.WriteLine("Creating line");

            var newlineresult = CurrentAxlClient.Instance.Client.Execute(client =>
            {
                var res = client.addLine(new AddLineReq
                {
                    line = new XLine
                    {
                        pattern = DirectoryNumber,
                        //description
                        usage = "Device",
                        routePartitionName = linetemplate.routePartitionName,
                        aarNeighborhoodName = linetemplate.aarNeighborhoodName,
                        aarDestinationMask = linetemplate.aarDestinationMask,
                        aarKeepCallHistory = linetemplate.aarKeepCallHistory,
                        aarVoiceMailEnabled = linetemplate.aarVoiceMailEnabled,
                        //callForwardBusy* = all ignored
                        autoAnswer = linetemplate.autoAnswer,
                        //networkHoldMohAudioSourceId
                        //userHoldMohAudioSourceId
                        alertingName = linetemplate.alertingName,
                        asciiAlertingName = linetemplate.asciiAlertingName,
                        presenceGroupName = linetemplate.presenceGroupName,
                        shareLineAppearanceCssName = linetemplate.shareLineAppearanceCssName,
                        voiceMailProfileName = linetemplate.voiceMailProfileName,
                        patternPrecedence = linetemplate.patternPrecedence,
                        releaseClause = linetemplate.releaseClause,
                        hrDuration = linetemplate.hrDuration,
                        hrInterval = linetemplate.hrInterval,
                        cfaCssPolicy = linetemplate.cfaCssPolicy,
                        defaultActivatedDeviceName = linetemplate.defaultActivatedDeviceName,
                        //parkMon* = ignore all park monitoring settings
                        partyEntranceTone = linetemplate.partyEntranceTone,
                        directoryURIs = new XDirectoryUri[] { new XDirectoryUri
                        {
                            isPrimary = "t",
                            advertiseGloballyViaIls = "t",
                            uri = DirectoryUri
                        } },
                        allowCtiControlFlag = linetemplate.allowCtiControlFlag,
                        rejectAnonymousCall = linetemplate.rejectAnonymousCall,
                        patternUrgency = linetemplate.patternUrgency,
                        //confidentialAccess
                        externalCallControlProfile = linetemplate.externalCallControlProfile,
                        //enterpriseAltNum
                        //e164AltNum
                        //pstnFailover
                        callControlAgentProfile = linetemplate.callControlAgentProfile,
                        //useEnterpriseAltNum
                        //useE164AltNum
                        active = linetemplate.active
                    }
                });
            });

            if (newlineresult.Exception != null)
                throw newlineresult.Exception;

            var createdline = CurrentAxlClient.Instance.Client.Execute(client =>
            {
                var res = client.getLine(new GetLineReq
                {
                    ItemsElementName = new ItemsChoiceType56[] { ItemsChoiceType56.pattern },
                    Items = new object[] { DirectoryNumber }
                });
                return res.@return;
            });

            if (createdline.Exception != null)
                throw createdline.Exception;

            Console.WriteLine("Creating Phone");

            var phonelinetemplate = (RPhoneLine)template.lines.Items[0];

            var result = CurrentAxlClient.Instance.Client.Execute(client =>
            {
                var res = client.addPhone(new AddPhoneReq
                {
                    phone = new XPhone
                    {
                        name = DeviceName,
                        description = Description,
                        product = template.product,
                        @class = template.@class,
                        protocol = template.protocol,
                        protocolSide = template.protocolSide,
                        callingSearchSpaceName = template.callingSearchSpaceName,
                        devicePoolName = template.devicePoolName,
                        commonDeviceConfigName = template.commonDeviceConfigName,
                        commonPhoneConfigName = template.commonPhoneConfigName,
                        networkLocation = template.networkLocation,
                        locationName = template.locationName,
                        mediaResourceListName = template.mediaResourceListName,
                        //networkHoldMohAudioSourceId = 
                        //userHoldMohAudioSourceId = 
                        automatedAlternateRoutingCssName = template.automatedAlternateRoutingCssName,
                        aarNeighborhoodName = template.aarNeighborhoodName,
                        loadInformation = template.loadInformation,
                        vendorConfig = template.vendorConfig,
                        //versionStamp = Generate a GUID?
                        traceFlag = template.traceFlag,
                        //mlppDomainId = 
                        mlppIndicationStatus = template.mlppIndicationStatus,
                        preemption = template.preemption,
                        useTrustedRelayPoint = template.useTrustedRelayPoint,
                        retryVideoCallAsAudio = template.retryVideoCallAsAudio,
                        securityProfileName = template.securityProfileName,
                        sipProfileName = template.sipProfileName,
                        cgpnTransformationCssName = template.cgpnTransformationCssName,
                        useDevicePoolCgpnTransformCss = template.useDevicePoolCgpnTransformCss,
                        geoLocationName = template.geoLocationName,
                        geoLocationFilterName = template.geoLocationFilterName,
                        sendGeoLocation = template.sendGeoLocation,                        
                        //lines = new XPhoneLines
                        //{
                        //    Items = new object[]
                        //    {
                        //        new XPhoneLine
                        //        {
                        //            index = "1",
                        //            display = PhoneDisplay,
                        //            dirn = new XDirn { pattern = DirectoryNumber, uuid = createdline.Value.line.uuid },
                        //            ringSetting = phonelinetemplate.consecutiveRingSetting,
                        //            consecutiveRingSetting = phonelinetemplate.consecutiveRingSetting,
                        //            ringSettingIdlePickupAlert = phonelinetemplate.ringSettingIdlePickupAlert,
                        //            ringSettingActivePickupAlert = phonelinetemplate.ringSettingActivePickupAlert,
                        //            displayAscii = PhoneDisplay,
                        //            e164Mask = phonelinetemplate.e164Mask,
                        //            mwlPolicy = phonelinetemplate.mwlPolicy,
                        //            maxNumCalls = phonelinetemplate.maxNumCalls,
                        //            busyTrigger = phonelinetemplate.busyTrigger,
                        //            //callInfoDisplay 
                        //            //recordingProfileName
                        //            //monitoringCssName
                        //            recordingFlag = phonelinetemplate.recordingFlag,
                        //            audibleMwi = phonelinetemplate.audibleMwi,
                        //            speedDial = phonelinetemplate.speedDial,
                        //            partitionUsage = phonelinetemplate.partitionUsage,
                        //            //associatedEndusers
                        //            missedCallLogging = phonelinetemplate.missedCallLogging,
                        //            recordingMediaSource = phonelinetemplate.recordingMediaSource,
                        //            //ctiid
                        //        }
                        //    }
                        //},
                        phoneTemplateName = template.phoneTemplateName,
                        //speeddials = template.speeddials,
                        //busyLampFields = template.busyLampFields,
                        primaryPhoneName = template.primaryPhoneName,
                        ringSettingIdleBlfAudibleAlert = template.ringSettingIdleBlfAudibleAlert,
                        ringSettingBusyBlfAudibleAlert = template.ringSettingBusyBlfAudibleAlert,
                        //blfDirectedCallParks = template.blfDirectedCallParks,
                        //addOnModules = template.addOnModules,
                        //userLocale = 
                        //networkLocale
                        //idleTimeout
                        //authenticationUrl
                        //directoryUrl
                        //idleUrl
                        //informationUrl
                        //messagesUrl
                        //proxyServerUrl
                        //servicesUrl
                        //services
                        softkeyTemplateName = template.softkeyTemplateName,
                        defaultProfileName = template.defaultProfileName,
                        enableExtensionMobility = template.enableExtensionMobility,
                        singleButtonBarge = template.singleButtonBarge,
                        joinAcrossLines = template.joinAcrossLines,
                        builtInBridgeStatus = template.builtInBridgeStatus,
                        callInfoPrivacyStatus = template.callInfoPrivacyStatus,
                        hlogStatus = template.hlogStatus,
                        //ownerUserName = generated from association?
                        ignorePresentationIndicators = template.ignorePresentationIndicators,
                        packetCaptureMode = template.packetCaptureMode,
                        packetCaptureDuration = template.packetCaptureDuration,
                        subscribeCallingSearchSpaceName = template.subscribeCallingSearchSpaceName,
                        rerouteCallingSearchSpaceName = template.rerouteCallingSearchSpaceName,
                        allowCtiControlFlag = template.allowCtiControlFlag,
                        presenceGroupName = template.presenceGroupName,
                        unattendedPort = template.unattendedPort,
                        requireDtmfReception = template.requireDtmfReception,
                        rfc2833Disabled = template.rfc2833Disabled,
                        //certificateOperation = template.certificateOperation,
                        authenticationMode = template.authenticationMode,
                        //keySize = template.keySize,
                        //authenticationString
                        //upgradeFinishTime
                        deviceMobilityMode = template.deviceMobilityMode,
                        remoteDevice = template.remoteDevice,
                        dndOption = template.dndOption,
                        dndRingSetting = template.dndRingSetting,
                        dndStatus = template.dndStatus,
                        isActive = template.isActive,
                        isDualMode = template.isDualMode,
                        //mobilityUserIdName
                        phoneSuite = template.phoneSuite,
                        phoneServiceDisplay = template.phoneServiceDisplay,
                        isProtected = template.isProtected,
                        mtpRequired = template.mtpRequired,
                        dialRulesName = template.dialRulesName,
                        //sshUserId
                        //sshPwd
                        //digestUser
                        outboundCallRollover = template.outboundCallRollover,
                        hotlineDevice = template.hotlineDevice,
                        secureInformationUrl = template.secureInformationUrl,
                        secureDirectoryUrl = template.secureDirectoryUrl,
                        secureMessageUrl = template.secureMessageUrl,
                        secureServicesUrl = template.secureServicesUrl,
                        secureAuthenticationUrl = template.secureAuthenticationUrl,
                        secureIdleUrl = template.secureIdleUrl,
                        alwaysUsePrimeLine = template.alwaysUsePrimeLine,
                        alwaysUsePrimeLineForVoiceMessage = template.alwaysUsePrimeLineForVoiceMessage,
                        featureControlPolicy = template.featureControlPolicy,
                        deviceTrustMode = template.deviceTrustMode,
                        earlyOfferSupportForVoiceCall = template.earlyOfferSupportForVoiceCall,
                        requireThirdPartyRegistration = template.requireThirdPartyRegistration,                        
                        blockIncomingCallsWhenRoaming = template.blockIncomingCallsWhenRoaming,
                        homeNetworkId = template.homeNetworkId,
                        AllowPresentationSharingUsingBfcp = template.AllowPresentationSharingUsingBfcp,
                        //confidentialAccess = template.confidentialAccess
                        requireOffPremiseLocation = template.requireOffPremiseLocation,
                        allowiXApplicableMedia = template.allowiXApplicableMedia,
                        cgpnIngressDN = template.cgpnIngressDN,
                        useDevicePoolCgpnIngressDN = template.useDevicePoolCgpnIngressDN,
                        //msisdn
                        enableCallRoutingToRdWhenNoneIsActive = template.enableCallRoutingToRdWhenNoneIsActive,
                        wifiHotspotProfile = template.wifiHotspotProfile,
                        wirelessLanProfileGroup = template.wirelessLanProfileGroup
                        //ctiid
                    }
                });
            });

            if (result.Exception != null)
                throw result.Exception;

            Console.WriteLine("Associating user with phone");

            var associationresult = CurrentAxlClient.Instance.Client.Execute(client =>
            {
                var res = client.updateUser(new UpdateUserReq
                {
                    ItemElementName = ItemChoiceType6.userid,
                    Item = Username,
                    associatedDevices = new string[] { DeviceName }
                });
                return res.@return;
            });

            Console.WriteLine("Phone created successfully");
        }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 0,
            HelpMessage = "DeviceName of existing device to use as a template")]
        public string TemplateDevice;

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 1,
            HelpMessage = "DeviceName of phone to add")]
        public string DeviceName;

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 2,
            HelpMessage = "DeviceNumber of phone to add")]
        public string DirectoryNumber;

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 3,
            HelpMessage = "Description of phone")]
        public string Description;

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 4,
            HelpMessage = "Phone display")]
        public string PhoneDisplay;

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 5,
            HelpMessage = "Directory URI (usually email address for jabber phones)")]
        public string DirectoryUri;

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 6,
            HelpMessage = "User to associate the phone to")]
        public string Username;
    }
}
