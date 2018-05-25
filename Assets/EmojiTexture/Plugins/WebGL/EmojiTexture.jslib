const EmojiTexturePrefix = "EmojiTexture_";

var EmojiTextureHelper = {
	canvas: null,
    ToCsString: function (str) 
    {
        if (typeof str === 'object') {
            str = JSON.stringify(str);
        }
        var bufferLength = lengthBytesUTF8(str) + 1;
        var buffer = _malloc(bufferLength);
        stringToUTF8(str, buffer, bufferLength);
        return buffer;
    },
	ToJsString : function (ptr) {
		return Pointer_stringify(ptr);
	},
	ToJsObject : function (ptr) {
		var str = Pointer_stringify(ptr);
		try {
			return JSON.parse(str);
		} catch (e) {
			return null;
		}
	},
	FreeMemory : function (ptr) {
		_free(ptr);
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
