using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using iBicha;
using UnityEditor;
using UnityEngine;

[Serializable]
public class EmojiDirectory
{
	[Serializable]
	public struct EmojiItem {
		public string emoji;
		public string name;
		public string shortname;
		//public string unicode;
		//public string html;
		//public string category;
		//public int order;
	}

	private EmojiDirectory instance;
	
	public EmojiItem[] emojis;


	public static EmojiDirectory GetAll()
	{
		var jsonString = EmojiTextureSettings.Get.jsonTest.text;
		var data = JsonUtility.FromJson<EmojiDirectory>(jsonString);
		return data;
	}
}

