#region copyright
/**
 * Copyright © The MITRE Corporation.
 *
 * This program is licensed under the terms of the GNU General Public License Version 2, as published by the Free Software Foundation. This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  Please see the
 * GNU General Public License for more details.
 
 * This program or code contains content developed by The MITRE Corporation. If this program is used in a deployment or embedded within another project, MITRE requests that you send an email to opensource@mitre.org to let us know where and how this program is being used.
 
 * NOTICE
 * This (software/technical data) was produced for the U. S. Government under Contract Number HHSM-500-2012-00008I, and is subject to Federal Acquisition Regulation Clause 52.227-14, Rights in Data-General. No other use other than that granted to the U. S. Government, or to those acting on behalf of the U. S. Government under that Clause is authorized without the express written permission of The MITRE Corporation. For further information, please contact The MITRE Corporation, Contracts Management Office, 7515 Colshire Drive, McLean, VA  22102-7539, (703) 983-6000.
 */
#endregion

namespace VATRP.LinphoneWrapper.Enums
{
    public enum LinphoneCallState
    {
        LinphoneCallIdle, // Initial call state
        LinphoneCallIncomingReceived, // This is a new incoming call
        LinphoneCallOutgoingInit, // An outgoing call is started
        LinphoneCallOutgoingProgress, // An outgoing call is in progress
        LinphoneCallOutgoingRinging, // An outgoing call is ringing at remote end
        LinphoneCallOutgoingEarlyMedia, // An outgoing call is proposed early media
        LinphoneCallConnected, // <Connected, the call is answered
        LinphoneCallStreamsRunning, // The media streams are established and running
        LinphoneCallPausing, // The call is pausing at the initiative of local end
        LinphoneCallPaused, // The call is paused, remote end has accepted the pause
        LinphoneCallResuming, // The call is being resumed by local end
        LinphoneCallRefered, // <The call is being transfered to another party, resulting in a new outgoing call to follow immediately
        LinphoneCallError, // The call encountered an error
        LinphoneCallEnd, // The call ended normally
        LinphoneCallPausedByRemote, // The call is paused by remote end
        LinphoneCallUpdatedByRemote, // The call's parameters change is requested by remote end, used for example when video is added by remote
        LinphoneCallIncomingEarlyMedia, // We are proposing early media to an incoming call
        LinphoneCallUpdating, // A call update has been initiated by us
        LinphoneCallReleased, // The call object is no more retained by the core
        LinphoneCallEarlyUpdatedByRemote, // The call is updated by remote while not yet answered (early dialog SIP UPDATE received).
        LinphoneCallEarlyUpdating // We are updating the call while not yet answered (early dialog SIP UPDATE sent)
    };
}