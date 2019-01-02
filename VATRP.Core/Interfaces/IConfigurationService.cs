﻿#region copyright
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
using VATRP.Core.Events;
using VATRP.Core.Model;

namespace VATRP.Core.Interfaces
{
    public interface IConfigurationService : IVATRPservice
    {
        bool IsStarting { get; }
        bool IsStarted { get; }

        bool IsStopping {get; }
        
        bool IsStopped { get; }

        string Get(Configuration.ConfSection section, Configuration.ConfEntry entry, string defaultValue);
        bool Set(Configuration.ConfSection section, Configuration.ConfEntry entry, string value);

        int Get(Configuration.ConfSection section, Configuration.ConfEntry entry, int defaultValue);
        bool Set(Configuration.ConfSection section, Configuration.ConfEntry entry, int value);

        bool Get(Configuration.ConfSection section, Configuration.ConfEntry entry, bool defaultValue);
        bool Set(Configuration.ConfSection section, Configuration.ConfEntry entry, bool value);

        void SaveConfig();

        event EventHandler<ConfigurationEventArgs> onConfigurationEvent;

        void Reset();
    }
}
