using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;

namespace Microsoft.HandsFree.Keyboard.Model
{
    class HandlerToCommand : ICommand
    {
        readonly MethodInfo method;
        readonly object obj;

        HandlerToCommand(object obj, MethodInfo method)
        {
            this.obj = obj;
            this.method = method;

            Debug.Assert(CanExecuteChanged == null);
        }

        /// <summary>
        /// Perform named action.
        /// </summary>
        /// <param name="ob">The object.</param>
        /// <param name="name">Name of action.</param>
        internal static ICommand Create(object ob, string name)
        {
            var type = ob.GetType();
            var method = type.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(object), typeof(EventArgs) }, null);

            ICommand action;
            if (method != null)
            {
                action = new HandlerToCommand(ob, method);
            }
            else
            {
                action = null;
            }

            return action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            method.Invoke(obj, new object[] { parameter, EventArgs.Empty });
        }
    }
}

