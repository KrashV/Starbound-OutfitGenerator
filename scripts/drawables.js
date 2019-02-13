// Script handler

var currentImage;

// On load
$(function() {
	
	// Bind image
	$('#btnSelectImage').on('click', function() {
		$('#selectImage').trigger('click');
	});
	
	$("#selectImage").change(function() {
		currentImage = null;
		readImageInput(this, imageLoaded);
		this.value = "";
	});
	

	// Bind generate output
	$("#btnGenerate").click(generateFile);
});

/**
  * Reads the image input
  * https://github.com/Silverfeelin/Starbound-Hatter/blob/master/scripts/drawables.js
**/
function readImageInput(input, callback) {
	if (input.files && input.files.length > 0) {
		// Use first file. By default, users shouldn't be able to select multiple files.
		var file = input.files[0];

		var fr = new FileReader();
		fr.onload = function() {
		  var img = new Image;
		  img.onload = callback;

		  img.src = this.result;
		};
		fr.readAsDataURL(file);
	}
}

/**
  * Process the loading of the selected image
  * https://github.com/Silverfeelin/Starbound-Hatter/blob/master/scripts/drawables.js
**/
function imageLoaded() {
  var image = this;
  // Animate the preview update in three steps.
  var step = -1;

  var steps = [
    // Step one: Fade out the previous hat, if there is one.
    function() {
      if ($("#cvsPrevewImage").is(":visible"))
        $("#cvsPrevewImage").fadeOut(100, nextStep);
      else
        nextStep();
    },
    // Step two: Draw the new hat, and animate the preview dimensions if the new hat is bigger or smaller than the previous hat.
    function() {
      currentImage = image;
	  
      clearCanvas($("#cvsPrevewImage").get(0));
      var bot, lef;
	  drawResizedImage($("#cvsPrevewImage").get(0), currentImage, 2);
	  bot = currentImage.height*2,
	  lef = currentImage.width*2;
	  $("#cvsPrevewImage").animate({bottom: bot, left: lef}, 200, nextStep);
    },
    // Step three: Fade in the new hat.
    function() {
      $("#cvsPrevewImage").fadeIn(100);
    }
  ];

  var nextStep = function() {
    if (typeof(steps[++step]) == "function")
      steps[step]();
  };

  nextStep();
}

/**
 * Clears the given canvas, or a part of it.
 * @param {object} canvas - DOM element to clear.
 * @param {number} [dx=0] - X-coordinate of the upper left corner of the area to clear.
 * @param {number} [dy=0] - Y-coordinate of the upper left corner of the area to clear.
 * @param {number} [width=canvas.width] - Width of area to clear.
 * @param {number} [height=canvas.height] - Height of area to clear.
 */
function clearCanvas(canvas, dx, dy, width, height) {
  if (dx === undefined || dx == null)
    dx = 0;
  if (dy === undefined || dy == null)
    dy = 0;
  if (width === undefined || width == null)
    width = canvas.width;
  if (height === undefined || height == null)
    height = canvas.height;

  var context = canvas.getContext('2d');

  context.clearRect(0, 0, canvas.width, canvas.height);
}

/**
 * Draws the given image on the canvas. Scaling is done without smoothing.
 * Sets the canvas to to the desired width and height, or calculate these values from the image dimensions and scale.
 * @param {object} canvas - Canvas DOM element to draw the image on.
 * @param {object} image - Image to draw.
 * @param {number} [scale=1] Scale of image, 1 is original size.
 * @param {object} [srcStart=[0,0]] Start point of the source image.
 * @param {object} [srcSize] Size of the region to capture from the source image. Defaults to (image size - srcStart).
 * @param {object} [destStart=[0,0]] Destination point of the drawn image.
 * @param {object} [destSize] Size of drawn image. Defaults to srcSize * scale.
 */
