using System;

using Castle.Windsor;

namespace NAntExtensions.TeamCity.Common.Container
{
	public static class IoC
	{
		static IWindsorContainer InternalContainer;

		static IoC()
		{
			Initialize(new WindsorContainer());
		}

		public static IWindsorContainer Container
		{
			get
			{
				IWindsorContainer result = InternalContainer;
				if (result == null)
				{
					throw new InvalidOperationException(
						"The container has not been initialized. Please call IoC.Initialize(container) before using it.");
				}
				return result;
			}
		}

		public static bool IsInitialized
		{
			get { return InternalContainer != null; }
		}

		public static void Initialize(IWindsorContainer windsorContainer)
		{
			InternalContainer = windsorContainer;

			foreach (var registration in Registrations.Get())
			{
				InternalContainer.Register(registration);
			}
		}

		/// <summary>
		/// Tries to resolve the component, but return null
		/// instead of throwing if it is not there.
		/// </summary>
		public static T TryResolve<T>()
		{
			return TryResolve(default(T));
		}

		/// <summary>
		/// Tries to resolve the component, but return the default 
		/// value if could not find it, instead of throwing.
		/// </summary>
		public static T TryResolve<T>(T defaultValue)
		{
			if (Container.Kernel.HasComponent(typeof(T)) == false)
			{
				return defaultValue;
			}
			return Container.Resolve<T>();
		}

		public static T Resolve<T>()
		{
			return Container.Resolve<T>();
		}

		public static T Resolve<T>(object argumentsAsAnonymousType)
		{
			return Container.Resolve<T>(argumentsAsAnonymousType);
		}

		public static T Resolve<T>(string name)
		{
			return Container.Resolve<T>(name);
		}

		public static void Reset()
		{
			InternalContainer = null;
		}

		public static Array ResolveAll(Type service)
		{
			return Container.ResolveAll(service);
		}

		public static T[] ResolveAll<T>()
		{
			return Container.ResolveAll<T>();
		}

		public static object Resolve(Type serviceType)
		{
			return Container.Resolve(serviceType);
		}

		public static object Resolve(string serviceName)
		{
			return Container.Resolve(serviceName);
		}

		public static object Resolve(Type serviceType, string serviceName)
		{
			return Container.Resolve(serviceName, serviceType);
		}
	}
}