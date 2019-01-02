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
using System.Windows.Input;

namespace com.vtcsecure.ace.windows.Model
{
	/// <summary>
	/// This class is similar to the "ActionCommand"-class from the 'Microsoft.Expression.Interactivity.Core'-namespace
	/// which is included in the 'Microsoft.Expression.Interactions.dll' from the Microsoft Blend 4 SDK
	/// (so you do not need to install the SDK to use this command)
	/// </summary>
	public class ActionCommand : ICommand
	{
		private readonly Action executeHandler;
		private readonly Action<object> executeHandlerWithParameter;
		private readonly Func<object, bool> canExecuteHandler;

		/// <summary>
		/// Parameterless version of the ActionCommand
		/// </summary>
		/// <param name="execute"></param>
		public ActionCommand(Action execute)
		{
			if (execute == null)
			{
				throw new ArgumentNullException( "Execute can't be null", new Exception() );
			}

			this.executeHandler = execute;
		}

		/// <summary>
		/// ActionCommand with an object-parameter
		/// </summary>
		/// <param name="execute"></param>
		public ActionCommand(Action<object> execute)
		{
			if (execute == null)
			{
				throw new ArgumentNullException( "Execute can't be null", new Exception() );
			}

			this.executeHandlerWithParameter = execute;
		}

		/// <summary>
		/// ActionCommand with an object-parameter and a CanExecute delegate
		/// </summary>
		/// <param name="execute"></param>
		/// <param name="canExecute"> </param>
		public ActionCommand(Action<object> execute, Func<object, bool> canExecute)
			: this( execute )
		{
			this.canExecuteHandler = canExecute;
		}

		#region Members of the 'ICommand' interface

		public void Execute(object args)
		{
			if (this.executeHandlerWithParameter != null)
			{
				this.executeHandlerWithParameter( args );
			}
			else
			{
				this.executeHandler();
			}
		}

		public bool CanExecute(object args)
		{
			if (this.canExecuteHandler == null)
			{
				return true;
			}

			return this.canExecuteHandler( args );
		}

		public event EventHandler CanExecuteChanged
		{
			add
			{
				CommandManager.RequerySuggested += value;
			}
			remove
			{
				CommandManager.RequerySuggested -= value;
			}
		}

		#endregion // Members of the 'ICommand' interface
	}
}
