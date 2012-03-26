namespace EB.Crime.DB.Rep
{
	interface IRepository
	{
		DatabaseDataContext DB { get; }
	}

	public abstract class Repository : IRepository
	{
		private readonly DatabaseDataContext _db;

		public Repository()
		{
			_db = new DatabaseDataContext();
		}

		public DatabaseDataContext DB
		{
			get { return _db; }
		}
	}
}
