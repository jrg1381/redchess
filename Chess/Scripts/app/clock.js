function Clock(gameId, initialTimelimit) {
    this.gameId = gameId;
    this.lastServerBlackTimeRemaining = initialTimelimit;
    this.lastServerWhiteTimeRemaining = initialTimelimit;
    this.isTimerEnabled = false;
    this.timerId = -1;
    this.theChess = null;
    var that = this;

    this.GetLastSyncTime = function() {
        return $.cookie("PageLastSeen" + this.gameId);
    };

    this.SetLastSyncTime = function() {
        $.cookie("PageLastSeen" + this.gameId, (new Date()).getTime());
    };

    this.InitializeClockTimeLimits = function() {
        $.post("/Clock/RefreshClock", { "id": gameId }).done(function(data) {
            that.lastServerBlackTimeRemaining = data.timeleftblack;
            that.lastServerWhiteTimeRemaining = data.timeleftwhite;
            $("#whitetime").text(new Date(that.lastServerWhiteTimeRemaining).toUTCString().substring(25, 17));
            $("#blacktime").text(new Date(that.lastServerBlackTimeRemaining).toUTCString().substring(25, 17));
        });
    };

    this.startClockTicking = function() {
        clearInterval(this.timerId); // Don't run multiple clocks
        this.timerId = setInterval(function () { that.LocalTimeCorrection(); }, 1000);
        this.isTimerEnabled = true;
    };

    this.pauseClock = function() {
        clearInterval(this.timerId);
        this.isTimerEnabled = false;
    };

    this.SyncClockWithServer = function() {
        // if we haven't already done so...
        this.theChess.unlockBoard();
        // Now update the time
        $.post("/Clock/RefreshClock", { "id": gameId }).done(function(data) {
            that.lastServerBlackTimeRemaining = data.timeleftblack;
            that.lastServerWhiteTimeRemaining = data.timeleftwhite;
            if (!that.isTimerEnabled) {
                that.LocalTimeCorrection();
                that.timerId = setInterval(function () { that.LocalTimeCorrection(); }, 1000);
                that.isTimerEnabled = true;
            }
        });
    };

    // This updates the UI with the time elapsed for each player    
    this.LocalTimeCorrection = function() {
        var displayTimeWhite, displayTimeBlack;
        var diff = (new Date()).getTime() - this.GetLastSyncTime();

        if (this.theChess.currentTurn == "b") {
            displayTimeWhite = new Date(this.lastServerWhiteTimeRemaining).toUTCString().substring(25, 17);
            displayTimeBlack = new Date(this.lastServerBlackTimeRemaining - diff).toUTCString().substring(25, 17);
            $("#whitetime").text(displayTimeWhite);
            $("#blacktime").text(displayTimeBlack);

            if (diff > this.lastServerBlackTimeRemaining) {
                this.SyncClockWithServer();
                clearInterval(this.timerId);
                $("#messages").text("Black is out of time");
                $("#blacktime").text("--:--");
                $("#turnindicator").text("GAME OVER");
                this.theChess.tellServerGameIsTimedOut("Black is out of time", "b");
            }
        }

        if (this.theChess.currentTurn == "w") {
            displayTimeWhite = new Date(this.lastServerWhiteTimeRemaining - diff).toUTCString().substring(25, 17);
            displayTimeBlack = new Date(this.lastServerBlackTimeRemaining).toUTCString().substring(25, 17);
            $("#whitetime").text(displayTimeWhite);
            $("#blacktime").text(displayTimeBlack);

            if (diff > this.lastServerWhiteTimeRemaining) {
                this.SyncClockWithServer();
                clearInterval(this.timerId);
                $("#messages").text("White is out of time");
                $("#whitetime").text("--:--");
                $("#turnindicator").text("GAME OVER");
                this.theChess.tellServerGameIsTimedOut("White is out of time", "w");
            }
        }
    };

    // Called when SignalR tells us that the other player is ready to play
    this.ReadyToPlay = function(data) {
        if (data.who == "w") {
            $("td#whiteready").removeClass("notready");
            $("td#whiteready").addClass("ready");
            $("td#whiteready").text("READY");
        } else if (data.who == "b") {
            $("td#blackready").removeClass("notready");
            $("td#blackready").addClass("ready");
            $("td#blackready").text("READY");
        }

        if (data.status == "OK") {
            if (null == this.GetLastSyncTime()) {
                this.SetLastSyncTime();
            }

            this.SyncClockWithServer();
        }
    };

    this.ClockDocumentReady = function() {
        //SignalR stuff
        var updater = $.connection.updateServer;
        // Define a client-side message which the server can call
        updater.client.startClock = function(message) {
            that.ReadyToPlay(message);
        };

        this.InitializeClockTimeLimits();

        if (this.theChess.currentPlayerColor == "") {
            $("td#ready").hide();
            return;
        }

        if (this.theChess.currentPlayerColor == "w" && $("td#whiteready").text() == "READY") {
            $("td#ready").hide();
            return;
        }

        if (this.theChess.currentPlayerColor == "b" && $("td#blackready").text() == "READY") {
            $("td#ready").hide();
            return;
        }

        $("div#readybutton").click(function() {
            $.post("/Clock/PlayerReady", { id: gameId }).done(function(data) {
                $("td#ready").hide();
                if (that.theChess.currentPlayerColor == "w") {
                    $("td#whiteready").removeClass("notready");
                    $("td#whiteready").addClass("ready");
                    $("td#whiteready").text("READY");
                } else if (that.theChess.currentPlayerColor == "b") {
                    $("td#blackready").removeClass("notready");
                    $("td#blackready").addClass("ready");
                    $("td#blackready").text("READY");
                }
            });
        });
    };
}
