using System;

namespace UnityTools.Util
{
	public interface IModel
	{
		event Action OnUpdated;
	}

	public abstract class BaseModel : IModel
	{
		public event Action OnUpdated;

		protected void NotifyUpdated()
		{
			OnUpdated?.Invoke();
		}
	}
}
