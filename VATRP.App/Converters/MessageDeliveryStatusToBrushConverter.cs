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
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using VATRP.Core.Enums;
using VATRP.LinphoneWrapper.Enums;
using Color = System.Windows.Media.Color;

namespace com.vtcsecure.ace.windows.Converters
{
    public class MessageDeliveryStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LinphoneChatMessageState)
            {
                switch ((LinphoneChatMessageState) value)
                {
                    case LinphoneChatMessageState.LinphoneChatMessageStateIdle:
                        return new SolidColorBrush(Color.FromArgb(255, 239, 244, 253));
                    case LinphoneChatMessageState.LinphoneChatMessageStateInProgress:
                        return new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));
                    case LinphoneChatMessageState.LinphoneChatMessageStateDelivered:
                        return new SolidColorBrush(Color.FromArgb(255, 108, 175, 74));
                    case LinphoneChatMessageState.LinphoneChatMessageStateNotDelivered:
                        return new SolidColorBrush(Color.FromArgb(255, 235, 62, 66));
                }
            }            
            return new SolidColorBrush(Color.FromArgb(0xff, 237, 237, 237));
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        
    }
}

