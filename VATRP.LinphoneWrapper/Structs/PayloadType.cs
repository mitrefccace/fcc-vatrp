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
    public struct PayloadType
    {
        public int type; //  one of PAYLOAD_* macros
        public int clock_rate; //  rtp clock rate
        public char bits_per_sample; //  in case of continuous audio data 
        public string zero_pattern;
        public int pattern_length;
        //  other useful information for the application
        public int normal_bitrate; // in bit/s 
        public string mime_type; // actually the submime, ex: pcm, pcma, gsm
        public int channels; //  number of channels of audio 
        public string recv_fmtp; //  various format parameters for the incoming stream 
        public string send_fmtp; //  various format parameters for the outgoing stream 
        public PayloadTypeAvpfParams avpf ; //  AVPF parameters 
        public int flags;
        public IntPtr user_data;
    }
}