using UnityEngine;
using System.Collections.Generic;

public class MountViewConfig : Config 
{
	/// <summary>
	/// 描述
	/// </summary>
	[ConfigComment ("描述")]
	public readonly string describe;

	/// <summary>
	/// 模型资源
	/// </summary>
	[ConfigComment ("模型资源")]
	public readonly string modelPath;

}