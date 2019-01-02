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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace com.vtcsecure.ace.windows.CustomControls.Resources
{
    public delegate void Resources_ContentChanging(ResourcesType contentType);
    public delegate void Resources_CallResource(ResourceInfo resourceInfo);

    public class BaseResourcePanel : UserControl
    {
        public event Resources_ContentChanging ContentChanging;
        public event Resources_CallResource CallResourceRequested;

        public string Title { get; set; }

        // Invoke the Content Changed event
        public virtual void OnContentChanging(ResourcesType contentType)
        {
            if (ContentChanging != null)
            {
                ContentChanging(contentType);
            }
        }

        public virtual void OnCallResourceRequested(ResourceInfo resourceInfo)
        {
            if (CallResourceRequested != null)
            {
                CallResourceRequested(resourceInfo);
            }
        }

    }
}
