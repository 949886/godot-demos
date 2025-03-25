
### 1. Add the SQLite NuGet package to your .csproj file

```xml
  <ItemGroup>
	<PackageReference Include="sqlite-net-pcl" Version="1.9.172"/>
  </ItemGroup>
```

```xml
<Project Sdk="Godot.NET.Sdk/4.3.0">
  <PropertyGroup>
	<TargetFramework>net6.0</TargetFramework>
	<TargetFramework Condition=" '$(GodotTargetPlatform)' == 'android' ">net7.0</TargetFramework>
	<TargetFramework Condition=" '$(GodotTargetPlatform)' == 'ios' ">net8.0</TargetFramework>
	<EnableDynamicLoading>true</EnableDynamicLoading>
  </PropertyGroup>
  <ItemGroup>
	<PackageReference Include="sqlite-net-pcl" Version="1.9.172"/>
  </ItemGroup>
</Project>
```


### 2. Add the SQLite.cs file to your project
