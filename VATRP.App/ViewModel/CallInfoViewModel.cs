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
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Model;
using VATRP.Linphone.VideoWrapper;
using VATRP.LinphoneWrapper;
using VATRP.LinphoneWrapper.Enums;
using VATRP.LinphoneWrapper.Structs;
using com.vtcsecure.ace.windows.Utilities;
using log4net;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class CallInfoViewModel : ViewModelBase
    {
        #region Members
        private string _audioCodec = "None";
        private string _videoCodec;
        private int _sipPort = 5060;
        private int _rtpPort = 7000;
        private string _sendingVideoResolution = string.Empty;
        private string _receivingVideoResolution = string.Empty;
        private float _sendingFps = 0.0f;
        private float _receivingFps = 0.0f;
        private string _uploadBandwidth = string.Empty;
        private string _downloadBandwidth = string.Empty;
        private string _iceSetup = string.Empty;
        private string _mediaEncryption = string.Empty;
        private string _rtpProfile;
        private bool _avpfenabled = false;
        private string _videoPacketLossSending;
        private string _videoPacketLossReceiving;
        private string _audioPacketLossSending;
        private string _audioPacketLossReceiving;
        private string _videoPacketLate;
        private string _AudioPacketLate;
        private string _videoInterarrivalJitterSending;
        private string _videoInterarrivalJitterReceiving;
        private string _audioInterarrivalJitterSending;
        private string _audioInterarrivalJitterReceiving;
        private LinphoneCallStats _audioStats;
        private LinphoneCallStats _videoStats;
        private QualityIndicator _quality;
        private readonly ILog _LOG; // cjm-aug17
        private VATRPCallState _currentState; // cjm-aug17
        #endregion

        #region Events

        public delegate void CallQualityChangedDelegate(QualityIndicator callQuality);
        public event CallQualityChangedDelegate CallQualityChangedEvent;

        #endregion

        public CallInfoViewModel()
        {
            _LOG = LogManager.GetLogger("TechincalInfo"); //cjm-aug17
            CurrentState = VATRPCallState.None;
        }

        #region Properties

        public VATRPCallState CurrentState // cjm-aug17
        {
            get { return _currentState; }
            set { _currentState = value; }
        }

        public string AudioCodec
        {
            get { return _audioCodec; }
            set
            {
                _audioCodec = value;
                OnPropertyChanged("AudioCodec");
            }
        }

        public string VideoCodec
        {
            get { return _videoCodec; }
            set
            {
                _videoCodec = value;
                OnPropertyChanged("VideoCodec");
            }
        }

        public int SipPort
        {
            get { return _sipPort; }
            set
            {
                _sipPort = value;
                OnPropertyChanged("SipPort");
            }
        }

        public int RtpPort
        {
            get { return _rtpPort; }
            set
            {
                _rtpPort = value;
                OnPropertyChanged("RtpPort");
            }
        }

        public string SendingVideoResolution
        {
            get { return _sendingVideoResolution; }
            set
            {
                _sendingVideoResolution = value;
                OnPropertyChanged("SendingVideoResolution");
            }
        }

        public string ReceivingVideoResolution
        {
            get { return _receivingVideoResolution; }
            set
            {
                _receivingVideoResolution = value;
                OnPropertyChanged("ReceivingVideoResolution");
            }
        }

        public float SendingFPS
        {
            get { return _sendingFps; }
            set
            {
                _sendingFps = value;
                OnPropertyChanged("SendingFPS");
            }
        }

        public float ReceivingFPS
        {
            get { return _receivingFps; }
            set
            {
                _receivingFps = value;
                OnPropertyChanged("ReceivingFPS");
            }
        }

        public string UploadBandwidth
        {
            get { return _uploadBandwidth; }
            set
            {
                _uploadBandwidth = value;
                OnPropertyChanged("UploadBandwidth");
            }
        }

        public string DownloadBandwidth
        {
            get { return _downloadBandwidth; }
            set
            {
                _downloadBandwidth = value;
                OnPropertyChanged("DownloadBandwidth");
            }
        }

        public string IceSetup
        {
            get { return _iceSetup; }
            set
            {
                _iceSetup = value;
                OnPropertyChanged("IceSetup");
            }
        }

        public string MediaEncryption
        {
            get { return _mediaEncryption; }
            set
            {
                _mediaEncryption = value;
                OnPropertyChanged("MediaEncryption");
            }
        }

        public string RtpProfile
        {
            get { return _rtpProfile; }
            set
            {
                _rtpProfile = value;
                OnPropertyChanged("RtpProfile");
            }
        }

        public QualityIndicator CallQuality
        {
            get { return _quality; }
            set
            {
                _quality = value;
                OnPropertyChanged("CallQuality");
            }
        }
        public bool AVPFEnabled
        {
            get { return _avpfenabled; }
            set
            {
                _avpfenabled = value;
                OnPropertyChanged("AVPFEnabled");
            }
        }

        public string VideoPacketLossSending
        {
            get { return _videoPacketLossSending; }
            set
            {
                _videoPacketLossSending = value;
                OnPropertyChanged("VideoPacketLossSending");
            }
        }
        public string VideoPacketLossReceiving
        {
            get { return _videoPacketLossReceiving; }
            set
            {
                _videoPacketLossReceiving = value;
                OnPropertyChanged("VideoPacketLossReceiving");
            }
        }
        public string AudioPacketLossSending
        {
            get { return _audioPacketLossSending; }
            set
            {
                _audioPacketLossSending = value;
                OnPropertyChanged("AudioPacketLossSending");
            }
        }
        public string AudioPacketLossReceiving
        {
            get { return _audioPacketLossReceiving; }
            set
            {
                _audioPacketLossReceiving = value;
                OnPropertyChanged("AudioPacketLossReceiving");
            }
        }
        public string VideoPacketLate
        {
            get { return _videoPacketLate; }
            set
            {
                _videoPacketLate = value;
                OnPropertyChanged("VideoPacketLate");
            }
        }
        public string AudioPacketLate
        {
            get { return _AudioPacketLate; }
            set
            {
                _AudioPacketLate = value;
                OnPropertyChanged("AudioPacketLate");
            }
        }
        public string VideoInterarrivalJitterSending
        {
            get { return _videoInterarrivalJitterSending; }
            set
            {
                _videoInterarrivalJitterSending = value;
                OnPropertyChanged("VideoInterarrivalJitterSending");
            }
        }
        public string VideoInterarrivalJitterReceiving
        {
            get { return _videoInterarrivalJitterReceiving; }
            set
            {
                _videoInterarrivalJitterReceiving = value;
                OnPropertyChanged("VideoInterarrivalJitterReceiving");
            }
        }
        public string AudioInterarrivalJitterSending
        {
            get { return _audioInterarrivalJitterSending; }
            set
            {
                _audioInterarrivalJitterSending = value;
                OnPropertyChanged("AudioInterarrivalJitterSending");
            }
        }
        public string AudioInterarrivalJitterReceiving
        {
            get { return _audioInterarrivalJitterReceiving; }
            set
            {
                _audioInterarrivalJitterReceiving = value;
                OnPropertyChanged("AudioInterarrivalJitterReceiving");
            }
        }

        #endregion

        private QualityIndicator ConvertToNamedQuality(VATRPCall call)
        {
            var rating = (float)LinphoneAPI.linphone_call_get_current_quality(call.NativeCallPtr);
            if (rating >= 4.0)
                return QualityIndicator.Good;
            if (rating >= 3.0)
                return QualityIndicator.Medium;
            if (rating >= 2.0)
                return QualityIndicator.Poor;
            if (rating >= 1.0)
                return QualityIndicator.VeryPoor;
            if (rating >= 0)
                return QualityIndicator.ToBad;
            return QualityIndicator.Unknown;
        }

        internal void UpdateCallInfo(VATRPCall call)
        {
            //*********************************************************************************************************************************
            // When anything related to call is changed or when call is running then this method is called.
            //*********************************************************************************************************************************
            if (call == null || call.CallState != VATRPCallState.StreamsRunning)
            {
                LogCallData(call); // cjm-aug17
                ResetCallInfoView();
                return;
            }

            ServiceManager.Instance.LinphoneService.LockCalls();
            ServiceManager.Instance.LinphoneService.GetCallAudioStats(call.NativeCallPtr, ref _audioStats);
            ServiceManager.Instance.LinphoneService.GetCallVideoStats(call.NativeCallPtr, ref _videoStats);

            IntPtr curparams = ServiceManager.Instance.LinphoneService.GetCallParams(call.NativeCallPtr);
            if (curparams != IntPtr.Zero)
            {

                
                int sipPort, rtpPort;
                ServiceManager.Instance.LinphoneService.GetUsedPorts(out sipPort, out rtpPort);

                SipPort = sipPort;
                RtpPort = rtpPort;
                bool has_video = LinphoneAPI.linphone_call_params_video_enabled(curparams) == 1;

                MSVideoSizeDef size_received = LinphoneAPI.linphone_call_params_get_received_video_size(curparams);
                MSVideoSizeDef size_sent = LinphoneAPI.linphone_call_params_get_sent_video_size(curparams);
                IntPtr rtp_profile = LinphoneAPI.linphone_call_params_get_rtp_profile(curparams);

                if (rtp_profile != IntPtr.Zero)
                {
                    RtpProfile = Marshal.PtrToStringAnsi(rtp_profile);
                }
                AudioCodec = ServiceManager.Instance.LinphoneService.GetUsedAudioCodec(curparams);

                int avpf_mode = ServiceManager.Instance.LinphoneService.GetAVPFMode();

                if (avpf_mode == 0)
                {
                    AVPFEnabled = false;
                }
                else if (avpf_mode == 1)
                {
                    AVPFEnabled = true;
                }


                var videoCodecName = ServiceManager.Instance.LinphoneService.GetUsedVideoCodec(curparams);

                //Console.WriteLine("videoCodecName " + videoCodecName);
                if (has_video && !string.IsNullOrWhiteSpace(videoCodecName))
                {
                    VideoCodec = videoCodecName;
                    UploadBandwidth = string.Format("{0:0.##} kbit/s a {1:0.##} kbit/s v {2:0.##} kbit/s",
                        _audioStats.upload_bandwidth + _videoStats.upload_bandwidth, _audioStats.upload_bandwidth,
                        _videoStats.upload_bandwidth);

                    DownloadBandwidth = string.Format("{0:0.##} kbit/s a {1:0.##} kbit/s v {2:0.##} kbit/s",
                        _audioStats.download_bandwidth + _videoStats.download_bandwidth, _audioStats.download_bandwidth,
                        _videoStats.download_bandwidth);
                    ReceivingFPS = ServiceManager.Instance.LinphoneService.GetFrameRate(curparams, false);
                    SendingFPS = ServiceManager.Instance.LinphoneService.GetFrameRate(curparams, true);

                  // System.Diagnostics.Debug.WriteLine(ReceivingFPS.ToString(), SendingFPS.ToString());
                    var vs = ServiceManager.Instance.LinphoneService.GetVideoSize(curparams, false);
                    ReceivingVideoResolution = string.Format("{0}({1}x{2})", "", vs.width, vs.height);

                    vs = ServiceManager.Instance.LinphoneService.GetVideoSize(curparams, true);
                    SendingVideoResolution = string.Format("{0}({1}x{2})", "", vs.width, vs.height);

                }
                else
                {
                    VideoCodec = "Not used";
                    ReceivingFPS = 0;
                    SendingFPS = 0;
                    UploadBandwidth = string.Format("a {0:0.##} kbit/s", _audioStats.upload_bandwidth);
                    DownloadBandwidth = string.Format("a {0:0.##} kbit/s", _audioStats.download_bandwidth);
                    ReceivingVideoResolution = "N/A";
                    SendingVideoResolution = "N/A";
                }
                switch ((LinphoneIceState) _audioStats.ice_state)
                {
                    case LinphoneIceState.LinphoneIceStateNotActivated:
                        IceSetup = "Not Activated";
                        break;
                    case LinphoneIceState.LinphoneIceStateFailed:
                        IceSetup = "Failed";
                        break;
                    case LinphoneIceState.LinphoneIceStateInProgress:
                        IceSetup = "In Progress";
                        break;
                    case LinphoneIceState.LinphoneIceStateHostConnection:
                        IceSetup = "Connected directly";
                        break;
                    case LinphoneIceState.LinphoneIceStateReflexiveConnection:
                        IceSetup = "Connected through NAT";
                        break;
                    case LinphoneIceState.LinphoneIceStateRelayConnection:
                        IceSetup = "Connected through a relay";
                        break;
                }

                switch (ServiceManager.Instance.LinphoneService.GetMediaEncryption(curparams))
                {
                    case LinphoneMediaEncryption.LinphoneMediaEncryptionNone:
                        MediaEncryption = "None";
                        break;
                    case LinphoneMediaEncryption.LinphoneMediaEncryptionSRTP:
                        MediaEncryption = "SRTP";
                        break;
                    case LinphoneMediaEncryption.LinphoneMediaEncryptionZRTP:
                        MediaEncryption = "ZRTP";
                        break;
                    case LinphoneMediaEncryption.LinphoneMediaEncryptionDTLS:
                        MediaEncryption = "DTLS";
                        break;
                }

                var curQuality = ConvertToNamedQuality(call);
                if (CallQuality != curQuality)
                {
                    CallQuality = curQuality;
                    if (CallQualityChangedEvent != null)
                        CallQualityChangedEvent(curQuality);
                }

                    AudioPacketLossSending = "Sending " + _audioStats.sender_loss_rate;
                    AudioPacketLossReceiving = "Receiving " + _audioStats.receiver_loss_rate;
                    AudioPacketLate = _audioStats.total_late_packets.ToString();
                    AudioInterarrivalJitterSending = "Sending " + _audioStats.sender_interarrival_jitter;
                    AudioInterarrivalJitterReceiving = "Receiving " + _audioStats.receiver_interarrival_jitter;

                    VideoPacketLossSending = "Sending " + _videoStats.sender_loss_rate;
                    VideoPacketLossReceiving = "Receiving " + _audioStats.receiver_loss_rate;
                    VideoPacketLate = _videoStats.total_late_packets.ToString();
                    VideoInterarrivalJitterSending = "Sending " + _audioStats.sender_interarrival_jitter;
                    VideoInterarrivalJitterReceiving = "Receiving " + _audioStats.receiver_interarrival_jitter;
            }
            ServiceManager.Instance.LinphoneService.UnlockCalls();
            LogCallData(call); // cjm-aug17
        }

        private void ResetCallInfoView()
        {
            AudioCodec = "Not used";
            VideoCodec = "Not used";
            ReceivingFPS = 0;
            SendingFPS = 0;
            DownloadBandwidth = "N/A";
            UploadBandwidth = "N/A";
            IceSetup = "N/A";
            MediaEncryption = "N/A";
            ReceivingVideoResolution = "N/A";
            SendingVideoResolution = "N/A";
            SipPort = 5060;
            RtpPort = 0;
            CallQuality = 0f;
            AVPFEnabled = false;
            VideoPacketLossSending = "Sending -1";
            VideoPacketLossReceiving = "Receiving -1";
            AudioPacketLossSending = "Sending -1";
            AudioPacketLossReceiving = "Receiving -1";
            VideoPacketLate = "-1";
            AudioPacketLate = "-1";
            VideoInterarrivalJitterSending = "Sending -1";
            VideoInterarrivalJitterReceiving = "Receiving -1";
            AudioInterarrivalJitterSending = "Sending -1";
            AudioInterarrivalJitterReceiving = "Receiving -1";
			
            if (CallQualityChangedEvent != null)
                CallQualityChangedEvent(QualityIndicator.Unknown);
        }

        internal void OnCallStatisticsChanged(VATRPCall call)
        {

            //****************************************************************************************************************************************
            // When anything related to call is changed like mute mic etc. Then this method will called. Or a message is sent from chat.
            //****************************************************************************************************************************************
            if (ServiceManager.Instance.Dispatcher.Thread != Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke((Action)(() => this.OnCallStatisticsChanged(call)));
                return;
            }
            //Console.WriteLine("OnCallStatisticsChanged");
            UpdateCallInfo(call);
        }

        private void LogCallData(VATRPCall call) //cjm-aug17
        {
            if (App.CurrentAccount.EnableTechCallLog)
            {
                if (call != null && CurrentState != call.CallState)
                {
                    if (CurrentState == VATRPCallState.None)
                    {
                        _LOG.Info("=========================================================================\n" +
                                 "                             Call Started                                \n" +
                                 "=========================================================================");
                    }
                    DateTime time = DateTime.Now;
                    string formattedTime = time.ToString();
                    CurrentState = call.CallState;
                    string data = String.Empty;
                    data = "Date                                  :   {0}\n" +
                           "Call State                            :   {1}\n" +
                           "Audio Codec                           :   {2}\n" +
                           "Video Codec                           :   {3}\n" +
                           "SIP Port                              :   {4}\n" +
                           "RTP Port                              :   {5}\n" +
                           "Receiving FPS                         :   {6}\n" +
                           "Sending FPS                           :   {7}\n" +
                           "Download Bandwidth                    :   {8}\n" +
                           "Upload Bandwidth                      :   {9}\n" +
                           "ICE Setup                             :   {10}\n" +
                           "Media Encryption                      :   {11}\n" +
                           "Call Quality                          :   {12}\n" +
                           "Receiving Video Resolution            :   {13}\n" +
                           "Sending Video Resolution              :   {14}\n" +
                           "AVPF Enabled                          :   {15}\n" +
                           "Video Packet Loss Sending             :   {16}\n" +
                           "Video Packet Loss Receiving           :   {17}\n" +
                           "Audio Packet Loss Sending             :   {18}\n" +
                           "Audio Packet Loss Receiving           :   {19}\n" +
                           "Video Packet Late                     :   {20}\n" +
                           "Audio Packet Late                     :   {21}\n" +
                           "Video Interarrival Jitter Sending     :   {22}\n" +
                           "Video Interarrival Jitter Receiving   :   {23}\n" +
                           "Audio Interarrival Jitter Sending     :   {24}\n" +
                           "Audio Interarrival Jitter Receiving   :   {25}\n";
                    _LOG.Info(String.Format(data, formattedTime, CurrentState, AudioCodec, VideoCodec,
                                            SipPort, RtpPort, ReceivingFPS, SendingFPS,
                                            DownloadBandwidth, UploadBandwidth, IceSetup,
                                            MediaEncryption, CallQuality, ReceivingVideoResolution,
                                            SendingVideoResolution, AVPFEnabled, VideoPacketLossSending,
                                            VideoPacketLossReceiving, AudioPacketLossSending,
                                            AudioPacketLossReceiving, VideoPacketLate, AudioPacketLate,
                                            VideoInterarrivalJitterSending, VideoInterarrivalJitterReceiving,
                                            AudioInterarrivalJitterSending, AudioInterarrivalJitterReceiving));
                }
            }
        }
    }
}