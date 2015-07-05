Chess.prototype.processServerResponse = function (data) {
    this.updateUi(data.fen);

    if (this.isTimedGame) {
        this.myClock.startClockTicking();
    }

    if (data.message) {
        $("#messages").parent().css('visibility', 'visible');
        $("#messages").text(data.message);
    } else {
        $("#messages").parent().css('visibility', 'hidden');
        $("#messages").html('&nbsp;');
    }

    if (data.mayClaimDraw) {
        $("#drawbutton").show();
    }

    if (data.status == "AUTH") {
        return; /* Not allowed to play on this board */
    }

    if (data.status == "RESIGN" || data.status == "TIME" || data.status == "DRAW") {
        this.endGame();
        return;
    }

    if (data.status != "OK")
        return;

    if (this.isTimedGame) {
        this.myClock.SetLastSyncTime(this.myClock);
        this.myClock.SyncClockWithServer(this.myClock);
    }

    $("#lastmove").text(data.lastmove);
    $("#fen").text(data.fen);
    this.updateTakenPieces(data.fen);

    var board = $('#board');

    board.find('.square-55d63').removeClass('highlight-white');
    board.find('.square-' + data.movefrom.toLowerCase()).addClass('highlight-white');
    board.find('.square-' + data.moveto.toLowerCase()).addClass('highlight-white');

    //crappy
    if (data.message.match(/(draw)|(mate)/i) != null) {
        this.endGame();
        return;
    }
};

function Chess(gameId, currentPlayerColor, clock) {
    this.gameId = gameId;
    this.currentPlayerColor = currentPlayerColor;
    this.boardLocked = false;
    this.spinner = null;
    this.isTimedGame = (clock != null);
    this.myClock = clock;
    if (clock != null) {
        clock.theChess = this;
    }

    /* Unicode chess piece characters */
    this.pieceMapping = {
        "K": "\u2654", "Q": "\u2655", "N": "\u2658", "R": "\u2656", "P": "\u2659", "B": "\u2657",
        "k": "\u265a", "q": "\u265b", "n": "\u265e", "r": "\u265c", "p": "\u265f", "b": "\u265d"
    };

    this.onDragStart = function (source, piece, position, orientation) {
        if ((orientation === 'white' && piece.search(/^w/) === -1) ||
            (orientation === 'black' && piece.search(/^b/) === -1) ||
            !$("form").is(":hidden") || piece.search(this.currentPlayerColor) == -1 || this.currentTurn != this.currentPlayerColor || this.boardLocked) {
            return false;
        }
        return true;
    };

    var cfg = { pieceTheme: '/Images/{piece}.png', showNotation: false, draggable: true, onDrop: this.onDrop.bind(this), onDragStart : this.onDragStart };
    this.chessBoard = new ChessBoard('board', cfg);

    if (this.currentPlayerColor == 'b') {
        this.chessBoard.flip();
    }

    //SignalR stuff
    var updater = $.connection.updateServer;
    // Define a client-side message which the server can call
    updater.client.addMessage = function (message) {
        this.processServerResponse(message);
    }.bind(this);

    // Define a callback for when the connection is established. We join the client group corresponding to our game ID.
    $.connection.hub.start(function () {
        $.connection.hub.proxies.updateserver.server.join(gameId);
    });

    // Hide the promotion UI and set its selection to nothing
    $("#submitmove form").hide();
    $("#Promote").val([]);

    // Hide the claim the draw button
    $("#drawbutton").hide();

    $("#submitmove form").submit(function () {
        this.postMove($("input#Start").val(), $("input#End").val(), $("#Promote option:selected").text());
        $("#submitmove form").hide();
        $("#Promote").val([]);
        return false;
    }.bind(this));
};

Chess.prototype.postMove = function (start, end, promote) {
    var gameId = this.gameId;

    if (start == end)
        return;

    this.getSpinner().spin(this.parentOfSpinny()[0]);

    if (this.isTimedGame) {
        this.myClock.pauseClock();
    }

    $.post("/Board/PlayMove", {
        "id": gameId,
        "Start": start,
        "End": end,
        "Promote": promote,
        "__RequestVerificationToken": $('[name=__RequestVerificationToken]').val()
    }
    ).done(this.processServerResponse.bind(this));
};

Chess.prototype.onDrop = function (source, target, piece) {
    if ((target[1] == '8' && piece == 'wP') || (target[1] == '1' && piece == 'bP')) {
        $("input#Start").val(source.toUpperCase());
        $("input#End").val(target.toUpperCase());
        $("#submitmove form").show();
        $("#Promote").val("Queen");
        return '';
    }

    this.postMove(source.toUpperCase(), target.toUpperCase(), $("#Promote option:selected").text());
    $("#submitmove form").hide();
    $("#Promote").val([]);
};

