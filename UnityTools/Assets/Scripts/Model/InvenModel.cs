using System.Collections.Generic;
using UnityTools.Util;

namespace UnityTools.Model
{
	public class InvenModel : BaseModel
	{
		private List<InvenItemModel> _itemModels = new();
		
		public int ItemCount => _itemModels.Count;
		
		public InvenModel(int cnt)
		{
			for (int i = 0; i < cnt; i++)
			{
				_itemModels.Add(new(i));
			}
		}
		
		public InvenItemModel Get(int idx)
		{
			return _itemModels.IsValidIndex(idx) ? _itemModels[idx] : null;
		}
	}
}
