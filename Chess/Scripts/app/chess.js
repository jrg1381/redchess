Chess.prototype.processServerResponse = function (data) {
    if (data.message) {
        $("#messages").parent().css('visibility', 'visible');
        $("#messages").text(data.message);
    } else {
        $("#messages").parent().css('visibility', 'hidden');
        $("#messages").html('&nbsp;');
    }

    if (data.status == "AUTH") {
        return; /* Not allowed to play on this board */
    }

    this.updateUi(data.fen);

    if (this.isTimedGame) {
        this.myClock.startClockTicking();
    }

    if (data.mayClaimDraw) {
        $("#drawbutton").text("Claim draw");
        $("#drawbutton").off();
        $("#drawbutton").addClass("btn-primary");
        $("#drawbutton").click(function () {
            this.claimDraw();
        }.bind(this));
    }

    if (data.status == "RESIGN" || data.status == "TIME" || data.status == "DRAW") {
        this.endGame();
        return;
    }

    if (data.status === "REJECT") {
        $("#drawoffer-sent").hide();
        return;
    };

    // Draw offer doesn't persist between moves. Making a move is the same as rejecting the offer.
    $("#drawoffer-sent").hide();
    $("#drawoffer").hide();

    if (data.status != "OK")
        return;

    if (this.isTimedGame) {
        this.myClock.SetLastSyncTime(this.myClock);
        this.myClock.SyncClockWithServer(this.myClock);
    }

    $("#lastmove").text(data.lastmove);
    $("#fen").text(data.fen);

    if (Date.prototype.toJSON !== undefined && !this.isTimedGame) {
        $("#moveplayedat").show();
        var now = (new Date()).toJSON();
        $("#moveplayedat time").attr("datetime", now);
        $("#moveplayedat time").timeago('update',now);
    }

    this.updateTakenPieces(data.fen);

    this.highlightLastMove(data.movefrom, data.moveto);

    //crappy
    if (data.message.match(/(draw)|(mate)/i) != null) {
        this.endGame();
        return;
    }
};

Chess.prototype.setFaviconTag = function (tagIcon) {
    // IE < 11 caches the favicon so we need a text change to the page title too
    var titleWithoutAnnotation = $("head > title").text().replace(/^\* /, "");

    if (tagIcon) {
        $("link[rel='shortcut icon']").attr("href", "/favicon-tagged.ico");
        $("head > title").text("* " + titleWithoutAnnotation);
    } else {
        $("link[rel='shortcut icon']").attr("href", "/favicon.ico");
        $("head > title").text(titleWithoutAnnotation);
    }
}

Chess.prototype.highlightLastMove = function(moveFrom, moveTo) {
    var board = $('#board');

    // This is the class assigned by chessboard.js to all squares
    board.find('.square-55d63').removeClass('highlight-white');
    // These classes let you find a particular square
    board.find('.square-' + moveFrom.toLowerCase()).addClass('highlight-white');
    board.find('.square-' + moveTo.toLowerCase()).addClass('highlight-white');
}

Chess.prototype.respondToDrawOffer = function(accepted) {
    $.post("/Board/AgreeDraw", {
        "id": this.gameId,
        "offerAccepted": accepted,
        "__RequestVerificationToken": $('[name=__RequestVerificationToken]').val()
    }).done(this.processServerResponse.bind(this));
};

Chess.prototype.showDrawOffer = function (message) {
    if (this.currentPlayerColor === "") {
        // Not a participant, so do nothing
        return;
    }

    // Ignore your own draw offer coming back at you, and if you're not a participant
    if (message.Data.DrawOfferedBy !== this.currentPlayerColor) {
        $("#drawoffer").show();
    } else {
        $("#drawoffer-sent").show();
    };
};

