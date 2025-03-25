using Godot;
using SQLite;
using System;
using System.IO;
// using SQLiteNetExtensions.Attributes;
using Yamanote;

public partial class SQLiteTest : Node
{
	private SQLiteConnection _db;

	private YamanoteDAO _dao = new YamanoteDAO();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var path = "";
		if (OS.HasFeature("editor"))
			path = ProjectSettings.GlobalizePath("res://data/sqlite/");
		else path = OS.GetExecutablePath().GetBaseDir().PathJoin("data");
		
		string databasePath = Path.Combine(path, "test1.sqlite");

		_db = new SQLiteConnection(databasePath);
		_db.CreateTable<Stock>();
		_db.CreateTable<Valuation>();

		GetStocks();
		
		// _dao.GetCategories();
		_dao.Test();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		AddStock();
	}

	public void AddStock()
	{
		// Crate a random stock
		var stock = new Stock
		{
			Symbol = Guid.NewGuid().ToString().Substring(0, 4)
		};

		var count = _db.Insert(stock);
		Console.WriteLine($"{count} records added [Stock: {stock.Symbol}]");
	}

	public void GetStocks()
	{
		var stocks = _db.Query<Stock>("SELECT * FROM Stocks");

		foreach (var stock in stocks)
		{
			Console.WriteLine(stock.Symbol);
		}
	}


	[Table("Stocks")]
	public class Stock
	{
		[PrimaryKey, AutoIncrement]
		[Column("id")]
		public int Id { get; set; }

		[Column("symbol")]
		public string Symbol { get; set; }
	}

	[Table("Valuation")]
	public class Valuation
	{
		[PrimaryKey, AutoIncrement]
		[Column("id")]
		public int Id { get; set; }

		[Indexed]
		[Column("stock_id")]
		public int StockId { get; set; }

		[Column("time")]
		public DateTime Time { get; set; }

		[Column("price")]
		public decimal Price { get; set; }
		
		
		// [ManyToOne]      // Many to one relationship with Stock
		// public Stock Stock { get; set; }
	}
}
