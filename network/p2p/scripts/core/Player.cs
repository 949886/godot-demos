// // Created by LunarEclipse on 2025-06-06 03:06.

using System;
using Godot;

public class Player
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime JoinTime { get; set; }
    public string AvatarPath { get; set; } = "";
    public Node3D Instance { get; set; }
}