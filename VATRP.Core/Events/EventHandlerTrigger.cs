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

namespace VATRP.Core.Events
{
    /// <summary>
    /// Helper class to handle event triggering.
    /// </summary>
    public static class EventHandlerTrigger
    {
        /// <summary>
        /// Check that the event handler is not null, and trigger this event with the given
        /// source and an <see cref="EventArgs.Empty"/>.
        /// </summary>
        /// <param name="handler">The event handler to trigger</param>
        /// <param name="source">The source to use</param>
        public static void TriggerEvent(EventHandler handler, Object source)
        {
            if (handler != null)
            {
                handler(source, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Check that the event handler is not null, and trigger this event with the given source
        /// and event data. This method has been made generic to handle all the <see cref="EventArgs"/>.
        /// </summary>
        /// <param name="handler">The event handler to trigger</param>
        /// <param name="source">The source to use</param>
        /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public static void TriggerEvent<T>(EventHandler<T> handler, Object source, T args) where T : EventArgs
        {
            if (handler != null)
            {
                handler(source, args);
            }
        }
    }
}
