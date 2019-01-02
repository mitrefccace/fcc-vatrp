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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VATRP.LinphoneWrapper.Enums;

namespace VATRP.LinphoneWrapper
{
    public static partial class LinphoneAPI
    {
        /**
 * Create an outgoing subscription, specifying the destination resource, the event name, and an optional content body.
 * If accepted, the subscription runs for a finite period, but is automatically renewed if not terminated before.
 * @param lc the #LinphoneCore
 * @param resource the destination resource
 * @param event the event name
 * @param expires the whished duration of the subscription
 * @param body an optional body, may be NULL.
 * @return a LinphoneEvent holding the context of the created subcription.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_subscribe(IntPtr lc, IntPtr resource, string lEvent, int expires,
            IntPtr body);

/**
 * Create an outgoing subscription, specifying the destination resource, the event name, and an optional content body.
 * If accepted, the subscription runs for a finite period, but is automatically renewed if not terminated before.
 * Unlike linphone_core_subscribe() the subscription isn't sent immediately. It will be send when calling linphone_event_send_subscribe().
 * @param lc the #LinphoneCore
 * @param resource the destination resource
 * @param event the event name
 * @param expires the whished duration of the subscription
 * @return a LinphoneEvent holding the context of the created subcription.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_create_subscribe(IntPtr lc, IntPtr resource, string lEvent,
            int expires
            );

/**
 * Send a subscription previously created by linphone_core_create_subscribe().
 * @param ev the LinphoneEvent
 * @param body optional content to attach with the subscription.
 * @return 0 if successful, -1 otherwise.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_event_send_subscribe(IntPtr ev, IntPtr body);

/**
 * Update (refresh) an outgoing subscription.
 * @param lev a LinphoneEvent
 * @param body an optional body to include in the subscription update, may be NULL.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_event_update_subscribe(IntPtr lev, IntPtr body);


/**
 * Accept an incoming subcription.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_event_accept_subscription(IntPtr lev);

/**
 * Deny an incoming subscription with given reason.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_event_deny_subscription(IntPtr lev, LinphoneReason reason);

/**
 * Send a notification.
 * @param lev a #LinphoneEvent corresponding to an incoming subscription previously received and accepted.
 * @param body an optional body containing the actual notification data.
 * @return 0 if successful, -1 otherwise.
 **/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_event_notify(IntPtr lev, IntPtr body);


/**
 * Publish an event state.
 * This first create a LinphoneEvent with linphone_core_create_publish() and calls linphone_event_send_publish() to actually send it.
 * After expiry, the publication is refreshed unless it is terminated before.
 * @param lc the #LinphoneCore
 * @param resource the resource uri for the event
 * @param event the event name
 * @param expires the lifetime of event being published, -1 if no associated duration, in which case it will not be refreshed.
 * @param body the actual published data
 * @return the LinphoneEvent holding the context of the publish.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_publish(IntPtr lc, IntPtr resource, string lEvent, int expires,
            IntPtr body);

/**
 * Create a publish context for an event state.
 * After being created, the publish must be sent using linphone_event_send_publish().
 * After expiry, the publication is refreshed unless it is terminated before.
 * @param lc the #LinphoneCore
 * @param resource the resource uri for the event
 * @param event the event name
 * @param expires the lifetime of event being published, -1 if no associated duration, in which case it will not be refreshed.
 * @return the LinphoneEvent holding the context of the publish.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_create_publish(IntPtr lc, IntPtr resource, string lEvent, int expires);

/**
 * Send a publish created by linphone_core_create_publish().
 * @param lev the #LinphoneEvent
 * @param body the new data to be published
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_event_send_publish(IntPtr lev, IntPtr body);

/**
 * Update (refresh) a publish.
 * @param lev the #LinphoneEvent
 * @param body the new data to be published
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_event_update_publish(IntPtr lev, IntPtr body);


/**
 * Return reason code (in case of error state reached).
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern LinphoneReason linphone_event_get_reason(IntPtr lev);

/**
 * Get full details about an error occurred.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_event_get_error_info(IntPtr lev);

/**
 * Get subscription state. If the event object was not created by a subscription mechanism, #LinphoneSubscriptionNone is returned.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern LinphoneSubscriptionState linphone_event_get_subscription_state(IntPtr lev);

/**
 * Get publish state. If the event object was not created by a publish mechanism, #LinphonePublishNone is returned.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_event_get_publish_state(IntPtr lev);

/**
 * Get subscription direction.
 * If the object wasn't created by a subscription mechanism, #LinphoneSubscriptionInvalidDir is returned.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_event_get_subscription_dir(IntPtr lev);


/**
 * Add a custom header to an outgoing susbscription or publish.
 * @param ev the LinphoneEvent
 * @param name header's name
 * @param value the header's value.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_event_add_custom_header(IntPtr ev, string name, string value);

/**
 * Obtain the value of a given header for an incoming subscription.
 * @param ev the LinphoneEvent
 * @param name header's name
 * @return the header's value or NULL if such header doesn't exist.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern string linphone_event_get_custom_header(IntPtr ev, string name);

/**
 * Terminate an incoming or outgoing subscription that was previously acccepted, or a previous publication.
 * This function does not unref the object. The core will unref() if it does not need this object anymore.
 *
 * For subscribed event, when the subscription is terminated normally or because of an error, the core will unref.
 * For published events, no unref is performed. This is because it is allowed to re-publish an expired publish, as well as retry it in case of error.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_event_terminate(IntPtr lev);


/**
 * Get the name of the event as specified in the event package RFC.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern string linphone_event_get_name(IntPtr lev);

/**
 * Get the "from" address of the subscription.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_event_get_from(IntPtr lev);

/**
 * Get the resource address of the subscription or publish.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_event_get_resource(IntPtr lev);

/**
 * Returns back pointer to the LinphoneCore that created this LinphoneEvent
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_event_get_core(IntPtr lev);

    }
}
