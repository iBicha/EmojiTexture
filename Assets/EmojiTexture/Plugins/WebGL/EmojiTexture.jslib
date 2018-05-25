const EmojiTexturePrefix = "EmojiTexture_";

var EmojiTextureHelper = {
	canvas: null,
	ToJsString : function (ptr) {
		return Pointer_stringify(ptr);
	}
};

var EmojiTexturePlugin = {
	render : function (emojiTextPtr, buffer, width, height, /*ignored*/ sanitize) {
		if (!EmojiTextureHelper.canvas)
		{
			EmojiTextureHelper.canvas = document.createElement('canvas');
			EmojiTextureHelper.canvas.style.display = "none";
			EmojiTextureHelper.canvas.id = "emojiCanvas";
		}

		var emojiText = EmojiTextureHelper.ToJsString(emojiTextPtr);

		EmojiTextureHelper.canvas.width = width;
		EmojiTextureHelper.canvas.height = height;

		var ctx = EmojiTextureHelper.canvas.getContext("2d");
		ctx.textAlign = "center";
		ctx.textBaseline = "middle"
		ctx.font = "256px sans-serif";

		var size = width;
		var scaleFactor = size / ctx.measureText(emojiText).width;

		ctx.translate(size/2, size/2);
		ctx.scale(scaleFactor, -scaleFactor);
		ctx.translate(-size/2, -size/2);

		ctx.fillText(emojiText, size/2, size/2);

		var imageData = ctx.getImageData(0, 0, width, height);
		writeArrayToMemory(imageData.data, buffer);

		//TODO: handle 'sanitize'
		return emojiText.length;
	}
};

function MergePlugins(plugins, prefixes) {
	if (!Array.isArray(plugins)) {
		plugins = [plugins];
	}
	if (!Array.isArray(prefixes)) {
		prefixes = [prefixes];
	}
	for (var i = 0; i < plugins.length; i++) {
		//keys
		for (var key in plugins[i]) {
			if (plugins[i].hasOwnProperty(key)) {
				plugins[i][prefixes[i] + key] = plugins[i][key];
				delete plugins[i][key];
			}
		}
		//helper
		plugins[i].$EmojiTextureHelper = EmojiTextureHelper;
		autoAddDeps(plugins[i], '$EmojiTextureHelper');
		//merge
		mergeInto(LibraryManager.library, plugins[i]);
	}
}

MergePlugins(EmojiTexturePlugin, EmojiTexturePrefix);
