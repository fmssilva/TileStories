using System;
using System.Collections.Generic;

namespace TileStories
{
    // The services a block view may ask of the card it sits on (_3.1 step 11): one small typed lookup, filled once at startup. A
    // service is registered under the type a view will ASK for (usually an interface), and a view asks through
    // BlockBindContext.Service<T>(). An app adds its own the same way it adds its own block kinds (BlockRegistry.Shared), so a kind
    // it registers gets its dependencies with no edit to the Framework; the shared ones (Strings, Media, Host, State, Events) stay plain
    // fields of the context. No reflection, no container: a dictionary keyed by the exact type.
    public sealed class CardServices
    {
        private readonly Dictionary<Type, object> _services = new();

        // The app's services: what the wall's card hosts and the gallery harness hand to every block by default. Tests build their own.
        public static CardServices Shared { get; } = new();

        // Register `instance` as THE T. A second T is refused (a silent replace would hide a clash between two registrations).
        public void Add<T>(T instance) where T : class
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (!_services.TryAdd(typeof(T), instance)) throw new ArgumentException("[Blocks] a service of type " + typeof(T).Name + " is already registered");
        }

        // The registered T, or null when there is none (a view then behaves as if the service did not exist)
        public T Get<T>() where T : class => _services.TryGetValue(typeof(T), out var instance) ? (T)instance : null;

        public bool Has<T>() where T : class => _services.ContainsKey(typeof(T));

        // Forget the registered T; false when there was none (a test cleaning up after itself)
        public bool Remove<T>() where T : class => _services.Remove(typeof(T));
    }
}
