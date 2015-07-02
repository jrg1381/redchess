function BoardViewer(positions, board) {
    this.positions = positions;
    this.board = board;
    this.lastMove = positions.Moves.length;
    this.currentMove = 0;
    this.spinner = CreateSpinner();

    this.parentOfSpinny = function() {
        return $('#spinner-location');
    }

    this.updateBoard = function(newMove) {
        $("span#goBack").show();
        $("span#goForward").show();

        if (newMove === 0) {
            $("span#goBack").hide();
        }

        if (newMove === this.lastMove - 1) {
            $("span#goForward").hide();
        }

        board.position(positions.Moves[newMove].Fen);
    }

    this.populateMovesBox = function() {
        var moveNumber = 1;
        $('#moves').empty();

        for (i = 1; i < this.lastMove; i += 2) {
            var originalI1 = i;
            var originalI2 = i + 1;

            var text = "<tr><td class=\"movenumber\">" + moveNumber++ + ".</td><td id=\"m" + originalI1 + "\">" + positions.Moves[originalI1].Move + "</td> ";
            if (originalI2 < this.lastMove) {
                text += "<td id=\"m" + originalI2 + "\">" + positions.Moves[originalI2].Move + "</td>";
            }
            text += "</tr>";

            $("#moves").append(text);

            $("#m" + originalI1).on("click", this, this.clickText);
            $("#m" + originalI2).on("click", this, this.clickText);
        }
    }

    this.configureActionButtons = function() {
        $("span.button").mouseover(function () {
            $(this).parent().fadeTo(40, 1.0);
        }).mouseout(function () {
            $(this).parent().fadeTo(40, 0.7);
        });

        $("#goForward").on("click", this, function (event) {
            if (event.data.currentMove == event.data.lastMove - 1) return;
            event.data.updateBoard(event.data.currentMove + 1);
            $("#m" + event.data.currentMove).removeClass("highlightText");
            event.data.currentMove++;
            $("#m" + event.data.currentMove).addClass("highlightText");
        });

        $("#goBack").on("click", this, function (event) {
            if (event.data.currentMove === 0) return;
            event.data.updateBoard(event.data.currentMove - 1);
            $("#m" + event.data.currentMove).removeClass("highlightText");
            event.data.currentMove--;
            $("#m" + event.data.currentMove).addClass("highlightText");
        });

        $("#goStart").on("click", this, function (event) {
            event.data.updateBoard(0);
            $("#m" + event.data.currentMove).removeClass("highlightText");
            event.datacurrentMove = 0;
            $("#m" + event.data.currentMove).addClass("highlightText");
        });

        $("#goEnd").on("click", this, function (event) {
            event.data.updateBoard(event.data.lastMove - 1);
            $("#m" + event.data.currentMove).removeClass("highlightText");
            event.data.currentMove = event.data.lastMove - 1;
            $("#m" + event.data.currentMove).addClass("highlightText");
        });

        $("#goFlip").on("click", this, function (event) {
            event.data.board.flip();
        });

        $("#playFromHere").on("click", this, function (event) {
            var queryDict = {};
            location.search.substr(1).split("&").forEach(function(item) { queryDict[item.split("=")[0]] = item.split("=")[1] });
            window.location = "/History/PlayFromHere?move=" + event.data.currentMove + "&gameId=" + queryDict["gameId"];
        });
    };

    this.clickText = function(event) {
        $("#m" + event.data.currentMove).removeClass("highlightText");
        $(this).addClass("highlightText");
        event.data.currentMove = parseInt(this.id.substr(1));
        event.data.updateBoard(event.data.currentMove);
    }

    this.populateMovesBox();
    this.configureActionButtons();
    board.position(this.positions.Moves[0].Fen);
    this.updateBoard(0);
}


