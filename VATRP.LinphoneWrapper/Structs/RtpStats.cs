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

using System;
using System.Runtime.InteropServices;

namespace VATRP.LinphoneWrapper.Structs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct RtpStats
    {
        public UInt64 packet_sent;		/*number of outgoing packets */
        public UInt64 packet_dup_sent;	/*number of outgoing duplicate packets */
        public UInt64 sent;				/* outgoing total bytes (excluding IP header) */
        public UInt64 packet_recv;		/* number of incoming packets */
        public UInt64 packet_dup_recv;	/* number of incoming duplicate packets */
        public UInt64 recv;				/* incoming bytes of payload and delivered in time to the application */
        public UInt64 hw_recv;			/* incoming bytes of payload */
        public UInt64 outoftime;			/* number of incoming packets that were received too late */
        public Int64 cum_packet_loss;	/* cumulative number of incoming packet lost */
        public UInt64 bad;				/* incoming packets that did not appear to be RTP */
        public UInt64 discarded;			/* incoming packets discarded because the queue exceeds its max size */
        public UInt64 sent_rtcp_packets;	/* outgoing RTCP packets counter (only packets that embed a report block are considered) */ 
    }
}