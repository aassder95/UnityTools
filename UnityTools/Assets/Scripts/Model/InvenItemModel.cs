using UnityTools.Util;

namespace UnityTools.Model
{
	public class InvenItemModel : BaseModel
	{
		public int Id { get; private set; }
		
		public InvenItemModel(int id)
		{
			Id = id;
		}
	}
}
