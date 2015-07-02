function BoardViewer(positions, board) {
    this.positions = positions;
    this.board = board;
    this.currentMove = 0;

    var that = this;
    var spinner = CreateSpinner();

    function getLastMove() {
        return positions.Moves.length;
    }

    function getSpinner() {
        return spinner;
    }

    this.startSpinning = function () {
        getSpinner().spin($("#spinner-location")[0]);
    }

    this.stopSpinning = function () {
        getSpinner().stop();
    }

    this.updateBoard = function (newMove) {
        $("span#goBack").show();
        $("span#goForward").show();

        if (newMove === 0) {
            $("span#goBack").hide();
        }

        if (newMove === getLastMove() - 1) {
            $("span#goForward").hide();
        }

        board.position(positions.Moves[newMove].Fen);
    }

    function clickText() {
        $("#m" + that.currentMove).removeClass("highlightText");
        $(this).addClass("highlightText");
        that.currentMove = parseInt(this.id.substr(1));
        that.updateBoard(that.currentMove);
    }

    function populateMovesBox() {
        var moveNumber = 1;
        $("#moves").empty();

        for (var i = 1; i < getLastMove(); i += 2) {
            var originalI1 = i;
            var originalI2 = i + 1;

            var text = "<tr><td class=\"movenumber\">" + moveNumber++ + ".</td><td id=\"m" + originalI1 + "\">" + positions.Moves[originalI1].Move + "</td> ";
            if (originalI2 < getLastMove()) {
                text += "<td id=\"m" + originalI2 + "\">" + positions.Moves[originalI2].Move + "</td>";
            }
            text += "</tr>";

            $("#moves").append(text);

            $("#m" + originalI1).on("click", clickText);
            $("#m" + originalI2).on("click", clickText);
        }
    }

    function configureActionButtons() {
        $("span.button").mouseover(function () {
            $(this).parent().fadeTo(40, 1.0);
        }).mouseout(function () {
            $(this).parent().fadeTo(40, 0.7);
        });

        $("#goForward").on("click", function () {
            if (that.currentMove === getLastMove() - 1) return;
            that.updateBoard(that.currentMove + 1);
            $("#m" + that.currentMove).removeClass("highlightText");
            that.currentMove++;
            $("#m" + that.currentMove).addClass("highlightText");
        });

        $("#goBack").on("click", function () {
            if (that.currentMove === 0) return;
            that.updateBoard(that.currentMove - 1);
            $("#m" + that.currentMove).removeClass("highlightText");
            that.currentMove--;
            $("#m" + that.currentMove).addClass("highlightText");
        });

        $("#goStart").on("click", function () {
            that.updateBoard(0);
            $("#m" + that.currentMove).removeClass("highlightText");
            that.currentMove = 0;
            $("#m" + that.currentMove).addClass("highlightText");
        });

        $("#goEnd").on("click", function () {
            that.updateBoard(getLastMove() - 1);
            $("#m" + that.currentMove).removeClass("highlightText");
            that.currentMove = getLastMove() - 1;
            $("#m" + that.currentMove).addClass("highlightText");
        });

        $("#goFlip").on("click", function () {
            that.board.flip();
        });

        $("#playFromHere").on("click", function () {
            var queryDict = {};
            location.search.substr(1).split("&").forEach(function(item) { queryDict[item.split("=")[0]] = item.split("=")[1] });
            window.location = "/History/PlayFromHere?move=" + that.currentMove + "&gameId=" + queryDict["gameId"];
        });
    };

    populateMovesBox();
    configureActionButtons();
    board.position(this.positions.Moves[0].Fen);
    this.updateBoard(0);
}


