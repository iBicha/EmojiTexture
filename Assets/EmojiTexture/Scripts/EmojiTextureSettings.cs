using UnityEngine;
using UnityEngine.Serialization;

namespace iBicha
{
	public class EmojiTextureSettings : ScriptableObject
	{
		private static EmojiTextureSettings instance;

		public static EmojiTextureSettings Get
		{
			get
			{
				if (instance == null)
				{
					instance = Resources.Load<EmojiTextureSettings>("EmojiTextureSettings");
				}

				return instance;
			}
		}
		
		public bool UseGPUTextureCopy = true;

		[FormerlySerializedAs("EmojiSize")] 
		public int EmojiSizeInPixels = 128;
		public int SheetTiles = 4;

		public TextAsset githubEmojisJson;
		
		public TextAsset jsonTest;
	}


}
