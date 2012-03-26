using System.Collections.Generic;
using System.Linq;

namespace EB.Crime.DB.Rep
{
	public class CategoryRepository : Repository
	{
		public IEnumerable<Category> GetCategories()
		{
			return DB.Categories.ToList().Concat(
					new List<Category>() { new Category { DisplayName = "Andet", CategoryId = 42 } });
		}
	}
}
