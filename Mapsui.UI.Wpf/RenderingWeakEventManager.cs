using System;
using System.Windows;
using System.Windows.Media;

namespace Mapsui.UI.Wpf
{
    class RenderingWeakEventManager : WeakEventManager
    {
        // based on: https://docs.microsoft.com/en-us/dotnet/framework/wpf/advanced/weak-event-patterns

        // chose weak events instead of Unsubscribe (The user needs to keep track of this) 
        // or Dispose (it breaks the contract https://stackoverflow.com/a/452294/85325).

        private RenderingWeakEventManager()
        {
        }

        /// <summary>
        /// Add a handler for the given source's event.
        /// </summary>
        public static void AddHandler(EventHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            // Thanks Dennis: https://stackoverflow.com/a/36739742/85325
            CurrentManager.ProtectedAddHandler(null, handler);
        }

        /// <summary>
        /// Remove a handler for the given source's event.
        /// </summary>
        public static void RemoveHandler(
            EventHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            
            CurrentManager.ProtectedRemoveHandler(null, handler);
        }

        /// <summary>
        /// Get the event manager for the current thread.
        /// </summary>
        private static RenderingWeakEventManager CurrentManager
        {
            get
            {
                var managerType = typeof(RenderingWeakEventManager);
                var manager =
                    (RenderingWeakEventManager)GetCurrentManager(managerType);

                // at first use, create and register a new manager
                if (manager == null)
                {
                    manager = new RenderingWeakEventManager();
                    SetCurrentManager(managerType, manager);
                }

                return manager;
            }
        }

        protected override void StartListening(object source)
        {
            CompositionTarget.Rendering += OnRendering;
        }

        protected override void StopListening(object source)
        {
            CompositionTarget.Rendering -= OnRendering;
        }

        void OnRendering(object sender, EventArgs e)
        {
            // Thanks objo! https://stackoverflow.com/a/8909689/85325
            DeliverEvent(null, e);
        }
    }
}