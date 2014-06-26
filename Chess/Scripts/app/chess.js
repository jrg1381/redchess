var currentPlayerColor;
var gameId;
var lastMoveFromX;
var lastMoveFromY;
var lastMovedLayer;
var currentTurn;
var files = ["A", "B", "C", "D", "E", "F", "G", "H"];
var boardSize = 512;
var squareSize = 64;
var symbolOffset = 32;
/* Unicode chess piece characters */
var pieceMapping = {
    "K": "\u2654", "Q": "\u2655", "N": "\u2658", "R": "\u2656", "P": "\u2659", "B": "\u2657",
    "k": "\u265a", "q": "\u265b", "n": "\u265e", "r": "\u265c", "p": "\u265f", "b": "\u265d"
};
/* The location of the bitmap image for each kind of piece in the sprite */
var imageMapping = {
    "K": [194, 95], "Q": [130, 95],"N": [260, 95], "R": [0, 95], "P": [325, 95], "B": [64, 95],
    "k": [194, 0], "q": [130, 0],"n": [260, 0], "r": [0, 0], "p": [325, 0], "b": [64, 0]
};
    
function DrawBoard() {
    var board = $("canvas#chessboard");

    var block = {
        fillStyle: "#4f4f4f",
        width: squareSize,
        height: squareSize,
        fromCenter: false
    };

    for (var x = 0; x < 8; x++)
        for (var y = 0; y < 8; y++) {

            if (x % 2 == 0 ^ y % 2 == 0) {
                block['x'] = x * squareSize;
                block['y'] = y * squareSize;
                board.drawRect(block);
            }
        }
}
	    
function isWhitePiece(z) {
    var re = /^[RNBQKP?]$/;
    return re.test(z);
}

function MovePiece(layer) {
    lastMovedLayer = layer;
    var board = $("canvas#pieces");
    var file, rank;
        
    if (isWhitePiece(layer.name)) { // White pieces
        file = files[Math.floor(layer.x / squareSize)];
        rank = 8 - Math.floor(layer.y / squareSize);
    }
    else { // Black pieces, so the board is reversed
        file = files[7 - Math.floor(layer.x / squareSize)];
        rank = Math.floor(layer.y / squareSize) + 1;
    }
    
    // It's possible to drag to bogus locations
    if (rank <= 0 || rank > 9 || file.length == 0) {
        ResetEarlierMoves();
        return;
    }

    $("input#Start").val(layer.data);
    $("input#End").val(file + rank);
        
    // Go to the nearest correct square
    board.setLayer(layer, { x: Math.floor(layer.x / squareSize) * squareSize + symbolOffset, y: Math.floor(layer.y / squareSize) * squareSize + symbolOffset });
        
    // Promotions always take place on the far side of the board, from the current player's point of view
    if ((rank == 8 && layer.name == "P") || (rank == 1 && layer.name == "p")) {
        $("#submitmove form").show();
        $("#Promote").val("Queen");
    }
    else {
        $("#submitmove form").hide();
        $("#Promote").val([]);
        PostMove(layer.data, file + rank, "");
    }
}

function PostMove(start, end, promote) {
    if(start == end)
        return;
    $.post("/Board/PlayMove", {
        "id": gameId,
        "Start": start, 
        "End": end, 
        "Promote": promote, 
        "__RequestVerificationToken": $('[name=__RequestVerificationToken]').val() }
    ).done(ProcessServerResponse);
}

function Resign() {
    if(!confirm("Are you sure you want to resign?")) {
        return;
    }

    $.post("/Board/Resign", {
        "id": gameId,
        "__RequestVerificationToken": $('[name=__RequestVerificationToken]').val()
    }).done(ProcessServerResponse);
}

function TellServerGameIsTimedOut(message) {
    $.post("/Board/TimedOut", {
        "id": gameId,
        "message": message,
        "__RequestVerificationToken": $('[name=__RequestVerificationToken]').val()
    }).done(ProcessServerResponse);
}

function EndGame() {
    LockBoard();
    if (isTimedGame) {
        clearInterval(myClock.timerId);
        $("div#readybutton").hide();
    }
    $("#turnindicator").text("GAME OVER");
    $("#resign").hide();
}

function ProcessServerResponse(data) {
    DrawPieces(data.fen);
    $("#messages").text(data.message);
   
    if (data.status == "AUTH") {
        return; /* Not allowed to play on this board */
    }

    if (data.status == "RESIGN" || data.status == "TIME") {
        EndGame();
        return;
    }

    if (data.status != "OK")
        return;

    if (isTimedGame) {
        myClock.SetLastSyncTime(myClock);
        myClock.SyncClockWithServer(myClock);
    }

    $("#lastmove").text(data.movefrom +" -> " + data.moveto);
    $("#fen").text(data.fen);
    UpdateTakenPieces(data.fen);
    
    //crappy
    if(data.message.match(/(draw)|(mate)/i) != null) {
        EndGame();
        return;
    }

    HighlightSquares(data.movefrom, data.moveto);
}

