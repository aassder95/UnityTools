using UnityTools.Util;

namespace UnityTools.Samples.Inven
{
	public class InvenItemModel : BaseModel
	{
		//============================================================
		//Readonly
		//============================================================
		private readonly int _id;

		//============================================================
		//Properties
		//============================================================
		public int Id => _id;

		//============================================================
		//Constructors
		//============================================================
		public InvenItemModel(int id)
		{
			_id = id;
		}
	}
}
