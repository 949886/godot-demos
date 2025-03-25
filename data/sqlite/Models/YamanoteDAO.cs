using Godot;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yamanote;

namespace Yamanote;

public partial class YamanoteDAO
	{
		private SQLiteConnection _db;

		// Called when the node enters the scene tree for the first time.
		public YamanoteDAO(string databaseName = null)
		{
			var path = "";
			if (OS.HasFeature("editor"))
				path = ProjectSettings.GlobalizePath("res://data/sqlite/");
			else path = OS.GetExecutablePath().GetBaseDir().PathJoin("data");
			
			string databasePath = Path.Combine(path, databaseName ?? "yamanote.sqlite");
			_db = new SQLiteConnection(databasePath);
			_db.CreateTable<YamanoteQuestion>();
		}

		public void AddQuestion(YamanoteQuestion question)
		{
			_db.Insert(question);
		}

		public List<YamanoteQuestion> GetQuestions(string fromCategory = null)
		{
			var questions = _db.Query<YamanoteQuestion>("SELECT * FROM questions");
			if (fromCategory == null)
				return questions.ToList();

			var knowledgeQuestions =
				from question in questions
				where question.Category == fromCategory
				select question;

			return knowledgeQuestions.ToList();
		}

		public List<YamanoteCategory> GetCategories()
		{
			var questions = _db.Query<YamanoteQuestion>("SELECT * FROM questions");

			var categories =
				from question in questions
				group question by question.Category into categoryGroup
				select new YamanoteCategory
				{
					Name = categoryGroup.Key,
					Questions = categoryGroup.ToList()
				};

			return categories.ToList();
		}

		public void Test()
		{
			var questions = GetQuestions("knowledge");

			foreach (var question in questions)
				Console.WriteLine($"{question.Content} - {question.Category} - {question.Theme} - {question.Difficulty}");

			var categories = GetCategories();

			foreach (var category in categories)
			{
				Console.WriteLine($"{category.Name} - {category.Questions.Count} questions");
				foreach (var question in category.Questions)
				{
					Console.WriteLine($"  {question.Content} - {question.Theme} - {question.Difficulty}");
				}
			}
		}
	}