function HighlightSquares(moveFrom, moveTo) {
    var board = $("canvas#annotations");
    // The annotations layer is 5px bigger all round than the board and pieces layers, to prevent the edges of the highlights being truncated
    var offset = 5;
        
    board.clearCanvas();

    var yStart = boardSize - moveFrom.substring(1, 2) * squareSize;
    var yEnd = boardSize - moveTo.substring(1, 2) * squareSize;
    var xStart = files.indexOf(moveFrom.substring(0, 1)) * squareSize;
    var xEnd = files.indexOf(moveTo.substring(0, 1)) * squareSize;
        
    if(currentPlayerColor == "b") {
        // Looking at the board from black's point of view, co-ordinates are reversed
        xStart = boardSize - xStart - squareSize;
        yStart = boardSize - yStart - squareSize;
        xEnd = boardSize - xEnd - squareSize;
        yEnd = boardSize - yEnd - squareSize;
    }

    var rectangle = {
        strokeWidth: 4,
        strokeStyle: "#ff9999",
        x: xStart + offset,
        y: yStart + offset,
        width: squareSize,
        height: squareSize,
        cornerRadius: 5,
        fromCenter: false
    };

    board.drawRect(rectangle);

    rectangle['strokeStyle'] = "#ff9900";
    rectangle['x'] = xEnd + offset;
    rectangle['y'] = yEnd + offset;
        
    board.drawRect(rectangle);
}

function ResetEarlierMoves(layer) {
    var board = $("canvas#pieces");
    board.setLayer(lastMovedLayer, { x: lastMoveFromX, y: lastMoveFromY });
    lastMovedLayer = layer;
    lastMoveFromX = layer.x;
    lastMoveFromY = layer.y;
}

function LockBoard() {
    $("canvas#annotations").css("z-index", "2");
    $("canvas#pieces").css("z-index", "1");
    $("canvas#annotations").clearCanvas();
    $("canvas#annotations").drawText({
        fillStyle: "#fff",
        strokeStyle: "#25a",
        strokeWidth: 2,
        x: 256, y: 256,
        fontSize: "36pt",
        fontFamily: "Verdana, sans-serif",
        text: "Locked"
    });
}

function UnlockBoard() {
    $("canvas#annotations").clearCanvas();
    $("canvas#annotations").css("z-index", "1");
    $("canvas#pieces").css("z-index", "2");
}

function DrawPieces(fen) {
    var board = $("canvas#pieces");
    board.clearCanvas();
    board.removeLayerGroup("pieces");
               
    // All_Chess_Pieces_Png_by_abener.png

    var image = {
        source : "../../Images/All_Chess_Pieces_Png_by_abener.png",
        layer: true,
        index: 1,
        draggable: false,
        bringToFront: true,
        dragstart: function (layer) { ResetEarlierMoves(layer); },
        dragstop: function (layer) { MovePiece(layer); },
        sWidth: 64,
        sHeight: 90,
        group: "pieces",
        cropFromCenter : false
    };

    var index = 0;
    var splitFen = fen.split(" ");
    var position = splitFen[0];
    var turn = splitFen[1];

    if ($("#turnindicator").text() != "GAME OVER") {
        currentTurn = turn;
        $("#turnindicator").text((turn == "b" ? "Black" : "White") + " to play");
    }

    var isDigit = (function () {
        var re = /^\d$/;
        return function(z) {
            return re.test(z);
        };
    }());

    for (var i = 0; i < position.length; i++) {
        var c = position.substring(i,i+1);
	            
        if (isDigit(c)) {
            index += parseInt(c);
            continue;
        }

        if (c === "/") {
            continue;
        }
                       
        if (isWhitePiece(c)) {
            if (currentPlayerColor == "w" && (turn == "w")) {
                image['draggable'] = true;
            }
            else {
                image['draggable'] = false;
            }
        }
        else {
            if (currentPlayerColor == "b" && (turn == "b")) {
                image['draggable'] = true;
            }
            else {
                image['draggable'] = false;
            }
        }

        if (currentPlayerColor == "b") {
            image['x'] = (7 - (index % 8)) * squareSize + symbolOffset;
            image['y'] = (7 - Math.floor((index / 8))) * squareSize + symbolOffset;
        }
        else {
            image['x'] = (index % 8) * squareSize + symbolOffset;
            image['y'] = Math.floor((index / 8)) * squareSize + symbolOffset;
        }

        image['data'] = files[index % 8] + (8 - Math.floor(index / 8));
        image['name'] = c;

        image['sx'] = imageMapping[c][0];
        image['sy'] = imageMapping[c][1];
        board.drawImage(image);

        index++;
    }
}
    
function UpdateTakenPieces(fen) {
    var initialFen = fen.split(" ")[0];
    var blackArmy = "rrnnbbqpppppppp";
    var whiteArmy = "RRNNBBQPPPPPPPP";
        
    for(var i=0;i<initialFen.length;i++) {
        var c = initialFen[i];
        blackArmy = blackArmy.replace(c, "");
        whiteArmy = whiteArmy.replace(c, "");
    }

    $("#blacktaken").text(blackArmy.split("").map(function(x) { return pieceMapping[x]; }).join(""));
    $("#whitetaken").text(whiteArmy.split("").map(function(x) { return pieceMapping[x]; }).join(""));
}