function Chess(gameId, currentPlayerColor, clock, analysisBoard) {
    this.gameId = gameId;
    this.currentPlayerColor = currentPlayerColor;
    this.boardLocked = false;
    this.spinner = null;
    this.source = "";
    this.target = "";
    this.gameOver = false;
    this.pendingPromotion = false;
    this.isTimedGame = (clock != null && clock.IsTimedGame);
    this.isAnalysisBoard = analysisBoard;
    this.myClock = clock;
    if (clock != null) {
        clock.theChess = this;
    }
    var that = this;

    /* Unicode chess piece characters */
    this.pieceMapping = {
        "K": "\u2654", "Q": "\u2655", "N": "\u2658", "R": "\u2656", "P": "\u2659", "B": "\u2657",
        "k": "\u265a", "q": "\u265b", "n": "\u265e", "r": "\u265c", "p": "\u265f", "b": "\u265d"
    };

    this.onDragStart = function (source, piece, position, orientation) {
        if (that.isAnalysisBoard) {
            if (that.pendingPromotion || piece.search(that.currentTurn) === -1 || that.boardLocked) {
                return false;
            }
            return true;
        }

        if ((orientation === 'white' && piece.search(/^w/) === -1) ||
            (orientation === 'black' && piece.search(/^b/) === -1) ||
            that.pendingPromotion || piece.search(that.currentPlayerColor) == -1 || that.currentTurn != that.currentPlayerColor || that.boardLocked) {
            return false;
        }
        return true;
    };

    var cfg = { pieceTheme: '/Images/{piece}.png', showNotation: true, draggable: true, onDrop: this.onDrop.bind(this), onDragStart : this.onDragStart };
    this.chessBoard = new ChessBoard('board', cfg);

    $(window).resize(function() {
        that.chessBoard.resize();
    });

    if (this.currentPlayerColor === 'b' && !this.isAnalysisBoard) {
        this.chessBoard.flip();
    }

    //SignalR stuff
    var updater = $.connection.updateServer;
    // Define a client-side message which the server can call
    updater.client.addMessage = function (message) {
        this.processServerResponse(message);
    }.bind(this);

    updater.client.showDrawOffer = function(message) {
        this.showDrawOffer(message);
    }.bind(this);

    // Define a callback for when the connection is established. We join the client group corresponding to our game ID.
    $.connection.hub.start(function () {
        $.connection.hub.proxies.updateserver.server.join(gameId);
    });

    var tryingToReconnect = false;

    $.connection.hub.reconnecting(function () {
        $("#messages").parent().css('visibility', 'visible');
        $("#messages").text("SignalR reconnecting...");

        tryingToReconnect = true;
    });

    $.connection.hub.reconnected(function() {
        $("#messages").parent().css('visibility', 'hidden');
        $("#messages").html('&nbsp;');

        tryingToReconnect = false;
    });

    var disconnectionFunction = function () {
        if (tryingToReconnect) {
            $.connection.hub.start(function() {
                $.connection.hub.proxies.updateserver.server.join(gameId);
                $("#messages").parent().css('visibility', 'hidden');
                $("#messages").html('&nbsp;');
                tryingToReconnect = false;
            });
            setTimeout(disconnectionFunction, 30000);
        }
    }

    $.connection.hub.disconnected(disconnectionFunction);
};

Chess.prototype.offerDraw = function() {
    var gameId = this.gameId;

    $.post("/Board/OfferDraw", {
        "id": gameId,
        "__RequestVerificationToken": $('[name=__RequestVerificationToken]').val()
    });
};

Chess.prototype.postMove = function (promote) {
    var gameId = this.gameId;
    var start = this.source;
    var end = this.target;

    if (start == end)
        return;

    this.getSpinner().spin(this.parentOfSpinny()[0]);

    if (this.isTimedGame) {
        this.myClock.pauseClock();
    }

    this.pendingPromotion = false;
    $("#promotion-selection").hide();

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
    this.source = source.toUpperCase();
    this.target = target.toUpperCase();

    if ((target[1] == '8' && piece == 'wP') || (target[1] == '1' && piece == 'bP')) {
        $("#promotion-selection").show();
        this.pendingPromotion = true;
        return '';
    }

    this.postMove();
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
    $("#drawoffer").hide();
    $("#drawoffer-sent").hide();

    this.gameOver = true;
};

Chess.prototype.updateUi = function(fen) {
    this.chessBoard.position(fen);

    var splitFen = fen.split(" ");
    var turn = splitFen[1];

    if (!this.gameOver) {
        this.currentTurn = turn;
        $("#turnindicator").text((turn == "b" ? "Black" : "White") + " to play");
    }

    if (this.parentOfSpinny()[0].hasChildNodes()) {
        this.getSpinner().stop();
    }

    // Set the marker unless it's game over in which case always remove the marker
    this.setFaviconTag((turn === this.currentPlayerColor) && !this.gameOver);
};

Chess.prototype.lockBoard = function() {
    if (this.boardLocked)
        return;
    this.boardLocked = true;
    $("#board-controls > a").attr("disabled", true);
    var lockIconParentSquare = this.parentOfSpinny().prepend('<div class="lockIcon"></div>');
    $(".lockIcon").width(lockIconParentSquare.width() * 2);
    $(".lockIcon").height(lockIconParentSquare.height() * 2);
};

Chess.prototype.unlockBoard = function() {
    if (!this.boardLocked)
        return;
    this.boardLocked = false;
    $("#board-controls > a").removeAttr("disabled");
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
