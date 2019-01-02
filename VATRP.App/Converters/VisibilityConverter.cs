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
using System.Windows;
using System.Windows.Data;
using VATRP.Core.Enums;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.Converters
{
    public class IntToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// FalseEquivalent (default : Visibility.Collapsed => see Constructor)
        /// </summary>
        public Visibility FalseEquivalent { get; set; }
        /// <summary>
        /// Define whether the opposite boolean value is crucial (default : false)
        /// </summary>
        public bool OppositeBooleanValue { get; set; }

        public IntToVisibilityConverter()
        {
            this.FalseEquivalent = Visibility.Collapsed;
            this.OppositeBooleanValue = false;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is int && targetType == typeof(Visibility))
            {
                bool booleanValue = (int)value == 0;

                if (OppositeBooleanValue)
                {
                    booleanValue = !booleanValue;
                }

                return booleanValue ? Visibility.Visible : FalseEquivalent;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return null;
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// FalseEquivalent (default : Visibility.Collapsed => see Constructor)
        /// </summary>
        public Visibility FalseEquivalent { get; set; }
        /// <summary>
        /// Define whether the opposite boolean value is crucial (default : false)
        /// </summary>
        public bool OppositeBooleanValue { get; set; }

        public BoolToVisibilityConverter()
        {
            this.FalseEquivalent = Visibility.Collapsed;
            this.OppositeBooleanValue = false;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is bool && targetType == typeof(Visibility))
            {
                bool booleanValue = (bool)value ;

                if (OppositeBooleanValue)
                {
                    booleanValue = !booleanValue;
                }

                return booleanValue ? Visibility.Visible : FalseEquivalent;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return null;
        }

    }

    public class StringLengthToVisibilityConverter : System.Windows.Markup.MarkupExtension, IValueConverter
    {
        /// <summary>
        /// FalseEquivalent (default : Visibility.Collapsed => see Constructor)
        /// </summary>
        public Visibility FalseEquivalent { get; set; }
        /// <summary>
        /// Define whether the opposite boolean value is crucial (default : false)
        /// </summary>
        public bool OppositeBooleanValue { get; set; }

        public StringLengthToVisibilityConverter()
        {
            this.FalseEquivalent = Visibility.Collapsed;
            this.OppositeBooleanValue = false;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is string && targetType == typeof(Visibility))
            {
                bool booleanValue = string.IsNullOrEmpty((string)value);

                if (OppositeBooleanValue)
                {
                    booleanValue = !booleanValue;
                }

                return booleanValue ? Visibility.Visible : FalseEquivalent;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    public class DirectionToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// FalseEquivalent (default : Visibility.Collapsed => see Constructor)
        /// </summary>
        public Visibility FalseEquivalent { get; set; }
        /// <summary>
        /// Define whether the opposite boolean value is crucial (default : false)
        /// </summary>
        public bool OppositeBooleanValue { get; set; }

        public DirectionToVisibilityConverter()
        {
            this.FalseEquivalent = Visibility.Collapsed;
            this.OppositeBooleanValue = false;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is MessageDirection && targetType == typeof(Visibility))
            {
                bool booleanValue = (MessageDirection)value == MessageDirection.Incoming;

                if (OppositeBooleanValue)
                {
                    booleanValue = !booleanValue;
                }

                return booleanValue ? Visibility.Visible : FalseEquivalent;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return null;
        }

    }

    public class CallStatusDurationVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// FalseEquivalent (default : Visibility.Collapsed => see Constructor)
        /// </summary>
        public Visibility FalseEquivalent { get; set; }
        /// <summary>
        /// Define whether the opposite boolean value is crucial (default : false)
        /// </summary>
        public bool OppositeBooleanValue { get; set; }

        public CallStatusDurationVisibilityConverter()
        {
            this.FalseEquivalent = Visibility.Visible;
            this.OppositeBooleanValue = false;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is VATRPHistoryEvent.StatusType && targetType == typeof(Visibility))
            {
                bool booleanValue = (VATRPHistoryEvent.StatusType)value == VATRPHistoryEvent.StatusType.Missed;

                if (OppositeBooleanValue)
                {
                    booleanValue = !booleanValue;
                }

                return booleanValue ? Visibility.Collapsed : FalseEquivalent;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return null;
        }

    }
}
