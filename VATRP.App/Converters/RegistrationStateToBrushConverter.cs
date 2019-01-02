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
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using VATRP.Core.Model;
using VATRP.LinphoneWrapper.Enums;

namespace com.vtcsecure.ace.windows.Converters
{
    public class RegistrationStateToBrushConverter : IValueConverter
    {
        public RegistrationStateToBrushConverter()
        {
            
        }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {   // CJM: Login problem
            if (value is LinphoneRegistrationState)
            {
                var state = (LinphoneRegistrationState)value;

                switch (state)
                {
                    case LinphoneRegistrationState.LinphoneRegistrationOk:
                        return new SolidColorBrush(Color.FromArgb(255, 0, 255, 0)); // Light Green
                    case LinphoneRegistrationState.LinphoneRegistrationFailed:
                        return new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)); // Red
                    default:
                        return new SolidColorBrush(Color.FromArgb(255, 200, 200, 200)); // Gray
                }
            }

            return new SolidColorBrush(Color.FromArgb(255, 0x0d, 0x6e, 0x0f)); // Dark green
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}