Chess.prototype.getSpinner = function () {
    if (this.spinner == null) {
        this.spinner = CreateSpinner();
    }
    return this.spinner;
};

Chess.prototype.parentOfSpinny = function() {
    if (this.chessBoard.orientation() == "black") {
        return $('#board').find(".square-e4");
    } else if (this.chessBoard.orientation() == "white") {
        return $('#board').find(".square-d5");
    }
};

Chess.prototype.resignGame = function() {
    if (!confirm("Are you sure you want to resign?")) {
        return;
    }

    $.post("/Board/Resign", {
        "id": this.gameId,
        "__RequestVerificationToken": $('[name=__RequestVerificationToken]').val()
    }).done(this.processServerResponse.bind(this));
};

Chess.prototype.claimDraw = function() {
    if (!confirm("Are you sure you want to claim a draw?")) {
        return;
    }

    $.post("/Board/ClaimDraw", {
        "id": this.gameId,
        "__RequestVerificationToken": $('[name=__RequestVerificationToken]').val()
    }).done(this.processServerResponse.bind(this));
};

Chess.prototype.tellServerGameIsTimedOut = function(message, timedOutColor) {
    $.post("/Board/TimedOut", {
        "id": this.gameId,
        "message": message,
        "timedoutcolor": timedOutColor,
        "__RequestVerificationToken": $('[name=__RequestVerificationToken]').val()
    }).done(this.processServerResponse.bind(this));
};

Chess.prototype.endGame = function() {
    this.lockBoard();
    if (this.isTimedGame) {
        clearInterval(this.myClock.timerId);
        $("div#readybutton").hide();
    }
    $("#turnindicator").text("GAME OVER");
    $("#resignbutton").hide();
    $("#drawbutton").hide();
};

Chess.prototype.updateUi = function(fen) {
    this.chessBoard.position(fen);

    var splitFen = fen.split(" ");
    var turn = splitFen[1];

    if ($("#turnindicator").text() != "GAME OVER") {
        this.currentTurn = turn;
        $("#turnindicator").text((turn == "b" ? "Black" : "White") + " to play");
    }

    if (this.parentOfSpinny()[0].hasChildNodes()) {
        this.getSpinner().stop();
    }
};

Chess.prototype.lockBoard = function() {
    if (this.boardLocked)
        return;
    this.boardLocked = true;
    this.parentOfSpinny().prepend('<div class="lockIcon"></div>');
};

Chess.prototype.unlockBoard = function() {
    if (!this.boardLocked)
        return;
    this.boardLocked = false;
    $(".lockIcon").remove();
};

Chess.prototype.updateTakenPieces = function (fen) {
    var pieceMapping = this.pieceMapping;
    var initialFen = fen.split(" ")[0];
    var blackArmy = "rrnnbbqpppppppp";
    var whiteArmy = "RRNNBBQPPPPPPPP";

    for (var i = 0; i < initialFen.length; i++) {
        var c = initialFen[i];
        blackArmy = blackArmy.replace(c, "");
        whiteArmy = whiteArmy.replace(c, "");
    }

    var regexBlack = /p+/;
    var answer = regexBlack.exec(blackArmy);
    if (answer != null && answer[0].length > 1) {
        blackArmy = blackArmy.replace(answer, 'p(' + answer[0].length + ')');
    }
    var regexWhite = /P+/;
    answer = regexWhite.exec(whiteArmy);
    if (answer != null && answer[0].length > 1) {
        whiteArmy = whiteArmy.replace(answer, 'P(' + answer[0].length + ')');
    }

    var whitePawnsTaken = "";
    var blackPawnsTaken = "";

    var partialWhitePieces = whiteArmy.split('P', 2);
    var partialBlackPieces = blackArmy.split('p', 2);
    var whitePieces = partialWhitePieces[0];
    var blackPieces = partialBlackPieces[0];
    if (partialWhitePieces.length > 1) {
        whitePawnsTaken = this.pieceMapping['P'] + "<span style=\"font-size : medium\">" + partialWhitePieces[1] + "</span>";
    }
    if (partialBlackPieces.length > 1) {
        blackPawnsTaken = this.pieceMapping['p'] + "<span style=\"font-size : medium\">" + partialBlackPieces[1] + "</span>";
    }

    $("#blacktaken").html(blackPieces.split("").map(function(x) { return pieceMapping[x]; }).join("&#8203;") + blackPawnsTaken);
    $("#whitetaken").html(whitePieces.split("").map(function(x) { return pieceMapping[x]; }).join("&#8203;") + whitePawnsTaken);
};
