using Godot;
using System;
using System.Collections.Generic;
using Luna.Network;
using Yamanote;

public partial class NetworkTest : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Request.Get<List<YamanoteQuestion>>("usen/get/测试").ContinueWith(task =>
		{
			if (task.IsFaulted)
			{
				GD.PrintErr($"Failed to get yamanote questions: {task.Exception}");
				return;
			}

			var questions = task.Result;
			foreach (var question in questions)
			{
				GD.Print(question.Content);
			}
		});
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
