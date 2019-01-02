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
using System.Media;
using System.Timers;
using VATRP.Core.Interfaces;

namespace VATRP.Core.Services
{
    public partial class SoundService : ISoundService
    {
        private readonly ServiceManagerBase manager;

        private SoundPlayer ringTonePlayer;
        private SoundPlayer ringBackTonePlayer;
        private SoundPlayer eventPlayer;
        private SoundPlayer connPlayer;
        private bool isRingTonePlaying;
        private bool isRingbackPlaying;

        public SoundService(ServiceManagerBase manager)
        {
            this.manager = manager;
        }

        public bool IsStarted { get; set; }

        public bool IsStopped { get; set; }

        public event EventHandler<EventArgs> ServiceStarted;

        public event EventHandler<EventArgs> ServiceStopped;

        public bool Start()
        {
            if (IsStarted)
                return true;
            try
            {
                this.ringTonePlayer = new SoundPlayer(Properties.Resources.ringtone);
                this.ringBackTonePlayer = new SoundPlayer(Properties.Resources.ringbacktone);
                this.eventPlayer = new SoundPlayer(Properties.Resources.newmsg);
                this.connPlayer = new SoundPlayer(Properties.Resources.connevent);               
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public bool Stop()
        {
            if (IsStopped)
                return true;

            return true;
        }

        public void PlayRingTone()
        {
            //***************************************************************************************************************
            // Start Ringing for Incoming call.
            //****************************************************************************************************************
            if (isRingTonePlaying)
                return;
            isRingTonePlaying = true;
            this.ringTonePlayer.PlayLooping();
        }

        public void StopRingTone()
        {
            if (!isRingTonePlaying)
                return;
            isRingTonePlaying = false;
            this.ringTonePlayer.Stop();
        }

        public void PlayRingBackTone()
        {
            if (isRingbackPlaying)
                return;
            isRingbackPlaying = true;
            this.ringBackTonePlayer.PlayLooping();
        }

        public void StopRingBackTone()
        {
            if (!isRingbackPlaying)
                return;
            isRingbackPlaying = false;
            this.ringBackTonePlayer.Stop();
        }

        public void PlayNewEvent()
        {
            this.eventPlayer.Play();
        }

        public void StopNewEvent()
        {
            this.eventPlayer.Stop();
        }

        public void PlayConnectionChanged(bool connected)
        {
            this.connPlayer.Play();
        }

        public void StopConnectionChanged(bool connected)
        {
            this.connPlayer.Stop();
        }
    }
}