function drawResizedImage(canvas, image, scale, srcStart, srcSize, destStart, destSize) {
	
  if (scale === undefined || scale == null)
    scale = 1;
  if (srcStart === undefined || srcStart == null)
    srcStart = [0,0];
  if (srcSize === undefined || srcSize == null)
    srcSize = [image.width - srcStart[0], image.height - srcStart[1]];
  if (destStart === undefined || destStart == null)
    destStart = [0,0];
  if (destSize === undefined || destSize == null)
    destSize = [srcSize[0] * scale, srcSize[1] * scale];

  if (canvas.width != destSize[0] || canvas.height != destSize[1])
  {
    $(canvas).css("width", destSize[0]);
    $(canvas).css("height", destSize[1]);
    canvas.setAttribute("width", destSize[0]);
    canvas.setAttribute("height", destSize[1]);
  }

  var context = canvas.getContext('2d');

  context.imageSmoothingEnabled = false;
  context.msImageSmoothingEnabled = false;
  context.imageSmoothingEnabled = false;
  context.drawImage(image, srcStart[0], srcStart[1], srcSize[0], srcSize[1], destStart[0], destStart[1], destSize[0], destSize[1]);
  originalImageData = context.getImageData(srcStart[0], srcStart[1], destSize[0], destSize[1]);
  currentImageData = new ImageData(
	  new Uint8ClampedArray(originalImageData.data),
	  originalImageData.width,
	  originalImageData.height
	)
}

/**
 * Generates a emoteDirective export for the current spritesheet, and starts a download for it.
 */
function generateFile() {
    if (currentImage == null) {
        showAlert("#warning-empty-sheet");
        return;
    }
    getImageData("templates/" + $('#typeSelect').val() + ".png", function(width, height, templateImageData) {
		if (currentImage.width != width || currentImage.height != height)
		{
			showAlert('#warning-dimension-sheet')
			return
		}
		
        var directive = generateDirectives(width, height, templateImageData);
		$.ajax({
			dataType: "json",
			url: "templates/" + $('#typeSelect').val() + ".json",
			mimeType: "application/json",
			success: function(result){
				result.parameters.directives += directive
				var cmd = "/spawnitem " + result.name + " 1 '" + JSON.stringify(result.parameters).replace(/'/g, "\\'") + "'";
				var blob = new Blob([cmd], {
					type: "text/plain;charset=utf8"
				});
				saveAs(blob, "Custom" + $('#typeSelect').val() + ".txt");
				showAlert('#success-alert');
			}
		});
    });
}

/**
 * Get the ImageData from the image
 * @param {string} source - path to the image file
 **/
function getImageData(source, callback) {
    var image = new Image();

    image.onload = function() {
        var canvas = document.createElement('canvas');
        canvas.width = this.naturalWidth;
        canvas.height = this.naturalHeight;
        var ctx = canvas.getContext('2d');
        ctx.drawImage(this, 0, 0, canvas.width, canvas.height);
        // Return the image data
        callback(canvas.width, canvas.height, ctx.getImageData(0, 0, canvas.width, canvas.height).data);
    };
    image.src = source;
}

/**
 * Generate and returns an emoteDirective string to form the given spritesheet.
 * Thanks to NeetleBoy for his hard work.
 * @param {ImageData} templateData - ImageData of the template
 * @returns {string} Formatted directives string.
 */
function generateDirectives(width, height, templateData) {

    // Draw the selected image on a canvas, to fetch the pixel data.
    var canvas = document.createElement("canvas");
    canvas.width = width;
    canvas.height = height;

    var canvasContext = canvas.getContext("2d");
    canvasContext.drawImage(currentImage, 0, 0);
    var imageData = canvasContext.getImageData(0, 0, canvas.width, canvas.height).data;
    var directive = "";

    directive += "?replace";

    for (var px = 0, ct = canvas.width * canvas.height * 4; px < ct; px += 4) {
		var r = imageData[px];
		var g = imageData[px + 1];
		var b = imageData[px + 2];
		var a = imageData[px + 3];

		var tr = templateData[px];
		var tg = templateData[px + 1];
		var tb = templateData[px + 2];
		var ta = templateData[px + 3];

		if (ta != 0 && a != 0 && (r != tr || g != tg || b != tb)) {
			directive += ";" + colorToHex([tr, tg, tb, 0]) + "=" + colorToHex([r, g, b, a]);
		}
    }

    return directive;
}